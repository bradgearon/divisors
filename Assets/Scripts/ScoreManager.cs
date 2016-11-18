using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreDisplay;
    public Text stepDisplay;
    public Text scorePop;

    public int scoreMultiplier = 100;

    public int maxSteps = 10;
    public int stepsTaken = 0;
    private int score = 0;
    private Vector3 initialPosition;

    public static ScoreManager Instance { get; set; }

    // Use this for initialization
    void Start()
    {
        Instance = this;
        initialPosition = scorePop.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeStep()
    {
        stepsTaken++;
        var remaining = maxSteps - stepsTaken;
        stepDisplay.text = remaining + string.Empty;
    }

    public void CheckOver()
    {
        if (maxSteps - stepsTaken < 1)
        {
            GameManager.Instance.OnGameOver(score);
        }
    }

    public void AddScore(int value)
    {
        score += Mathf.RoundToInt(value * scoreMultiplier);
        scoreDisplay.text = score + string.Empty;
    }

    public void Display(int value)
    {
        scorePop.gameObject.SetActive(true);
        scorePop.transform.position = initialPosition;
        scorePop.DOFade(1, 0f);

        scorePop.text = "+" + value * scoreMultiplier + "";

        scorePop.transform.DOMoveY(initialPosition.y + 200, .5f)
            .OnComplete(() =>
            {
                scorePop.DOFade(0f, .5f).OnComplete(() =>
                {
                    scorePop.gameObject.SetActive(false);
                });
            });
    }

}
