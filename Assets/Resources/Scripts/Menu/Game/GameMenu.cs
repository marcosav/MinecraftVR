using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenu : MonoBehaviour
{
    private const float CAMERA_DISTANCE = 35f;

    public Transform player;

    private PlayerBehavior playerBehavior;

    public bool Open { get; private set; } = false;

    private float oldFarClipPlane;

    void Start()
    {
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
        if (!Open && !playerBehavior.Enabled)
            return;

        Open = !Open;

        if (Open)
        {
            gameObject.SetActive(true);
            playerBehavior.Enable(false);

            oldFarClipPlane = Camera.main.farClipPlane;

            StartCoroutine(Move());
        }
        else
            StartCoroutine(Restore());

        Camera.main.farClipPlane = Open ? CAMERA_DISTANCE : oldFarClipPlane;
    }

    private IEnumerator Move()
    {
        player.position += new Vector3(0, 250, 0);

        yield return new WaitForFixedUpdate();

        Quaternion rot = PlayerLook.GetRotation();
        Vector3 dir = rot * Vector3.forward;

        transform.position = player.position + dir * 20;
        transform.rotation = rot;
    }

    private IEnumerator Restore()
    {
        player.position -= new Vector3(0, 250, 0);

        yield return new WaitForFixedUpdate();

        playerBehavior?.Enable(true);
        gameObject.SetActive(false);
    }

    private void LockMouse(bool b)
    {
        Cursor.visible = !b;
        Cursor.lockState = b ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
