using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasUIControllerM : MonoBehaviour {

	UIControllerM uiController; 

	// Use this for initialization
	void Start () {
		uiController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<UIControllerM> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void EndAnimationQuitScene () {
		uiController.EndAnimationQuitScene ();
	}
}
