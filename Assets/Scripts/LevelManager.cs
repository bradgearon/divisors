using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public string starsName = "stars";
    public string lockName = "lock";
    
    public GameObject LevelContainer;
    
	// Use this for initialization
	void Start ()
	{
	    var levelDisplay = LevelContainer.GetComponentsInChildren<Button>();
        Level lastLevel = null;
        
        for (var i = 0; i < GameManager.Instance.Levels.Length; i++)
        {
            var levelButton = levelDisplay[i];
            levelButton.interactable = true;

            var level = GameManager.Instance.Levels[i];

            if (lastLevel != null)
            {
                if (lastLevel.HighScore < GameManager.Instance.minWin)
                {
                    break;
                }
            }

            var levelImage = levelButton.GetComponent<Image>();
	        levelImage.color = Color.white;

	        levelButton.gameObject.SetActive(true);

            if (i > 0)
            { 
                var lockButton = levelButton.transform.Find(lockName);
                lockButton.gameObject.SetActive(false);
            }

            var stars = levelButton.transform.Find(starsName);
            stars.gameObject.SetActive(true);

            StartCoroutine(GameManager.Instance.SetStars(stars, level, false));

            lastLevel = level;
	    }


	}

    // Update is called once per frame
	void Update () {
	
	}
}
