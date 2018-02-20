using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp;
using UnityEngine.Networking;


class KeySi {

	public static string demand = "demand";
} 

class DemandSi {

	public static string register = "register";
	public static string sendPasswordAgain = "send_password_again";
}

class ErrorSi {
	public static string sendingAborted = "sending_aborted";
	public static string alreadyExists = "already_exists";
}


public class ClientSi : MonoBehaviour {

	// ---------- For communication with the server -------------------------- //

	string url;
	float timeBeforeRetryingDemand; 

	Dictionary<string, string> requestInMemory;

	bool serverError;
	string serverResponse;

	Dictionary<string, string> request;
	Dictionary<string, string> userData;

	// -------------- For player ------------------------ //
	string userName;
	string password;
	string gameId;
	int t;
	string role;

	bool alreadyRegistered;
	int missingPlayers;

	GameControllerSi gameController;

	TimeLinePlayerSi state;

	bool occupied;

	// --------------- Overloaded Unity's functions -------------------------- //

	void Awake () {
		gameController = GetComponent <GameControllerSi> ();
	}

	// Use this for initialization
	void Start () {

		state = TimeLinePlayerSi.WaitingUserFillIn;
		occupied = false;
		request = new Dictionary<string, string> ();

		url = gameController.GetUrl ();
		timeBeforeRetryingDemand = gameController.GetTimeBeforeRetryingDemand ();
	}

	// Update is called once per frame
	void Update () {
		if (!occupied) {

			occupied = true;
			ManageState ();
			occupied = false;
		}
	}

	void ManageState () {

		switch (state) {

		case TimeLinePlayerSi.WaitingUserFillIn:
			break;
		case TimeLinePlayerSi.SendForm:

			request.Clear ();
			request [KeySi.demand] = DemandSi.register;
			foreach (KeyValuePair<string, string> entry in userData) {
				request [entry.Key] = entry.Value;
			}

			StartCoroutine (AskServer (request));

			state = TimeLinePlayerSi.SendFormWaitReply;
			break;

		case TimeLinePlayerSi.SendAgain:

			request.Clear ();
			request [KeySi.demand] = DemandSi.sendPasswordAgain;
			foreach (KeyValuePair<string, string> entry in userData) {
				request [entry.Key] = entry.Value;
			}

			StartCoroutine (AskServer (request));

			state = TimeLinePlayerSi.SendAgainWaitReply;
			break;

		case TimeLinePlayerSi.SendFormWaitReply:
		case TimeLinePlayerSi.SendAgainWaitReply:
			if (HasResponse ()) {
				HandleServerResponse ();
			}

			break;
		case TimeLinePlayerSi.SendingAborted:
			break;
		case TimeLinePlayerSi.FormSent:
			break;
		}
	}

	// --------------------- For treating response ------------------------------------------------------- //

	void ReplyRegister (string[] args) {

		if (state == TimeLinePlayerSi.SendFormWaitReply) {

			int ok = int.Parse (args [0]);

			if (ok == 1) {
				gameController.FormSent ();
				state = TimeLinePlayerSi.FormSent;
			} else {
				string reason = args [1];
				if (reason == ErrorSi.sendingAborted) {
					gameController.SendingFailed ();
					state = TimeLinePlayerSi.SendingAborted;
				} else if (reason == ErrorSi.alreadyExists) {
					gameController.AlreadyExists ();
					state = TimeLinePlayerSi.AlreadyExists;
				} else {
					throw new System.Exception ("Reason of failure not understood ('" + reason +"').");	
				}
			}
		}

		else {
			Debug.Log("ClientSi: Time problem or state problem. Maybe I already received a response.");
		}
	}

	void ReplyMailSendAgain (string[] args) {

		if (state == TimeLinePlayerSi.SendAgainWaitReply) {

			bool ok = int.Parse (args [0]) == 1;

			if (ok) {
				gameController.SendAgainGotReply ();
				state = TimeLinePlayerSi.SendAgainGotReply;
			} else {
				// Should be implemented otherwise.
				gameController.SendAgainGotReply ();
				state = TimeLinePlayerSi.SendAgainGotReply;
			}

			gameController.SendAgainGotReply ();
			state = TimeLinePlayerSi.SendAgainGotReply;
		}

		else {
			Debug.Log("ClientSi: Time problem or state problem. Maybe I already received a response.");
		}
	}

	// ------------------ General methods for communicating with the server --------- //

	bool HandleServerResponse () {

		string [] parts = serverResponse.Split (new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length > 1 && parts [0] == "reply") {

			string[] args = new string[parts.Length - 2];
			Array.Copy (parts, 2, args, 0, parts.Length - 2);

			MakeTheCorrectCall (parts[1], args);

			Debug.Log ("ClientSi: Server response handled");
			return true;

		} else {
			Debug.Log ("ClientSi: I couldn't handle the server response. Retry the same demand.");

			serverResponse = "";
			StartCoroutine (RetryDemand ());
			return false;
		}

	}

	void MakeTheCorrectCall (string what, string [] strArgs) {

		if (what == DemandSi.register) {
			ReplyRegister (strArgs);
		} else if (what == DemandSi.sendPasswordAgain) {
			ReplyMailSendAgain (strArgs);
		} else {
			throw new System.Exception ("I received '" + what + "' with args '" + strArgs + "' but I can not catch that.");
		}
	}

	// ---------------------- For communicating with game controller ------------------------------ //

	public void SendForm (Dictionary<string, string> userData) {
		this.userData = userData;
		state = TimeLinePlayerSi.SendForm;
	}

	public void SendAgain () {
		state = TimeLinePlayerSi.SendAgain;
	}

	// ----------------------- Communication ----------------- //

	public IEnumerator AskServer (Dictionary<string, string> request) {

		serverResponse = "";

		if (request != requestInMemory) {
			requestInMemory = request;
		}

		string toDebug = "ClientSi: I send a request with post entries: ";

		WWWForm form = new WWWForm();
		foreach(KeyValuePair<string, string> entry in request) {
			form.AddField (entry.Key, entry.Value);
			toDebug += "('" + entry.Key + "', '" + entry.Value + "')";
		}	

		toDebug += ".";
		Debug.Log (toDebug);

		UnityWebRequest www = UnityWebRequest.Post(url, form);
		www.chunkedTransfer = false;
		// www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

		serverError = false;

		yield return www.SendWebRequest();

		if (www.error != null) {
			serverError = true;
			serverResponse = www.error;
			Debug.Log ("ClientSi: I got an error: '" + serverResponse + "'.");
		}
		else {
			serverResponse = www.downloadHandler.text;
			Debug.Log ("ClientSi: I got a response: '" + serverResponse + "'.");
			if (string.IsNullOrEmpty (serverResponse)) {
				Debug.Log ("ClientSi: Response is empty. I will consider it as an error");
				serverResponse = "EmptyResponse";
				serverError = true;
			}
		}
	}

	public bool HasResponse () {

		// Return if client recieves a response.
		if (!string.IsNullOrEmpty (serverResponse)) {

			Debug.Log ("ClientSi: Got response from server.");

			if (serverError == false) {
				return true;
			} else {
				// In case of error consider that no response has been received.
				Debug.Log ("ClientSi: I got error '" + serverResponse + "' and will retry the same demand.");

				serverResponse = "";
				StartCoroutine (RetryDemand ());
				return false;
			}
		} else {
			return false;
		}	
	}

	IEnumerator RetryDemand () {
		yield return new WaitForSeconds (timeBeforeRetryingDemand);
		StartCoroutine (AskServer (requestInMemory));
	}
}


