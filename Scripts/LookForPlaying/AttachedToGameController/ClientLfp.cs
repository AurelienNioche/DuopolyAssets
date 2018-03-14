using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp;
using UnityEngine.Networking;

class CodeErrorLfp {

	public static int opponentDisconnected = -3;
	public static int playerDisconnected = -4;
	public static int noOtherPlayer = -5;
}


public class DemandLfp  {
	public static string registeredAsPlayer = "registered_as_player";
	public static string roomAvailable = "room_available";
	public static string missingPlayers = "missing_players";
	public static string proceedToRegistrationAsPlayer = "proceed_to_registration_as_player";
	public static string playerInfo = "player_info";
}

class KeyLfp {

	public static string demand = "demand";
	public static string username = "username";
	public static string playerId = "player_id";
}


public class ClientLfp : MonoBehaviour {

	GameControllerLfp gameController;

	// ---------- For communication with the server -------------------------- //

	string url;
	float timeBeforeRetryingDemand; 

	bool serverError;
	string serverResponse;

	Dictionary<string, string> request;
	Dictionary<string, string> requestInMemory;

	// -------------- For player ------------------------ //

	bool errorRaised;
	int error;

	string userName;
	string playerId;

	int missingPlayers;
	string currentStep;
	bool registeredAsPlayer;
	bool roomAvailable;
	bool displayOpponentScore;
	int[,] consumersFieldOfView;

	TimeLineClientLfp state;

	bool occupied;

	// --------------- Overloaded Unity's functions -------------------------- //

	void Awake () {
		gameController = GetComponent <GameControllerLfp> ();
	}

	// Use this for initialization
	void Start () {
		state = TimeLineClientLfp.WaitingCommand;
		occupied = false;
		request = new Dictionary<string, string> ();

		url = gameController.GetUrl ();
		timeBeforeRetryingDemand = gameController.GetTimeBeforeRetryingDemand ();

		currentStep = GameStep.tutorial;

		errorRaised = false;
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

		case TimeLineClientLfp.RegisteredAsPlayerAsk:

			request.Clear ();
			request [KeyLfp.demand] = DemandLfp.registeredAsPlayer;
			request [KeyLfp.username] = userName;

			StartCoroutine (AskServer (request));

			state = TimeLineClientLfp.RegisteredAsPlayerWaitReply;

			LogState ();
			break;

		case TimeLineClientLfp.RoomAvailableAsk:

			request.Clear ();
			request [KeyLfp.username] = userName;
			request [KeyLfp.demand] = DemandLfp.roomAvailable;

			StartCoroutine (AskServer (request));

			state = TimeLineClientLfp.RoomAvailableWaitReply;

			LogState ();
			break;

		case TimeLineClientLfp.MissingPlayersAsk:

			request.Clear ();
			request [KeyLfp.demand] = DemandLfp.missingPlayers;
			request [KeyLfp.playerId] = playerId;

			StartCoroutine (AskServer (request));

			state = TimeLineClientLfp.MissingPlayersWaitReply;
			LogState ();
			break;

		case TimeLineClientLfp.ProceedToRegistrationAsPlayerAsk:

			request.Clear ();
			request [KeyLfp.demand] = DemandLfp.proceedToRegistrationAsPlayer;
			request [KeyLfp.username] = userName;

			StartCoroutine (AskServer (request));

			state = TimeLineClientLfp.ProceedToRegistrationAsPlayerWaitReply;
			LogState ();
			break;

		case TimeLineClientLfp.RegisteredAsPlayerWaitReply:
		case TimeLineClientLfp.ProceedToRegistrationAsPlayerWaitReply:
		case TimeLineClientLfp.RoomAvailableWaitReply:
		case TimeLineClientLfp.MissingPlayersWaitReply:

			if (HasResponse ()) {
				HandleServerResponse ();
			}
			break;

		case TimeLineClientLfp.WaitingCommand:
		case TimeLineClientLfp.RegisteredAsPlayerGotAnswer:
		case TimeLineClientLfp.RoomAvailableGotAnswer:
		case TimeLineClientLfp.MissingPlayersGotAnswer:
		case TimeLineClientLfp.ProceedToRegistrationAsPlayerGotAnswer:
			break;
		default:
			throw new Exception ("Bad state '" + state + "'");
		}
	}

	// --------------------- For treating response ------------------------------------------------------- //

	void ReplyRegisteredAsPlayer (string [] args) {

		if (state == TimeLineClientLfp.RegisteredAsPlayerWaitReply) {

			registeredAsPlayer = int.Parse (args [0]) == 1;
			if (registeredAsPlayer) {

				playerId = args [1];
				Debug.Log ("ClientLfp: my playerId is " + playerId + ".");

				currentStep = args [2];
				Debug.Log ("ClientLfp: current current step is " + currentStep + ".");

				if (currentStep != GameStep.end) {

					consumersFieldOfView = GameTools.TranslateFromFieldOfView (args [3]);
					displayOpponentScore = int.Parse (args [4]) == 1;
				}
			}

			state = TimeLineClientLfp.RegisteredAsPlayerGotAnswer;
			LogState ();

		} else {
			Debug.Log ("ClientLfp: Time problem or state problem. Maybe I already received a response.");
		}
	}

	void ReplyRoomAvailable (string [] args) {
		if (state == TimeLineClientLfp.RoomAvailableWaitReply) {

			roomAvailable = int.Parse (args [0]) == 1;

			state = TimeLineClientLfp.RoomAvailableGotAnswer;
			LogState ();

		} else {
			Debug.Log ("ClientLfp: Time problem or state problem. Maybe I already received a response.");
		}
	}

	void ReplyMissingPlayers (string [] args) {

		if (state == TimeLineClientLfp.MissingPlayersWaitReply) {

			missingPlayers = int.Parse (args [0]);

			if (missingPlayers < 0) {
				errorRaised = true;
				error = missingPlayers;
			}

			state = TimeLineClientLfp.MissingPlayersGotAnswer;
			LogState ();

		} else {
			Debug.Log("ClientLfp: Time problem or state problem. Maybe I already received a response.");
		}

	}

	void ReplyProceedToRegistrationAsPlayer (string [] args) {

		if (state == TimeLineClientLfp.ProceedToRegistrationAsPlayerWaitReply) {

			registeredAsPlayer = int.Parse (args [0]) == 1;

			if (registeredAsPlayer) {

				playerId = args [1];
				Debug.Log ("ClientLfp: my playerId is " + playerId + ".");

				currentStep = args [2];
				Debug.Log ("ClientLfp: current current step is " + currentStep + ".");

				consumersFieldOfView = GameTools.TranslateFromFieldOfView(args [3]);

				displayOpponentScore = int.Parse (args [4]) == 1;
			}

			state = TimeLineClientLfp.ProceedToRegistrationAsPlayerGotAnswer;
			LogState ();

		} else {
			Debug.Log("ClientLfp: Time problem or state problem. Maybe I already received a response.");
		}
	}

	// ------------------ General methods for communicating with the server --------- //

	bool HandleServerResponse () {

		string [] parts = serverResponse.Split (new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length > 1 && parts [0] == "reply") {

			string[] args = new string[parts.Length - 2];
			Array.Copy (parts, 2, args, 0, parts.Length - 2);

			MakeTheCorrectCall (parts[1], args);

			Debug.Log ("ClientLfp: Server response handled");
			return true;

		} else {
			Debug.Log ("ClientLfp: I couldn't handle the server response. Retry the same demand.");

			serverResponse = "";
			StartCoroutine (RetryDemand ());
			return false;
		}

	}

	void MakeTheCorrectCall (string what, string [] strArgs) {

		if (what == DemandLfp.registeredAsPlayer) { 
			ReplyRegisteredAsPlayer (strArgs);
		} else if (what == DemandLfp.roomAvailable) {
			ReplyRoomAvailable (strArgs); 
		} else if (what == DemandLfp.missingPlayers) {
			ReplyMissingPlayers (strArgs);
		} else if (what == DemandLfp.proceedToRegistrationAsPlayer) {
			ReplyProceedToRegistrationAsPlayer (strArgs);
		} else { 
			throw new System.Exception ("I received '" + what + "' with args '" + strArgs + "' but I can not catch that.");
		}
	}

	void LogState () {
		Debug.Log ("ClientLfp: My state is '" + state + "'.");
	}

	// ---------------------- For communicating with game controller ------------------------------ //

	public void SetUserName (string userName) {

		this.userName = userName;
	}

	// ------- State ------------- // 

	public void SetState(TimeLineClientLfp value) {
		state = value;
		LogState ();
	}

	public bool IsState (TimeLineClientLfp value) {
		return state == value;
	}

	// ----- 'classic' getters -------- //

	public int GetMissingPlayers () {
		return missingPlayers;
	}

	public string GetPlayerId () {
		return playerId;
	}

	public string GetCurrentStep () {
		return currentStep;
	}

	public bool GetRoomAvailable() {
		return roomAvailable;
	}

	public bool GetRegisteredAsPlayer () {
		return registeredAsPlayer;
	}

	public bool GetErrorRaised () {
		return errorRaised;
	}

	public int GetError () {
		return error;
	}

	public bool GetDisplayOpponentScore () {
		return displayOpponentScore;
	}

	public int[, ] GetConsumersFieldOfView () {
		return consumersFieldOfView;
	}

	// ----------------------- Communication ----------------- //

	public IEnumerator AskServer (Dictionary<string, string> request) {

		serverResponse = "";

		if (request != requestInMemory) {
			requestInMemory = request;
		}

		string toDebug = "ClientLfp: I send a request with post entries: ";

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
			Debug.Log ("ClientLfp: I got an error: '" + serverResponse + "'.");
		}
		else {
			serverResponse = www.downloadHandler.text;
			Debug.Log ("ClientLfp: I got a response: '" + serverResponse + "'.");
			if (string.IsNullOrEmpty (serverResponse)) {
				Debug.Log ("ClientF: Response is empty. I will consider it as an error");
				serverResponse = "EmptyResponse";
				serverError = true;
			}
		}
	}

	public bool HasResponse () {

		// Return if client recieves a response.
		if (!string.IsNullOrEmpty (serverResponse)) {

			Debug.Log ("ClientLfp: Got response from server.");

			if (serverError == false) {
				return true;
			} else {
				// In case of error consider that no response has been received.
				Debug.Log ("ClientLfp: I got error '" + serverResponse + "' and will retry the same demand.");

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
