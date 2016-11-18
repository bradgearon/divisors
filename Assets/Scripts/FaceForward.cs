using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class FaceForward : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        StartCoroutine(showImage());
    }

    private IEnumerator showImage()
    {
        var image = GetComponent<Image>();
        yield return new WaitForSeconds(.15f);
        yield return image.transform.DORotate(Vector3.zero, .25f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
