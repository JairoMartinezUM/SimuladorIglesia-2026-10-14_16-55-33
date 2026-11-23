using UnityEngine;

[RequireComponent(typeof(SignalNode))]
public class SpeakerOutput : MonoBehaviour
{
    public Renderer led;
    public SignalNode mixer;   // para distinguir 'señal débil' de 'sin señal'
    public Color okColor = Color.green;
    public Color weakColor = Color.yellow;
    public Color offColor = Color.red;
    public AudioSource tone;   // opcional: sonido en bucle cuando hay señal

    SignalNode node;
    int lastState = -1;

    void Awake() { node = GetComponent<SignalNode>(); }

    void Update()
    {
        int state = node.HasSignal() ? 2
                  : (mixer != null && mixer.fault == NodeFault.LowGain) ? 1
                  : 0;
        if (state == lastState) return;
        lastState = state;

        if (led != null)
            led.material.color = state == 2 ? okColor : state == 1 ? weakColor : offColor;

        if (tone != null)
        {
            if (state == 2 && !tone.isPlaying) tone.Play();
            else if (state != 2 && tone.isPlaying) tone.Stop();
        }
    }
}