using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Crea los prefabs de la Estación Orión-7 a partir de los modelos de ThirdParty.
/// Cada prefab es un objeto vacío (padre) con el modelo como hijo, igual que en la Clase 3.
/// El script mide los modelos y los acomoda para que encajen en una casilla de 1 x 1:
///   - Muros y compuertas: a lo largo del borde de ARRIBA (+Z) de la casilla, con el frente hacia la casilla.
///   - Pisos: cubren la casilla y su cara superior queda en y = 0.
///   - Criaturas, esporas, escuadrón y prototipos: centrados y apoyados en el piso.
/// Los muros del Ultimate Modular Sci-Fi usan un material de acento cian que brilla (con Bloom).
/// Menú: Rescate > Crear prefabs de la estación
/// </summary>
public static class CreadorPrefabsEstacion
{
    const string THIRD_PARTY = "Assets/ThirdParty/";
    const string UMS = "UltimateModularSciFi/";
    const string RUTA_PREFABS = "Assets/Prefabs";
    const string RUTA_MATERIALES = "Assets/Materials";
    const string RUTA_ANIMACION = "Assets/Animacion";

    static readonly Color CIAN = new Color(0.12f, 0.71f, 0.82f);
    static readonly Color ROJO = new Color(0.75f, 0.22f, 0.17f);
    static readonly Color LIQUIDO = new Color(0.22f, 0.83f, 0.33f);

    // Grises de los muros del Ultimate Modular Sci-Fi. Se pueden ajustar después en Assets/Materials.
    static readonly Color GRIS_PRINCIPAL = new Color(0.16f, 0.17f, 0.19f);
    static readonly Color GRIS_CLARO = new Color(0.24f, 0.25f, 0.27f);
    static readonly Color GRIS_OSCURO = new Color(0.08f, 0.08f, 0.09f);
    static Material muroPrincipal, muroClaro, muroOscuro;

    // Brillo del acento cian (antes 1.2) y qué tanto se oscurece el piso (1 = original).
    const float BRILLO_CIAN = 0.6f;
    const float TONO_PISO = 0.55f;

    // Escala de los muros (1 / largo del muro). Las columnas y los objetos del mismo pack usan la misma.
    static float escalaPack = 0.25f;

    [MenuItem("Rescate/Crear prefabs de la estación")]
    public static void CrearPrefabs()
    {
        CrearCarpeta("Assets/Prefabs", "Estacion");
        CrearCarpeta("Assets/Prefabs", "Aliens");
        CrearCarpeta("Assets/Prefabs", "Personajes");
        CrearCarpeta("Assets/Prefabs", "Decoracion");
        CrearCarpeta("Assets", "Materials");
        CrearCarpeta("Assets", "Animacion");

        Material acento = MaterialBrillante("AcentoCian", CIAN, BRILLO_CIAN);
        Material acentoOscuro = MaterialBrillante("AcentoCianOscuro", CIAN * 0.35f, 0f);
        Material acentoRojo = MaterialBrillante("AcentoRojo", ROJO, 1.2f);
        Material liquido = MaterialBrillante("LiquidoTubo", LIQUIDO, 1.5f);
        muroPrincipal = MaterialBrillante("MuroPrincipal", GRIS_PRINCIPAL, 0f);
        muroClaro = MaterialBrillante("MuroClaro", GRIS_CLARO, 0f);
        muroOscuro = MaterialBrillante("MuroOscuro", GRIS_OSCURO, 0f);

        // Piso (MegaKit).
        // El piso de la estación se oscurece (TONO_PISO) para que resalten los muros y los personajes.
        GameObject piso = CrearPiso("Estacion/Piso", "MegaKit/Platforms/Platform_Metal.fbx", TONO_PISO);
        GameObject pisoHangar = CrearPiso("Estacion/PisoHangar", "MegaKit/Platforms/Platform_DarkPlates.fbx", 1f);

        // Muros (Ultimate Modular Sci-Fi) con acento cian. Dos llevan los cables colgando del MegaKit.
        const string CABLES = "MegaKit/Walls/TopCables_Straight_Hanging.fbx";
        var paredes = new List<GameObject>();
        for (int i = 1; i <= 5; i++)
            paredes.Add(CrearMuro($"Estacion/Pared_{i}", acento, acentoOscuro, null, UMS + $"Wall_{i}.fbx"));
        paredes.Add(CrearMuro("Estacion/Pared_Cables_1", acento, acentoOscuro, CABLES, UMS + "Wall_1.fbx"));
        paredes.Add(CrearMuro("Estacion/Pared_Cables_3", acento, acentoOscuro, CABLES, UMS + "Wall_3.fbx"));

        var paredesVentana = new List<GameObject>
        {
            CrearMuro("Estacion/Pared_Ventana", acento, acentoOscuro, null, UMS + "Window_Wall_SideA.fbx"),
            CrearMuro("Estacion/Pared_VentanaLarga", acento, acentoOscuro, null, UMS + "LongWindow_Wall_SideA.fbx"),
            CrearMuro("Estacion/Pared_VentanasChicas", acento, acentoOscuro, null, UMS + "SmallWindows_Wall_SideA.fbx"),
            CrearMuro("Estacion/Pared_TresVentanas", acento, acentoOscuro, null, UMS + "ThreeWindows_Wall_SideA.fbx"),
        };

        // Pared dañada: muro liso con el acento en rojo (alerta).
        CrearMuro("Estacion/ParedDanada", acentoRojo, acentoOscuro, null, UMS + "Wall_Empty.fbx");

        // Compuertas: el muro con marco y la puerta van juntos; la puerta queda como hijo "Puerta".
        GameObject compuerta = CrearMuro("Estacion/Compuerta", acento, acentoOscuro, null,
                                         UMS + "DoorSingle_Wall_SideA.fbx", UMS + "Door_Single.fbx");
        CrearMuro("Estacion/CompuertaAbierta", acento, acentoOscuro, null, UMS + "DoorSingle_Wall_SideA.fbx");
        GameObject entrada = CrearMuro("Estacion/Entrada", acento, acentoOscuro, null, UMS + "DoorSingle_Wall_SideA.fbx");

        GameObject columna = CrearDelPack("Estacion/Columna", UMS + "Column_3.fbx", acento, acentoOscuro, null);
        GameObject senal = CrearSenal("Estacion/Senal");

        // Decoración del laboratorio (para usar más adelante).
        CrearDelPack("Decoracion/TuboExperimento", UMS + "Props_Pod.fbx", acento, acentoOscuro, liquido);
        CrearDelPack("Decoracion/Capsula", UMS + "Props_Capsule.fbx", acento, acentoOscuro, liquido);
        CrearDelPack("Decoracion/Computadora", UMS + "Props_ComputerSmall.fbx", acento, acentoOscuro, null);

        // Criaturas (= fuego) y esporas (= humo).
        GameObject[] criaturas =
        {
            CrearEnCelda("Aliens/Criatura_Fish", "UltimateMonsters/Fish.fbx", 0.75f),
            CrearEnCelda("Aliens/Criatura_GlubEvolved", "UltimateMonsters/Glub_Evolved.fbx", 0.7f),
            CrearEnCelda("Aliens/Criatura_Squidle", "UltimateMonsters/Squidle.fbx", 0.95f),
            CrearEnCelda("Aliens/Criatura_Scolitex", "MegaKit/Aliens/Alien_Scolitex.fbx", 0.8f),
        };
        GameObject[] esporas =
        {
            CrearEnCelda("Aliens/Espora_GreenSpikyBlob", "UltimateMonsters/GreenSpikyBlob.fbx", 0.45f),
            CrearEnCelda("Aliens/Espora_Cyclop", "MegaKit/Aliens/Alien_Cyclop.fbx", 0.4f),
        };

        // Escuadrón de rescate (= bomberos) y Prototipos M con sus pilotos (= víctimas).
        GameObject[] escuadron =
        {
            CrearEnCelda("Personajes/Escuadron_1", "SpaceKit/Astronaut.fbx", 0.55f),
            CrearEnCelda("Personajes/Escuadron_2", "SpaceKit/Astronaut_2.fbx", 0.55f),
            CrearEnCelda("Personajes/Escuadron_3", "SpaceKit/Astronaut_3.fbx", 0.55f),
        };
        GameObject[] prototipos =
        {
            CrearEnCelda("Personajes/Prototipo_1", "SpaceKit/Mech.fbx", 0.75f),
            CrearEnCelda("Personajes/Prototipo_2", "SpaceKit/Mech_2.fbx", 0.75f),
            CrearEnCelda("Personajes/Prototipo_3", "SpaceKit/Mech_3.fbx", 0.75f),
            CrearEnCelda("Personajes/Prototipo_4", "SpaceKit/Mech_4.fbx", 0.75f),
        };

        AsignarAlConstructor(piso, pisoHangar, compuerta, entrada, senal, columna,
                             SinVacios(paredes.ToArray()), SinVacios(paredesVentana.ToArray()),
                             SinVacios(criaturas), SinVacios(esporas), SinVacios(escuadron), SinVacios(prototipos));

        AssetDatabase.SaveAssets();
        Debug.Log("Prefabs de Orión-7 creados en " + RUTA_PREFABS + " (Estacion, Aliens, Personajes y Decoracion).");
    }

    // ------------------------------------------------------------ muros
    /// <summary>
    /// Muro sobre el borde de ARRIBA (+Z) de la casilla. "modelos" comparten origen (por ejemplo, el muro y su puerta)
    /// y se acomodan juntos. Si se indica "cables", se cuelgan del lado de la casilla, alineados arriba.
    /// </summary>
    static GameObject CrearMuro(string nombre, Material acento, Material acentoOscuro, string cables, params string[] modelos)
    {
        GameObject raiz = Envolver(nombre, out Transform hijo, modelos);
        if (raiz == null) return null;

        // Lado largo sobre X.
        Bounds b = Medir(hijo.gameObject);
        if (b.size.z > b.size.x)
        {
            hijo.localRotation = Quaternion.Euler(0f, 90f, 0f) * hijo.localRotation;
            b = Medir(hijo.gameObject);
        }

        // El frente del muro (lado A) mira hacia +Z en el pack; se gira para que mire hacia la casilla (-Z).
        hijo.localRotation = Quaternion.Euler(0f, 180f, 0f) * hijo.localRotation;

        // Largo = 1 casilla.
        escalaPack = 1f / b.size.x;
        hijo.localScale *= escalaPack;
        b = Medir(hijo.gameObject);

        // Centrado en X, apoyado en el piso y sobre el borde de arriba (z = 0.5).
        hijo.localPosition += new Vector3(-b.center.x, -b.min.y, 0.5f - b.center.z);
        b = Medir(hijo.gameObject);

        CambiarMateriales(hijo.gameObject, acento, acentoOscuro, null);

        // Los muros del pack solo tienen detalle por un lado: por detrás no tienen caras y Unity no las dibuja
        // (backface culling), así que desde la otra casilla el muro se ve transparente.
        // Se agrega una copia girada 180° pegada por la espalda, y se aplasta a la mitad para que el grosor no cambie.
        GameObject modelo = new GameObject("Modelo");
        modelo.transform.SetParent(raiz.transform, false);
        hijo.name = "Frente";
        hijo.SetParent(modelo.transform, true);
        Transform atras = Object.Instantiate(hijo.gameObject, modelo.transform).transform;
        atras.name = "Atras";
        atras.RotateAround(new Vector3(b.center.x, 0f, b.max.z), Vector3.up, 180f);
        modelo.transform.localScale = new Vector3(1f, 1f, 0.5f);
        b = Medir(modelo);
        modelo.transform.localPosition += new Vector3(0f, 0f, 0.5f - b.center.z);
        b = Medir(modelo);

        if (cables != null) AgregarCables(raiz, cables, b);
        return Guardar(raiz, nombre);
    }

    /// <summary>Cuelga los cables del MegaKit en la cara del muro que da a la casilla, alineados con la parte de arriba.</summary>
    static void AgregarCables(GameObject raiz, string modelo, Bounds muro)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(THIRD_PARTY + modelo);
        if (asset == null)
        {
            Debug.LogWarning("No se encontró " + THIRD_PARTY + modelo + "; el muro se crea sin cables.");
            return;
        }

        GameObject cables = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        cables.name = "Cables";
        cables.transform.SetParent(raiz.transform, false);

        Bounds b = Medir(cables);
        if (b.size.z > b.size.x)
        {
            cables.transform.localRotation = Quaternion.Euler(0f, 90f, 0f) * cables.transform.localRotation;
            b = Medir(cables);
        }
        cables.transform.localScale *= 1f / b.size.x;
        b = Medir(cables);

        // Arriba alineado con el muro y pegado a su cara interior (lado -Z).
        cables.transform.localPosition += new Vector3(-b.center.x, muro.max.y - b.max.y, (muro.min.z + 0.01f) - b.max.z);
    }

    /// <summary>Objeto del Ultimate Modular Sci-Fi con la misma escala que los muros (columnas, tubos, computadoras).</summary>
    static GameObject CrearDelPack(string nombre, string modelo, Material acento, Material acentoOscuro, Material vidrio)
    {
        GameObject raiz = Envolver(nombre, out Transform hijo, modelo);
        if (raiz == null) return null;

        hijo.localScale *= escalaPack;
        Bounds b = Medir(hijo.gameObject);
        hijo.localPosition += new Vector3(-b.center.x, -b.min.y, -b.center.z);

        CambiarMateriales(hijo.gameObject, acento, acentoOscuro, vidrio);
        return Guardar(raiz, nombre);
    }

    /// <summary>
    /// Cambia los materiales del pack: el acento naranja por cian, los grises por grises más oscuros y el vidrio por el líquido verde.
    /// Es lo mismo que asignar materiales a mano en el Inspector (Lab 5), pero en todos los modelos a la vez.
    /// </summary>
    static void CambiarMateriales(GameObject modelo, Material acento, Material acentoOscuro, Material vidrio)
    {
        foreach (Renderer r in modelo.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                string n = mats[i].name.ToLower();
                if (n.Contains("darkaccent")) mats[i] = acentoOscuro;
                else if (n.Contains("accent")) mats[i] = acento;
                else if (vidrio != null && n.Contains("glass")) mats[i] = vidrio;
                else if (n.StartsWith("main")) mats[i] = muroPrincipal;
                else if (n.StartsWith("light")) mats[i] = muroClaro;
                else if (n.StartsWith("darkgrey") || n.StartsWith("black")) mats[i] = muroOscuro;
            }
            r.sharedMaterials = mats;
        }
    }

    static Material MaterialBrillante(string nombre, Color color, float brillo)
    {
        string ruta = RUTA_MATERIALES + "/" + nombre + ".mat";
        // Si el material ya existe se actualiza con los valores del script (los cambios hechos a mano se pierden).
        Material material = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        bool nuevo = material == null;
        if (nuevo)
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(lit != null ? lit : Shader.Find("Standard"));
        }
        material.color = color;
        if (brillo > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * brillo);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        if (nuevo) AssetDatabase.CreateAsset(material, ruta);
        else EditorUtility.SetDirty(material);
        return material;
    }

    // ------------------------------------------------------------ otros tipos de prefab
    static GameObject CrearPiso(string nombre, string modelo, float tono)
    {
        GameObject raiz = Envolver(nombre, out Transform hijo, modelo);
        if (raiz == null) return null;

        // Copia de cada material del piso con el color multiplicado por "tono" (1 = sin cambio). Conserva la textura.
        if (tono < 1f)
        {
            foreach (Renderer r in hijo.GetComponentsInChildren<Renderer>())
            {
                Material[] mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    string ruta = RUTA_MATERIALES + "/Piso_" + mats[i].name + ".mat";
                    Material oscuro = AssetDatabase.LoadAssetAtPath<Material>(ruta);
                    if (oscuro == null)
                    {
                        oscuro = new Material(mats[i]);
                        AssetDatabase.CreateAsset(oscuro, ruta);
                    }
                    oscuro.color = mats[i].color * tono;
                    EditorUtility.SetDirty(oscuro);
                    mats[i] = oscuro;
                }
                r.sharedMaterials = mats;
            }
        }

        Bounds b = Medir(raiz);
        hijo.localScale *= 1f / Mathf.Max(b.size.x, b.size.z);
        b = Medir(raiz);
        hijo.localPosition += new Vector3(-b.center.x, -b.max.y, -b.center.z);
        return Guardar(raiz, nombre);
    }

    /// <summary>Objeto que ocupa una casilla: su lado más ancho mide "tamano" y queda apoyado en el piso.</summary>
    static GameObject CrearEnCelda(string nombre, string modelo, float tamano)
    {
        // Primero se ajusta el importador (si se reimporta después, el modelo ya colocado se pierde).
        AnimationClip idle = PrepararIdle(THIRD_PARTY + modelo);
        GameObject raiz = Envolver(nombre, out Transform hijo, modelo);
        if (raiz == null) return null;

        Bounds b = Medir(raiz);
        hijo.localScale *= tamano / Mathf.Max(b.size.x, b.size.z);
        b = Medir(raiz);
        hijo.localPosition += new Vector3(-b.center.x, -b.min.y, -b.center.z);

        if (idle != null) PonerAnimator(hijo.gameObject, idle, THIRD_PARTY + modelo);
        return Guardar(raiz, nombre);
    }

    /// <summary>Señal de energía: esfera azul que brilla (con Bloom se ve como holograma).</summary>
    static GameObject CrearSenal(string nombre)
    {
        Material material = MaterialBrillante("Senal", new Color(0.15f, 0.65f, 1f), 2f);

        GameObject raiz = new GameObject("Senal");
        GameObject esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        esfera.name = "Modelo";
        esfera.transform.SetParent(raiz.transform, false);
        esfera.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        esfera.transform.localScale = Vector3.one * 0.35f;
        Object.DestroyImmediate(esfera.GetComponent<Collider>());
        esfera.GetComponent<Renderer>().sharedMaterial = material;
        return Guardar(raiz, nombre);
    }

    // ------------------------------------------------------------ animación
    /// <summary>
    /// Marca en bucle las animaciones Idle, Walk y Run del modelo (Loop Time, como en la Clase 3)
    /// y devuelve su animación Idle, o null si no trae.
    /// </summary>
    static AnimationClip PrepararIdle(string rutaModelo)
    {
        ModelImporter importador = AssetImporter.GetAtPath(rutaModelo) as ModelImporter;
        if (importador == null) return null;

        ModelImporterClipAnimation[] clips = importador.clipAnimations.Length > 0
            ? importador.clipAnimations
            : importador.defaultClipAnimations;
        if (clips.Length == 0) return null;

        bool cambio = importador.clipAnimations.Length == 0;
        foreach (ModelImporterClipAnimation clip in clips)
        {
            string n = clip.name.ToLower();
            bool enBucle = n.EndsWith("idle") || n.EndsWith("walk") || n.EndsWith("run");
            if (enBucle && !clip.loopTime) { clip.loopTime = true; cambio = true; }
        }
        if (cambio)
        {
            importador.clipAnimations = clips;
            importador.SaveAndReimport();
        }

        return AssetDatabase.LoadAllAssetsAtPath(rutaModelo)
            .OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__") && c.name.ToLower().Contains("idle") && !c.name.ToLower().Contains("jump"))
            .OrderBy(c => c.name.Length)
            .FirstOrDefault();
    }

    /// <summary>Animator Controller con Idle como estado por defecto (la animación solo acompaña, Clase 3).</summary>
    static void PonerAnimator(GameObject modelo, AnimationClip idle, string rutaModelo)
    {
        string nombreModelo = System.IO.Path.GetFileNameWithoutExtension(rutaModelo);
        string rutaControlador = RUTA_ANIMACION + "/" + nombreModelo + ".controller";
        AnimatorController controlador = AssetDatabase.LoadAssetAtPath<AnimatorController>(rutaControlador);
        if (controlador == null)
            controlador = AnimatorController.CreateAnimatorControllerAtPathWithClip(rutaControlador, idle);

        Animator animator = modelo.GetComponent<Animator>();
        if (animator == null) animator = modelo.AddComponent<Animator>();
        animator.runtimeAnimatorController = controlador;
        animator.applyRootMotion = false;
    }

    // ------------------------------------------------------------ utilidades
    /// <summary>
    /// Crea el padre vacío y un hijo "Modelo" con uno o varios modelos dentro (comparten origen).
    /// Si son varios, el segundo se llama "Puerta" para poder abrirla después.
    /// </summary>
    static GameObject Envolver(string nombre, out Transform hijo, params string[] modelos)
    {
        hijo = null;
        var assets = new List<GameObject>();
        foreach (string m in modelos)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(THIRD_PARTY + m);
            if (asset == null)
            {
                Debug.LogWarning("No se encontró el modelo " + THIRD_PARTY + m + ". Se omite el prefab " + nombre + ".");
                return null;
            }
            assets.Add(asset);
        }

        GameObject raiz = new GameObject(System.IO.Path.GetFileName(nombre));
        if (assets.Count == 1)
        {
            GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(assets[0]);
            instancia.name = "Modelo";
            instancia.transform.SetParent(raiz.transform, false);
            hijo = instancia.transform;
        }
        else
        {
            GameObject grupo = new GameObject("Modelo");
            grupo.transform.SetParent(raiz.transform, false);
            for (int i = 0; i < assets.Count; i++)
            {
                GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(assets[i]);
                instancia.name = i == 0 ? "Marco" : "Puerta";
                instancia.transform.SetParent(grupo.transform, false);
            }
            hijo = grupo.transform;
        }
        return raiz;
    }

    /// <summary>Caja que envuelve todos los renderers del objeto (en coordenadas del mundo; la raíz está en el origen).</summary>
    static Bounds Medir(GameObject objeto)
    {
        Renderer[] renderers = objeto.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.one);
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }

    static GameObject Guardar(GameObject raiz, string nombre)
    {
        string ruta = RUTA_PREFABS + "/" + nombre + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
        Object.DestroyImmediate(raiz);
        return prefab;
    }

    static GameObject[] SinVacios(GameObject[] lista) => lista.Where(p => p != null).ToArray();

    static void CrearCarpeta(string padre, string nombre)
    {
        if (!AssetDatabase.IsValidFolder(padre + "/" + nombre))
            AssetDatabase.CreateFolder(padre, nombre);
    }

    static void AsignarAlConstructor(GameObject piso, GameObject pisoHangar, GameObject compuerta, GameObject entrada,
                                     GameObject senal, GameObject columna, GameObject[] paredes, GameObject[] paredesVentana,
                                     GameObject[] criaturas, GameObject[] esporas, GameObject[] escuadron, GameObject[] prototipos)
    {
        ConstructorEstacion constructor = Object.FindAnyObjectByType<ConstructorEstacion>();
        if (constructor == null)
        {
            Debug.Log("No hay un ConstructorEstacion en la escena abierta; arrastra los prefabs a mano.");
            return;
        }

        Undo.RecordObject(constructor, "Asignar prefabs de la estación");
        constructor.prefabPiso = piso;
        constructor.prefabPisoHangar = pisoHangar;
        constructor.prefabPared = paredes.Length > 0 ? paredes[0] : null;
        constructor.prefabsPared = paredes;
        constructor.prefabsParedExterior = paredesVentana;
        constructor.prefabCompuerta = compuerta;
        constructor.prefabEntrada = entrada;
        constructor.prefabSenal = senal;
        constructor.prefabColumna = columna;
        constructor.prefabsAlien = criaturas;
        constructor.prefabsEsporas = esporas;
        constructor.prefabsEscuadron = escuadron;
        constructor.prefabsPrototipo = prototipos;
        EditorUtility.SetDirty(constructor);
        EditorSceneManager.MarkSceneDirty(constructor.gameObject.scene);
    }
}
