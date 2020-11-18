using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

public class AdLauncher : MonoBehaviour {

    public string zoneId = "defaultVideoAndPictureZone";
    
    void Start () {
        Advertisement.Initialize("1234567", true);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ShowAdPlacement()
    {
        if (string.IsNullOrEmpty(zoneId))
        {
            zoneId = null;
        }

        if (false && DateTime.Now < new DateTime(2016, 11, 24))
        {
            return;
        }

        ShowOptions options = new ShowOptions();
        options.resultCallback = HandleShowResult;
        Advertisement.Show(options);
    }

    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("Video completed. Offer a reward to the player.");
                break;
            case ShowResult.Skipped:
                Debug.LogWarning("Video was skipped.");
                break;
            case ShowResult.Failed:
                Debug.LogError("Video failed to show.");
                break;
        }
    }


}
