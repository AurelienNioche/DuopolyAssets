using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using AssemblyCSharp;


public class UIControllerSi : MonoBehaviour {
	
	Dictionary <string, Button> buttons;
	Dictionary <string, InputField> inputFields;
	Dictionary <string, Toggle> toggles;
	Dictionary <string, Text> texts;
	Dictionary <string, Animator> animators;

	Dictionary<string, string> userData;

	GameControllerSi gameController;

	bool allowEnter;

	string[] inputFieldsNames = {
		"Email",
		"MtId",
		"Nationality",
		"Age"
	};

	string [] textsNames = {	
		"Information"
	};

	string [] toggleNames = {
		"Male",
		"Female",
		"Consent"
	};

	string [] animatorNames = {
		"CanvasUI",
		"ButtonSubmit",
		"ButtonLogIn",
		"ButtonSendAgain",
		"TextInformation",
		"ButtonHome"
	};

	Dictionary<string, UnityAction> buttonAssociations;

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake() {

		buttonAssociations = new Dictionary<string, UnityAction> () {

			{"ButtonSubmit", ButtonSubmit},
			{"ButtonLogIn", ButtonLogIn},
			{"ButtonHome", ButtonHome},
			{"ButtonSendAgain", ButtonSendAgain}
		};
			
		gameController = GetComponent<GameControllerSi> ();

		GetAnimators ();
		GetInputFields ();
		GetPushButtons ();
		GetTexts ();
		GetToggles ();
	}

	void Start () {
		
		inputFields [inputFieldsNames[0]].ActivateInputField ();
		animators ["ButtonSubmit"].SetBool ("Visible", true);
		animators ["ButtonHome"].SetBool ("Visible", true);
	}

	void Update () {

		// Use tab to navigate between input fields
		string inputFieldSelected = GetInputFieldSelected();
		bool allowTab = !string.IsNullOrEmpty(inputFieldSelected);

		if (allowTab && Input.GetKeyUp (KeyCode.Tab)) {
			allowTab = false;
			string inputFieldToSelect = GetNextInputField (inputFieldSelected);
			inputFields [inputFieldToSelect].ActivateInputField ();
		} 

		// Use enter to validate (if one of the entry is empty, go to this entry)
		if (allowEnter && (Input.GetKey (KeyCode.Return) || Input.GetKey (KeyCode.KeypadEnter))) {

			allowEnter = false;

			ButtonSubmit ();
			
		} else {
			allowEnter = EvaluateUserData (true);
			if (allowEnter) {
				animators ["ButtonSubmit"].SetBool ("Glow", true);
			}
		}
	}
		
	// ---------------- Get components ----------------------- //


	GameObject GetGameObject (string name) {
		
		GameObject go;
		try {
			go = GameObject.FindGameObjectWithTag (name);
		} catch (NullReferenceException e) {
			Debug.Log ("UIController: I could not find object with tag '" + name + "'");
			throw e;
		}

		return go;
	}

	void GetPushButtons () {

		buttons = new Dictionary<string, Button> ();

		foreach(KeyValuePair<string, UnityAction> entry in buttonAssociations) {

			Button btn = GetGameObject(entry.Key).GetComponent<Button> ();
			btn.onClick.AddListener (entry.Value);
			buttons [entry.Key] = btn;
		}
	}

	void GetInputFields () {
		
		inputFields = new Dictionary<string, InputField> ();

		foreach (string name in inputFieldsNames) {
			inputFields [name] = GetGameObject("Input" + name).GetComponent<InputField> ();
		}
	}

	void GetToggles () {

		toggles = new Dictionary<string, Toggle> ();

		foreach (string name in toggleNames) {
			toggles [name] = GetGameObject("Toggle" + name).GetComponent<Toggle> ();
		}
	}

	void GetTexts () {
	
		texts = new Dictionary <string, Text> (); 

		foreach (string name in textsNames) {
			texts[name] = GetGameObject("Text" + name).GetComponent<Text> ();
		}
	}

	void GetAnimators () {

		animators = new Dictionary<string, Animator> ();

		foreach (string name in animatorNames) {
			animators [name] = GetGameObject(name).GetComponent<Animator> ();
		}
	}

	// ------------- For input fields management ------------ //

	string GetInputFieldSelected () {
		foreach (string name in inputFieldsNames) {
			if (inputFields [name].isFocused) {
				return name;
			}
		}

		return "";
	}

	string GetNextInputField (string name) {
		if (name == inputFieldsNames.Last ()) {
			return inputFieldsNames [0];
		} else {
			int i = Array.IndexOf (inputFieldsNames, name);
			return inputFieldsNames [i + 1];
		}
	}


	// ------------- Evaluate user data ----------------- //

	bool EvaluateUserData (bool silent=false) {

		foreach (string name in inputFieldsNames) {

			if (inputFields [name].text.Length == 0) {
				if (!silent) {
					InformationMessage ("You should complete the '" + name + "' field to suscribe");
				}
				return false;
			}
		}	

		if (!toggles ["Male"].isOn && !toggles ["Female"].isOn) {
			if (!silent) {
				InformationMessage ("You should choose a gender to suscribe");
			}
			return false;
		}

		if (!toggles ["Consent"].isOn) {
			if (!silent) {
				InformationMessage ("You should give your consent to suscribe");
			}
			return false;
		}

		if (!TestEmail.IsEmail (inputFields ["Email"].text)) {
			if (!silent) {
				InformationMessage ("You should give a valid email to be able to receive a password");
			}
			return false;
		}

		return true;
	}

	void FormatUserData () {
		
		userData = new Dictionary <string, string> ();

		userData ["email"] = inputFields["Email"].text;
		userData ["mechanical_id"] = inputFields["MtId"].text;
		userData ["nationality"] = inputFields["Nationality"].text;
		userData ["age"] = inputFields["Age"].text;

		if (toggles ["Male"].isOn) {
			userData ["gender"] = "Male";
		} else if (toggles ["Female"].isOn) {
			userData ["gender"] = "Female";
		} else {
			throw new Exception ("UIController: Should have a gender.");
		}

		if (!toggles ["Consent"].isOn) {
			throw new Exception ("UIController: Should have consent.");
		}
	}

	void SetFormInteractable (bool value) {

		foreach (string name in inputFieldsNames) { 
			inputFields [name].interactable = value;
		}

		foreach (string name in toggleNames) {
			toggles [name].interactable = value;
		}

		buttons ["ButtonSubmit"].interactable = value;
		
	}

	void InformationMessage (string msg) {
		 
		animators ["TextInformation"].SetBool ("Visible", true);
		animators ["TextInformation"].SetBool ("Glow", true);
		texts ["Information"].text = msg;
	}

	// --- Push buttons --- //

	void ButtonSubmit () {

		SetFormInteractable (false);

		if (EvaluateUserData ()) {
			
			InformationMessage ("Sending you a mail to '" + inputFields["Email"].text + "' with an ID and a password...");

			animators ["ButtonSubmit"].SetBool ("Visible", false);
			FormatUserData ();	

			gameController.UserChooseSubmit ();
		} else {
			SetFormInteractable (true);
			animators ["ButtonSubmit"].SetBool ("Glow", false);
		}
	}

	void ButtonLogIn () {
		
		buttons ["ButtonLogIn"].interactable = false;
		animators ["ButtonLogIn"].SetBool ("Visible", false);

		buttons ["ButtonLogIn"].interactable = false;
		animators ["ButtonLogIn"].SetBool ("Visible", false);

		animators ["TextInformation"].SetBool ("Visible", false);

		gameController.UserChooseLogIn ();
	}

	void ButtonSendAgain () {

		buttons ["ButtonSendAgain"].interactable = false;
		animators ["ButtonSendAgain"].SetBool ("Visible", false);

		buttons ["ButtonLogIn"].interactable = false;
		animators ["ButtonLogIn"].SetBool ("Visible", false);

		animators ["TextInformation"].SetBool ("Visible", false);

		InformationMessage ("Sending you a mail to '" + inputFields["Email"].text + "' with an ID and a password...");

		gameController.UserChooseSendAgain ();
	}

	void ButtonHome () {

		foreach (KeyValuePair<string, Button> entry in buttons) {
			entry.Value.interactable = false;
		}
		SetFormInteractable (false);
		gameController.UserChooseHome ();
	}


	// --------------- Communication with gameController ---------- //

	public void QuitScene () {

		foreach (string name in new string[] {
			"ButtonHome",
			"ButtonLogIn",
			"ButtonSendAgain",
			"ButtonSubmit"
		}) {
			animators [name].SetBool ("Visible", false);
			buttons [name].interactable = false;
		}

		animators ["TextInformation"].SetBool ("Glow", false);
		animators ["TextInformation"].SetBool ("Visible", false);

		animators ["CanvasUI"].SetTrigger ("Quit");
	}
		
	public void EndAnimationQuitScene () {
		gameController.EndAnimationQuitScene ();
	}

	public Dictionary <string, string> GetUserData () {
		return userData;
	}

	public void ProposeToLogin () {

		InformationMessage ("You should have receive a mail with a password.\n " +
			"If it is the case, go to log in! Otherwise, click on 'send mail again'.");
		animators ["TextInformation"].SetBool ("Glow", false);

		animators ["ButtonLogIn"].SetBool ("Visible", true);
		animators ["ButtonLogIn"].SetBool ("Glow", true);

		animators ["ButtonSendAgain"].SetBool ("Visible", true);

		buttons ["ButtonLogIn"].interactable = true;
		buttons ["ButtonSendAgain"].interactable = true;
	}

	public void SendingFailed () {

		InformationMessage ("Something went wrong. Please retry.");

		SetFormInteractable (true);

		animators ["TextInformation"].SetBool ("Glow", false);

		animators ["ButtonSubmit"].SetBool ("Visible", true);
		animators ["ButtonSubmit"].SetBool ("Glow", false);
	}

	public void AlreadyExists () {
		
		InformationMessage ("You are already registered! Please use the 'log in' option.\n" +
			"If you did not receive (or lost) the mail with a password, click on 'send mail again'.");
		animators ["TextInformation"].SetBool ("Glow", false);
		animators ["ButtonLogIn"].SetBool ("Visible", true);
		animators ["ButtonLogIn"].SetBool ("Glow", true);
		animators ["ButtonSendAgain"].SetBool ("Visible", true);
	}
}


