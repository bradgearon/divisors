using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreDisplay;
    public Text stepDisplay;

    public int scoreMultiplier = 100;

    public int maxSteps = 10;
    public int stepsTaken = 0;
    private int score = 0;

    public static ScoreManager Instance { get; set; }

    // Use this for initialization
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeStep()
    {
        stepsTaken++;
        stepDisplay.text = maxSteps - stepsTaken + string.Empty;
    }

    public void AddScore(int value)
    {
        score += Mathf.RoundToInt(value * scoreMultiplier);
        scoreDisplay.text = score + string.Empty;
    }

}
