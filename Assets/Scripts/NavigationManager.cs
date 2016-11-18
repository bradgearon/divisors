using UnityEngine;
using System.Collections;

public class NavigationManager : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        if (GameManager.Instance == null)
        {
            var prefab = Resources.Load("GameManager");
            Instantiate(prefab);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    new void SendMessage(string message)
    {
        GameManager.Instance.SendMessage(message);
    }
}
