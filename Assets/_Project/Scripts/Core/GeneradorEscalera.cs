using UnityEngine;

public class GeneradorEscalera : MonoBehaviour
{
    public int pasos = 12;
    public float alturaPaso = 0.2f;
    public float profundidad = 0.25f;
    public float ancho = 0.9f;

    [ContextMenu("Generar")]
    void Generar()
    {
        Limpiar();
        for (int i = 1; i <= pasos; i++)
        {
            var paso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            paso.name = $"Paso_{i:00}";
            paso.transform.SetParent(transform, false);
            paso.transform.localScale = new Vector3(ancho, alturaPaso * i, profundidad);
            paso.transform.localPosition = new Vector3(0f, alturaPaso * i / 2f, (pasos - i + 0.5f) * profundidad);
        }
    }


    [ContextMenu("Rellenar hueco bajo el descanso")]
    void RellenarHueco()
    {
        var viejo = transform.Find("Relleno_Descanso");
        if (viejo != null) DestroyImmediate(viejo.gameObject);
        var ultimo = transform.Find($"Paso_{pasos:00}");
        if (ultimo == null) { Debug.LogWarning("No hay pasos generados"); return; }
        float alto = alturaPaso * pasos;
        float fondo = 0.8f;
        var r = GameObject.CreatePrimitive(PrimitiveType.Cube);
        r.name = "Relleno_Descanso";
        r.transform.SetParent(transform, false);
        r.transform.localScale = new Vector3(ancho, alto, fondo);
        r.transform.localPosition = new Vector3(0f, alto / 2f, ultimo.localPosition.z - profundidad / 2f - fondo / 2f);
        var mr = ultimo.GetComponent<MeshRenderer>();
        if (mr != null) r.GetComponent<MeshRenderer>().sharedMaterial = mr.sharedMaterial;
    }
    void Limpiar()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
}