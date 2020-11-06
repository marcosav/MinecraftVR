using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : ButtonColliderClick
{
    public override void OnClick()
    {
        GetComponentInParent<GameMenu>().Toggle();
    }
}
