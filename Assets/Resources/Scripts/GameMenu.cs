using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public CanvasGroup cg;
    public Transform player;

    private PlayerBehavior playerBehavior;

    public bool Open { get; private set; } = false;

    void Start()
    {
        playerBehavior = GetComponent<PlayerBehavior>();
        LockMouse(true);
    }

    public void OnMenuToggle(InputAction.CallbackContext context)
    {
        /*if (context.canceled)
        {*/
            LockMouse(Open);
            Open = !Open;
            Time.timeScale = Open ? 0f : 1f;
            cg.alpha = Open ? 1f : 0f;
            cg.interactable = Open;
            playerBehavior.Enable(false);
        //}
    }

    private void LockMouse(bool b)
    {
        Cursor.visible = !b;
        Cursor.lockState = b ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("World");
    }
}
