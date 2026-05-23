using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaidSceneLoader : MonoBehaviour
{
    public static RaidSceneLoader Instance;

    [Header("Scene")]
    public string raidSceneName = "RaidMeadow";

    [Header("Main Scene Objects")]
    public GameObject mainSceneVisualRoot;
    public Camera mainSceneCamera;

    private bool raidSceneLoaded;
    private Scene mainScene;

    private void Awake()
    {
        Instance = this;
        mainScene = gameObject.scene;
    }

    public void OpenRaidScene()
    {
        if (raidSceneLoaded)
            return;

        StartCoroutine(OpenRaidSceneRoutine());
    }

    public void CloseRaidScene()
    {
        if (!raidSceneLoaded)
            return;

        StartCoroutine(CloseRaidSceneRoutine());
    }

    private IEnumerator OpenRaidSceneRoutine()
    {
        if (mainSceneVisualRoot != null)
            mainSceneVisualRoot.SetActive(false);

        if (mainSceneCamera != null)
            mainSceneCamera.gameObject.SetActive(false);

        AsyncOperation operation = SceneManager.LoadSceneAsync(raidSceneName, LoadSceneMode.Additive);

        while (!operation.isDone)
            yield return null;

        raidSceneLoaded = true;

        Scene raidScene = SceneManager.GetSceneByName(raidSceneName);

        if (raidScene.IsValid())
            SceneManager.SetActiveScene(raidScene);
    }

    private IEnumerator CloseRaidSceneRoutine()
    {
        AsyncOperation operation = SceneManager.UnloadSceneAsync(raidSceneName);

        while (operation != null && !operation.isDone)
            yield return null;

        raidSceneLoaded = false;

        if (mainSceneVisualRoot != null)
            mainSceneVisualRoot.SetActive(true);

        if (mainSceneCamera != null)
            mainSceneCamera.gameObject.SetActive(true);

        if (mainScene.IsValid())
            SceneManager.SetActiveScene(mainScene);
    }
}