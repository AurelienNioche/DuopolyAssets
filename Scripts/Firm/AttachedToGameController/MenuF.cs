using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using AssemblyCSharp;

public class MenuF : MonoBehaviour {

	TLMenuF stateMenu;

	UIControllerF uiController;
	GameControllerF gameController;
	ACF ac;

	IEnumerator coroutine;

	// Use this for initialization
	void Start () {
		stateMenu = TLMenuF.Init;
	}
	
	// Update is called once per frame
	void Update () {
	}

	void Awake () {

		// Get components
		uiController = GetComponent<UIControllerF> ();
		gameController = GetComponent<GameControllerF> (); 
		ac = GetComponent<ACF> ();
	}

	// ------------------------------------------------------------------------ //

	public void ManageState () {

		switch (stateMenu) {

		case TLMenuF.Init:

			string currentStep = gameController.GetCurrentStep ();

			if (currentStep == GameStep.tutorial) {
				uiController.ShowMessageTutorial ();
			
			} else if (currentStep == GameStep.pve) {
				uiController.ShowMessagePVE ();

			} else if (currentStep == GameStep.pvp) {
				uiController.ShowMessagePVP ();
			
			} else if (currentStep == GameStep.end) {
				uiController.ShowMessageAlreadyPlayed ();
				break; // Exit menu without displaying it
			
			} else {
				throw new Exception ("Menu: 'gameStep' not understood ('" + currentStep + "')");
			}

			uiController.ShowMenu ();

			uiController.AuthorizeButtonMenu (true);

			MenuNextStep ();
			break;

		case TLMenuF.UserTutorial:

			uiController.HideMenu ();

			CallGameControllerToBeginTutorial();

			stateMenu = TLMenuF.EndMenu;

			LogMenuState ();
			break;

		case TLMenuF.UserGame:

			uiController.ShowMessageWaiting ();

			CallGameControllerToBeginNewRound ();

			stateMenu = TLMenuF.EndMenu;

			LogMenuState ();
			break;

		case TLMenuF.WaitUser:
			break;
		}
	}

	// ---------- Next steps -------------- //

	void MenuNextStep() {
		stateMenu += 1;
		LogMenuState ();
	}

	// -------- Logs -------------------- //

	void LogMenuState () {
		Debug.Log ("GameController: New 'stateMenu' is '" + stateMenu + "'.");
	}

	// --------------------------------------------------------------------------- //

	public void Begin() {
		stateMenu = TLMenuF.Init;
	}

	public void UserPushedButtonMenu () {
		
		ac.buttonMenu.SetBool (Bool.visible, false);

		if (stateMenu == TLMenuF.WaitUser) {
			if (gameController.GetCurrentStep() == GameStep.tutorial) {
				stateMenu = TLMenuF.UserTutorial;
			} else {
				stateMenu = TLMenuF.UserGame;
			}
			// 
			LogMenuState ();
		}
	}

	// --------------------------------- //

//	IEnumerator ActionWithDelay(Action methodName, float seconds) {
//		yield return new WaitForSeconds(seconds);
//
//		methodName ();
//	}

	// ------------------------------- //

	void CallGameControllerToBeginTutorial () {
		Debug.Log ("Menu: tell the GC to begin the tutorial");
		gameController.BeginTheTutorial ();
		
	}

	void CallGameControllerToBeginNewRound () {
		Debug.Log ("Menu: tell the GC to a new round");
		gameController.BeginTheGame ();
	}

}
