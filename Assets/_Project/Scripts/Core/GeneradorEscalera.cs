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

    void Limpiar()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
}