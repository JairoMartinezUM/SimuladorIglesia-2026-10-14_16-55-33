using UnityEngine;

[RequireComponent(typeof(SignalNode))]
public class ScreenOutput : MonoBehaviour
{
    public Renderer screenRenderer;
    public Color onColor = new Color(0.3f, 0.7f, 1f);
    public Color offColor = Color.black;

    SignalNode node;
    bool? lastState;

    void Awake() { node = GetComponent<SignalNode>(); }

    void Update()
    {
        bool state = node.HasSignal();
        if (lastState == state) return;
        lastState = state;
        screenRenderer.material.color = state ? onColor : offColor;
    }
}
