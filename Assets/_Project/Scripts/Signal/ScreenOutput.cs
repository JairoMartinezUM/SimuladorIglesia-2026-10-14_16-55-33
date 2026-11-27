using TMPro;
using UnityEngine;

[RequireComponent(typeof(SignalNode))]
public class ScreenOutput : MonoBehaviour
{
    public Renderer screenRenderer;
    public Color onColor = new Color(0.3f, 0.7f, 1f);
    public Color offColor = Color.black;
    public Color noSignalColor = new Color(0.05f, 0.1f, 0.45f);

    [Header("Texto en la pantalla")]
    [Tooltip("Si se deja vacio, se crea solo, pegado al frente de la pantalla.")]
    public TMP_Text texto;
    public string textoConectado = "Conectado";
    public string textoSinSenal = "No hay señal";
    public Color colorTextoConectado = Color.white;
    public Color colorTextoSinSenal = Color.white;

    SignalNode node;
    int lastState = -1;

    void Awake()
    {
        node = GetComponent<SignalNode>();
        if (texto == null) texto = CrearTexto();
    }

    TMP_Text CrearTexto()
    {
        // La pantalla mira hacia -Z; el texto se coloca 1 cm delante de su cara frontal.
        var b = screenRenderer.bounds;
        var go = new GameObject("TextoPantalla");
        go.transform.position = new Vector3(b.center.x, b.center.y, b.min.z - 0.01f);
        go.transform.rotation = Quaternion.identity;

        var t = go.AddComponent<TextMeshPro>();
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.enableAutoSizing = true;
        t.fontSizeMin = 0.5f;
        t.fontSizeMax = 8f;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.rectTransform.sizeDelta = new Vector2(b.size.x * 0.9f, b.size.y * 0.5f);
        return t;
    }

    void Update()
    {
        // 2 = con señal, 1 = entrada equivocada (pantalla azul), 0 = apagada / sin cable
        int state = node.HasSignal() ? 2
                  : node.fault == NodeFault.WrongInput ? 1
                  : 0;
        if (state == lastState) return;
        lastState = state;

        screenRenderer.material.color = state == 2 ? onColor
                                      : state == 1 ? noSignalColor
                                      : offColor;

        if (texto != null)
        {
            texto.text = state == 2 ? textoConectado : textoSinSenal;
            texto.color = state == 2 ? colorTextoConectado : colorTextoSinSenal;
        }
    }
}
