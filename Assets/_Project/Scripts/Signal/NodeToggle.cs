using UnityEngine;

[DefaultExecutionOrder(100)]   // después del LevelManager, que aplica las fallas del nivel
public class NodeToggle : MonoBehaviour, IInteractable
{
    public SignalNode target;
    public NodeFault faultWhenOn = NodeFault.Muted;
    public string promptWhenOn = "E: quitar mute del canal";
    public string promptWhenOff = "E: poner mute al canal";
    public Renderer indicator;
    public Color onColor = Color.red;
    public Color offColor = Color.green;

    bool startedFaulty;

    public string Prompt => target.fault == faultWhenOn ? promptWhenOn : promptWhenOff;

    void Start()
    {
        startedFaulty = target.fault == faultWhenOn;
        Refresh();
    }

    public void Interact()
    {
        bool wasOn = target.fault == faultWhenOn;

        // No pisar otra falla (por ejemplo, ganancia baja)
        if (target.fault != NodeFault.None && !wasOn)
        {
            if (HudMessage.Instance != null) HudMessage.Instance.Show("El canal no está en mute.", 3f);
            return;
        }

        target.SetFault(wasOn ? NodeFault.None : faultWhenOn);
        Refresh();

        // Error: poner en mute un canal que el nivel no había dañado
        if (!wasOn && !startedFaulty && LevelManager.Instance != null)
            LevelManager.Instance.RegisterMistake();
    }

    void Refresh()
    {
        if (indicator != null)
            indicator.material.color = target.fault == faultWhenOn ? onColor : offColor;
    }
}