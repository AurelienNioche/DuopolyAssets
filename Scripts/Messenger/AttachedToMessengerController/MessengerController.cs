using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class MessengerController: MonoBehaviour {

	public bool debug = false;

	Ear ear;
	Mouth mouth;
	UIControllerMessenger uiController;

	bool isOccupied;

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake () {
		ear = GetComponent<Ear> ();
		mouth = GetComponent<Mouth>();
		uiController = GetComponent<UIControllerMessenger> ();
	}

	void Start () {

		DontDestroyOnLoad (this);

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		isOccupied = false;
	}

	void Update () {

		if (!isOccupied) {

			isOccupied = true;

			ManageInbox ();
			ManageOutbox ();

			isOccupied = false;
		}
	}

	// ---------------------------- Message ----------------------------------- //

	void ManageInbox () {

		if (ear.HeardSomething ()) {
			
			string message = ear.GetMessage ();

			if (debug) {
				Debug.Log ("GameController: I received mail '" + message + "'.");
			}

			uiController.DisplayMessage (message);
		}

	}
		
	void ManageOutbox () {

		if (!mouth.IsOccupied() && uiController.HasSendingDemand ()) {
			
			string toSendMessage = uiController.GetSendingDemand ();

			if (debug) {
				Debug.Log ("GameController: I will send: '" + toSendMessage + "'.");
			}
				
			StartCoroutine (mouth.AskServer (toSendMessage, "client_speaks"));
		}

	}
}
