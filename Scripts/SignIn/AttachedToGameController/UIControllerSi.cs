using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using AssemblyCSharp;


class KeyUserData {

	public static string email = "email";
	public static string mechanicalId = "mechanical_id";
	public static string nationaliy = "nationality";
	public static string age = "age";
	public static string gender = "gender";
	
}


class BoolSi {
	public static string visible = "Visible";
	public static string glow = "Glow";
}

class TriggerSi {
	public static string quit = "Quit";
}


public class UIControllerSi : MonoBehaviour {
	
	Dictionary <string, InputField> inputFields;
	Dictionary <string, Toggle> toggles;

	Dictionary<string, string> userData;

	GameControllerSi gameController;

	bool allowEnter;

	string[] inputFieldsNames = {
		"Email",
		"MtId",
		"Nationality",
		"Age"
	};

	string [] toggleNames = {
		"Male",
		"Female",
		"Consent"
	};

	Text textInformation;

	Animator animCanvasUI;
	Animator animButtonSubmit;
	Animator animButtonLogIn;
	Animator animButtonSendAgain;
	Animator animTextInformation;
	Animator animButtonHome;

	Button buttonSubmit;
	Button buttonLogIn;
	Button buttonHome;
	Button buttonSendAgain;

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake() {

		animCanvasUI = AssociateAnim ("CanvasUI");
		animButtonSubmit = AssociateAnim ("ButtonSubmit");
		animButtonLogIn = AssociateAnim ("ButtonLogIn");
		animButtonSendAgain = AssociateAnim ("ButtonSendAgain");
		animButtonHome = AssociateAnim ("ButtonHome");

		textInformation = AssociateText ("TextInformation");

		buttonSubmit = AssociatePushButton ("ButtonSubmit", ButtonSubmit);
		buttonLogIn = AssociatePushButton("ButtonLogIn", ButtonLogIn);
		buttonHome = AssociatePushButton("ButtonHome", ButtonHome);
		buttonSendAgain = AssociatePushButton ("ButtonSendAgain", ButtonSendAgain);
			
		gameController = GetComponent<GameControllerSi> ();

		GetInputFields ();
		GetToggles ();
	}

	void Start () {
		
		inputFields [inputFieldsNames[0]].ActivateInputField ();
		animButtonSubmit.SetBool (BoolSi.visible, true);
		animButtonHome.SetBool (BoolSi.visible, true);
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
				animButtonSubmit.SetBool (BoolSi.glow, true);
			}
		}
	}
		
	// ---------------- Get components ----------------------- //

	Animator AssociateAnim (string name) {
		
		Animator anim = GetGameObject(name).GetComponent<Animator> ();
		return anim;
	}

	Text AssociateText (string name) {

		Text text = GetGameObject(name).GetComponent<Text> ();
		return text;
	}

	Button AssociatePushButton (string name, UnityAction action) {

		Button btn = GetGameObject (name).GetComponent<Button> ();
		btn.onClick.AddListener (action);
		return btn;
	}

	// ------------------------------------------------ //

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

	// ------------------------------------------ //

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

		userData [KeyUserData.email] = inputFields["Email"].text;
		userData [KeyUserData.mechanicalId] = inputFields["MtId"].text;
		userData [KeyUserData.nationaliy] = inputFields["Nationality"].text;
		userData [KeyUserData.age] = inputFields["Age"].text;

		if (toggles ["Male"].isOn) {
			userData [KeyUserData.gender] = "Male";
		} else if (toggles ["Female"].isOn) {
			userData [KeyUserData.gender] = "Female";
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

		buttonSubmit.interactable = value;
		
	}

	void InformationMessage (string msg) {
		 
		animTextInformation.SetBool (BoolSi.visible, true);
		animTextInformation.SetBool (BoolSi.glow, true);

		textInformation.text = msg;
	}

	// --- Push buttons --- //

	void ButtonSubmit () {

		SetFormInteractable (false);

		if (EvaluateUserData ()) {
			
			InformationMessage ("Sending you a mail to '" + inputFields["Email"].text + "' with an ID and a password...");

			animButtonSubmit.SetBool (BoolSi.visible, false);
			FormatUserData ();	

			gameController.UserChooseSubmit ();
		} else {
			SetFormInteractable (true);
			animButtonSubmit.SetBool (BoolSi.glow, false);
		}
	}

	void ButtonLogIn () {
		
		buttonLogIn.interactable = false;
		animButtonLogIn.SetBool (BoolSi.visible, false);

		buttonLogIn.interactable = false;
		animButtonLogIn.SetBool (BoolSi.visible, false);

		animTextInformation.SetBool (BoolSi.visible, false);

		gameController.UserChooseLogIn ();
	}

	void ButtonSendAgain () {

		buttonSendAgain.interactable = false;
		animButtonSendAgain.SetBool (BoolSi.visible, false);

		buttonLogIn.interactable = false;
		animButtonLogIn.SetBool (BoolSi.visible, false);

		animTextInformation.SetBool (BoolSi.visible, false);

		InformationMessage ("Sending you a mail to '" + inputFields["Email"].text + "' with an ID and a password...");

		gameController.UserChooseSendAgain ();
	}

	void ButtonHome () {

		buttonSubmit.interactable = false;
		buttonLogIn.interactable = false;
		buttonHome.interactable = false;
		buttonSendAgain.interactable = false;

		SetFormInteractable (false);
		gameController.UserChooseHome ();
	}


	// --------------- Communication with gameController ---------- //

	public void QuitScene () {

		buttonSubmit.interactable = false;
		buttonLogIn.interactable = false;
		buttonHome.interactable = false;
		buttonSendAgain.interactable = false;

		animButtonHome.SetBool (Bool.visible, false);
		animButtonLogIn.SetBool (Bool.visible, false);
		animButtonSendAgain.SetBool (Bool.visible, false);
		animButtonSubmit.SetBool (Bool.visible, false);

		animTextInformation.SetBool (BoolSi.glow, false);
		animTextInformation.SetBool (BoolSi.visible, false);

		animCanvasUI.SetTrigger (TriggerSi.quit);
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
		animTextInformation.SetBool (BoolSi.glow, false);

		animButtonLogIn.SetBool (BoolSi.visible, true);
		animButtonLogIn.SetBool (BoolSi.glow, true);

		animButtonSendAgain.SetBool (BoolSi.visible, true);

		buttonLogIn.interactable = true;
		buttonSendAgain.interactable = true;
	}

	public void SendingFailed () {

		InformationMessage ("Something went wrong. Please retry.");

		SetFormInteractable (true);

		animTextInformation.SetBool (BoolSi.glow, false);

		animButtonSubmit.SetBool (BoolSi.visible, true);
		animButtonSubmit.SetBool (BoolSi.glow, false);
	}

	public void AlreadyExists () {
		
		InformationMessage ("You are already registered! Please use the 'log in' option.\n" +
			"If you did not receive (or lost) the mail with a password, click on 'send mail again'.");
		animTextInformation.SetBool (BoolSi.glow, false);
		animButtonLogIn.SetBool (BoolSi.visible, true);
		animButtonLogIn.SetBool (BoolSi.glow, true);
		animButtonSendAgain.SetBool (BoolSi.visible, true);
	}
}


