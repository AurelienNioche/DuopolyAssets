using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class ButtonConnectedToEnter : MonoBehaviour {

	Button button;
	UIControllerMessenger uiMessenger;

	string tagGameControllerMessenger = "MessengerController";

	// Use this for initialization
	void Start () {
		button = GetComponent<Button> ();
		GameObject go = GameObject.FindGameObjectWithTag (tagGameControllerMessenger);
		if (go != null) {
			uiMessenger = go.GetComponent<UIControllerMessenger> ();
		}
	}

	// Update is called once per frame
	void Update () {
		if (button.interactable && (Input.GetKeyUp (KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))) {

			if (uiMessenger != null && uiMessenger.LockEnterKey()) {
				// Pass
			} else {
				button.onClick.Invoke ();
			}
		}
	}
}
