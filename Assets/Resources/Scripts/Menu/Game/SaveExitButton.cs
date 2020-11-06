using UnityEngine.SceneManagement;

public class SaveExitButton : ButtonColliderClick
{

    public TerrainGenerator terrainGenerator;

    public override void OnClick()
    {
        terrainGenerator.GetComponent<TerrainPersistor>().Save();

        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("World");
    }
}
