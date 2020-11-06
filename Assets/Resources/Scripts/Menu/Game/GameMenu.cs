using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenu : MonoBehaviour
{
    private const float CAMERA_DISTANCE = 35f;

    public Transform player;

    private PlayerBehavior playerBehavior;
    private CharacterController cntrl;

    public bool Open { get; private set; } = false;

    private Vector3 oldPosition;
    private float oldFarClipPlane;

    void Start()
    {
        cntrl = player.GetComponent<CharacterController>();
        playerBehavior = player.GetComponent<PlayerBehavior>();

        LockMouse(true);

        gameObject.SetActive(Open);
    }

    public void OnMenuToggle(InputAction.CallbackContext context)
    {
        if (context.canceled)
            Toggle();
    }

    public void Toggle()
    {
        Open = !Open;

        playerBehavior?.Enable(!Open);

        gameObject.SetActive(Open);

        if (Open)
        {
            oldPosition = cntrl.center;
            oldFarClipPlane = Camera.main.farClipPlane;

            StartCoroutine(Move());
        }
        else
        {
            cntrl.Move(oldPosition - new Vector3(0, 200, 0));
        }

        Camera.main.farClipPlane = Open ? CAMERA_DISTANCE : oldFarClipPlane;
    }

    private IEnumerator Move()
    {
        yield return new WaitForFixedUpdate();
        cntrl.Move(new Vector3(oldPosition.x, 200, oldPosition.z));

        yield return new WaitForFixedUpdate();

#if UNITY_EDITOR
        Quaternion rot = Camera.main.transform.rotation;
#else
        Quaternion rot = GvrVRHelpers.GetHeadRotation();
#endif

        Vector3 dir = rot * Vector3.forward;

        transform.position = player.position + dir * 20;
        transform.rotation = rot;
    }

    private void LockMouse(bool b)
    {
        Cursor.visible = !b;
        Cursor.lockState = b ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
