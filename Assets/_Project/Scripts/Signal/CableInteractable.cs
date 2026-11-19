using UnityEngine;

[RequireComponent(typeof(Cable))]
public class CableInteractable : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    public string cableName = "HDMI";
    public Renderer visual;
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;
    public Color damagedColor = new Color(1f, 0.6f, 0f);   // naranja
    public Color repairingColor = Color.yellow;

    [Header("Mayordomía")]
    public float repairSeconds = 8f;   // reparar es lento pero gratis; reemplazar es inmediato pero cuesta puntos

    Cable cable;
    bool repairing;
    float repairLeft;

    void Awake() { cable = GetComponent<Cable>(); }
    void Start() { Refresh(); }

    void Update()
    {
        if (!repairing) return;

        repairLeft -= Time.deltaTime;
        if (repairLeft <= 0f)
        {
            repairing = false;
            cable.SetDamaged(false);
            Refresh();
            if (HudMessage.Instance != null) HudMessage.Instance.Show("Cable reparado.", 3f);
        }
    }

    public string Prompt
    {
        get
        {
            if (repairing) return $"Reparando {cableName}… {Mathf.CeilToInt(repairLeft)} s";
            if (cable.isDamaged) return $"E: reparar {cableName} ({Mathf.RoundToInt(repairSeconds)} s, sin costo)";
            return cable.isConnected ? $"E: desconectar {cableName}" : $"E: conectar {cableName}";
        }
    }

    public string SecondaryPrompt =>
        cable.isDamaged && !repairing ? $"F: reemplazar {cableName} (inmediato, resta puntos)" : "";

    public void Interact()
    {
        if (repairing) return;

        if (cable.isDamaged)
        {
            repairing = true;
            repairLeft = repairSeconds;
            Refresh();
            return;
        }

        bool wasConnected = cable.isConnected;
        cable.SetConnected(!cable.isConnected);
        Refresh();

        if (wasConnected && LevelManager.Instance != null)
            LevelManager.Instance.RegisterMistake();
    }

    public void InteractSecondary()
    {
        if (!cable.isDamaged || repairing) return;

        cable.SetDamaged(false);
        Refresh();

        if (LevelManager.Instance != null)
            LevelManager.Instance.RegisterReplacement();
    }

    void Refresh()
    {
        if (visual == null) return;
        visual.material.color = repairing ? repairingColor
            : cable.isDamaged ? damagedColor
            : cable.isConnected ? connectedColor : disconnectedColor;
    }
}