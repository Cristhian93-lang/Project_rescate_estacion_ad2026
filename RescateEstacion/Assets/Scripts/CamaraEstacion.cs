using UnityEngine;

/// <summary>
/// Cámara de vista superior para recorrer la estación.
///   - Mover: WASD o flechas, o arrastrar con el botón derecho del mouse.
///   - Zoom: rueda del mouse (o + y -).
///   - R: regresar a la vista inicial.
/// Va en la Main Camera, que debe estar en modo Orthographic.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CamaraEstacion : MonoBehaviour
{
    public float velocidad = 6f;
    public float velocidadZoom = 2f;
    public float zoomMinimo = 2f;
    public float zoomMaximo = 8f;

    [Header("Límites del recorrido (en casillas)")]
    public Vector2 limiteX = new Vector2(-1f, 10f);
    public Vector2 limiteZ = new Vector2(-8f, 1f);

    private Camera camara;
    private Vector3 posicionInicial;
    private float zoomInicial;

    void Start()
    {
        camara = GetComponent<Camera>();
        camara.orthographic = true;
        posicionInicial = transform.position;
        zoomInicial = camara.orthographicSize;
    }

    // LateUpdate: la cámara se mueve después de todo lo demás (Lab 1-2).
    void LateUpdate()
    {
        // Mover con teclado. Se usa unscaledDeltaTime para que funcione aunque la simulación esté en pausa.
        Vector3 mov = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Vector3 posicion = transform.position + mov.normalized * velocidad * Time.unscaledDeltaTime;

        // Mover arrastrando con el botón derecho.
        if (Input.GetMouseButton(1))
        {
            float escala = camara.orthographicSize / 5f;
            posicion -= new Vector3(Input.GetAxis("Mouse X"), 0f, Input.GetAxis("Mouse Y")) * escala;
        }

        posicion.x = Mathf.Clamp(posicion.x, limiteX.x, limiteX.y);
        posicion.z = Mathf.Clamp(posicion.z, limiteZ.x, limiteZ.y);
        transform.position = posicion;

        // Zoom.
        float rueda = Input.mouseScrollDelta.y;
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus)) rueda += 3f * Time.unscaledDeltaTime;
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) rueda -= 3f * Time.unscaledDeltaTime;
        camara.orthographicSize = Mathf.Clamp(camara.orthographicSize - rueda * velocidadZoom * 0.5f, zoomMinimo, zoomMaximo);

        // Regresar a la vista inicial.
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = posicionInicial;
            camara.orthographicSize = zoomInicial;
        }
    }
}
