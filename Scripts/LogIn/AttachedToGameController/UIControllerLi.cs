using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class UIControllerLi : MonoBehaviour {
	
	Dictionary <string, Button> buttons;
	Dictionary <string, InputField> inputFields;
	Dictionary <string, Text> texts;
	Dictionary <string, Animator> animators;

	GameControllerLi gameController;

	bool gotUserIdentification;

	bool allowEnter;
	bool waitForRetry;

	Dictionary<string, UnityAction> buttonAssociations;

	string [] inputFieldsNames = {
		"UserName",
		"Password"
	};

	string [] textsNames = {
		"Central"
	};

	string [] animatorNames = {
		"ButtonRetry",
		"ButtonHome",
		"ConnectionMenu",
		"ButtonHome",
		"TextCentral",
		"Title"

	};


	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake() {

		buttonAssociations = new Dictionary<string, UnityAction> () {

			{"ButtonRetry", ButtonRetry},
			{"ButtonIdentification", ButtonIdentification},
			{"ButtonHome", ButtonHome},
		};

		gameController = GetComponent<GameControllerLi> ();

		GetAnimators ();
		GetInputFields ();
		GetPushButtons ();
		GetTexts ();
	}

	void Start () {
		
		gotUserIdentification = false;
		waitForRetry = false;

		IdentificationMenu ();

		animators ["ButtonHome"].SetBool ("Visible", true);
		animators ["Title"].SetBool ("Visible", true);

	}

	void Update () {

		// Use tab to navigate between input fields
		bool allowTab = inputFields["UserName"].isFocused || inputFields["Password"].isFocused;

		if (allowTab && Input.GetKeyUp (KeyCode.Tab)) {
			if (inputFields ["UserName"].isFocused) {
				// inputField ["UserName"].Select ();
				inputFields ["Password"].ActivateInputField ();
			} else {
				// inputField ["UserName"].Select ();
				inputFields ["UserName"].ActivateInputField ();
			}
			allowTab = false;
		} 

		// Event e = Event.current;

		// Use enter to validate (if one of the entry is empty, go to this entry)
		//if (e != null && allowEnter && e.type == EventType.KeyUp && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)) {
		if (allowEnter && (Input.GetKeyUp (KeyCode.Return) || Input.GetKeyUp (KeyCode.KeypadEnter))) {

			allowEnter = false;

			if (waitForRetry) {
				ButtonRetry ();

			} else if (inputFields ["UserName"].text.Length > 0 && inputFields ["Password"].text.Length > 0) {
				ButtonIdentification ();

			} else if (inputFields ["UserName"].text.Length == 0) { 
				inputFields ["UserName"].ActivateInputField ();
			
			} else if (inputFields ["Password"].text.Length == 0) { 
				inputFields ["Password"].ActivateInputField ();
			}
			
		} else {
			allowEnter = !gotUserIdentification || waitForRetry;
		}
	}

	// ---------------- Get components ----------------------- //

	void GetPushButtons () {

		buttons = new Dictionary<string, Button> ();

		foreach(KeyValuePair<string, UnityAction> entry in buttonAssociations) {

			GameObject go;
			try {
				go = GameObject.FindGameObjectWithTag (entry.Key);
			} catch (NullReferenceException e) {
				Debug.Log ("UIControllerLi: I could not find object with tag '" + name + "'");
				throw e;
			}

			Button btn = go.GetComponent<Button> ();
			btn.onClick.AddListener (entry.Value);
			buttons [entry.Key] = btn;
		}
	}

	void GetInputFields () {
		
		inputFields = new Dictionary<string, InputField> ();

		foreach (string name in inputFieldsNames) {
			inputFields [name] = GameObject.FindGameObjectWithTag ("Input" + name).GetComponent<InputField> ();
		}
	}

	void GetTexts () {
	
		texts = new Dictionary <string, Text> (); 

		foreach (string name in textsNames) {
			texts[name] = GameObject.FindGameObjectWithTag("Text" + name).GetComponent<Text> ();
		}
	}

	void GetAnimators () {

		animators = new Dictionary<string, Animator> ();
	
		foreach (string name in animatorNames) {
			animators [name] = GameObject.FindGameObjectWithTag (name).GetComponent<Animator> ();
		}
	}

	// --- Push buttons --- //

	public void ButtonIdentification () {
		if (inputFields ["Password"].text.Length > 0 && inputFields ["UserName"].text.Length > 0) {

			Connecting ();

			gotUserIdentification = true;
			
		}
	}

	public void ButtonRetry () {

		waitForRetry = false;

		buttons ["ButtonRetry"].interactable = false;

		animators ["ButtonRetry"].SetBool ("Visible", false);

		IdentificationMenu ();
	}

	void ButtonHome () {

		waitForRetry = false;

		foreach (KeyValuePair<string, Button> entry in buttons) {
			entry.Value.interactable = false;
		}
		gameController.UserChooseHome ();
	}

	// --------------- Communication with gameController ---------- //

	public bool GotUserIdentification () {
		return gotUserIdentification;
	}

	public string GetUserName () {
		return inputFields ["UserName"].text;
	}

	public string GetUserPassword () {
		return inputFields ["Password"].text;
	}

	void Connecting () {

		buttons ["ButtonIdentification"].interactable = false;

		texts ["Central"].text = "Connecting...";

		animators["ConnectionMenu"].SetBool("Visible", false);
		animators ["TextCentral"].SetBool ("Visible", true);
		animators ["TextCentral"].SetBool ("Glow", true);
	}

	public void Connected () {

		texts ["Central"].text = "Connected!";

		animators ["ButtonHome"].SetBool ("Visible", false);

		Debug.Log ("UIControllerLi: Connected.");
	}

	public void ErrorOfConnection () {

		waitForRetry = true;
		gotUserIdentification = false;

		buttons ["ButtonRetry"].interactable = true;

		animators ["ButtonRetry"].SetBool ("Visible", true);
		animators ["ButtonRetry"].SetBool ("Glow", true);
		animators ["TextCentral"].SetBool ("Glow", false);

		texts ["Central"].text = "Name and password don't match...\nMaybe a typing error?";

		Debug.Log ("UIControllerLi: Error of connection.");
	}

	public void IdentificationMenu () {

		// inputFields ["UserName"].text = "";
		// inputFields ["Password"].text = "";
		animators ["TextCentral"].SetBool("Visible", false);
		animators ["ConnectionMenu"].SetBool ("Visible", true);

		buttons ["ButtonIdentification"].interactable = true;

		inputFields ["UserName"].ActivateInputField ();
	}
		
	public void QuitScene () {
		foreach (string name in animatorNames) {
			animators [name].SetBool ("Visible", false);
		}
		StartCoroutine (ActionWithDelay(EndAnimationQuitScene, 1f));

	}

	public void EndAnimationQuitScene () {
		gameController.EndAnimationQuitScene ();
	}

	IEnumerator ActionWithDelay (Action methodName, float seconds) {
		yield return new WaitForSeconds(seconds);

		methodName ();
	}
}


