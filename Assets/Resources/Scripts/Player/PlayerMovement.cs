using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class PlayerMovement : MonoBehaviour
{

    public MinecraftVR controls;

    public float jumpSpeed = 5.3f;
    public float checkRadius = .2f;
    public float sprintMultiplier = 1.3f;
    public float speed = 4.5f;

    public Transform groundChecker;
    public LayerMask groundLayer;

    private float g = 9.81f;

    private CharacterController cntrl;

    private float directionY;

    void Start()
    {
        cntrl = GetComponent<CharacterController>();
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
        Vector2 input = controls.Player.Move.ReadValue<Vector2>();
        float x = input.x;
        float z = input.y;

        bool sprint = controls.Player.Sprint.ReadValue<float>() == 1;
        bool jump = controls.Player.Jump.ReadValue<float>() == 1;

#if UNITY_EDITOR
        Vector3 dir = Camera.main.transform.right * x + Camera.main.transform.forward * z;
#else
        var c = GvrVRHelpers.GetHeadRotation();
        Vector3 dir = c * Vector3.right * x + c * Vector3.forward * z;
#endif

        bool grounded = IsOnGround();

        if (!grounded)
            dir *= .7f;

        float actualSpeed = speed;
        if (sprint)
            actualSpeed *= sprintMultiplier;

        dir *= actualSpeed;

        if (jump && grounded)
            directionY = jumpSpeed;

        if (!grounded)
            directionY -= g * 1.4f * Time.deltaTime;

        else if (directionY < 0)
            directionY = -g / 2;

        dir.y = directionY;

        cntrl.Move(dir * Time.deltaTime);
    }

    bool IsOnGround()
    {
        Collider[] colliders = Physics.OverlapSphere(groundChecker.position, checkRadius, groundLayer);
        return colliders.Length > 0;
    }
}
