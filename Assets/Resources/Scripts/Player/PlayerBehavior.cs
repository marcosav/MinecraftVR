using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    public bool Enabled { get; private set; } = true;

    private PlayerLook look;
    private PlayerMovement mov;
    private Inventory inv;

    void Start()
    {
        //look = GetComponent<PlayerLook>();
        mov = GetComponent<PlayerMovement>();
        inv = GetComponent<Inventory>();

        Enable(false);
    }

    public void Enable(bool e)
    {
        Enabled = e;

        //look.enabled = e;
        mov.enabled = e;
        inv.enabled = e;
    }
}
