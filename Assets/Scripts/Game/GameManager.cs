using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Win,
        Loss,
        /*
            |  ||
            || |_
        */
        Paused
    }

    public GameData gameData;

    public static GameManager Instance; // Singleton pattern

    public GameState currentState = GameState.Playing; // by default the game is not paused
    private IGamemode gameMode;
    FullScreenMessage fullScreenMessage;

    void Start()
    {
        // Make sure there is only one GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep the GameManager object between scenes
        }
        else
        {
            Destroy(gameObject);
        }

        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            currentState = GameState.MainMenu;
        }

        // temp? set default settings
        PlayerPrefs.SetInt("BulletCollision", 1); // 1=on, 0=off
        PlayerPrefs.SetInt("DynamicReloading", 1); // 1=on, 0=off
        PlayerPrefs.Save();

        SceneManager.sceneLoaded += OnSceneLoaded;

        FindObjects();
    }

    private void Update()
    {
        if (gameMode != null)
        {
            if (gameMode.WinConditionMet() && currentState == GameState.Playing)
            {
                GameWon();
            }

            if (gameMode.LossConditionMet() && currentState == GameState.Playing)
            {
                GameLost();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && (currentState == GameState.Win || currentState == GameState.Loss))
        {
            StopAllCoroutines();
            QuitToMenu();
        }
        if (Input.GetKeyDown(KeyCode.R) && (currentState == GameState.Win || currentState == GameState.Loss))
        {
            StopAllCoroutines();
            RestartScene();
        }
    }

    private void FindObjects()
    {
        gameMode = FindObjectOfType<BaseGamemode>();

        if (gameMode == null)
        {
            Debug.Log("No gamemode found");
        }

        GameObject fullScreenMessageTransform = GameObject.Find("FullScreenMessage");

        if (fullScreenMessageTransform)
        {
            fullScreenMessage = fullScreenMessageTransform.GetComponent<FullScreenMessage>();
        }
        else
        {
            Debug.Log("No FullScreenMessage found in scene");
        }
    }

    public void StartGame(string scene)
    {
        currentState = GameState.Playing;
        Cursor.visible = false;
        SceneManager.LoadScene(scene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentState == GameState.Playing)
        {
            FindObjects();
        }
    }

    public void RestartScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        StartGame(currentSceneName);
    }

    public void QuitToMenu()
    {
        Cursor.visible = true;
        currentState = GameState.MainMenu;
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        Cursor.visible = true;
        currentState = GameState.Paused;
    }

    public void ResumeGame()
    {
        Cursor.visible = false;
        currentState = GameState.Playing;
    }

    public void GameLost()
    {
        Cursor.visible = true;
        currentState = GameState.Loss;
        StartCoroutine(GameLostCoroutine(7f));
    }

    private IEnumerator GameLostCoroutine(float time)
    {
        fullScreenMessage.SetMessage("Game Over", Color.red);
        fullScreenMessage.DisplayMessage(2f, 5f, 5f);
        yield return new WaitForSeconds(time);
        QuitToMenu();
    }

    public void GameWon()
    {
        Cursor.visible = true;
        currentState = GameState.Win;
        StartCoroutine(GameWonCoroutine(7f));
    }

    private IEnumerator GameWonCoroutine(float time)
    {
        fullScreenMessage.SetMessage("Victory!", Color.green);
        fullScreenMessage.DisplayMessage(2f, 5f, 5f);
        yield return new WaitForSeconds(time);
        QuitToMenu();
    }

}
