using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

class Bool {
	public static string visible = "Visible";
	public static string glow = "Glow";
}


public class UIControllerLfp : MonoBehaviour {

	GameControllerLfp gameController;

	ClientLfp client;

	bool gotUserParticipation;
	bool allowEnter;

	Dictionary<string, UnityAction> buttonAssociations;

	Text textCentral;

	Animator animLogo; 
	Animator animButtonParticipation;
	Animator animTextCentral;

	Button buttonParticipation;


	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake() {

		// Associate animators
		animLogo = AssociateAnim ("Logo"); 
		animButtonParticipation = AssociateAnim ("ButtonParticipation");
		animTextCentral = AssociateAnim ("TextCentral");

		// ...texts
		textCentral = AssociateText ("TextCentral");

		// ...buttons
		buttonParticipation = AssociatePushButton ("ButtonParticipation", ButtonParticipation);

		// ...and get GameController
		gameController = GetComponent<GameControllerLfp> ();
	}

	void Start () {

		animLogo.SetBool (Bool.visible, true);

		animTextCentral.SetBool (Bool.visible, true);
		animTextCentral.SetBool (Bool.glow, true);
	}

	void Update () {
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

	// --- Push buttons --- //

	public void ButtonParticipation () {
		
		buttonParticipation.interactable = false;
		gotUserParticipation = true;
		animButtonParticipation.SetBool (Bool.visible, false);
		animTextCentral.SetBool (Bool.glow, true);

		SubmittingParticipation ();
	}

	// --------------- Communication with gameController ---------- //

	public bool GotUserParticipaton () {
		return gotUserParticipation;
	}

	public void NoRoomAvailable () {
		textCentral.text = "No game is running,\nor all players have already been recruited!"; 
		animButtonParticipation.SetBool (Bool.visible, false);
	}

	public void Participation () {

		buttonParticipation.interactable = true;

		animButtonParticipation.SetBool (Bool.visible, true);
		animButtonParticipation.SetBool (Bool.glow, true);

		animTextCentral.SetBool (Bool.glow, false);
		textCentral.text = "There are games with available places. Do you want to join?"; 
	}

	public void SubmittingParticipation () {
		animTextCentral.SetBool (Bool.glow, true);
		textCentral.text = "Booking the place...";
	}

	public void UpdateMissingPlayers (int missingPlayers) {

		string newText;

		if (missingPlayers == 1) {
			newText = "1 player is missing." + 
				"\nPlease wait..."; 
		} else {
			newText = missingPlayers.ToString () + " players are missing.\n" +
				"\nPlease wait...";
		}

		textCentral.text = newText;
	}

	public void WaitingForPlay () {
		
		textCentral.text = "All players are ready.\nGame will start in a short moment...";
	}

	public void QuitScene () {	

		animLogo.SetBool (Bool.visible, false);
		animButtonParticipation.SetBool (Bool.visible, false);
		animTextCentral.SetBool (Bool.visible, false);

		StartCoroutine (ActionWithDelay(EndAnimationQuitScene, 0.5f));

	}

	public void EndAnimationQuitScene () {
		gameController.EndAnimationQuitScene ();
	}

	IEnumerator ActionWithDelay (Action methodName, float seconds) {
		yield return new WaitForSeconds(seconds);

		methodName ();
	}

	public void ShowMessageOpponentDisconnected () {

		animTextCentral.SetBool (Bool.glow, false);
		textCentral.text = 
			"Unfortunately, the other player is disconnected\n" +
			"There is no choice but to put an end to the game\n" +
			"Your HIT will be accepted within three days";

		animTextCentral.SetBool (Bool.visible, true);
	}

	public void ShowMessagePlayerDisconnected () {

		animTextCentral.SetBool (Bool.glow, false);
		textCentral.text = "After a long delay without news from you,\n" +
			"you have been banned in order to not block the other player";
		animTextCentral.SetBool (Bool.visible, true);
	}

	public void ShowMessageAlreadyPlayed () {

		animTextCentral.SetBool (Bool.glow, true);
		textCentral.text = "We know that this game is the best game you ever played, " +
			"\nbut it is a single shot experiment!"; 
	}
}


