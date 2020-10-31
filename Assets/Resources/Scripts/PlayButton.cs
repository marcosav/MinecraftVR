using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : ButtonColliderClick
{
    public override void OnClick()
    {
        SceneManager.LoadScene("World");
        SceneManager.UnloadSceneAsync("MainMenu");
    }
}
