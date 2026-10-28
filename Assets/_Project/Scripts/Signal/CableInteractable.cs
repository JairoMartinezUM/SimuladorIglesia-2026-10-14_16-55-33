using UnityEngine;

[RequireComponent(typeof(Cable))]
public class CableInteractable : MonoBehaviour, IInteractable
{
    public string cableName = "HDMI";
    public Renderer visual;
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;
    public Color damagedColor = new Color(1f, 0.6f, 0f);   // naranja

    Cable cable;

    void Awake() { cable = GetComponent<Cable>(); }
    void Start() { Refresh(); }

    public string Prompt =>
        cable.isDamaged ? $"E: reemplazar {cableName} dañado"
        : cable.isConnected ? $"E: desconectar {cableName}"
        : $"E: conectar {cableName}";

    public void Interact()
    {
        if (cable.isDamaged)
        {
            cable.SetDamaged(false);   // se cambia por un cable de repuesto
            Refresh();
            return;
        }

        bool wasConnected = cable.isConnected;
        cable.SetConnected(!cable.isConnected);
        Refresh();

        if (wasConnected && LevelManager.Instance != null)
            LevelManager.Instance.RegisterMistake();
    }

    void Refresh()
    {
        if (visual == null) return;
        visual.material.color = cable.isDamaged ? damagedColor
            : cable.isConnected ? connectedColor : disconnectedColor;
    }
}