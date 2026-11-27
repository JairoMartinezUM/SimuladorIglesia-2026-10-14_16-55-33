using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// Reemplaza los cubos "Pulpito", "Macetero_Izq" y "Macetero_Der" por modelos armados con piezas.
// Uso: Add Component en el objeto "Entorno", asigna los materiales y en el menu del
// componente (los tres puntos) elige "Generar pulpito y maceteros".
// Conserva la posicion de cada mueble. Se puede regenerar las veces que haga falta.
public class GeneradorDecoracion : MonoBehaviour
{
    [Header("Materiales existentes")]
    public Material madera;     // M_Madera
    public Material piedra;     // M_Piedra
    public Material negro;      // M_Negro

    [Header("Materiales nuevos (si se dejan vacios, se crean solos en Art/)")]
    public Material hojas;
    public Material flor;
    public Material tierra;

    [ContextMenu("Generar pulpito y maceteros")]
    void Generar()
    {
        if (madera == null) { Debug.LogWarning("GeneradorDecoracion: asigna M_Madera en el campo Madera."); return; }
        if (piedra == null) piedra = madera;
        if (negro == null) negro = madera;
        Crear(ref hojas, "M_Hojas", new Color(0.18f, 0.45f, 0.16f));
        Crear(ref flor, "M_Flor", new Color(0.96f, 0.93f, 0.85f));
        Crear(ref tierra, "M_Tierra", new Color(0.22f, 0.14f, 0.09f));

        Reconstruir("Pulpito", ConstruirPulpito);
        Reconstruir("Macetero_Izq", r => ConstruirMacetero(r, 0));
        Reconstruir("Macetero_Der", r => ConstruirMacetero(r, 3));

#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    // ---------- infraestructura ----------

    void Reconstruir(string nombre, System.Action<GameObject> construir)
    {
        var viejo = transform.Find(nombre);
        if (viejo == null) { Debug.LogWarning("GeneradorDecoracion: no encontre '" + nombre + "' dentro de " + name); return; }

        Vector3 pos = viejo.localPosition;
        Quaternion rot = viejo.localRotation;
        // cubo original: el pivote esta a media altura, bajamos al piso
        if (viejo.GetComponent<MeshFilter>() != null) pos.y -= viejo.localScale.y * 0.5f;
        int indice = viejo.GetSiblingIndex();

        DestroyImmediate(viejo.gameObject);

        var raiz = new GameObject(nombre);
        raiz.transform.SetParent(transform, false);
        raiz.transform.localPosition = pos;
        raiz.transform.localRotation = rot;
        raiz.transform.SetSiblingIndex(indice);
        construir(raiz);
    }

    GameObject Parte(GameObject padre, string nombre, Vector3 pos, Vector3 escala, Material mat, Vector3? euler = null)
    {
        var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.name = nombre;
        p.transform.SetParent(padre.transform, false);
        p.transform.localPosition = pos;
        p.transform.localScale = escala;
        if (euler.HasValue) p.transform.localRotation = Quaternion.Euler(euler.Value);
        DestroyImmediate(p.GetComponent<Collider>());
        p.GetComponent<Renderer>().sharedMaterial = mat;
        return p;
    }

    void Crear(ref Material m, string archivo, Color color)
    {
        if (m != null) return;
#if UNITY_EDITOR
        string ruta = "Assets/_Project/Art/" + archivo + ".mat";
        var existente = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (existente != null) { m = existente; return; }
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Smoothness", 0.1f);
        AssetDatabase.CreateAsset(mat, ruta);
        m = mat;
#endif
    }

    // ---------- pulpito (el frente mira hacia -Z, la nave) ----------

    void ConstruirPulpito(GameObject r)
    {
        Parte(r, "Base", new Vector3(0f, 0.05f, 0f), new Vector3(0.78f, 0.10f, 0.58f), piedra);
        Parte(r, "Cuerpo", new Vector3(0f, 0.54f, 0f), new Vector3(0.66f, 0.88f, 0.46f), madera);
        Parte(r, "Panel_Frontal", new Vector3(0f, 0.52f, -0.235f), new Vector3(0.48f, 0.64f, 0.03f), negro);
        Parte(r, "Moldura_Sup", new Vector3(0f, 0.97f, 0f), new Vector3(0.74f, 0.05f, 0.54f), madera);
        // atril inclinado hacia quien lee (borde alto del lado de la nave)
        Parte(r, "Atril", new Vector3(0f, 1.03f, 0f), new Vector3(0.74f, 0.04f, 0.5f), madera, new Vector3(12f, 0f, 0f));
        Parte(r, "Tope_Papeles", new Vector3(0f, 1.0f, 0.235f), new Vector3(0.74f, 0.04f, 0.03f), madera);

        var col = r.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.55f, 0f);
        col.size = new Vector3(0.78f, 1.1f, 0.58f);
    }

    // ---------- macetero con planta y flores ----------

    void ConstruirMacetero(GameObject r, int desfase)
    {
        Parte(r, "Base", new Vector3(0f, 0.03f, 0f), new Vector3(0.38f, 0.06f, 0.38f), piedra);
        Parte(r, "Cuerpo", new Vector3(0f, 0.26f, 0f), new Vector3(0.30f, 0.40f, 0.30f), piedra);
        Parte(r, "Borde", new Vector3(0f, 0.50f, 0f), new Vector3(0.38f, 0.10f, 0.38f), piedra);
        Parte(r, "Tierra", new Vector3(0f, 0.545f, 0f), new Vector3(0.32f, 0.02f, 0.32f), tierra);

        // hojas largas que salen en abanico
        float[] altos = { 0.46f, 0.38f, 0.50f, 0.40f, 0.48f, 0.36f, 0.44f, 0.52f };
        for (int i = 0; i < 8; i++)
        {
            var pivote = new GameObject("Hoja_" + (i + 1));
            pivote.transform.SetParent(r.transform, false);
            pivote.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            pivote.transform.localRotation = Quaternion.Euler(0f, i * 45f + desfase * 7f, 0f);
            float h = altos[(i + desfase) % 8];
            Parte(pivote, "Lamina", new Vector3(0f, h * 0.5f, 0.05f), new Vector3(0.09f, h, 0.015f), hojas,
                  new Vector3(18f + (i % 3) * 8f, 0f, 0f));
        }

        // flores: tallo + flor con centro amarillo-blanco
        Vector3[] sitios =
        {
            new Vector3( 0.00f, 0.90f,  0.00f),
            new Vector3( 0.12f, 0.80f,  0.06f),
            new Vector3(-0.11f, 0.78f,  0.07f),
            new Vector3( 0.06f, 0.74f, -0.12f),
            new Vector3(-0.07f, 0.84f, -0.09f)
        };
        for (int i = 0; i < sitios.Length; i++)
        {
            Vector3 s = sitios[(i + desfase) % sitios.Length];
            float largo = s.y - 0.55f;
            Parte(r, "Tallo_" + (i + 1), new Vector3(s.x, 0.55f + largo * 0.5f, s.z), new Vector3(0.015f, largo, 0.015f), hojas);
            Parte(r, "Flor_" + (i + 1), s, new Vector3(0.10f, 0.04f, 0.10f), flor, new Vector3(0f, i * 25f, 0f));
            Parte(r, "FlorCentro_" + (i + 1), s + new Vector3(0f, 0.025f, 0f), new Vector3(0.04f, 0.03f, 0.04f), flor, new Vector3(0f, i * 25f + 45f, 0f));
        }

        var col = r.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.5f, 0f);
        col.size = new Vector3(0.4f, 1.0f, 0.4f);
    }
}
