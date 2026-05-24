using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    public string gameSceneName = "SampleScene";
    public float sceneLoadClickDelay = 0.12f;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject authorsPanel;

    [Header("Buttons")]
    public Button PlayBut;
    public Button NewGameBut;
    public Button AuthorsBut;
    public Button OptionBut;

    private void Awake()
    {
        FindMissingReferences();
    }

    private void Start()
    {
        BindButtons();
        RefreshNewGameButton();
        ShowMainMenu();
    }

    public void PlayGame()
    {
        PlayClick();
        StartCoroutine(LoadGameSceneAfterClick());
    }

    public void NewGame()
    {
        PlayClick();
        SaveManager.DeleteSave();
        RefreshNewGameButton();
    }

    public void ShowOptions()
    {
        PlayClick();
        ShowPanel(optionsPanel);
    }

    public void ShowAuthors()
    {
        PlayClick();
        ShowPanel(authorsPanel);
    }

    public void ShowMainMenu()
    {
        ShowPanel(mainMenuPanel);
    }

    private void BindButtons()
    {
        if (PlayBut != null)
        {
            PlayBut.onClick.RemoveListener(PlayGame);
            PlayBut.onClick.AddListener(PlayGame);
        }

        if (NewGameBut != null)
        {
            NewGameBut.onClick.RemoveListener(NewGame);
            NewGameBut.onClick.AddListener(NewGame);
        }

        if (OptionBut != null)
        {
            OptionBut.onClick.RemoveListener(ShowOptions);
            OptionBut.onClick.AddListener(ShowOptions);
        }

        if (AuthorsBut != null)
        {
            AuthorsBut.onClick.RemoveListener(ShowAuthors);
            AuthorsBut.onClick.AddListener(ShowAuthors);
        }

        BindCancelButtons();
    }

    private void RefreshNewGameButton()
    {
        if (NewGameBut == null)
            return;

        NewGameBut.gameObject.SetActive(SaveManager.HasSave());
    }

    private void BindCancelButtons()
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform == null)
                continue;

            if (transform.name != "Cansel_Button")
                continue;

            if (!transform.gameObject.scene.IsValid())
                continue;

            Button cancelButton = transform.GetComponent<Button>();

            if (cancelButton == null)
                continue;

            cancelButton.onClick.RemoveListener(ReturnToMainMenu);
            cancelButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void ReturnToMainMenu()
    {
        PlayClick();
        ShowMainMenu();
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(panelToShow == mainMenuPanel);

        if (optionsPanel != null)
            optionsPanel.SetActive(panelToShow == optionsPanel);

        if (authorsPanel != null)
            authorsPanel.SetActive(panelToShow == authorsPanel);
    }

    private void FindMissingReferences()
    {
        if (mainMenuPanel == null)
            mainMenuPanel = FindSceneObject("MainMenu_panel");

        if (optionsPanel == null)
            optionsPanel = FindSceneObject("Options_panel");

        if (authorsPanel == null)
            authorsPanel = FindSceneObject("Authors_panel");

        if (PlayBut == null)
            PlayBut = FindButton("Play_but");

        if (NewGameBut == null)
            NewGameBut = FindButton("New_game_but");

        if (OptionBut == null)
            OptionBut = FindButton("options_but");

        if (AuthorsBut == null)
            AuthorsBut = FindButton("Authors_but");
    }

    private Button FindButton(string objectName)
    {
        GameObject found = FindSceneObject(objectName);
        return found != null ? found.GetComponent<Button>() : null;
    }

    private GameObject FindSceneObject(string objectName)
    {
        GameObject found = GameObject.Find(objectName);

        if (found != null)
            return found;

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform == null)
                continue;

            if (transform.name != objectName)
                continue;

            if (!transform.gameObject.scene.IsValid())
                continue;

            return transform.gameObject;
        }

        return null;
    }

    private void PlayClick()
    {
        if (AudioManager.Instanse != null)
            AudioManager.Instanse.PlayClick();
    }

    private IEnumerator LoadGameSceneAfterClick()
    {
        if (sceneLoadClickDelay > 0f && AudioManager.Instanse != null)
            yield return new WaitForSeconds(sceneLoadClickDelay);

        SceneManager.LoadScene(gameSceneName);
    }
}
