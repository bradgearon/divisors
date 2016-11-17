using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private ScreenFader _screenFader;
    public Level SelectedLevel { get; private set; }

    public static GameManager Instance { get; private set; }

    public Level[] Levels = new Level[12];

    void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _screenFader = GetComponent<ScreenFader>();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickLevel()
    {
        var clicked = EventSystem.current.currentSelectedGameObject;
        var level = clicked.name.ToLower().Split(
            new[] {"level"}, StringSplitOptions.None)[1];

        var selected = int.Parse(level);
        SelectedLevel = Levels[selected - 1];

        DontDestroyOnLoad(this);
        Debug.Log("selected level: " + selected);
        _screenFader.EndScene("game");
    }

    public void OnClickPlay()
    {
        DontDestroyOnLoad(this);
        _screenFader.EndScene("game");
    }

    public void OnClickTitle()
    {
        DontDestroyOnLoad(this);
        _screenFader.EndScene("title");
    }

    public void OnGameOver()
    {
        DontDestroyOnLoad(this);
        _screenFader.EndScene("gameover");
    }
}