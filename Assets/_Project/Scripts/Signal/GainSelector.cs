using UnityEngine;

[DefaultExecutionOrder(100)]
public class GainSelector : MonoBehaviour, IInteractable
{
    public SignalNode target;
    public string[] levels = { "Bajo", "Medio", "Alto" };
    public int correctIndex = 2;
    public int wrongStartIndex = 0;   // nivel con el que arranca cuando el nivel deja la ganancia mal
    public Renderer indicator;
    public Color okColor = Color.green;
    public Color wrongColor = Color.yellow;

    int currentIndex;
    bool startedWrong;

    public string Prompt => $"E: subir ganancia (actual: {levels[currentIndex]})";

    void Start()
    {
        startedWrong = target.fault == NodeFault.LowGain;
        currentIndex = startedWrong ? wrongStartIndex : correctIndex;
        Refresh();
    }

    public void Interact()
    {
        // Con el canal en mute no hay señal que ajustar
        if (target.fault != NodeFault.None && target.fault != NodeFault.LowGain)
        {
            if (HudMessage.Instance != null) HudMessage.Instance.Show("El canal está en mute.", 3f);
            return;
        }

        bool wasCorrect = currentIndex == correctIndex;
        currentIndex = (currentIndex + 1) % levels.Length;
        target.SetFault(currentIndex == correctIndex ? NodeFault.None : NodeFault.LowGain);
        Refresh();

        // Error: sacar de su punto correcto una ganancia que estaba bien
        if (!startedWrong && wasCorrect && currentIndex != correctIndex && LevelManager.Instance != null)
            LevelManager.Instance.RegisterMistake();
    }

    void Refresh()
    {
        if (indicator != null)
            indicator.material.color = currentIndex == correctIndex ? okColor : wrongColor;
    }
}