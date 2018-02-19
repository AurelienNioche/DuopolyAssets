using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasUIControllerSi : MonoBehaviour {

	UIControllerSi uiController; 

	// Use this for initialization
	void Start () {
		uiController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<UIControllerSi> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void EndAnimationQuitScene () {
		uiController.EndAnimationQuitScene ();
	}
}
