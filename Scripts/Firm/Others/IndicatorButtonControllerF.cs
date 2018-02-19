using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class IndicatorButtonControllerF : MonoBehaviour {

	public string buttonName;
	public float delayOfInactivationAfterClick = 1.0f;
	public Animator animator;

	UIControllerF uiController;
	Button btn;

	// Use this for initialization
	void Start () {

		// Get button component and connect it
		btn = GetComponent<Button> ();
		try {
			btn.onClick.AddListener (TaskOnClick);
		} catch (NullReferenceException e) {
			Debug.Log ("Error with buttonName '" + buttonName + "'");
			throw e;
		}

		// Get UIController
		uiController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<UIControllerF> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (animator.GetBool("Visible") && btn.interactable && (Input.GetKeyUp (KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))) {
			btn.onClick.Invoke ();
		}
		
	}

	void TaskOnClick () {
		// Send to UIController position
		btn.interactable = false;
		StartCoroutine (Deactivate ());
		uiController.ButtonIndicator (buttonName);
	}

	IEnumerator Deactivate () {
		yield return new WaitForSeconds(delayOfInactivationAfterClick);
		btn.interactable = true;
	}
}
