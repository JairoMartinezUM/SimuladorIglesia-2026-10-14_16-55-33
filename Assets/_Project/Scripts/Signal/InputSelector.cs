using UnityEngine;

public class InputSelector : MonoBehaviour, IInteractable
{
    public SignalNode target;                                   // la pantalla o proyector
    public string[] inputs = { "HDMI 1", "HDMI 2", "VGA" };
    public int correctIndex = 0;

    int currentIndex;

    bool startedWrong;

    public string Prompt => $"E: cambiar entrada (actual: {inputs[currentIndex]})";

    void Start()
    {
        startedWrong = target.fault == NodeFault.WrongInput;
        // Corre después del LevelManager, que ya aplicó las fallas del nivel
        if (target.fault == NodeFault.WrongInput)
            currentIndex = (correctIndex + 1) % inputs.Length;   // el nivel dejó la entrada mal
        else
            currentIndex = correctIndex;
    }

    public void Interact()
    {
        bool wasCorrect = currentIndex == correctIndex;
        currentIndex = (currentIndex + 1) % inputs.Length;

        // No pisar otras fallas (por ejemplo, un equipo apagado)
        if (target.fault == NodeFault.None || target.fault == NodeFault.WrongInput)
            target.SetFault(currentIndex == correctIndex ? NodeFault.None : NodeFault.WrongInput);

        // Error: sacar la entrada de la posición correcta cuando el nivel no la había dañado
        if (!startedWrong && wasCorrect && currentIndex != correctIndex && LevelManager.Instance != null)
            LevelManager.Instance.RegisterMistake();
    }
}