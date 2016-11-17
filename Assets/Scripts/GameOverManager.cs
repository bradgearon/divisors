using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public Text scoreDisplay;

	// Use this for initialization
	void Start ()
	{
	    scoreDisplay.text = "" + GameManager.Instance.Score;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
