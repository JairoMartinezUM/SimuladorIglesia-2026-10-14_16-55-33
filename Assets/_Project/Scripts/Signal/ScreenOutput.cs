using UnityEngine;

[RequireComponent(typeof(SignalNode))]
public class ScreenOutput : MonoBehaviour
{
    public Renderer screenRenderer;
    public Color onColor = new Color(0.3f, 0.7f, 1f);
    public Color offColor = Color.black;
    public Color noSignalColor = new Color(0.05f, 0.1f, 0.45f);

    SignalNode node;
    Color? lastColor;

    void Awake() { node = GetComponent<SignalNode>(); }

    void Update()
    {
        Color c = node.HasSignal() ? onColor
                : node.fault == NodeFault.WrongInput ? noSignalColor
                : offColor;

        if (lastColor == c) return;
        lastColor = c;
        screenRenderer.material.color = c;
    }
}