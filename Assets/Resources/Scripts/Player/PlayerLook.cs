﻿using UnityEngine;

public class PlayerLook : MonoBehaviour
{
/*#if false

    public MinecraftVR controls;

    public float sensitivity = .55f;
    public float inverted = -1f;

    private float headRotationX, headRotationY;
    private readonly float headRotationLimit = 90f;

    void Start()
    {
    }

    protected void Awake() => controls = new MinecraftVR();
    protected void OnEnable() => controls.Enable();
    protected void OnDisable() => controls.Disable();

    void Update()
    {
        Vector2 input = controls.Player.Look.ReadValue<Vector2>();
        float x = input.x * sensitivity;
        float y = input.y * sensitivity * inverted;

        headRotationY += y;
        headRotationX += x;

        headRotationY = Mathf.Clamp(headRotationY, -headRotationLimit, headRotationLimit);
        Camera.main.transform.localEulerAngles = new Vector3(headRotationY, headRotationX, 0f);
    }

#endif*/

    public static Quaternion GetRotation()
    {
#if UNITY_EDITOR
        return Camera.main.transform.rotation;
#else
        return GvrVRHelpers.GetHeadRotation();
#endif
    }
}