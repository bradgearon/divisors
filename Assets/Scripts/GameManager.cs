using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private ScreenFader _screenFader;

    public static GameManager Instance { get; private set; }

	// Use this for initialization
	void Start ()
	{
        Instance = this;
	    _screenFader = GetComponent<ScreenFader>();
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void OnClickPlay()
    {
        _screenFader.EndScene("game");
    }

    public void OnClickTitle()
    {
        _screenFader.EndScene("title");
    }

    public void OnGameOver()
    {
        _screenFader.EndScene("gameover");
    }
}
