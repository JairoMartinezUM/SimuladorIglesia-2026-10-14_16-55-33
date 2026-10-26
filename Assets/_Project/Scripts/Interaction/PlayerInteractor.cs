using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float reach = 3f;
    public float aimRadius = 0.08f;       // tolerancia al apuntar (en metros)
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
        IInteractable target = null;

        var ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.SphereCast(ray, aimRadius, out var hit, reach, ~0, QueryTriggerInteraction.Ignore))
            target = hit.collider.GetComponentInParent<IInteractable>();

        if (promptText != null)
            promptText.text = target != null ? target.Prompt : "";

        SetCrosshair(target != null);

        if (target != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            target.Interact();
    }

    void SetCrosshair(bool onTarget)
    {
        if (crosshair == null) return;
        crosshair.color = onTarget ? targetColor : idleColor;
        float size = onTarget ? targetSize : idleSize;
        crosshair.rectTransform.sizeDelta = new Vector2(size, size);
    }
}