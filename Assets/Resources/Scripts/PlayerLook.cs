using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public MinecraftVR controls;

    public Transform cam;

    public float sensitivity = .55f;
    public float inverted = -1f;

    private float headRotation = 0f;
    private readonly float headRotationLimit = 90f;

    void Start() {
        if (Application.platform == RuntimePlatform.Android)
            sensitivity *= 2;
    }

    protected void Awake()
    {
        controls = new MinecraftVR();
    }

    protected void OnEnable()
    {
        controls.Enable();
    }

    protected void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        Vector2 input = controls.Player.Look.ReadValue<Vector2>();
        float x = input.x * sensitivity;
        float y = input.y * sensitivity * inverted;

        transform.Rotate(0f, x, 0f);
        headRotation += y;
        headRotation = Mathf.Clamp(headRotation, -headRotationLimit, headRotationLimit);
        cam.localEulerAngles = new Vector3(headRotation, 0f, 0f);
    }
}
