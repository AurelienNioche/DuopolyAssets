using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class UIControllerMessenger : MonoBehaviour {

	Button sendingButton;
	InputField inputField;
	ScrollRect scrollRect;
	Text historic;
	bool allowEnter;

	Animator ac;

	Parameters parameters;

	List<string> queueSendingDemand;

	void Awake ()  {

		sendingButton = GameObject.FindGameObjectWithTag ("MessengerSendingButton").GetComponent<Button> ();
		inputField = GameObject.FindGameObjectWithTag ("MessengerInputField").GetComponent<InputField> ();
		scrollRect = GameObject.FindGameObjectWithTag ("MessengerScrollRect").GetComponent<ScrollRect> ();
		historic = GameObject.FindGameObjectWithTag ("MessengerHistoric").GetComponent<Text> ();
		ac = GameObject.FindGameObjectWithTag ("Messenger").GetComponent<Animator> ();

		GetParameters (); 

		queueSendingDemand = new List<string> ();
	}

	// Use this for initialization
	void Start () {
		ac.SetBool ("Visible", true);
	}
	
	// Update is called once per frame
	void Update () {

		if (allowEnter && (inputField.text.Length > 0) && (Input.GetKey (KeyCode.Return) || Input.GetKey (KeyCode.KeypadEnter))) {
		
			ClickOnSend ();
			allowEnter = false;
		
		} else
			allowEnter = inputField.isFocused;
	}

	// ------------------ Get parameters ---------------------------- //

	void GetParameters () {

		GameObject[] gos = GameObject.FindGameObjectsWithTag ("Parameters");
		if (gos.Length == 0) {
			parameters = GameObject.FindGameObjectWithTag ("GameController").GetComponent<Parameters> ();
		} else {
			parameters = GameObject.FindGameObjectWithTag ("Parameters").GetComponent<Parameters> ();
		}
	}

	// ------------------------ Manage events -------------------------- //

	public void ClickOnSend () {
		
		if (inputField.text.Length > 0) {

			sendingButton.enabled = false;
			string message = inputField.text;
			inputField.text = "";
			queueSendingDemand.Add (message);
			historic.text += "\n[You] " + message;
			inputField.ActivateInputField();
			ScrollDown ();
			sendingButton.enabled = true;
		} 
	}

	public void OnEndEdit() {

		if (Input.GetKey (KeyCode.Return)) {
			ClickOnSend ();
		}
	}

	public void DisplayMessage (string message) {

		historic.text += "\n[" + parameters.GetServerName () + "] " + message;
		ScrollDown ();
	}

	void ScrollDown () {

		Canvas.ForceUpdateCanvases();
		scrollRect.verticalNormalizedPosition=0f;
		Canvas.ForceUpdateCanvases();
	}

	// --------------------------------------- Interaction gameController-Server ------------------------------------- // 

	public bool HasSendingDemand () {
		return queueSendingDemand.Count > 0;
	}		

	public string GetSendingDemand () {
		string sendingDemand = queueSendingDemand[0];
		queueSendingDemand.Remove (sendingDemand);
		return sendingDemand;
	}	

	public bool LockEnterKey () {
		return allowEnter;
	}
}
