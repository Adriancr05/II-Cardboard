using UnityEngine;
using UnityEngine.InputSystem;

public class EditorLook : MonoBehaviour
{
#if UNITY_EDITOR
    private float sensitivity = 0.15f;
    private float yaw;
    private float pitch;

    [Header("Punto central (solo editor)")]
    public float dotSize = 8f;
    public Color dotColor = Color.white;
    public Color outlineColor = Color.black;

    private void Start()
    {
        Vector3 e = transform.localEulerAngles;
        yaw = e.y;
        pitch = Mathf.DeltaAngle(0f, e.x);
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        // Mantén el botón derecho para girar la cabeza
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            yaw += delta.x * sensitivity;
            pitch -= delta.y * sensitivity;
            pitch = Mathf.Clamp(pitch, -85f, 85f);
            transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    // Dibuja un punto en el centro de la pantalla (Game view)
    private void OnGUI()
    {
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;

        // Borde oscuro para que se vea sobre cualquier fondo
        float o = dotSize + 4f;
        GUI.color = outlineColor;
        GUI.DrawTexture(new Rect(cx - o * 0.5f, cy - o * 0.5f, o, o), Texture2D.whiteTexture);

        GUI.color = dotColor;
        GUI.DrawTexture(new Rect(cx - dotSize * 0.5f, cy - dotSize * 0.5f, dotSize, dotSize),
                        Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
#endif
}
