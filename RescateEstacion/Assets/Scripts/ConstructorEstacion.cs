using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Arma la estación (interior de 6 x 8 + hangar alrededor) a partir de final.txt.
/// Formato del archivo:
///   6 líneas de paredes: 8 celdas con 4 bits (arriba, izquierda, abajo, derecha).
///   3 líneas de señales de vida: fila columna tipo (v = científico, f = falsa).
///   10 líneas de aliens iniciales: fila columna.
///   8 líneas de compuertas: fila1 columna1 fila2 columna2.
///   4 líneas de entradas: fila columna.
/// Filas 1..6 y columnas 1..8 son el interior; la fila 0 y 7 y la columna 0 y 9 son el hangar.
/// </summary>
public class ConstructorEstacion : MonoBehaviour
{
    public const int FILAS = 6;
    public const int COLUMNAS = 8;
    public const int ARRIBA = 0, IZQUIERDA = 1, ABAJO = 2, DERECHA = 3;

    [Header("Archivo del tablero (arrastra final.txt)")]
    public TextAsset archivoTablero;

    [Header("Prefabs. Si se dejan vacíos se dibujan cubos grises (graybox)")]
    public GameObject prefabPiso;
    public GameObject prefabPisoHangar;
    [Tooltip("Su modelo debe quedar en el borde de ARRIBA (+Z) de la celda.")]
    public GameObject prefabPared;
    [Tooltip("Igual que la pared: en el borde de ARRIBA (+Z) de la celda.")]
    public GameObject prefabCompuerta;
    [Tooltip("Opcional. Marco abierto en el borde de ARRIBA (+Z) de la celda.")]
    public GameObject prefabEntrada;
    public GameObject prefabAlien;
    public GameObject prefabSenal;

    public float tamCelda = 1f;

    // Lo que se leyó del archivo, para que otros scripts lo consulten.
    public readonly List<Vector2Int> aliensIniciales = new List<Vector2Int>();
    public readonly List<Vector2Int> senalesIniciales = new List<Vector2Int>();
    public readonly List<Vector2Int> entradas = new List<Vector2Int>();

    // Cada pared, compuerta o entrada queda registrada por su borde ("fila,columna,lado").
    public readonly Dictionary<string, GameObject> bordes = new Dictionary<string, GameObject>();

    private readonly int[,,] bits = new int[FILAS + 2, COLUMNAS + 2, 4];
    private readonly HashSet<string> bordesCompuerta = new HashSet<string>();
    private readonly HashSet<string> bordesEntrada = new HashSet<string>();
    private Transform raizPisos, raizParedes, raizCompuertas, raizEntradas, raizAliens, raizSenales;

    void Start()
    {
        LeerArchivo();
        Construir();
    }

    // ------------------------------------------------------------ lectura
    void LeerArchivo()
    {
        var lineas = new List<string>();
        foreach (string l in archivoTablero.text.Split('\n'))
        {
            string t = l.Trim();
            if (t.Length > 0 && char.IsDigit(t[0])) lineas.Add(t);
        }

        int i = 0;
        for (int fila = 1; fila <= FILAS; fila++, i++)
        {
            string[] celdas = lineas[i].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            for (int col = 1; col <= COLUMNAS; col++)
                for (int lado = 0; lado < 4; lado++)
                    bits[fila, col, lado] = celdas[col - 1][lado] - '0';
        }

        for (int k = 0; k < 3; k++, i++)
        {
            string[] p = lineas[i].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            senalesIniciales.Add(new Vector2Int(int.Parse(p[0]), int.Parse(p[1])));
        }

        for (int k = 0; k < 10; k++, i++)
        {
            string[] p = lineas[i].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            aliensIniciales.Add(new Vector2Int(int.Parse(p[0]), int.Parse(p[1])));
        }

        for (int k = 0; k < 8; k++, i++)
        {
            string[] p = lineas[i].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            int f1 = int.Parse(p[0]), c1 = int.Parse(p[1]), f2 = int.Parse(p[2]), c2 = int.Parse(p[3]);
            int lado = f1 == f2 ? (c2 > c1 ? DERECHA : IZQUIERDA) : (f2 > f1 ? ABAJO : ARRIBA);
            bordesCompuerta.Add(Clave(f1, c1, lado));
        }

        for (int k = 0; k < 4; k++, i++)
        {
            string[] p = lineas[i].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            int f = int.Parse(p[0]), c = int.Parse(p[1]);
            entradas.Add(new Vector2Int(f, c));
            bordesEntrada.Add(Clave(f, c, LadoExterior(f, c)));
        }
    }

    // ------------------------------------------------------------ construcción
    void Construir()
    {
        raizPisos = CrearRaiz("Pisos");
        raizParedes = CrearRaiz("Paredes");
        raizCompuertas = CrearRaiz("Compuertas");
        raizEntradas = CrearRaiz("Entradas");
        raizAliens = CrearRaiz("Aliens");
        raizSenales = CrearRaiz("SenalesDeVida");

        // Piso: interior y hangar.
        for (int fila = 0; fila <= FILAS + 1; fila++)
            for (int col = 0; col <= COLUMNAS + 1; col++)
            {
                bool interior = EsInterior(fila, col);
                GameObject prefab = interior ? prefabPiso : prefabPisoHangar;
                GameObject piso = prefab != null
                    ? Instantiate(prefab, Celda(fila, col), Quaternion.identity, raizPisos)
                    : CuboGris(Celda(fila, col) + Vector3.down * 0.05f, new Vector3(tamCelda, 0.1f, tamCelda),
                               interior ? new Color(0.80f, 0.83f, 0.86f) : new Color(0.25f, 0.28f, 0.32f), raizPisos);
                piso.name = $"Piso_{fila}_{col}";
            }

        // Bordes: cada borde se revisa una sola vez (arriba e izquierda de cada celda,
        // más abajo en la última fila y derecha en la última columna).
        for (int fila = 1; fila <= FILAS; fila++)
            for (int col = 1; col <= COLUMNAS; col++)
                for (int lado = 0; lado < 4; lado++)
                {
                    if (lado == ABAJO && fila < FILAS) continue;
                    if (lado == DERECHA && col < COLUMNAS) continue;
                    if (!HayPared(fila, col, lado)) continue;

                    string clave = Clave(fila, col, lado);
                    if (bordesCompuerta.Contains(clave))
                        bordes[clave] = CrearBorde(prefabCompuerta, fila, col, lado, raizCompuertas, new Color(0.88f, 0.54f, 0.12f), "Compuerta");
                    else if (bordesEntrada.Contains(clave))
                        bordes[clave] = CrearBorde(prefabEntrada, fila, col, lado, raizEntradas, new Color(0.18f, 0.75f, 0.36f), "Entrada", 0.02f);
                    else
                        bordes[clave] = CrearBorde(prefabPared, fila, col, lado, raizParedes, new Color(0.12f, 0.14f, 0.16f), "Pared");
                }

        foreach (Vector2Int a in aliensIniciales)
            CrearEnCelda(prefabAlien, a, raizAliens, new Color(0.48f, 0.25f, 0.72f), "Alien");

        foreach (Vector2Int s in senalesIniciales)
            CrearEnCelda(prefabSenal, s, raizSenales, new Color(0.12f, 0.56f, 0.84f), "Senal");
    }

    GameObject CrearBorde(GameObject prefab, int fila, int col, int lado, Transform padre, Color color, string nombre, float alto = 1f)
    {
        Quaternion giro = Quaternion.Euler(0f, AnguloDe(lado), 0f);
        GameObject borde;
        if (prefab != null)
        {
            borde = Instantiate(prefab, Celda(fila, col), giro, padre);
        }
        else
        {
            borde = new GameObject();
            borde.transform.SetPositionAndRotation(Celda(fila, col), giro);
            borde.transform.SetParent(padre);
            GameObject cubo = CuboGris(Vector3.zero, new Vector3(tamCelda, alto, 0.1f * tamCelda), color, borde.transform);
            cubo.transform.localPosition = new Vector3(0f, alto / 2f, tamCelda / 2f);
        }
        borde.name = $"{nombre}_{fila}_{col}_{NombreLado(lado)}";
        return borde;
    }

    void CrearEnCelda(GameObject prefab, Vector2Int celda, Transform padre, Color color, string nombre)
    {
        GameObject obj;
        if (prefab != null)
        {
            obj = Instantiate(prefab, Celda(celda.x, celda.y), Quaternion.identity, padre);
        }
        else
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.SetParent(padre);
            obj.transform.position = Celda(celda.x, celda.y) + Vector3.up * 0.3f * tamCelda;
            obj.transform.localScale = Vector3.one * 0.5f * tamCelda;
            obj.GetComponent<Renderer>().material.color = color;
        }
        obj.name = $"{nombre}_{celda.x}_{celda.y}";
    }

    // ------------------------------------------------------------ utilidades
    /// <summary>Centro de la celda en el mundo. La fila 1 queda arriba (+Z).</summary>
    public Vector3 Celda(int fila, int col)
    {
        return transform.position + new Vector3(col * tamCelda, 0f, -fila * tamCelda);
    }

    /// <summary>Hay pared si cualquiera de las dos celdas que comparten el borde lo dice.</summary>
    public bool HayPared(int fila, int col, int lado)
    {
        int v = bits[fila, col, lado];
        Vector2Int vecina = Vecina(fila, col, lado);
        if (EsInterior(vecina.x, vecina.y))
            v |= bits[vecina.x, vecina.y, Opuesto(lado)];
        return v == 1;
    }

    /// <summary>Nombre único de un borde: siempre se guarda como ARRIBA o IZQUIERDA de una celda.</summary>
    public static string Clave(int fila, int col, int lado)
    {
        if (lado == ABAJO) { fila += 1; lado = ARRIBA; }
        if (lado == DERECHA) { col += 1; lado = IZQUIERDA; }
        return $"{fila},{col},{lado}";
    }

    static bool EsInterior(int fila, int col) => fila >= 1 && fila <= FILAS && col >= 1 && col <= COLUMNAS;

    static Vector2Int Vecina(int fila, int col, int lado)
    {
        switch (lado)
        {
            case ARRIBA: return new Vector2Int(fila - 1, col);
            case IZQUIERDA: return new Vector2Int(fila, col - 1);
            case ABAJO: return new Vector2Int(fila + 1, col);
            default: return new Vector2Int(fila, col + 1);
        }
    }

    static int Opuesto(int lado) => (lado + 2) % 4;

    /// <summary>Lado que da al hangar. En una esquina se elige en orden arriba, abajo, izquierda, derecha.</summary>
    static int LadoExterior(int fila, int col)
    {
        if (fila == 1) return ARRIBA;
        if (fila == FILAS) return ABAJO;
        if (col == 1) return IZQUIERDA;
        return DERECHA;
    }

    static float AnguloDe(int lado)
    {
        switch (lado)
        {
            case ARRIBA: return 0f;
            case DERECHA: return 90f;
            case ABAJO: return 180f;
            default: return 270f;
        }
    }

    static string NombreLado(int lado)
    {
        string[] nombres = { "Arriba", "Izquierda", "Abajo", "Derecha" };
        return nombres[lado];
    }

    Transform CrearRaiz(string nombre)
    {
        var raiz = new GameObject(nombre).transform;
        raiz.SetParent(transform, false);
        return raiz;
    }

    GameObject CuboGris(Vector3 posicion, Vector3 escala, Color color, Transform padre)
    {
        GameObject cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubo.transform.SetParent(padre, false);
        cubo.transform.position = posicion;
        cubo.transform.localScale = escala;
        cubo.GetComponent<Renderer>().material.color = color;
        return cubo;
    }
}
