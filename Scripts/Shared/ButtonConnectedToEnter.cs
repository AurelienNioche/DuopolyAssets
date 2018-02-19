using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class ButtonConnectedToEnter : MonoBehaviour {

	Button button;

	// Use this for initialization
	void Start () {
		button = GetComponent<Button> ();
	}

	// Update is called once per frame
	void Update () {
		if (button.interactable && (Input.GetKeyUp (KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))) {
			button.onClick.Invoke ();
		}
	}
}
