using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class TileByScale : MonoBehaviour
{
    public enum Plano { XZ, XY, ZY }

    [Tooltip("XZ: pisos y techos. XY: paredes que miran a Z. ZY: paredes que miran a X.")]
    public Plano plano = Plano.XZ;
    public float metrosPorMosaico = 1f;

    void OnEnable() { Aplicar(); }
    void OnValidate() { Aplicar(); }

    void Aplicar()
    {
        var r = GetComponent<Renderer>();
        if (r == null) return;

        Vector3 s = transform.lossyScale;
        float u, v;
        switch (plano)
        {
            case Plano.XY: u = s.x; v = s.y; break;
            case Plano.ZY: u = s.z; v = s.y; break;
            default: u = s.x; v = s.z; break;
        }

        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetVector("_BaseMap_ST", new Vector4(u / metrosPorMosaico, v / metrosPorMosaico, 0f, 0f));
        r.SetPropertyBlock(mpb);
    }
}