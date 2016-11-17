using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RandomImage : MonoBehaviour
{
    public Sprite[] Images;
    public Color[] Background;
    
    private Image image;

    // Use this for initialization
	void Start ()
	{
	    image = GetComponent<Image>();

	    var imageIndex = Random.Range(0, Images.Length);
	    image.sprite = Images[imageIndex];

	    if (Background.Length == 0)
	    {
	        return;
	    }

        var background = image.transform.parent.GetComponent<Image>();
        if (background == null)
        {
            return;
        }

        background.color = Background[imageIndex];
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
