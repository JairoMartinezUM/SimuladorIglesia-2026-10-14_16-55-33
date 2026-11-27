using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla final del juego. Se construye sola por codigo (no hay que armar nada en la escena).
// La llama LevelManager despues del ultimo nivel: PantallaFinal.Mostrar(puntos).
public class PantallaFinal : MonoBehaviour
{
    public const string EscenaMenu = "Menu";

    float creado;

    public static void Mostrar(int puntos)
    {
        if (FindFirstObjectByType<PantallaFinal>() != null) return;
        var go = new GameObject("PantallaFinal");
        go.AddComponent<PantallaFinal>().Construir(puntos);
    }

    void Construir(int puntos)
    {
        creado = Time.unscaledTime;

        // cursor libre para poder usar los botones
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var inputs = FindFirstObjectByType<StarterAssetsInputs>();
        if (inputs != null) { inputs.cursorInputForLook = false; inputs.look = Vector2.zero; }

        // canvas encima de todo
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // fondo
        var fondo = Nuevo("Fondo", transform);
        Estirar(fondo);
        var img = fondo.gameObject.AddComponent<Image>();
        img.color = new Color(0.03f, 0.06f, 0.13f, 0.97f);

        Texto("Titulo", "¡FELICIDADES!", 120, new Color(1f, 0.82f, 0.3f), new Vector2(0f, 320f), new Vector2(1700f, 170f), true);
        Texto("Subtitulo", "HAS COMPLETADO LA VERSIÓN ALFA\nDE ESTE SIMULADOR", 58, Color.white, new Vector2(0f, 150f), new Vector2(1700f, 190f), true);
        Texto("Puntos", $"Puntos totales: <b>{puntos}</b>", 46, new Color(0.7f, 0.88f, 1f), new Vector2(0f, 10f), new Vector2(1400f, 80f), false);
        Texto("Mensaje",
              "Gracias por servir detrás de escena.\n" +
              "<size=80%><color=#D0D0D0>“Ahora bien, hay diversidad de dones, pero el Espíritu es el mismo.”\n— 1 Corintios 12:4</color></size>\n" +
              "<size=70%><color=#A0A0A0>Esta es una versión alfa: habrá más niveles y más escenarios.</color></size>",
              40, Color.white, new Vector2(0f, -150f), new Vector2(1500f, 260f), false);

        Boton("Jugar de nuevo  (R)", new Vector2(-270f, -380f), JugarDeNuevo);
        Boton("Menú principal  (ENTER)", new Vector2(270f, -380f), AlMenu);
    }

    void Update()
    {
        if (Time.unscaledTime - creado < 0.6f) return;   // evita que el ENTER del nivel la cierre al instante
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.enterKey.wasPressedThisFrame) AlMenu();
        else if (kb.rKey.wasPressedThisFrame) JugarDeNuevo();
    }

    void JugarDeNuevo()
    {
        LevelManager.ResetProgress();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void AlMenu()
    {
        LevelManager.ResetProgress();
        Time.timeScale = 1f;
        SceneManager.LoadScene(EscenaMenu);
    }

    // ---------- ayudas para armar la interfaz ----------

    static RectTransform Nuevo(string nombre, Transform padre)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go.GetComponent<RectTransform>();
    }

    static void Estirar(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    static void Centrar(RectTransform r, Vector2 pos, Vector2 tam)
    {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = tam;
    }

    TMP_Text Texto(string nombre, string contenido, float tamano, Color color, Vector2 pos, Vector2 tam, bool negritas)
    {
        var r = Nuevo(nombre, transform);
        Centrar(r, pos, tam);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = contenido;
        t.fontSize = tamano;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = negritas ? FontStyles.Bold : FontStyles.Normal;
        t.raycastTarget = false;     // para que no tape los botones
        return t;
    }

    void Boton(string etiqueta, Vector2 pos, UnityAction accion)
    {
        var r = Nuevo("Boton", transform);
        Centrar(r, pos, new Vector2(480f, 100f));
        var img = r.gameObject.AddComponent<Image>();
        img.color = Color.white;
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var cb = b.colors;
        cb.normalColor = new Color(0.16f, 0.38f, 0.65f);
        cb.highlightedColor = new Color(0.26f, 0.52f, 0.82f);
        cb.pressedColor = new Color(0.10f, 0.25f, 0.45f);
        cb.selectedColor = cb.normalColor;
        cb.fadeDuration = 0.1f;
        b.colors = cb;
        b.onClick.AddListener(accion);

        var rt = Nuevo("Texto", r);
        Estirar(rt);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = etiqueta;
        t.fontSize = 38;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
    }
}
