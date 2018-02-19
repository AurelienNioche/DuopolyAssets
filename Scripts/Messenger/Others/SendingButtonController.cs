using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class SendingButtonController : MonoBehaviour {

	UIControllerMessenger uiController;

	// Use this for initialization
	void Start () {

		// Get button component and connect it
		Button btn = GetComponent<Button> ();
		btn.onClick.AddListener (TaskOnClick);

		// Get UIController
		uiController = GameObject.FindGameObjectWithTag ("MessengerController").GetComponent<UIControllerMessenger> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void TaskOnClick () {
		// Send to UIController position
		uiController.ClickOnSend ();
	}
}
