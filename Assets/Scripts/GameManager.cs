using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private ScreenFader _screenFader;

	// Use this for initialization
	void Start ()
	{
	    _screenFader = GetComponent<ScreenFader>();
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void OnClickPlay()
    {
        _screenFader.EndScene("game");
    }
}
