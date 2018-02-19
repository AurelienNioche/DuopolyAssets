using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoresControllerF : MonoBehaviour {

	UIControllerF uiController;

	// Use this for initialization
	void Start () {
		
		// Get components
		uiController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<UIControllerF> ();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void UpdateScore () {
		uiController.UpdateScore ();
	}

	void EndAddingScore () {
		uiController.EndAddingScore ();
	}
}
