using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AssemblyCSharp;
using UnityEngine.Networking;

class DemandF {

	public static string tutorialDone = "tutorial_done";
	public static string askFirmInit = "ask_firm_init";
	public static string askFirmPassiveOpponentChoice = "ask_firm_passive_opponent_choice";
	public static string askFirmPassiveConsumerChoices = "ask_firm_passive_consumer_choices";
	public static string askFirmActiveChoiceRecording = "ask_firm_active_choice_recording";
	public static string askFirmActiveConsumerChoices = "ask_firm_active_consumer_choices";
	public static string submitTutorialProgression = "submit_tutorial_progression";
}

class KeyF {
	
	public static string demand = "demand";
	public static string username = "username";
	public static string playerId = "player_id";
	public static string t = "t";
	public static string position = "position";
	public static string price = "price";
	public static string progressOnTutorial = "tutorial_progression";
} 

class CodeErrorF {

	public static int haveToWait = -1;
	public static int timeSuperior = -2;
	public static int opponentDisconnected = -3;
	public static int playerDisconnected = -4;
}


public class ClientF : MonoBehaviour {

	GameControllerF gameController;
	TLClientF state;

	// ---------- For communication with the server -------------------------- //

	string url;
	float timeBeforeRetryingDemand; 

	bool serverError;
	string serverResponse;

	Dictionary<string, string> request;
	Dictionary<string, string> requestInMemory;

	// ------------------------------------------------------------------- //

	int error;
	bool fatalError;

	string roomId;
	string playerId;

	string stateOfPlay;
	string initialStateOfPlay;
	string progressOtherPlayer;

	float progressOnTutorial;

	int t;
	int endingT;
	int position;
	int price;
	int score;
	int opponentPosition;
	int opponentPrice;
	int opponentScore;
	int endGame;

	int[] consumerChoicesActive;
	int[] consumerChoicesPassive;

	// -------------------------------------------------------------------- //

	bool occupied = false;

	void Awake () {

		gameController = GetComponent <GameControllerF> ();
	}


	// Use this for initialization
	void Start () {

		consumerChoicesActive = new int[GameFeatures.nPositions];
		consumerChoicesPassive = new int[GameFeatures.nPositions];

		request = new Dictionary<string, string> ();

		url = gameController.GetUrl ();
		timeBeforeRetryingDemand = gameController.GetTimeBeforeRetryingDemand ();
		playerId = gameController.GetPlayerId ();

		state = TLClientF.WaitCommand;
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

		case TLClientF.WaitCommand:
			break;

		case TLClientF.TutorialDone:

			request.Clear ();
			request [KeyF.demand] = DemandF.tutorialDone;
			request [KeyF.playerId] = playerId;

			foreach (KeyValuePair<string, string> entry in request) {
				Debug.Log (entry.Key + " " + entry.Value);
			}

			StartCoroutine (AskServer (request));

			state = TLClientF.TutorialDoneWaitReply;
			LogState ();
			break;

		case TLClientF.Init:

			request.Clear ();
			request [KeyF.playerId] = playerId;
			request [KeyF.demand] = DemandF.askFirmInit;

			StartCoroutine (AskServer (request));

			state = TLClientF.InitWaitReply;
			LogState ();
			break;

		case TLClientF.PassiveOpponentChoice:

			request.Clear ();
			request [KeyF.playerId] = playerId;
			request [KeyF.t] = t.ToString ();
			request [KeyF.demand] = DemandF.askFirmPassiveOpponentChoice;

			StartCoroutine (AskServer (request));

			state = TLClientF.PassiveOpponentChoiceWaitReply;
			LogState ();
			break;

		case TLClientF.PassiveConsumerChoices:

			request.Clear ();
			request [KeyF.playerId] = playerId;
			request [KeyF.t] = t.ToString ();
			request [KeyF.demand] = DemandF.askFirmPassiveConsumerChoices;

			StartCoroutine (AskServer (request));

			state = TLClientF.PassiveConsumerChoicesWaitReply;
			LogState ();
			break;

		case TLClientF.ActiveChoiceRecording:

			request.Clear ();
			request [KeyF.playerId] = playerId;
			request [KeyF.t] = t.ToString ();
			request [KeyF.position] = position.ToString ();
			request [KeyF.price] = price.ToString ();
			request [KeyF.demand] = DemandF.askFirmActiveChoiceRecording;

			StartCoroutine (AskServer (request));

			state = TLClientF.ActiveChoiceRecordingWaitReply;
			LogState ();
			break;

		case TLClientF.ActiveConsumerChoices:

			request.Clear ();
			request [KeyF.playerId] = playerId;
			request [KeyF.t] = t.ToString ();
			request [KeyF.demand] = DemandF.askFirmActiveConsumerChoices;

			StartCoroutine (AskServer (request));

			state = TLClientF.ActiveConsumerChoicesWaitReply;
			LogState ();
			break;
		
		case TLClientF.SubmitTutorialProgression:

			request [KeyF.playerId] = playerId;
			request [KeyF.demand] = DemandF.submitTutorialProgression;
			request [KeyF.progressOnTutorial] = progressOnTutorial.ToString ();

			StartCoroutine (AskServer (request));

			state = TLClientF.SubmitTutorialProgressionWaitReply;
			LogState ();
			break;


		case TLClientF.GotTutorialDone:
		case TLClientF.GotInit:
		case TLClientF.GotActiveChoiceRecording:
		case TLClientF.GotActiveConsumerChoices:
		case TLClientF.GotPassiveConsumerChoices:
		case TLClientF.GotPassiveOpponentChoice:
		case TLClientF.GotSubmitTutorialProgression:
			break;

		case TLClientF.TutorialDoneWaitReply:
		case TLClientF.InitWaitReply:
		case TLClientF.PassiveOpponentChoiceWaitReply:
		case TLClientF.PassiveConsumerChoicesWaitReply:
		case TLClientF.ActiveChoiceRecordingWaitReply:
		case TLClientF.ActiveConsumerChoicesWaitReply:
		case TLClientF.SubmitTutorialProgressionWaitReply:

			if (HasResponse ()) {
				HandleServerResponse ();
			}
			break;

		default: 
			throw new Exception ("ClientF: The state '" + state + "' is not handled.");
		}
	}

	void LogState () {
		Debug.Log ("ClientF: My state is '" + state + "'.");
	}

	// --------------------- Init ------------------------------------------------------- //

	public void AskInit () {
		
		state = TLClientF.Init;
		LogState ();

	}

	void ReplyFirmInit(string [] args) {

		TLClientF nextState = TLClientF.GotInit;

		int firstArg = int.Parse (args [0]);

		if (firstArg < 0) {
			TreatError (firstArg, args, nextState);
		
		} else {

			t = firstArg; 
			Debug.Log ("ClientF: t is " + t + ".");

			stateOfPlay = args[1];		
			initialStateOfPlay = stateOfPlay;
			Debug.Log("ClientF: my initial state of play is " + stateOfPlay + ".");

			position = int.Parse (args [2]);
			Debug.Log("ClientF: my initial position is " + position + ".");

			price = int.Parse(args[3]);
			Debug.Log("ClientF: my initial price is " + price + ".");

			score = int.Parse (args [4]);
			Debug.Log("ClientF: initial score is " + score + ".");

			opponentPosition = int.Parse(args[5]);
			Debug.Log("ClientF: intial opponent position is " + opponentPosition + ".");

			opponentPrice = int.Parse (args [6]);
			Debug.Log("ClientF: initial opponent price is " + opponentPrice + ".");

			opponentScore = int.Parse (args [7]);
			Debug.Log ("ClientF: initial opponent score is " + opponentScore + ".");

			endingT = int.Parse (args [8]);
			Debug.Log ("ClientF: ending t is " + endingT + ".");

			state = nextState;
		}
		LogState ();
	}

	// ------------------- Treat errors -------------------------------------------- //

	public void TreatError (int firstArg, string[] args, TLClientF nextState) {

		error = firstArg;

		if (error == CodeErrorF.haveToWait) {

			if (nextState == TLClientF.GotInit) {

				t = firstArg;

				// Get progress of other player
				progressOtherPlayer = args [1];		
				Debug.Log ("ClientF: Have to wait for init. Progress of other player: " + progressOtherPlayer);

				state = nextState;
			
			} else {
				Debug.Log ("ClientF: Have to wait.");
				StartCoroutine (RetryDemand ());
			}
		
		} else if (error == CodeErrorF.timeSuperior) {
			Debug.Log ("ClientF: Time is superior.");
			StartCoroutine (RetryDemand ());
		
		} else {
			fatalError = true;
			state = nextState;
		}
		
	}

	// --------------------- Firm communication ------------------------------------------ //

	public void AskTutorialDone () {

		state = TLClientF.TutorialDone;
		LogState ();
	}

	public void ReplyTutorialDone () {
		state = TLClientF.GotTutorialDone;
		LogState ();
	}

	public void AskFirmPassiveOpponentChoice () {

		state = TLClientF.PassiveOpponentChoice;
		LogState ();
	}

	void ReplyFirmPassiveOpponentChoice (string [] args) {

		TLClientF nextState = TLClientF.GotPassiveOpponentChoice;

		int firstArg = int.Parse (args [0]);

		if (firstArg < 0) {
			TreatError (firstArg, args, nextState);

		} else {

			opponentPosition = int.Parse (args [1]);
			opponentPrice = int.Parse (args [2]); 

			state = nextState;
			LogState ();
		}
	}

	public void AskFirmPassiveConsumerChoices () {

		state = TLClientF.PassiveConsumerChoices;
	}

	void ReplyFirmPassiveConsumerChoices (string [] args) {

		TLClientF nextState = TLClientF.GotPassiveConsumerChoices;

		int firstArg = int.Parse (args [0]);

		if (firstArg < 0) {
			TreatError (firstArg, args, nextState);

		} else {

			for (int i = 0; i < GameFeatures.nPositions; i++) {
				consumerChoicesPassive [i] = int.Parse (args [1 + i]);
			}

			endGame = int.Parse (args [1 + GameFeatures.nPositions]);

			if (endGame == 0) {
				t++;
			}

			state = TLClientF.GotPassiveConsumerChoices;
			LogState ();
		}
	}

	public void AskFirmActiveChoiceRecording (int userPosition, int userPrice) {

		position = userPosition;
		price = userPrice;

		state = TLClientF.ActiveChoiceRecording;
		LogState ();
	}

	void ReplyFirmActiveChoiceRecording(string [] args) {

		TLClientF nextState = TLClientF.GotActiveChoiceRecording;

		int firstArg = int.Parse (args [0]);

		if (firstArg < 0) {
			TreatError (firstArg, args, nextState);

		} else {

			state = TLClientF.GotActiveChoiceRecording;
			LogState ();

		}
	}

	public void AskFirmActiveConsumerChoices () {

		state = TLClientF.ActiveConsumerChoices;
	}

	void ReplyFirmActiveConsumerChoices (string [] args) {

		TLClientF nextState = TLClientF.GotActiveConsumerChoices;

		int firstArg = int.Parse (args [0]);

		if (firstArg < 0) {
			TreatError (firstArg, args, nextState);
		
		} else {

			for (int i = 0; i < GameFeatures.nPositions; i++) {
				consumerChoicesActive [i] = int.Parse (args [1 + i]);
			}
			endGame = int.Parse (args [1 + GameFeatures.nPositions]);

			if (endGame == 0) {
				t++;
			}

			state = nextState;
			LogState ();
		}
	}

	public void SubmitTutorialProgression (float value) {
		progressOnTutorial = value;
		state = TLClientF.SubmitTutorialProgression;
	}

	public void ReplySubmitTutorialProgression () {
		state = TLClientF.GotSubmitTutorialProgression;
	}


	// ------------------ General methods for communicating with the server --------- //

	void MakeTheCorrectCall (string what, string [] strArgs) {

		if (what == DemandF.tutorialDone && state == TLClientF.TutorialDoneWaitReply) {
			ReplyTutorialDone ();
		} else if (what == DemandF.askFirmInit && state == TLClientF.InitWaitReply) {
			ReplyFirmInit (strArgs);
		} else if (what == DemandF.askFirmPassiveOpponentChoice && state == TLClientF.PassiveOpponentChoiceWaitReply) {
			ReplyFirmPassiveOpponentChoice (strArgs);
		} else if (what == DemandF.askFirmPassiveConsumerChoices && state == TLClientF.PassiveConsumerChoicesWaitReply) {
			ReplyFirmPassiveConsumerChoices (strArgs);
		} else if (what == DemandF.askFirmActiveChoiceRecording && state == TLClientF.ActiveChoiceRecordingWaitReply) {
			ReplyFirmActiveChoiceRecording (strArgs);
		} else if (what == DemandF.askFirmActiveConsumerChoices && state == TLClientF.ActiveConsumerChoicesWaitReply) {
			ReplyFirmActiveConsumerChoices (strArgs);
		} else if (what == DemandF.submitTutorialProgression && state == TLClientF.SubmitTutorialProgressionWaitReply) {
			ReplySubmitTutorialProgression ();
		} else {
			Debug.Log ("ClientF:  State problem (or bad function name).");
		}
	}

	// --------------------------------------- Public: Relating to game state ---------------------------------- // 

	public bool IsState (TLClientF value) {
		return state == value;
	}

	public void SetState (TLClientF value) {
		state = value;
	}

	// --------------------- Public: Getters ---------------- //

	public string GetInitialStateOfPlay () {
		return initialStateOfPlay;  // "passive" or "active"
	}

	public int GetPrice () {
		return price;
	}

	public int GetPosition () {
		return position;
	}

	public int GetOpponentPosition () {
		return opponentPosition;
	}

	public int GetOpponentPrice () {
		return opponentPrice;
	}

	public int[] GetConsumerChoicesActive () {
		return consumerChoicesActive;
	}		

	public int[] GetConsumerChoicesPassive () {
		return consumerChoicesPassive;
	}

	public int GetScore () {
		return score;
	}

	public int GetOpponentScore () {
		return opponentScore;
	}

	public int GetEndGame () {
		return endGame;
	}

	public int GetTime () {
		return t;
	}

	public int GetEndingT () {
		return endingT;
	}

	public string GetProgressOtherPlayer () {
		return progressOtherPlayer;
	}

	public bool GotFatalError () {
		return fatalError;
	}

	public int GetError () {
		return error;
	}


	// ----------------------- Communication ----------------- //

	public IEnumerator AskServer (Dictionary<string, string> request) {

		serverResponse = "";

		if (request != requestInMemory) {
			requestInMemory = request;
		}

		string toDebug = "ClientF: I send a request with post entries: ";

		WWWForm form = new WWWForm();
		foreach(KeyValuePair<string, string> entry in request) {
			toDebug += "('" + entry.Key + "', '" + entry.Value + "')";
			form.AddField (entry.Key, entry.Value);
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
			Debug.Log ("ClientF: I got an error: '" + serverResponse + "'.");
		}
		else {
			serverResponse = www.downloadHandler.text;
			Debug.Log ("ClientF: I got a response: '" + serverResponse + "'.");
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

			if (serverError == false) {
				return true;

			} else {

				StartCoroutine (RetryDemand ());
				return false;
			}
		} else {
			return false;
		}	
	}

	IEnumerator RetryDemand () {

		serverResponse = "";
		yield return new WaitForSeconds (timeBeforeRetryingDemand);
		StartCoroutine (AskServer (requestInMemory));
	}

	string FormulateRequest (string demand, string [] args) {
		return demand + "/" + string.Join("/", args);
	}

	void HandleServerResponse () {

		string [] parts = serverResponse.Split (new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length > 1 && parts [0] == "reply") {

			string[] args = new string[parts.Length - 2];
			Array.Copy (parts, 2, args, 0, parts.Length - 2);
			MakeTheCorrectCall (parts [1], args);

		} else {

			Debug.Log ("ClientF: I couldn't handle the server response ('" + serverResponse + "'). Retry the same demand.");
			StartCoroutine (RetryDemand ());
		}
	}
}
