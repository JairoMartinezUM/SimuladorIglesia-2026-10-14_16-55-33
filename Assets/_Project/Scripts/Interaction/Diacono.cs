using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class Diacono : MonoBehaviour, IInteractable
{
    [Header("Animacion (arrastra los clips del FBX)")]
    [SerializeField] Animator animator;          // el del modelo hijo (character-q)
    [SerializeField] AnimationClip idleClip;     // idle
    [SerializeField] AnimationClip talkClip;     // interact-right

    [Header("Comportamiento")]
    [SerializeField] bool alwaysFacePlayer = true; // si esta apagado, solo mira al jugador mientras habla
    [SerializeField] float turnSpeed = 6f;       // que tan rapido se voltea
    [SerializeField] float talkSeconds = 8f;     // igual que el mensaje en pantalla
    [SerializeField] float blendSpeed = 5f;      // transicion entre animaciones

    public string Prompt => "E: pedir ayuda al diácono";

    PlayableGraph graph;
    AnimationMixerPlayable mixer;
    AnimationClipPlayable idle, talk;
    bool hasAnim;
    float talkTimer;
    float talkWeight;
    Quaternion startRotation;
    Transform player;
    float idleTime;
    float talkTime = float.MaxValue;

    void Awake()
    {
        startRotation = transform.rotation;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Diacono: no encontre un Animator en el modelo hijo. Arrastra character-q al campo Animator.");
            return;
        }
        if (idleClip == null)
        {
            Debug.LogWarning("Diacono: falta el Idle Clip.");
            return;
        }
        animator.runtimeAnimatorController = null;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        graph = PlayableGraph.Create("Diacono");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var output = AnimationPlayableOutput.Create(graph, "Anim", animator);
        mixer = AnimationMixerPlayable.Create(graph, 2);
        idle = AnimationClipPlayable.Create(graph, idleClip);
        graph.Connect(idle, 0, mixer, 0);
        idle.SetSpeed(0);   // el tiempo lo manejamos nosotros en Update
        if (talkClip != null)
        {
            talk = AnimationClipPlayable.Create(graph, talkClip);
            talk.SetSpeed(0);
            graph.Connect(talk, 0, mixer, 1);
        }
        else Debug.LogWarning("Diacono: falta el Talk Clip.");
        mixer.SetInputWeight(0, 1f);
        mixer.SetInputWeight(1, 0f);
        output.SetSourcePlayable(mixer);
        graph.Play();
        hasAnim = true;
        Debug.Log($"Diacono: animacion lista (Animator en '{animator.gameObject.name}', idle={idleClip.length:F2}s, talk={(talkClip != null ? talkClip.length : 0):F2}s)");
    }

    void OnDestroy()
    {
        if (graph.IsValid()) graph.Destroy();
    }

    public void Interact()
    {
        if (LevelManager.Instance == null) return;
        string pista = LevelManager.Instance.AskForHelp();
        if (pista != null && HudMessage.Instance != null)
            HudMessage.Instance.Show($"<b>Diácono:</b> {pista}", talkSeconds);

        if (pista != null) StartTalking();
    }

    void StartTalking()
    {
        talkTimer = talkSeconds;
        talkTime = 0f;
        Debug.Log($"Diacono: empieza a hablar (hasAnim={hasAnim})");
    }

    void Update()
    {
        // 1) Mirar al jugador en todo momento (solo gira en el eje vertical)
        if (talkTimer > 0f) talkTimer -= Time.deltaTime;

        if (alwaysFacePlayer || talkTimer > 0f)
        {
            if (player == null && Camera.main != null) player = Camera.main.transform;
            if (player != null)
            {
                Vector3 dir = player.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion target = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
                }
            }
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, startRotation, turnSpeed * Time.deltaTime);
        }

        if (!hasAnim) return;

        // 2) Idle en bucle
        idleTime += Time.deltaTime;
        if (idleTime >= idleClip.length) idleTime -= idleClip.length;
        idle.SetTime(idleTime);

        // 3) Gesto una sola vez al empezar a hablar
        bool gesturing = talkClip != null && talkTime < talkClip.length;
        if (talkClip != null)
        {
            if (gesturing) talkTime += Time.deltaTime;
            talk.SetTime(Mathf.Min(talkTime, talkClip.length));
        }
        talkWeight = Mathf.MoveTowards(talkWeight, gesturing ? 1f : 0f, blendSpeed * Time.deltaTime);
        mixer.SetInputWeight(0, 1f - talkWeight);
        mixer.SetInputWeight(1, talkWeight);
    }
}
