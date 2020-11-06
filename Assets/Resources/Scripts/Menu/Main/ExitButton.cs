using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : ButtonColliderClick
{
    public override void OnClick() => Application.Quit();
}
