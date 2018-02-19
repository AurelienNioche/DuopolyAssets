using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp;
using UnityEngine.Networking;

public class ClientLi : MonoBehaviour {

	GameControllerLi gameController;

	// ---------- For communication with the server -------------------------- //

	string url;
	float timeBeforeRetryingDemand;

	bool serverError;
	string serverResponse;

	Dictionary<string, string> request;
	Dictionary<string, string> requestInMemory;

	// -------------- For player ------------------------ //

	string userName;
	string password;

	TimeLineClientLi state;

	bool occupied;

	// --------------- Overloaded Unity's functions -------------------------- //

	void Awake () {

		gameController = GetComponent <GameControllerLi> ();
	}

	// Use this for initialization
	void Start () {

		state = TimeLineClientLi.WaitingUserIdentification;
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

		case TimeLineClientLi.WaitingUserIdentification:
			break;

		case TimeLineClientLi.AskConnection:

			request.Clear ();
			request ["demand"] = "connect";
			request ["username"] = userName;
			request ["password"] = password;

			StartCoroutine (AskServer (request));

			state = TimeLineClientLi.AskConnectionWaitReply;
			LogState ();
			break;


		case TimeLineClientLi.AskConnectionWaitReply:

			if (HasResponse ()) {
				HandleServerResponse ();
			}
			break;

		case TimeLineClientLi.GotConnection:
		case TimeLineClientLi.ErrorIdentification:
			break;

		}
	}

	void LogState () {
		Debug.Log ("ClientLi: My state is '" + state + "'.");
	}


	// --------------------- For treating response ------------------------------------------------------- //

	// Connection

	void ReplyConnected (string[] args) {

		if (state == TimeLineClientLi.AskConnectionWaitReply) {

			int connected = int.Parse (args [0]);
			if (connected == 1) {
				state = TimeLineClientLi.GotConnection;
			} else {
				state = TimeLineClientLi.ErrorIdentification;
			}
			LogState ();
		}

		else {
			Debug.Log("ClientLi: Time problem or state problem. Maybe I already received a response.");
		}
	}

	// ------------------ General methods for communicating with the server --------- //

	bool HandleServerResponse () {

		string [] parts = serverResponse.Split (new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length > 1 && parts [0] == "reply") {

			string[] args = new string[parts.Length - 2];
			Array.Copy (parts, 2, args, 0, parts.Length - 2);

			MakeTheCorrectCall (parts[1], args);

			Debug.Log ("ClientLi: Server response handled");
			return true;

		} else {
			Debug.Log ("ClientLi: I couldn't handle the server response. Retry the same demand.");

			serverResponse = "";
			StartCoroutine (RetryDemand ());
			return false;
		}

	}

	void MakeTheCorrectCall (string what, string [] strArgs) {

		switch (what) {

		case "connect":
			ReplyConnected (strArgs);
			break;

		default:
			throw new System.Exception ("I received '" + what + "' with args '" + strArgs + "' but I can not catch that.");
		}
	}

	// ---------------------- For communicating with game controller ------------------------------ //

	public void AskConnection (string userName, string password) {

		this.userName = userName;
		this.password = password;
		state = TimeLineClientLi.AskConnection;
		LogState ();
	}

	public void WaitingCommand () {
		state = TimeLineClientLi.WaitingCommand;
		LogState ();
	}

	public bool GotConnection () {
		return state == TimeLineClientLi.GotConnection;
	}

	public bool GotAnErrorOfIdentification () {
		return state == TimeLineClientLi.ErrorIdentification;
	}

	// ----------------------- Communication ----------------- //

	public IEnumerator AskServer (Dictionary<string, string> request) {

		serverResponse = "";

		if (request != requestInMemory) {
			requestInMemory = request;
		}

		string toDebug = "ClientLi: I send a request with post entries: ";

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
			Debug.Log ("ClientLi: I got an error: '" + serverResponse + "'.");
		}
		else {
			serverResponse = www.downloadHandler.text;
			Debug.Log ("ClientLi: I got a response: '" + serverResponse + "'.");
		}
	}

	public bool HasResponse () {

		// Return if client recieves a response.
		if (!string.IsNullOrEmpty (serverResponse)) {

			Debug.Log ("ClientLi: Got response from server.");

			if (serverError == false) {
				return true;
			} else {
				// In case of error consider that no response has been received.
				Debug.Log ("ClientLi: I got error '" + serverResponse + "' and will retry the same demand.");

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

