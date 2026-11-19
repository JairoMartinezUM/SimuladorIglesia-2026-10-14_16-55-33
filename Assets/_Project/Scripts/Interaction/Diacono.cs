using UnityEngine;

public class Diacono : MonoBehaviour, IInteractable
{
    public string Prompt => "E: pedir ayuda al diácono";

    public void Interact()
    {
        if (LevelManager.Instance == null) return;
        string pista = LevelManager.Instance.AskForHelp();
        if (pista != null && HudMessage.Instance != null)
            HudMessage.Instance.Show($"<b>Diácono:</b> {pista}", 8f);
    }
}