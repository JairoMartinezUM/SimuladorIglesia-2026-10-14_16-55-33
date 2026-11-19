using TMPro;
using UnityEngine;

public class HudMessage : MonoBehaviour
{
    public static HudMessage Instance { get; private set; }
    public TMP_Text messageText;

    float hideAt;

    void Awake()
    {
        Instance = this;
        if (messageText != null) messageText.text = "";
    }

    public void Show(string text, float seconds = 6f)
    {
        if (messageText == null) return;
        messageText.text = text;
        hideAt = Time.unscaledTime + seconds;
    }

    void Update()
    {
        if (messageText != null && messageText.text != "" && Time.unscaledTime >= hideAt)
            messageText.text = "";
    }
}