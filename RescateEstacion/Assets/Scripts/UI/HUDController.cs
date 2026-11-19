using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD de la simulación (versión 0.1.0.4: solo visual).
/// Muestra turno, rescatados, perdidos y daño al casco, y expone los
/// controles de simulación. Más adelante (Lab 2 / Lab 3) los métodos
/// Set... se llamarán con los datos que mande el servidor.
/// </summary>
public class HUDController : MonoBehaviour
{
    // Límites del reto (condiciones de victoria / derrota)
    public const int MaxRescatados = 7;
    public const int MaxPerdidos = 4;
    public const int MaxDanio = 24;

    [Header("Indicadores")]
    [SerializeField] private TMP_Text turnoText;
    [SerializeField] private TMP_Text rescatadosText;
    [SerializeField] private TMP_Text perdidosText;
    [SerializeField] private TMP_Text danioText;
    [SerializeField] private Slider danioBar;          // barra de daño al casco

    [Header("Controles")]
    [SerializeField] private Button iniciarButton;
    [SerializeField] private Button pausarButton;
    [SerializeField] private Button velocidadButton;
    [SerializeField] private TMP_Text velocidadLabel;  // texto dentro del botón de velocidad
    [SerializeField] private TMP_Dropdown estrategiaDropdown;

    [Header("Colores de alerta")]
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorAlerta = new Color(1f, 0.75f, 0.2f);   // ámbar
    [SerializeField] private Color colorPeligro = new Color(1f, 0.3f, 0.3f);   // rojo

    private readonly float[] velocidades = { 1f, 2f, 4f };
    private int indiceVelocidad = 0;
    private bool corriendo = false;

    private void Start()
    {
        // Opciones del selector de estrategia
        estrategiaDropdown.ClearOptions();
        estrategiaDropdown.AddOptions(new System.Collections.Generic.List<string> { "Aleatoria", "Mejorada" });

        // Eventos de los botones
        iniciarButton.onClick.AddListener(OnIniciar);
        pausarButton.onClick.AddListener(OnPausar);
        velocidadButton.onClick.AddListener(OnCambiarVelocidad);
        estrategiaDropdown.onValueChanged.AddListener(OnCambiarEstrategia);

        if (danioBar != null)
        {
            danioBar.minValue = 0;
            danioBar.maxValue = MaxDanio;
            danioBar.interactable = false;
        }

        // Estado inicial
        SetTurno(0);
        SetRescatados(0);
        SetPerdidos(0);
        SetDanio(0);
        ActualizarVelocidadLabel();
        ActualizarBotones();
    }

    // ---------- Métodos que llenará el servidor más adelante ----------

    public void SetTurno(int turno)
    {
        turnoText.text = $"Turno {turno}";
    }

    public void SetRescatados(int n)
    {
        rescatadosText.text = $"Rescatados {n}/{MaxRescatados}";
        rescatadosText.color = colorNormal;
    }

    public void SetPerdidos(int n)
    {
        perdidosText.text = $"Perdidos {n}/{MaxPerdidos}";
        perdidosText.color = n >= MaxPerdidos - 1 ? colorPeligro : (n > 0 ? colorAlerta : colorNormal);
    }

    public void SetDanio(int n)
    {
        danioText.text = $"Daño al casco {n}/{MaxDanio}";
        float pct = (float)n / MaxDanio;
        danioText.color = pct >= 0.75f ? colorPeligro : (pct >= 0.5f ? colorAlerta : colorNormal);
        if (danioBar != null) danioBar.value = n;
    }

    // ---------- Controles (por ahora solo cambian la UI) ----------

    private void OnIniciar()
    {
        corriendo = true;
        ActualizarBotones();
        Debug.Log("[HUD] Iniciar simulación");
    }

    private void OnPausar()
    {
        corriendo = false;
        ActualizarBotones();
        Debug.Log("[HUD] Pausar simulación");
    }

    private void OnCambiarVelocidad()
    {
        indiceVelocidad = (indiceVelocidad + 1) % velocidades.Length;
        ActualizarVelocidadLabel();
        Debug.Log($"[HUD] Velocidad x{velocidades[indiceVelocidad]}");
    }

    private void OnCambiarEstrategia(int indice)
    {
        Debug.Log($"[HUD] Estrategia: {estrategiaDropdown.options[indice].text}");
    }

    private void ActualizarVelocidadLabel()
    {
        if (velocidadLabel != null) velocidadLabel.text = $"x{velocidades[indiceVelocidad]}";
    }

    private void ActualizarBotones()
    {
        // Solo un botón activo a la vez; la estrategia se bloquea mientras corre
        iniciarButton.interactable = !corriendo;
        pausarButton.interactable = corriendo;
        estrategiaDropdown.interactable = !corriendo;
    }

    // ---------- Solo para la demo: valores de prueba con teclas ----------
#if UNITY_EDITOR
    private int demoTurno, demoRescatados, demoPerdidos, demoDanio;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) SetTurno(++demoTurno);
        if (Input.GetKeyDown(KeyCode.R)) SetRescatados(demoRescatados = Mathf.Min(demoRescatados + 1, MaxRescatados));
        if (Input.GetKeyDown(KeyCode.P)) SetPerdidos(demoPerdidos = Mathf.Min(demoPerdidos + 1, MaxPerdidos));
        if (Input.GetKeyDown(KeyCode.D)) SetDanio(demoDanio = Mathf.Min(demoDanio + 2, MaxDanio));
    }
#endif
}
