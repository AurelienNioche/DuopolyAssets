using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class UIControllerM : MonoBehaviour {
	
	Dictionary <string, Button> buttons;
	Dictionary <string, Animator> animators;

	GameControllerM gameController;

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake() {
		
		gameController = GetComponent<GameControllerM> ();

		GetAnimators ();
		GetPushButtons ();
	}

	void Start () {
		// animators ["Buttons"].SetBool ("Visible", true);
	}

	void Update () {}

	// ---------------- Get components ----------------------- //

	void GetPushButtons () {

		buttons = new Dictionary<string, Button> ();

		Dictionary<string, UnityAction> buttonAssociations = new Dictionary<string, UnityAction> () {

			{"ButtonSignIn", ButtonSignIn},
			{"ButtonLogIn", ButtonLogIn}
		};

		foreach(KeyValuePair<string, UnityAction> entry in buttonAssociations) {

			GameObject go;
			try {
				go = GameObject.FindGameObjectWithTag (entry.Key);
			} catch (NullReferenceException e) {
				Debug.Log ("UIController: I could not find object with tag '" + name + "'");
				throw e;
			}

			Button btn = go.GetComponent<Button> ();
			btn.onClick.AddListener (entry.Value);
			// btn.interactable = false;
			buttons [entry.Key] = btn;
		}
	}

	void GetAnimators () {

		animators = new Dictionary<string, Animator> ();

		string [] names = {
			"CanvasUI",
			"Buttons"
		};

		foreach (string name in names) {

			GameObject go;
			try {
				go = GameObject.FindGameObjectWithTag (name);
			} catch (NullReferenceException e) {
				Debug.Log ("UIController: I could not find object with tag '" + name + "'");
				throw e;
			}
			animators [name] = go.GetComponent<Animator> ();
		}
	}

	// --- Push buttons --- //

	void ButtonSignIn () {

		Debug.Log("User clicked on button 'Sign In'.");

		buttons ["ButtonSignIn"].interactable = false;
		buttons ["ButtonLogIn"].interactable = false;
		gameController.UserChooseSignIn ();
		animators["Buttons"].SetBool ("Visible", false);
	}

	void ButtonLogIn () {

		Debug.Log("User clicked on button 'Log In'.");

		buttons ["ButtonSignIn"].interactable = false;
		buttons ["ButtonLogIn"].interactable = false;
		gameController.UserChooseLogIn ();
		animators ["Buttons"].SetBool ("Visible", false);
	}

	// --------------- Communication with gameController ---------- //

	public void QuitScene () {
		animators ["CanvasUI"].SetTrigger ("Quit");
	}

	public void EndAnimationQuitScene () {
		gameController.EndAnimationQuitScene ();
	}
}



