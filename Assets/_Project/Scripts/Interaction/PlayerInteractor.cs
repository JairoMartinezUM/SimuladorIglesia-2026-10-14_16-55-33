using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float reach = 3f;
    public float aimRadius = 0.08f;
    public TMP_Text promptText;

    [Header("Mira")]
    public Image crosshair;
    public Color idleColor = new Color(1f, 1f, 1f, 0.6f);
    public Color targetColor = Color.green;
    public float idleSize = 10f;
    public float targetSize = 16f;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        SetCrosshair(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (promptText != null) promptText.text = "";
            return;
        }

        IInteractable target = null;
        ISecondaryInteractable second = null;

        var ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.SphereCast(ray, aimRadius, out var hit, reach, ~0, QueryTriggerInteraction.Ignore))
        {
            target = hit.collider.GetComponentInParent<IInteractable>();
            second = hit.collider.GetComponentInParent<ISecondaryInteractable>();
        }

        if (promptText != null)
        {
            string text = target != null ? target.Prompt : "";
            if (second != null && !string.IsNullOrEmpty(second.SecondaryPrompt))
                text += "\n" + second.SecondaryPrompt;
            promptText.text = text;
        }

        SetCrosshair(target != null);

        var kb = Keyboard.current;
        if (kb == null) return;
        if (target != null && kb.eKey.wasPressedThisFrame)
        {
            target.Interact();
            AudioManager.Click();
        }
        if (second != null && kb.fKey.wasPressedThisFrame)
        {
            second.InteractSecondary();
            AudioManager.Click();
        }
    }

    void SetCrosshair(bool onTarget)
    {
        if (crosshair == null) return;
        crosshair.color = onTarget ? targetColor : idleColor;
        float size = onTarget ? targetSize : idleSize;
        crosshair.rectTransform.sizeDelta = new Vector2(size, size);
    }
}