using UnityEngine;

public class InputSelector : MonoBehaviour, IInteractable
{
    public SignalNode target;                                   // la pantalla o proyector
    public string[] inputs = { "HDMI 1", "HDMI 2", "VGA" };
    public int correctIndex = 0;

    int currentIndex;

    public string Prompt => $"E: cambiar entrada (actual: {inputs[currentIndex]})";

    void Start()
    {
        // Corre después del LevelManager, que ya aplicó las fallas del nivel
        if (target.fault == NodeFault.WrongInput)
            currentIndex = (correctIndex + 1) % inputs.Length;   // el nivel dejó la entrada mal
        else
            currentIndex = correctIndex;
    }

    public void Interact()
    {
        currentIndex = (currentIndex + 1) % inputs.Length;

        // No pisar otras fallas (por ejemplo, un equipo apagado)
        if (target.fault == NodeFault.None || target.fault == NodeFault.WrongInput)
            target.SetFault(currentIndex == correctIndex ? NodeFault.None : NodeFault.WrongInput);
    }
}