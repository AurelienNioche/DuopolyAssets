using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


class BoolM {
	public static string visible = "Visible";
	public static string glow = "Glow";
}

class TriggerM {
	public static string quit = "Quit";
}


public class UIControllerM : MonoBehaviour {

	GameControllerM gameController;

	Button buttonSignIn;
	Button buttonLogIn;

	Animator animCanvasUI;
	Animator animButtons;

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake() {
		
		gameController = GetComponent<GameControllerM> ();

		buttonLogIn = AssociatePushButton ("ButtonLogIn", ButtonLogIn);
		buttonSignIn = AssociatePushButton ("ButtonSignIn", ButtonSignIn);

		animCanvasUI = AssociateAnim ("CanvasUI");
		animButtons = AssociateAnim ("Buttons");
	}

	void Start () {
		// animators ["Buttons"].SetBool ("Visible", true);
	}

	void Update () {}

	// ---------------- Get components ----------------------- //

	// ---------------- Get components ----------------------- //

	Animator AssociateAnim (string name) {

		Animator anim = GetGameObject(name).GetComponent<Animator> ();
		return anim;
	}

	Button AssociatePushButton (string name, UnityAction action) {

		Button btn = GetGameObject(name).GetComponent<Button> ();
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

	void ButtonSignIn () {

		Debug.Log("UIController (Main): User clicked on button 'Sign In'.");

		buttonSignIn.interactable = false;
		buttonLogIn.interactable = false;
		gameController.UserChooseSignIn ();
		animButtons.SetBool (BoolM.visible, false);
	}

	void ButtonLogIn () {

		Debug.Log("UIController (Main): User clicked on button 'Log In'.");

		buttonSignIn.interactable = false;
		buttonLogIn.interactable = false;
		gameController.UserChooseLogIn ();
		animButtons.SetBool (BoolM.visible, false);
	}

	// --------------- Communication with gameController ---------- //

	public void QuitScene () {
		animCanvasUI.SetTrigger (TriggerM.quit);
	}

	public void EndAnimationQuitScene () {
		gameController.EndAnimationQuitScene ();
	}
}



