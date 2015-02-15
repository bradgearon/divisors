using DG.Tweening;
using UnityEngine;
using System.Collections;

public class DotTweenInit : MonoBehaviour {
	
    

    // Use this for initialization
	void Start ()
	{
	    DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
