using System;
using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using Facebook.Unity;
using I2.Loc;

public class GameManager : MonoBehaviour
{
    private ScreenFader _screenFader;
    public Level SelectedLevel { get; private set; }
    public bool EasyMode = false;

    public string facebookAppLinkUrl = "https://fb.me/1279421248782463";
    public string facebookPreviewImage = "https://github.com/bradgearon/divisors/raw/master/Assets/Textures/images/app-icon-512.png";

    public string leftStarName = "left-star";
    public string rightStarName = "right-star";
    public string starName = "star";

    public Color completeColor;

    public int minWin = 2500;
    public int midWin = 5000;
    public int maxWin = 10000;

    public Material glowMaterial;

    public int Score;

    public static GameManager Instance { get; private set; }

    public Level[] Levels = new Level[12];
    public int LevelIndex = 0;

    

    void Awake()
    {

        Debug.Log("on awake:" + Application.systemLanguage);
        Debug.Log("current I2 Language: " + LocalizationManager.CurrentLanguage);
        Instance = this;
        _screenFader = GetComponent<ScreenFader>();

        var levelsJson = PlayerPrefs.GetString("levels");
        if (!string.IsNullOrEmpty(levelsJson))
        {
            Debug.Log(levelsJson);
            var levels = JsonConvert.DeserializeObject<Level[]>(levelsJson);
            for (var i = 0; i < levels.Length; i++)
            {
                Levels[i].HighScore = levels[i].HighScore;
            }
        }


#if !UNITY_EDITOR
        UntitledLauncher.Init();
#endif

        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            FB.Init(FB.ActivateApp);
        }

    }

    void Start()
    {
        OnTrackAppNext();
    }

    public void SetLevel(int level)
    {
        LevelIndex = level;
        SelectedLevel = Levels[level];
    }

    public IEnumerator SetStars(Transform stars, Level level, bool setGlow)
    {
        var leftStar = stars.FindChild(leftStarName).GetComponent<Image>();
        var rightStar = stars.FindChild(rightStarName).GetComponent<Image>();
        var star = stars.FindChild(starName).GetComponent<Image>();

        if (level.HighScore > 2500)
        {
            leftStar.color = completeColor;
            if (setGlow)
            {
                leftStar.material = glowMaterial;
            }
        }

        if (level.HighScore > 5000)
        {
            rightStar.color = completeColor;
            if (setGlow)
            {
                rightStar.material = glowMaterial;
            }
        }

        if (level.HighScore > 10000)
        {
            star.color = completeColor;
            if (setGlow)
            {
                star.material = glowMaterial;
            }
        }

        if (!setGlow)
        {
            yield return null;
        }

        yield return DOTween.To(
            () => leftStar.fillAmount,
            amount => leftStar.fillAmount = amount, 1f, 1f)
            .WaitForCompletion();

        yield return DOTween.To(
            () => rightStar.fillAmount,
            amount => rightStar.fillAmount = amount, 1f, 1f)
            .WaitForCompletion();

        yield return DOTween.To(
            () => star.fillAmount,
            amount => star.fillAmount = amount, 1f, 1f)
            .WaitForCompletion();
    }

    

    public void OnClickLevel()
    {
        var clicked = EventSystem.current.currentSelectedGameObject;
        var level = clicked.name.ToLower().Split(
            new[] { "level" }, StringSplitOptions.None)[1];

        var selected = int.Parse(level);
        SetLevel(selected - 1);

        LoadLevel();
    }

    public void LoadLevel()
    {
        DontDestroyOnLoad(this);
        _screenFader.EndScene("game");
    }

    public void OnClickPlay()
    {
        DontDestroyOnLoad(this);
        _screenFader.EndScene("levels");
    }

    public void OnClickEasy()
    {
        EasyMode = true;
        OnClickPlay();
    }

    public void OnClickTitle()
    {
        DontDestroyOnLoad(this);
        _screenFader.EndScene("title");
    }

    public void OnGameOver(int score)
    {
        Score = score;
        DontDestroyOnLoad(this);
        _screenFader.EndScene("gameover");
    }

    public void OnClickHelp()
    {
        DontDestroyOnLoad(this);
        _screenFader.EndScene("help");
    }

    public void OnClickShare()
    {
        FB.Mobile.AppInvite(
            new Uri(facebookAppLinkUrl),
            new Uri(facebookPreviewImage),
            (result) => { }
        );
    }

    public void OnTrackAppNext()
    {
        var tracking = GetComponent<AppNextTracking>();
        tracking.Track();
    }
}