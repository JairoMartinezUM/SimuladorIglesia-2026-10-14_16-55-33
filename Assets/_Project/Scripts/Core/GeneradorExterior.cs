using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// Construye la puerta, las ventanas (con vidrio transparente) y un exterior plano:
// pasto, arboles, arbustos, rocas y una acera frente a la puerta.
// Uso: Add Component en el objeto "Entorno", asigna los materiales de arriba y, en el menu
// del componente (los tres puntos), elige "Generar puerta, ventanas y exterior".
// Se puede regenerar las veces que haga falta.
public class GeneradorExterior : MonoBehaviour
{
    [Header("Materiales existentes (arrastra de Art/)")]
    public Material pared;     // M_Pared
    public Material madera;    // M_Madera
    public Material piedra;    // M_Piedra
    public Material negro;     // M_Negro

    [Header("Materiales nuevos (si se dejan vacios, se crean solos en Art/)")]
    public Material vidrio;
    public Material pasto;
    public Material corteza;
    public Material follaje;
    public Material follaje2;
    public Material concreto;   // acera
    public Material roca;       // rocas (gris liso)
    public Material teja;       // techo a dos aguas y remate de la torre

    [Header("Exterior")]
    public int semilla = 7;
    public int arboles = 60;
    [Tooltip("Arboles lejanos que forman el horizonte en todas direcciones (sin colision).")]
    public int arbolesLejanos = 450;
    public float radioBosqueMin = 45f;
    public float radioBosqueMax = 98f;
    public int arbustos = 24;
    public int rocas = 30;
    public float limiteJugador = 24f;

    // Medidas de la iglesia (las mismas de la escena)
    const float Alto = 5f;
    const float Grosor = 0.2f;
    const float LargoLateral = 18.4f;
    const float PuertaAncho = 1.8f;
    const float PuertaAlto = 2.2f;
    const float ZPuerta = -9.1f;
    static readonly float[] VentanasZ = { 6f, 2.5f, -1f, -4f };
    const float VentanaAncho = 1.3f;
    const float Antepecho = 1.3f;
    const float DinteVentana = 3.5f;

    System.Random rng;

    [ContextMenu("Generar puerta, ventanas y exterior")]
    void Generar()
    {
        if (pared == null || madera == null || piedra == null)
        {
            Debug.LogWarning("GeneradorExterior: asigna M_Pared, M_Madera y M_Piedra en los campos de arriba.");
            return;
        }
        if (negro == null) negro = madera;
        rng = new System.Random(semilla);

        AsegurarMateriales();

        ParedFondoConPuerta();
        ParedLateral("Pared_Izq", -4.6f);
        ParedLateral("Pared_Der", 4.6f);
        ConstruirExterior();

#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        Debug.Log("GeneradorExterior: listo. Guarda la escena (Ctrl+S).");
    }

    // ---------------- paredes ----------------

    Transform NuevaRaiz(string nombre, Vector3 pos)
    {
        var viejo = transform.Find(nombre);
        int indice = transform.childCount;
        if (viejo != null)
        {
            indice = viejo.GetSiblingIndex();
            DestroyImmediate(viejo.gameObject);
        }
        var go = new GameObject(nombre);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.SetSiblingIndex(indice);
        return go.transform;
    }

    void ParedFondoConPuerta()
    {
        // La pared de la entrada (debajo de la cabina), con un hueco para la puerta.
        var raiz = NuevaRaiz("Pared_Fondo", new Vector3(0f, 0f, ZPuerta));
        float mitad = PuertaAncho / 2f;
        float anchoLado = 4.7f - mitad;
        float xLado = (4.7f + mitad) / 2f;

        Pieza(raiz, "Lado_Izq", new Vector3(-xLado, Alto / 2f, 0f), new Vector3(anchoLado, Alto, Grosor), pared, TileByScale.Plano.XY);
        Pieza(raiz, "Lado_Der", new Vector3(xLado, Alto / 2f, 0f), new Vector3(anchoLado, Alto, Grosor), pared, TileByScale.Plano.XY);
        Pieza(raiz, "Sobre_Puerta", new Vector3(0f, (PuertaAlto + Alto) / 2f, 0f),
              new Vector3(PuertaAncho, Alto - PuertaAlto, Grosor), pared, TileByScale.Plano.XY);

        ConstruirPuerta(raiz, mitad);
    }

    void ConstruirPuerta(Transform pared, float mitad)
    {
        var raiz = new GameObject("Puerta").transform;
        raiz.SetParent(pared, false);

        // marco
        Pieza(raiz, "Jamba_Izq", new Vector3(-mitad - 0.04f, PuertaAlto / 2f, 0f), new Vector3(0.12f, PuertaAlto, 0.3f), madera);
        Pieza(raiz, "Jamba_Der", new Vector3(mitad + 0.04f, PuertaAlto / 2f, 0f), new Vector3(0.12f, PuertaAlto, 0.3f), madera);
        Pieza(raiz, "Dintel", new Vector3(0f, PuertaAlto + 0.06f, 0f), new Vector3(PuertaAncho + 0.28f, 0.14f, 0.3f), madera);

        // dos hojas abiertas hacia afuera (hacia -Z)
        Hoja(raiz, "Hoja_Izq", new Vector3(-mitad + 0.02f, 0.1f, 0f), 1f, 80f);
        Hoja(raiz, "Hoja_Der", new Vector3(mitad - 0.02f, 0.1f, 0f), -1f, -80f);
    }

    void Hoja(Transform padre, string nombre, Vector3 bisagra, float lado, float angulo)
    {
        var pivote = new GameObject(nombre).transform;
        pivote.SetParent(padre, false);
        pivote.localPosition = bisagra;
        pivote.localRotation = Quaternion.Euler(0f, angulo, 0f);

        float ancho = PuertaAncho / 2f - 0.04f;
        float alto = PuertaAlto - 0.12f;
        Pieza(pivote, "Panel", new Vector3(lado * ancho / 2f, alto / 2f, 0f), new Vector3(ancho, alto, 0.05f), madera);
        // paneles hundidos oscuros en ambas caras
        foreach (float cara in new[] { -1f, 1f })
        {
            Pieza(pivote, "Inserto_Alto", new Vector3(lado * ancho / 2f, alto * 0.72f, cara * 0.028f), new Vector3(ancho * 0.7f, alto * 0.28f, 0.012f), negro, null, 2f, false);
            Pieza(pivote, "Inserto_Bajo", new Vector3(lado * ancho / 2f, alto * 0.30f, cara * 0.028f), new Vector3(ancho * 0.7f, alto * 0.36f, 0.012f), negro, null, 2f, false);
        }
        Pieza(pivote, "Manija", new Vector3(lado * (ancho - 0.1f), alto * 0.5f, -0.05f), new Vector3(0.04f, 0.16f, 0.04f), negro, null, 2f, false);
    }

    void ParedLateral(string nombre, float x)
    {
        var raiz = NuevaRaiz(nombre, new Vector3(x, 0f, 0f));
        float interior = x < 0f ? 1f : -1f;
        float inicio = -LargoLateral / 2f, fin = LargoLateral / 2f;

        var zs = (float[])VentanasZ.Clone();
        Array.Sort(zs);

        Pieza(raiz, "Bajo", new Vector3(0f, Antepecho / 2f, 0f), new Vector3(Grosor, Antepecho, LargoLateral), pared, TileByScale.Plano.ZY);
        Pieza(raiz, "Alto", new Vector3(0f, (DinteVentana + Alto) / 2f, 0f), new Vector3(Grosor, Alto - DinteVentana, LargoLateral), pared, TileByScale.Plano.ZY);

        float cursor = inicio;
        int n = 1;
        foreach (float z in zs)
        {
            float a = z - VentanaAncho / 2f;
            if (a > cursor) Pilar(raiz, n++, cursor, a);
            cursor = z + VentanaAncho / 2f;
            Ventana(raiz, z, interior);
        }
        if (fin > cursor) Pilar(raiz, n, cursor, fin);
    }

    void Pilar(Transform raiz, int n, float a, float b)
    {
        float alto = DinteVentana - Antepecho;
        Pieza(raiz, "Pilar_" + n, new Vector3(0f, (Antepecho + DinteVentana) / 2f, (a + b) / 2f),
              new Vector3(Grosor, alto, b - a), pared, TileByScale.Plano.ZY);
    }

    void Ventana(Transform raiz, float z, float interior)
    {
        float alto = DinteVentana - Antepecho;
        float yc = (Antepecho + DinteVentana) / 2f;
        float w = VentanaAncho;

        var v = Pieza(raiz, "Vidrio", new Vector3(0f, yc, z), new Vector3(0.04f, alto, w), vidrio);
        var rend = v.GetComponent<Renderer>();
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // marco
        Pieza(raiz, "Marco_Izq", new Vector3(0f, yc, z - (w / 2f - 0.04f)), new Vector3(0.26f, alto, 0.08f), madera, null, 2f, false);
        Pieza(raiz, "Marco_Der", new Vector3(0f, yc, z + (w / 2f - 0.04f)), new Vector3(0.26f, alto, 0.08f), madera, null, 2f, false);
        Pieza(raiz, "Marco_Sup", new Vector3(0f, DinteVentana - 0.04f, z), new Vector3(0.26f, 0.08f, w), madera, null, 2f, false);
        Pieza(raiz, "Marco_Inf", new Vector3(0f, Antepecho + 0.04f, z), new Vector3(0.26f, 0.08f, w), madera, null, 2f, false);
        // cruz de la ventana
        Pieza(raiz, "Barra_V", new Vector3(0f, yc, z), new Vector3(0.08f, alto, 0.05f), madera, null, 2f, false);
        Pieza(raiz, "Barra_H", new Vector3(0f, Antepecho + alto * 0.62f, z), new Vector3(0.08f, 0.05f, w), madera, null, 2f, false);
        // repisa interior
        Pieza(raiz, "Repisa", new Vector3(interior * 0.12f, Antepecho - 0.03f, z), new Vector3(0.35f, 0.06f, w + 0.2f), madera, null, 2f, false);
    }

    // ---------------- exterior ----------------

    void ConstruirExterior()
    {
        var raiz = NuevaRaiz("Exterior", Vector3.zero);

        // pasto: superficie en y = 0.12 (por ENCIMA del piso, que llega a 0.1), en cuatro tiras alrededor del edificio.
        // NO pasa por debajo del interior, para que nunca se mezcle con el piso.
        const float E = 100f;        // hasta donde llega el pasto
        const float X = 4.6f;        // mitad del ancho del edificio (hasta el centro del muro)
        const float Z = 9.1f;        // mitad del largo del edificio
        Pieza(raiz, "Pasto_Fondo", new Vector3(0f, 0.02f, (-Z - E) / 2f), new Vector3(2f * E, 0.2f, E - Z), pasto, TileByScale.Plano.XZ, 6f);
        Pieza(raiz, "Pasto_Frente", new Vector3(0f, 0.02f, (Z + E) / 2f), new Vector3(2f * E, 0.2f, E - Z), pasto, TileByScale.Plano.XZ, 6f);
        Pieza(raiz, "Pasto_Izq", new Vector3((-X - E) / 2f, 0.02f, 0f), new Vector3(E - X, 0.2f, 2f * Z), pasto, TileByScale.Plano.XZ, 6f);
        Pieza(raiz, "Pasto_Der", new Vector3((X + E) / 2f, 0.02f, 0f), new Vector3(E - X, 0.2f, 2f * Z), pasto, TileByScale.Plano.XZ, 6f);

        // acera frente a la puerta (2 cm sobre el pasto)
        Pieza(raiz, "Acera", new Vector3(0f, 0.07f, -15.5f), new Vector3(2.6f, 0.14f, 13f), concreto);   // un poco mas alta que el pasto

        ConstruirFachada(raiz);

        // arbustos pegados a las paredes laterales
        var arb = new GameObject("Arbustos").transform; arb.SetParent(raiz, false);
        for (int i = 0; i < arbustos; i++)
        {
            float lado = (i % 2 == 0) ? -1f : 1f;
            float x = lado * Rango(5.4f, 6.4f);
            float z = Rango(-8.5f, 8.5f);
            Arbusto(arb, i, new Vector3(x, 0f, z));
        }

        // arboles
        var arbs = new GameObject("Arboles").transform; arbs.SetParent(raiz, false);
        for (int i = 0; i < arboles; i++)
        {
            Vector3 p = PuntoLibre(8.5f, 12.5f, 38f);
            Arbol(arbs, i, p);
        }

        // rocas
        var roc = new GameObject("Rocas").transform; roc.SetParent(raiz, false);
        for (int i = 0; i < rocas; i++)
        {
            Vector3 p = (i < 6) ? PuntoCercaDelCamino() : PuntoLibre(6.5f, 11f, 30f);
            Roca(roc, i, p);
        }

        // bosque lejano: anillo de arboles que cubre todo el horizonte (sin colision)
        var lej = new GameObject("BosqueLejano").transform; lej.SetParent(raiz, false);
        float r2min = radioBosqueMin * radioBosqueMin, r2max = radioBosqueMax * radioBosqueMax;
        for (int i = 0; i < arbolesLejanos; i++)
        {
            float radio = Mathf.Sqrt(Rango(r2min, r2max));          // reparto uniforme en el anillo
            float ang = Rango(0f, Mathf.PI * 2f);
            Arbol(lej, i, new Vector3(Mathf.Cos(ang) * radio, 0f, Mathf.Sin(ang) * radio), false, 1.0f, 1.9f);
        }

        // limites invisibles para que el jugador no se pierda afuera
        var lim = new GameObject("Limites").transform; lim.SetParent(raiz, false);
        float L = limiteJugador;
        Limite(lim, new Vector3(0f, 5f, -L), new Vector3(2f * L, 10f, 1f));
        Limite(lim, new Vector3(0f, 5f, L), new Vector3(2f * L, 10f, 1f));
        Limite(lim, new Vector3(-L, 5f, 0f), new Vector3(1f, 10f, 2f * L));
        Limite(lim, new Vector3(L, 5f, 0f), new Vector3(1f, 10f, 2f * L));
    }

    // ---------------- fachada: techo, frontones y torre (igual a la del menu) ----------------

    void ConstruirFachada(Transform raiz)
    {
        var f = new GameObject("Fachada").transform;
        f.SetParent(raiz, false);

        // techo a dos aguas sobre el techo plano interior (que queda como cielo raso)
        float baseY = 5.2f, semi = 5.1f, ang = 28f;
        float largo = semi / Mathf.Cos(ang * Mathf.Deg2Rad);
        float subida = semi * Mathf.Tan(ang * Mathf.Deg2Rad);
        var izq = Pieza(f, "Techo_Izq", new Vector3(-semi / 2f, baseY + subida / 2f + 0.1f, 0f), new Vector3(largo, 0.28f, 19.6f), teja, null, 2f, false);
        izq.transform.localRotation = Quaternion.Euler(0f, 0f, ang);
        var der = Pieza(f, "Techo_Der", new Vector3(semi / 2f, baseY + subida / 2f + 0.1f, 0f), new Vector3(largo, 0.28f, 19.6f), teja, null, 2f, false);
        der.transform.localRotation = Quaternion.Euler(0f, 0f, -ang);

        // frontones escalonados (triangulo hecho con bloques) al frente y al fondo
        float[] anchos = { 8.0f, 5.6f, 3.4f, 1.4f };
        for (int i = 0; i < anchos.Length; i++)
            foreach (float z in new[] { -9.1f, 9.1f })
                Pieza(f, "Fronton", new Vector3(0f, baseY + 0.3f + i * 0.6f, z), new Vector3(anchos[i], 0.6f, 0.2f), pared, TileByScale.Plano.XY, 2f, false);

        // campanario sobre la entrada, con un paso en medio para entrar a la puerta
        float ancho = 3.6f, prof = 3.0f, zc = -10.7f, alto = 9f, paso = 1.8f, pasoAlto = 2.6f;
        float xp = paso / 2f + (ancho - paso) / 4f;
        Pieza(f, "Torre_Pilar_Izq", new Vector3(-xp, alto / 2f, zc), new Vector3((ancho - paso) / 2f, alto, prof), pared, TileByScale.Plano.XY);
        Pieza(f, "Torre_Pilar_Der", new Vector3(xp, alto / 2f, zc), new Vector3((ancho - paso) / 2f, alto, prof), pared, TileByScale.Plano.XY);
        Pieza(f, "Torre_Alta", new Vector3(0f, pasoAlto + (alto - pasoAlto) / 2f, zc), new Vector3(ancho, alto - pasoAlto, prof), pared, TileByScale.Plano.XY);

        float[] remate = { 4.0f, 3.1f, 2.2f, 1.3f, 0.5f };
        for (int i = 0; i < remate.Length; i++)
            Pieza(f, "Torre_Remate", new Vector3(0f, alto + 0.25f + i * 0.5f, zc), new Vector3(remate[i], 0.5f, remate[i]), teja, null, 2f, false);

        float zf = zc - prof / 2f;   // cara frontal de la torre
        Pieza(f, "Torre_Ventana_Marco", new Vector3(0f, 7.0f, zf - 0.01f), new Vector3(1.3f, 1.9f, 0.06f), madera, null, 2f, false);
        var vt = Pieza(f, "Torre_Ventana", new Vector3(0f, 7.0f, zf - 0.04f), new Vector3(1.0f, 1.6f, 0.06f), vidrio, null, 2f, false);
        vt.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // marco de madera en la boca del paso
        Pieza(f, "Entrada_Jamba_Izq", new Vector3(-paso / 2f - 0.05f, pasoAlto / 2f, zf - 0.04f), new Vector3(0.12f, pasoAlto, 0.1f), madera, null, 2f, false);
        Pieza(f, "Entrada_Jamba_Der", new Vector3(paso / 2f + 0.05f, pasoAlto / 2f, zf - 0.04f), new Vector3(0.12f, pasoAlto, 0.1f), madera, null, 2f, false);
        Pieza(f, "Entrada_Dintel", new Vector3(0f, pasoAlto + 0.04f, zf - 0.04f), new Vector3(paso + 0.3f, 0.14f, 0.1f), madera, null, 2f, false);
    }

    Vector3 PuntoLibre(float medioX, float medioZ, float rango)
    {
        // evita el edificio (con margen) y el camino frente a la puerta
        for (int intento = 0; intento < 200; intento++)
        {
            float x = Rango(-rango, rango);
            float z = Rango(-rango, rango);
            bool dentroEdificio = Mathf.Abs(x) < medioX && Mathf.Abs(z) < medioZ + 0.5f;
            bool enCamino = Mathf.Abs(x) < 6f && z < -9f && z > -40f;   // pasillo libre frente a la puerta
            if (!dentroEdificio && !enCamino) return new Vector3(x, 0f, z);
        }
        return new Vector3(rango, 0f, rango);
    }

    Vector3 PuntoCercaDelCamino()
    {
        float lado = rng.NextDouble() < 0.5 ? -1f : 1f;
        return new Vector3(lado * Rango(2.6f, 3.8f), 0f, Rango(-20f, -14f));
    }

    void Arbol(Transform padre, int n, Vector3 pos, bool colision = true, float escalaMin = 0.7f, float escalaMax = 1.2f)
    {
        float s = Rango(escalaMin, escalaMax);
        var r = new GameObject("Arbol_" + (n + 1)).transform;
        r.SetParent(padre, false);
        r.localPosition = pos;
        r.localRotation = Quaternion.Euler(0f, Rango(0f, 90f), 0f);

        float h = Rango(1.6f, 2.6f) * s;
        Pieza(r, "Tronco", new Vector3(0f, h / 2f, 0f), new Vector3(0.35f * s, h, 0.35f * s), corteza, null, 2f, colision);
        Material hoja = (n % 3 == 0) ? follaje2 : follaje;
        Pieza(r, "Copa_1", new Vector3(0f, h + 0.4f * s, 0f), new Vector3(2.2f * s, 1.2f * s, 2.2f * s), hoja, null, 2f, false);
        Pieza(r, "Copa_2", new Vector3(0f, h + 1.3f * s, 0f), new Vector3(1.6f * s, 1.0f * s, 1.6f * s), hoja, null, 2f, false);
        Pieza(r, "Copa_3", new Vector3(0f, h + 2.1f * s, 0f), new Vector3(0.9f * s, 0.8f * s, 0.9f * s), hoja, null, 2f, false);
    }

    void Arbusto(Transform padre, int n, Vector3 pos)
    {
        float s = Rango(0.7f, 1.2f);
        var r = new GameObject("Arbusto_" + (n + 1)).transform;
        r.SetParent(padre, false);
        r.localPosition = pos;
        r.localRotation = Quaternion.Euler(0f, Rango(0f, 90f), 0f);
        Material hoja = (n % 2 == 0) ? follaje : follaje2;
        Pieza(r, "Mata_1", new Vector3(0f, 0.25f * s, 0f), new Vector3(0.9f * s, 0.5f * s, 0.9f * s), hoja);
        Pieza(r, "Mata_2", new Vector3(0.12f * s, 0.55f * s, 0.05f * s), new Vector3(0.5f * s, 0.3f * s, 0.5f * s), hoja, null, 2f, false);
    }

    void Roca(Transform padre, int n, Vector3 pos)
    {
        float ancho = Rango(0.4f, 1.3f);
        float alto = Rango(0.25f, 0.7f);
        float prof = Rango(0.4f, 1.1f);
        var go = Pieza(padre, "Roca_" + (n + 1), new Vector3(pos.x, alto / 2f - 0.05f, pos.z),
                       new Vector3(ancho, alto, prof), roca);
        go.transform.localRotation = Quaternion.Euler(Rango(-6f, 6f), Rango(0f, 90f), Rango(-6f, 6f));
    }

    void Limite(Transform padre, Vector3 pos, Vector3 tam)
    {
        var go = new GameObject("Limite");
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        var c = go.AddComponent<BoxCollider>();
        c.size = tam;
    }

    float Rango(float a, float b) { return a + (float)rng.NextDouble() * (b - a); }

    // ---------------- piezas ----------------

    GameObject Pieza(Transform padre, string nombre, Vector3 pos, Vector3 escala, Material mat,
                     TileByScale.Plano? plano = null, float metrosPorMosaico = 2f, bool colision = true)
    {
        var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.name = nombre;
        p.transform.SetParent(padre, false);
        p.transform.localPosition = pos;
        p.transform.localScale = escala;
        if (!colision) DestroyImmediate(p.GetComponent<Collider>());
        p.GetComponent<Renderer>().sharedMaterial = mat;
        if (plano.HasValue)
        {
            var t = p.AddComponent<TileByScale>();
            t.plano = plano.Value;
            t.metrosPorMosaico = metrosPorMosaico;
            t.enabled = false; t.enabled = true;   // vuelve a aplicar el mosaico con los valores nuevos
        }
        return p;
    }

    // ---------------- materiales (solo en el editor) ----------------

    void AsegurarMateriales()
    {
#if UNITY_EDITOR
        string carpeta = "Assets/_Project/Art/";
        var texPasto = CrearTexturaPasto(carpeta + "Pasto.png");
        Crear(ref pasto, carpeta, "M_Pasto", Color.white, texPasto);
        Crear(ref corteza, carpeta, "M_Corteza", new Color(0.30f, 0.20f, 0.12f), null);
        Crear(ref follaje, carpeta, "M_Follaje", new Color(0.18f, 0.45f, 0.16f), null);
        Crear(ref follaje2, carpeta, "M_Follaje2", new Color(0.26f, 0.55f, 0.20f), null);
        Crear(ref concreto, carpeta, "M_Concreto", new Color(0.66f, 0.66f, 0.63f), null);
        Crear(ref roca, carpeta, "M_Roca", new Color(0.43f, 0.43f, 0.46f), null);
        Crear(ref teja, carpeta, "M_Teja", new Color(0.40f, 0.14f, 0.11f), null);

        if (vidrio == null)
        {
            string ruta = carpeta + "M_Vidrio.mat";
            vidrio = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (vidrio == null)
            {
                var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                m.SetFloat("_Surface", 1f);            // Transparent
                m.SetFloat("_Blend", 0f);              // Alpha
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetFloat("_ZWrite", 0f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                m.SetColor("_BaseColor", new Color(0.72f, 0.86f, 0.95f, 0.18f));
                m.SetFloat("_Metallic", 0f);
                m.SetFloat("_Smoothness", 0.9f);
                m.SetShaderPassEnabled("ShadowCaster", false);
                AssetDatabase.CreateAsset(m, ruta);
                vidrio = m;
            }
        }
#endif
    }

#if UNITY_EDITOR
    void Crear(ref Material m, string carpeta, string nombre, Color c, Texture2D tex)
    {
        if (m != null) return;
        string ruta = carpeta + nombre + ".mat";
        var existente = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (existente != null) { m = existente; return; }
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", c);
        if (tex != null) mat.SetTexture("_BaseMap", tex);
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Smoothness", 0.05f);
        AssetDatabase.CreateAsset(mat, ruta);
        m = mat;
    }

    Texture2D CrearTexturaPasto(string ruta)
    {
        const int N = 256;
        var rnd = new System.Random(11);
        float[,] g8 = Reticula(rnd, 8), g32 = Reticula(rnd, 32);
        var pix = new Color[N * N];
        Color oscuro = new Color(0.17f, 0.38f, 0.11f), claro = new Color(0.33f, 0.59f, 0.23f);
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float v = 0.55f * Muestra(g8, x, y, N) + 0.30f * Muestra(g32, x, y, N) + 0.15f * (float)rnd.NextDouble();
                pix[y * N + x] = Color.Lerp(oscuro, claro, v);
            }
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        tex.SetPixels(pix);
        tex.Apply();
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        DestroyImmediate(tex);
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);

        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        imp.wrapMode = TextureWrapMode.Repeat;
        imp.filterMode = FilterMode.Bilinear;
        imp.mipmapEnabled = true;
        imp.anisoLevel = 4;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
    }

    // ruido de valores que se repite sin costuras (la reticula "da la vuelta")
    static float[,] Reticula(System.Random rnd, int n)
    {
        var r = new float[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++) r[i, j] = (float)rnd.NextDouble();
        return r;
    }

    static float Muestra(float[,] r, int x, int y, int N)
    {
        int n = r.GetLength(0);
        float fx = (float)x / N * n, fy = (float)y / N * n;
        int ix = Mathf.FloorToInt(fx), iy = Mathf.FloorToInt(fy);
        float tx = Mathf.SmoothStep(0f, 1f, fx - ix), ty = Mathf.SmoothStep(0f, 1f, fy - iy);
        float a = r[ix % n, iy % n], b = r[(ix + 1) % n, iy % n];
        float c = r[ix % n, (iy + 1) % n], d = r[(ix + 1) % n, (iy + 1) % n];
        return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
    }
#endif
}
