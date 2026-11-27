using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Cuerpo invisible del jugador: solo proyecta sombra (para que no sea una capsula).
// Uso: Add Component en el objeto "Player" y en el menu del componente (tres puntos)
// elige "Configurar (buscar modelo y animaciones)". Despues Play.
public class CuerpoJugador : MonoBehaviour
{
    public GameObject modelo;
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public float escala = 0.65f;
    public Vector3 posicion = Vector3.zero;
    [Tooltip("Velocidad a la que la animacion de caminar va al 100%")]
    public float velocidadCaminar = 4f;
    public float suavizado = 8f;
    [Tooltip("Nombre del FBX a buscar en el proyecto (sin .fbx)")]
    public string nombreFbx = "character-q";

    PlayableGraph graph;
    AnimationMixerPlayable mixer;
    AnimationClipPlayable idleP, walkP;
    CharacterController cc;
    float mezcla, t;
    Vector3 ultimaPos;

    void Start()
    {
        // la capsula deja de proyectar sombra
        foreach (var r in GetComponentsInChildren<MeshRenderer>())
            if (r.name == "Capsule") r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        if (modelo == null) { Debug.LogWarning("CuerpoJugador: falta el modelo (usa 'Configurar')."); return; }

        var go = Instantiate(modelo, transform);
        go.name = "CuerpoSombra";
        go.transform.localPosition = posicion;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * escala;

        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            r.receiveShadows = false;
        }
        foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c);

        var anim = go.GetComponent<Animator>();
        if (anim == null) anim = go.AddComponent<Animator>();
        anim.applyRootMotion = false;
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        cc = GetComponent<CharacterController>();
        ultimaPos = transform.position;

        if (idleClip == null && walkClip == null) return;
        graph = PlayableGraph.Create("CuerpoJugador");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, 2);
        if (idleClip != null) { idleP = AnimationClipPlayable.Create(graph, idleClip); idleP.SetSpeed(0); graph.Connect(idleP, 0, mixer, 0); }
        if (walkClip != null) { walkP = AnimationClipPlayable.Create(graph, walkClip); walkP.SetSpeed(0); graph.Connect(walkP, 0, mixer, 1); }
        mixer.SetInputWeight(0, 1f);
        var output = AnimationPlayableOutput.Create(graph, "anim", anim);
        output.SetSourcePlayable(mixer);
        graph.Play();
    }

    void Update()
    {
        if (!graph.IsValid()) return;

        Vector3 d = transform.position - ultimaPos;
        d.y = 0f;
        float vel = Time.deltaTime > 0f ? d.magnitude / Time.deltaTime : 0f;
        ultimaPos = transform.position;

        float objetivo = walkClip != null ? Mathf.Clamp01(vel / velocidadCaminar) : 0f;
        mezcla = Mathf.Lerp(mezcla, objetivo, 1f - Mathf.Exp(-suavizado * Time.deltaTime));
        mixer.SetInputWeight(0, 1f - mezcla);
        mixer.SetInputWeight(1, mezcla);

        t += Time.deltaTime;
        if (idleClip != null) idleP.SetTime(Mathf.Repeat(t, Mathf.Max(0.01f, idleClip.length)));
        if (walkClip != null) walkP.SetTime(Mathf.Repeat(t * Mathf.Lerp(0.6f, 1.2f, mezcla), Mathf.Max(0.01f, walkClip.length)));
    }

    void OnDestroy()
    {
        if (graph.IsValid()) graph.Destroy();
    }

#if UNITY_EDITOR
    [ContextMenu("Configurar (buscar modelo y animaciones)")]
    void Configurar()
    {
        var guids = AssetDatabase.FindAssets(nombreFbx + " t:Model");
        string ruta = null;
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (System.IO.Path.GetFileNameWithoutExtension(p) == nombreFbx) { ruta = p; break; }
        }
        if (ruta == null) { Debug.LogError("CuerpoJugador: no encontre " + nombreFbx + ".fbx en el proyecto"); return; }

        Undo.RecordObject(this, "Configurar CuerpoJugador");
        modelo = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
        idleClip = walkClip = null;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(ruta))
        {
            var c = o as AnimationClip;
            if (c == null || c.name.StartsWith("__preview__")) continue;
            if (c.name == "idle") idleClip = c;
            else if (c.name == "walk") walkClip = c;
        }
        EditorUtility.SetDirty(this);
        Debug.Log($"CuerpoJugador: modelo={ruta} idle={(idleClip != null)} walk={(walkClip != null)}");
    }
#endif
}
