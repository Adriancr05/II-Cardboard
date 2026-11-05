using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Transform head;          // arrastra la Main Camera
    public float speed = 2f;
    public float gravity = -9.81f;

    CharacterController cc;
    bool walking, wasPressed;
    float vy;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (head == null && Camera.main != null) head = Camera.main.transform;
    }

    bool IsPressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool p = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        p |= Mouse.current != null && Mouse.current.leftButton.isPressed;
        return p;
#else
        return Input.touchCount > 0 || Input.GetMouseButton(0);
#endif
    }

    void Update()
    {
        bool pressed = IsPressed();
        if (pressed && !wasPressed) walking = !walking;   // toggle con el botón Cardboard
        wasPressed = pressed;

        Vector3 f = head.forward; f.y = 0; f.Normalize();
        Vector3 dir = walking ? f : Vector3.zero;

#if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null)                      // mando Bluetooth
        {
            Vector2 s = Gamepad.current.leftStick.ReadValue();
            if (s.sqrMagnitude > 0.04f)
            {
                Vector3 r = head.right; r.y = 0; r.Normalize();
                dir = f * s.y + r * s.x;
            }
        }
#endif
        if (cc.isGrounded) vy = -1f; else vy += gravity * Time.deltaTime;
        cc.Move((dir * speed + Vector3.up * vy) * Time.deltaTime);
    }
}
