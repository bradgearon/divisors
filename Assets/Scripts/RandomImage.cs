using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RandomImage : MonoBehaviour
{
    public Sprite[] Images;

    private Image image;

    // Use this for initialization
	void Start ()
	{
	    image = GetComponent<Image>();
	    var imageIndex = Random.Range(0, Images.Length);
	    image.sprite = Images[imageIndex];
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
