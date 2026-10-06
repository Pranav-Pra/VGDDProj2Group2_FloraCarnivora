using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookLimited : MonoBehaviour
{
    public float maxYaw;
    public float maxPitch;
    public float smoothing;

    Quaternion baseRotation;
    float yaw, pitch;

    void Start()
    {
        baseRotation = transform.localRotation; // whatever you set in the scene is the "center" view
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || !Application.isFocused) return;

        // mouse position -> -1..1 from screen center
        Vector2 p = mouse.position.ReadValue();
        float nx = Mathf.Clamp(p.x / Screen.width * 2f - 1f, -1f, 1f);
        float ny = Mathf.Clamp(p.y / Screen.height * 2f - 1f, -1f, 1f);

        float targetYaw = nx * maxYaw;
        float targetPitch = -ny * maxPitch; // mouse up -> look up

        float k = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        yaw = Mathf.Lerp(yaw, targetYaw, k);
        pitch = Mathf.Lerp(pitch, targetPitch, k);

        // yaw around world up, pitch around the camera's own right axis
        transform.localRotation = Quaternion.Euler(0f, yaw, 0f) * baseRotation * Quaternion.Euler(pitch, 0f, 0f);
    }
}