using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class UIControllerLfp : MonoBehaviour {
	
	Dictionary <string, Button> buttons;
	Dictionary <string, Text> texts;
	Dictionary <string, Animator> animators;

	GameControllerLfp gameController;

	ClientLfp client;

	bool gotUserParticipation;
	bool allowEnter;

	Dictionary<string, UnityAction> buttonAssociations;

	string [] textsNames = {
		"Central"
	};

	string [] animatorNames = {
		"ButtonParticipation",
		"Logo", 
		"TextCentral"
	};


	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake() {

		buttonAssociations = new Dictionary<string, UnityAction> () {
			{"ButtonParticipation", ButtonParticipation}
		};

		gameController = GetComponent<GameControllerLfp> ();

		GetAnimators ();
		GetPushButtons ();
		GetTexts ();
	}

	void Start () {
		animators ["Logo"].SetBool ("Visible", true);
		animators ["TextCentral"].SetBool ("Visible", true);
		animators ["TextCentral"].SetBool ("Glow", true);
	}

	void Update () {
	}

	// ---------------- Get components ----------------------- //

	void GetPushButtons () {

		buttons = new Dictionary<string, Button> ();

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
			buttons [entry.Key] = btn;
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

	public void ButtonParticipation () {
		buttons ["ButtonParticipation"].interactable = false;
		gotUserParticipation = true;
		animators ["ButtonParticipation"].SetBool ("Visible", false);
		animators ["TextCentral"].SetBool ("Glow", true);
	}

	// --------------- Communication with gameController ---------- //

	public bool GotUserParticipaton () {
		return gotUserParticipation;
	}

	public void NoRoomAvailable () {
		texts ["Central"].text = "No game is running,\nor all players have already been recruited!"; 
		animators ["ButtonParticipation"].SetBool ("Visible", false);
	}

	public void Participation () {

		buttons ["ButtonParticipation"].interactable = true;

		animators ["ButtonParticipation"].SetBool ("Visible", true);
		animators ["ButtonParticipation"].SetBool ("Glow", true);

		animators ["TextCentral"].SetBool ("Glow", false);
		texts ["Central"].text = "There are games with available places. Do you want to join?"; 
	}

	public void SubmittingParticipation () {
		animators ["TextCentral"].SetBool ("Glow", true);
		texts ["Central"].text = "Booking the place...";
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

		texts ["Central"].text = newText;
	}

	public void WaitingForPlay () {
		
		texts ["Central"].text = "All players are ready.\nGame will start in a short moment...";
	}

	public void QuitScene () {
		foreach (string name in animatorNames) {
			animators [name].SetBool ("Visible", false);
		}
		StartCoroutine (ActionWithDelay(EndAnimationQuitScene, 0.5f));

	}

	public void EndAnimationQuitScene () {
		gameController.EndAnimationQuitScene ();
	}

	IEnumerator ActionWithDelay (Action methodName, float seconds) {
		yield return new WaitForSeconds(seconds);

		methodName ();
	}
}


