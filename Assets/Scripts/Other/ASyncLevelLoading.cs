using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ASyncLevelLoading : MonoBehaviour
{
    public Scene mainGameScene;
    private Scene _tutorialGameScene;
    private string _mainGameSceneName;

    // Start is called before the first frame update
    void Start()
    {
        // Come back and add a failsafe to this
        _tutorialGameScene = SceneManager.GetActiveScene();
        _mainGameSceneName = mainGameScene.name;
    }

    public void LoadMainScene()
    {
        SceneManager.LoadSceneAsync(_mainGameSceneName, LoadSceneMode.Additive);
    }

    public void UnLoadMainScene()
    {
        SceneManager.UnloadSceneAsync(mainGameScene);
    }

    public void LoadTutorialScene()
    {
        SceneManager.LoadSceneAsync(_tutorialGameScene.name, LoadSceneMode.Additive);
    }

    public void UnLoadTutorialScene()
    {
        SceneManager.UnloadSceneAsync(_tutorialGameScene);
    }
}
