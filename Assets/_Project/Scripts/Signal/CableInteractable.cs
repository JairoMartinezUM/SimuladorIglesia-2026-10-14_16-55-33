using UnityEngine;

[RequireComponent(typeof(Cable))]
public class CableInteractable : MonoBehaviour, IInteractable
{
    public string cableName = "HDMI";
    public Renderer visual;
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;

    Cable cable;

    void Awake() { cable = GetComponent<Cable>(); }
    void Start() { Refresh(); }

    public string Prompt => cable.isConnected
        ? $"E: desconectar {cableName} "
        : $"E: conectar {cableName}";

    public void Interact()
    {
        bool wasConnected = cable.isConnected;
        cable.SetConnected(!cable.isConnected);
        Refresh();

        if (wasConnected && LevelManager.Instance != null)
            LevelManager.Instance.RegisterMistake();
    }

    void Refresh()
    {
        if (visual != null)
            visual.material.color = cable.isConnected ? connectedColor : disconnectedColor;
    }
}
