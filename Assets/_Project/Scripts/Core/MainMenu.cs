using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameScene = "Iglesia";

    [Header("Pie de la pantalla de inicio")]
    public string version = "Versión alfa 0.1";
    public string autor = "Autor: Jairo Martínez";
    public float tamanoLetra = 30f;
    public Color colorLetra = new Color(1f, 1f, 1f, 0.75f);

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        FondoMenu.Crear();
        CrearPie();
    }

    // Version abajo a la izquierda y autor abajo a la derecha, pegados al Canvas del menu.
    void CrearPie()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        Etiqueta(canvas.transform, "Version", version, new Vector2(0f, 0f), TextAlignmentOptions.BottomLeft, new Vector2(40f, 28f));
        Etiqueta(canvas.transform, "Autor", autor, new Vector2(1f, 0f), TextAlignmentOptions.BottomRight, new Vector2(-40f, 28f));
    }

    void Etiqueta(Transform padre, string nombre, string contenido, Vector2 ancla, TextAlignmentOptions alineacion, Vector2 margen)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);

        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = r.pivot = ancla;
        r.anchoredPosition = margen;
        r.sizeDelta = new Vector2(900f, 60f);

        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = contenido;
        t.fontSize = tamanoLetra;
        t.color = colorLetra;
        t.alignment = alineacion;
        t.raycastTarget = false;     // no estorba a los botones
    }

    public void Play()
    {
        LevelManager.ResetProgress();
        SceneManager.LoadScene(gameScene);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
