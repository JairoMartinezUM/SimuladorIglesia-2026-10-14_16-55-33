using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Fondo de la pantalla de inicio: la iglesia vista desde afuera, con pasto y arboles, y la
// camara girando despacio a su alrededor. Se crea sola por codigo: FondoMenu.Crear().
public class FondoMenu : MonoBehaviour
{
    public float radio = 36f;
    public float altura = 7f;
    public float velocidadGrados = 3.5f;
    public Vector3 centro = new Vector3(0f, 3.8f, -2f);

    Camera cam;
    float angulo = 150f;
    readonly Dictionary<Color, Material> materiales = new Dictionary<Color, Material>();
    System.Random rng = new System.Random(5);

    public static void Crear()
    {
        if (FindFirstObjectByType<FondoMenu>() != null) return;
        new GameObject("FondoMenu").AddComponent<FondoMenu>().Construir();
    }

    void Construir()
    {
        cam = Camera.main;
        if (cam != null)
        {
            if (RenderSettings.skybox != null) cam.clearFlags = CameraClearFlags.Skybox;
            else { cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.75f, 0.95f); }
            cam.farClipPlane = 400f;
        }

        ConstruirIglesia();
        ConstruirEntorno();
        OscurecerParaLeer();
        Orbitar(0f);
    }

    void Update() { Orbitar(Time.unscaledDeltaTime); }

    void Orbitar(float dt)
    {
        if (cam == null) return;
        angulo += velocidadGrados * dt;
        float a = angulo * Mathf.Deg2Rad;
        Vector3 pos = new Vector3(Mathf.Sin(a) * radio, altura + Mathf.Sin(a * 0.7f) * 1.2f, Mathf.Cos(a) * radio) + new Vector3(0f, 0f, centro.z);
        cam.transform.position = pos;
        cam.transform.LookAt(centro);
    }

    // ---------- iglesia ----------

    void ConstruirIglesia()
    {
        var raiz = new GameObject("Iglesia").transform; raiz.SetParent(transform, false);
        Color crema = new Color(0.90f, 0.88f, 0.82f), teja = new Color(0.40f, 0.14f, 0.11f);
        Color madera = new Color(0.30f, 0.17f, 0.10f), vidrio = new Color(0.20f, 0.32f, 0.48f);

        // base y cuerpo
        Cubo(raiz, "Base", new Vector3(0, 0.15f, 0), new Vector3(11f, 0.3f, 20.4f), new Color(0.6f, 0.6f, 0.58f));
        Cubo(raiz, "Cuerpo", new Vector3(0, 2.8f, 0), new Vector3(9.4f, 5f, 18.4f), crema);

        // techo a dos aguas
        float baseY = 5.3f;
        float semiancho = 5.1f, ang = 28f;
        float largo = semiancho / Mathf.Cos(ang * Mathf.Deg2Rad);
        float subida = semiancho * Mathf.Tan(ang * Mathf.Deg2Rad);
        Cubo(raiz, "Techo_Izq", new Vector3(-semiancho / 2f, baseY + subida / 2f, 0), new Vector3(largo, 0.28f, 19.6f), teja, new Vector3(0, 0, ang));
        Cubo(raiz, "Techo_Der", new Vector3(semiancho / 2f, baseY + subida / 2f, 0), new Vector3(largo, 0.28f, 19.6f), teja, new Vector3(0, 0, -ang));

        // frontones escalonados (triangulo hecho con bloques)
        float[] anchos = { 8.0f, 5.6f, 3.4f, 1.4f };
        for (int i = 0; i < anchos.Length; i++)
            foreach (float z in new[] { -9.1f, 9.1f })
                Cubo(raiz, "Fronton", new Vector3(0, baseY + 0.3f + i * 0.6f, z), new Vector3(anchos[i], 0.6f, 0.2f), crema);

        // campanario en la entrada (lado -Z)
        Cubo(raiz, "Torre", new Vector3(0, 4.8f, -10.7f), new Vector3(3f, 9f, 3f), crema);
        float[] torre = { 3.5f, 2.7f, 1.9f, 1.1f, 0.5f };
        for (int i = 0; i < torre.Length; i++)
            Cubo(raiz, "Torre_Techo", new Vector3(0, 9.55f + i * 0.5f, -10.7f), new Vector3(torre[i], 0.5f, torre[i]), teja);
        Cubo(raiz, "Torre_Ventana", new Vector3(0, 7.2f, -12.21f), new Vector3(0.9f, 1.4f, 0.1f), vidrio);

        // puerta doble y marco
        Cubo(raiz, "Puerta_Marco", new Vector3(0, 1.75f, -12.2f), new Vector3(2.3f, 2.9f, 0.12f), crema);
        Cubo(raiz, "Puerta", new Vector3(0, 1.65f, -12.27f), new Vector3(1.9f, 2.7f, 0.1f), madera);
        Cubo(raiz, "Puerta_Union", new Vector3(0, 1.65f, -12.33f), new Vector3(0.06f, 2.7f, 0.06f), Color.black);

        // ventanas altas en los dos costados
        float[] zs = { 6f, 2.5f, -1f, -4f };
        foreach (float lado in new[] { -1f, 1f })
            foreach (float z in zs)
            {
                Cubo(raiz, "Ventana_Marco", new Vector3(lado * 4.72f, 2.9f, z), new Vector3(0.14f, 2.5f, 1.5f), new Color(0.97f, 0.97f, 0.95f));
                Cubo(raiz, "Ventana", new Vector3(lado * 4.78f, 2.9f, z), new Vector3(0.12f, 2.2f, 1.2f), vidrio);
            }

        // camino de acceso
        Cubo(raiz, "Camino", new Vector3(0, 0.06f, -19f), new Vector3(2.8f, 0.12f, 13f), new Color(0.66f, 0.66f, 0.63f));
    }

    // ---------- suelo, arboles, rocas ----------

    void ConstruirEntorno()
    {
        var raiz = new GameObject("Entorno").transform; raiz.SetParent(transform, false);
        Cubo(raiz, "Pasto", new Vector3(0, -0.1f, 0), new Vector3(500f, 0.2f, 500f), new Color(0.24f, 0.50f, 0.18f));

        for (int i = 0; i < 140; i++)
        {
            Vector3 p = PuntoLibre(16f, 85f);
            Arbol(raiz, p);
        }
        for (int i = 0; i < 24; i++)
        {
            Vector3 p = PuntoLibre(8f, 40f);
            float a = R(0.4f, 1.2f), h = R(0.3f, 0.7f);
            Cubo(raiz, "Roca", new Vector3(p.x, h / 2f - 0.05f, p.z), new Vector3(a, h, R(0.4f, 1.1f)), new Color(0.43f, 0.43f, 0.46f), new Vector3(R(-6, 6), R(0, 90), R(-6, 6)));
        }
    }

    Vector3 PuntoLibre(float minimo, float maximo)
    {
        for (int i = 0; i < 100; i++)
        {
            float radio = Mathf.Sqrt(R(minimo * minimo, maximo * maximo));
            float a = R(0f, Mathf.PI * 2f);
            float x = Mathf.Cos(a) * radio, z = Mathf.Sin(a) * radio;
            bool edificio = Mathf.Abs(x) < 9f && z > -17f && z < 12f;
            bool camino = Mathf.Abs(x) < 4.5f && z < -12f && z > -32f;
            if (!edificio && !camino) return new Vector3(x, 0f, z);
        }
        return new Vector3(maximo, 0f, maximo);
    }

    void Arbol(Transform padre, Vector3 pos)
    {
        float s = R(0.9f, 1.8f), h = R(1.6f, 2.6f) * s;
        Color hoja = rng.NextDouble() < 0.35 ? new Color(0.26f, 0.55f, 0.20f) : new Color(0.18f, 0.45f, 0.16f);
        var r = new GameObject("Arbol").transform; r.SetParent(padre, false);
        r.localPosition = pos;
        r.localRotation = Quaternion.Euler(0, R(0, 90), 0);
        Cubo(r, "Tronco", new Vector3(0, h / 2f, 0), new Vector3(0.35f * s, h, 0.35f * s), new Color(0.30f, 0.20f, 0.12f));
        Cubo(r, "Copa_1", new Vector3(0, h + 0.4f * s, 0), new Vector3(2.2f * s, 1.2f * s, 2.2f * s), hoja);
        Cubo(r, "Copa_2", new Vector3(0, h + 1.3f * s, 0), new Vector3(1.6f * s, 1.0f * s, 1.6f * s), hoja);
        Cubo(r, "Copa_3", new Vector3(0, h + 2.1f * s, 0), new Vector3(0.9f * s, 0.8f * s, 0.9f * s), hoja);
    }

    // ---------- ayudas ----------

    GameObject Cubo(Transform padre, string nombre, Vector3 pos, Vector3 escala, Color color, Vector3? euler = null)
    {
        var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.name = nombre;
        p.transform.SetParent(padre, false);
        p.transform.localPosition = pos;
        p.transform.localScale = escala;
        if (euler.HasValue) p.transform.localRotation = Quaternion.Euler(euler.Value);
        Destroy(p.GetComponent<Collider>());
        p.GetComponent<Renderer>().sharedMaterial = Mat(color);
        return p;
    }

    Material Mat(Color c)
    {
        if (materiales.TryGetValue(c, out var m)) return m;
        m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", c);
        m.SetFloat("_Metallic", 0f);
        m.SetFloat("_Smoothness", 0.08f);
        materiales[c] = m;
        return m;
    }

    float R(float a, float b) { return a + (float)rng.NextDouble() * (b - a); }

    // velo oscuro suave detras del menu, para que los botones y el titulo se lean bien
    void OscurecerParaLeer()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        var go = new GameObject("Velo", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.35f);
        img.raycastTarget = false;
        go.transform.SetAsFirstSibling();
    }
}
