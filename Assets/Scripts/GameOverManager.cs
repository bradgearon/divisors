using UnityEngine;
using System.Collections;
using I2.Loc.SimpleJSON;
using Newtonsoft.Json;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public Text scoreDisplay;
    public Transform stars;

    public Text resultDisplay;
    public Text captionDisplay;

    public Button nextButton;
    public Button retryButton;

    public string failResultKey;
    public string winResultKey;

    public string failCaptionKey;
    public string winCaptionKey;

    // Use this for initialization
    void Start()
    {
        var score = 0;

        if (GameManager.Instance.Score != 0)
        {
            score = GameManager.Instance.Score;
        } else if (Debug.isDebugBuild)
        {
            score = 0;
        }

        scoreDisplay.text = "" + score;

        var level = GameManager.Instance.SelectedLevel ?? GameManager.Instance.Levels[0];

        if (score > level.HighScore)
        {
            level.HighScore = score;
            var levelsJson = JsonConvert.SerializeObject(GameManager.Instance.Levels);
            Debug.Log(levelsJson);
            PlayerPrefs.SetString("levels", levelsJson);
            PlayerPrefs.Save();
        }

        var resultLevel = new Level()
        {
            HighScore = score
        };

        StartCoroutine(ShowResults(stars, resultLevel));
    }

    IEnumerator ShowResults(Transform starTransform, Level level)
    {
        yield return new WaitForEndOfFrame();
        yield return GameManager.Instance.SetStars(starTransform, level, true);

        var didWin = level.HighScore >= GameManager.Instance.minWin;

        var winCaption = I2.Loc.ScriptLocalization.Get(winCaptionKey);
        var failCaption = I2.Loc.ScriptLocalization.Get(failCaptionKey);

        var winResult = I2.Loc.ScriptLocalization.Get(winResultKey);
        var failResult = I2.Loc.ScriptLocalization.Get(failResultKey);

        var result = didWin ? winResult : failResult;
        var caption = didWin ? winCaption : failCaption;

        resultDisplay.text = result;

        captionDisplay.transform.parent.gameObject.SetActive(true);
        captionDisplay.text = caption;

        yield return new WaitForSeconds(1f);

        if (Random.Range(0f, 1f) < .77f)
        {
            GameManager.Instance.ShowAd();
        }

        nextButton.gameObject.SetActive(didWin);
        nextButton.onClick.AddListener(() =>
        {
            var levelIndex = GameManager.Instance.LevelIndex + 1;
            if (levelIndex >= GameManager.Instance.Levels.Length)
            {
                GameManager.Instance.OnClickTitle();
                return;
            }

            GameManager.Instance.SetLevel(levelIndex);
            GameManager.Instance.LoadLevel();
        });

        if (!didWin)
        {
            retryButton.transform.localPosition =
                new Vector3(0, retryButton.transform.localPosition.y);
        }

        retryButton.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
