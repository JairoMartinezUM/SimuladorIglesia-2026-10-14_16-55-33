using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// Convierte los cubos de "Sillas" en sillas con forma (asiento, 4 patas, respaldo).
// Conserva el lugar de cada silla que ya existe. Uso: Add Component en el objeto "Sillas",
// asigna M_Madera y en el menu del componente (los tres puntos) elige "Generar".
public class GeneradorSillas : MonoBehaviour
{
    public Material material;

    [Tooltip("Hacia donde mira la silla. (0,0,-1) = hacia la nave, de espaldas al fondo del escenario.")]
    public Vector3 direccionFrente = new Vector3(0f, 0f, -1f);

    [ContextMenu("Generar")]
    void Generar()
    {
        // 1) recordar donde estaba cada silla
        var lugares = new List<(string nombre, Vector3 pos)>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var hijo = transform.GetChild(i);
            Vector3 p = hijo.localPosition;
            // si es un cubo viejo, el pivote esta a media altura: bajar al piso
            if (hijo.GetComponent<MeshFilter>() != null)
                p.y -= hijo.localScale.y * 0.5f;
            lugares.Add((hijo.name, p));
        }

        // 2) borrar lo anterior
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        // 3) reconstruir como sillas
        foreach (var l in lugares) CrearSilla(l.nombre, l.pos);

#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    void CrearSilla(string nombre, Vector3 pos)
    {
        var raiz = new GameObject(nombre);
        raiz.transform.SetParent(transform, false);
        raiz.transform.localPosition = pos;
        raiz.transform.localRotation = Quaternion.LookRotation(direccionFrente.normalized, Vector3.up);

        // En local, la silla mira a +Z; el respaldo queda en -Z.
        Parte(raiz, "Asiento", new Vector3(0f, 0.45f, 0f), new Vector3(0.45f, 0.05f, 0.45f));

        float o = 0.19f;
        Parte(raiz, "Pata_DelIzq", new Vector3(-o, 0.2125f, o), new Vector3(0.05f, 0.425f, 0.05f));
        Parte(raiz, "Pata_DelDer", new Vector3(o, 0.2125f, o), new Vector3(0.05f, 0.425f, 0.05f));

        // patas traseras: suben hasta formar los postes del respaldo
        Parte(raiz, "Poste_TrasIzq", new Vector3(-o, 0.475f, -o), new Vector3(0.05f, 0.95f, 0.05f));
        Parte(raiz, "Poste_TrasDer", new Vector3(o, 0.475f, -o), new Vector3(0.05f, 0.95f, 0.05f));

        Parte(raiz, "Respaldo", new Vector3(0f, 0.75f, -o), new Vector3(0.33f, 0.28f, 0.03f));
        Parte(raiz, "Travesano", new Vector3(0f, 0.9f, -o), new Vector3(0.33f, 0.06f, 0.04f));

        var col = raiz.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.475f, 0f);
        col.size = new Vector3(0.45f, 0.95f, 0.45f);
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
}
