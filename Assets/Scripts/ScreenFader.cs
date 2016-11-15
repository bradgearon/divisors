using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public Image FadeImg;
    public float fadeSpeed = .1f;
    public bool sceneStarting = true;
    
    void Update()
    {
        // If the scene is starting...
        if (sceneStarting)
        {
            // ... call the StartScene function.
            StartScene();
        }
    }


    void FadeTo(Color color)
    {
        FadeImg.color = Color.Lerp(FadeImg.color, color, (Time.deltaTime * 10 * fadeSpeed));
    }

    void StartScene()
    {
        // Fade the texture to clear.
        FadeTo(Color.clear);

        // If the texture is almost clear...
        if (!(FadeImg.color.a <= 0.05f))
        {
            return;
        }
        
        // ... set the colour to clear and disable the image.
        FadeImg.color = Color.clear;
        FadeImg.enabled = false;

        // The scene is no longer starting.
        sceneStarting = false;
    }


    public IEnumerator EndSceneRoutine(string scene)
    {
        // Make sure the image is enabled.
        FadeImg.enabled = true;

        for(;;)
        {
            // Start fading towards black.
            FadeTo(Color.white);

            // If the screen is almost black...
            if (FadeImg.color.a >= 0.9f)
            {
                // ... reload the level
                SceneManager.LoadScene(scene);
                yield break;
            }

            yield return null;
        }
    }

    public void EndScene(string scene)
    {
        sceneStarting = false;
        StartCoroutine(EndSceneRoutine(scene));
    }

}