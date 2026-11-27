using UnityEngine;

public class GeneradorBancas : MonoBehaviour
{
    public Material material;
    public int filas = 6;
    public float zInicial = 5f;
    public float separacion = 1.6f;
    public float xCentro = 2.25f;
    public float largo = 2.9f;

    [ContextMenu("Generar")]
    void Generar()
    {
        Limpiar();
        for (int lado = -1; lado <= 1; lado += 2)
            for (int i = 0; i < filas; i++)
                CrearBanca(
                    new Vector3(lado * xCentro, 0f, zInicial - i * separacion),
                    $"Banca_{(lado < 0 ? "Izq" : "Der")}_{i + 1:00}");
    }

    void CrearBanca(Vector3 pos, string nombre)
    {
        var raiz = new GameObject(nombre);
        raiz.transform.SetParent(transform, false);
        raiz.transform.localPosition = pos;

        Parte(raiz, "Asiento", new Vector3(0f, 0.45f, 0f), new Vector3(largo, 0.06f, 0.45f));
        Parte(raiz, "Respaldo", new Vector3(0f, 0.72f, -0.2f), new Vector3(largo, 0.55f, 0.06f));
        Parte(raiz, "Pata_Izq", new Vector3(-largo / 2f + 0.2f, 0.21f, 0f), new Vector3(0.08f, 0.42f, 0.4f));
        Parte(raiz, "Pata_Der", new Vector3(largo / 2f - 0.2f, 0.21f, 0f), new Vector3(0.08f, 0.42f, 0.4f));

        var col = raiz.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.45f, 0f);
        col.size = new Vector3(largo, 0.9f, 0.5f);
    }

    void Parte(GameObject padre, string nombre, Vector3 pos, Vector3 escala)
    {
        var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.name = nombre;
        p.transform.SetParent(padre.transform, false);
        p.transform.localPosition = pos;
        p.transform.localScale = escala;
        DestroyImmediate(p.GetComponent<Collider>());
        if (material != null) p.GetComponent<Renderer>().sharedMaterial = material;
    }

    void Limpiar()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
}