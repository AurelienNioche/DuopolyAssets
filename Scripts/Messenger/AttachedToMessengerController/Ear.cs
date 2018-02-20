using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Ear : MonoBehaviour {

	public bool debug = false;

	string url;
	float timeBeforeRetryingDemand; 

	string messageInMemory;
	string demandTypeInMemory;
	string serverResponse;
	string[] responseParts; 

	bool serverError;

	bool isOccupied;

	List<string> queueServerResponse;

	bool isEaring; 
	bool isWorking;

	Parameters parameters;

	void Awake () {
		GetParameters ();
	}

	// Use this for initialization
	void Start () {

		isOccupied = false;

		queueServerResponse = new List<string> ();
		isEaring = false;

		url = parameters.GetUrlMessenger ();
		timeBeforeRetryingDemand = parameters.GetTimeBeforeRetryingDemandMessenger (); 
	}
	
	// Update is called once per frame
	void Update () {

		if (!isWorking) {
			isWorking = true;
			if (HasResponse ()) {
				TreatServerResponse ();
			}
			if (!isEaring) {
				StartCoroutine(Hear ());
			}
			isWorking = false;
		}
	}

	// ------------------ Get parameters ---------------------------- //

	void GetParameters () {

		GameObject[] gos = GameObject.FindGameObjectsWithTag ("Parameters");
		if (gos.Length == 0) {
			parameters = GameObject.FindGameObjectWithTag ("GameController").GetComponent<Parameters> ();
		} else {
			parameters = GameObject.FindGameObjectWithTag ("Parameters").GetComponent<Parameters> ();
		}
		if (debug) {
			Debug.Log ("Ear: I got parameters.");
		}	
	}
	 // --------------------------------------------------------------------- //

	IEnumerator Hear () {

		isEaring = true;
		if (!IsOccupied () && !HasResponse()) {

			if (debug) {
				Debug.Log ("Ear: Ask for new messages.");
			}

			StartCoroutine(AskServer ("none", "client_hears"));
		}
		yield return new WaitForSeconds (timeBeforeRetryingDemand);
		isEaring = false;
	}

	public bool HeardSomething () {
		return queueServerResponse.Count > 0;
	}
		
	public string GetMessage () {
		string message = queueServerResponse[0];
		queueServerResponse.Remove (message);
		return message;
	}	

	void TreatServerResponse () {

		string[] responseParts = GetServerResponseByParts ();

		if (responseParts [1] == "done.") {

			if (debug) {
				Debug.Log ("I got a confirmation that my receipt confirmation has been received.");
			}
		
		} else {

			// Part 0 is 'reply'
			// Part 1 is the number of messages
			// Following parts are the messages themselves

			int nMessages = int.Parse(responseParts[1]);
			if (nMessages > 0) {

				if (debug) {
					Debug.Log ("Ear: I received " + nMessages + ".");
				}

				string toReply = "";

				for (int i = 2; i < nMessages + 2; i++) {

					string message = responseParts [i];
					queueServerResponse.Add (message);
					toReply += "/" + message;
				}
				StartCoroutine (AskServer (toReply, "client_receipt_confirmation"));
			} else {
				if (debug) {
					Debug.Log ("Ear: I didn't receive new messages.");
				}

			}
		}
	}

	// --------------------- Communication ------------------------- //

	public IEnumerator AskServer (string message, string demandType) {

		isOccupied = true;

		WWWForm form = new WWWForm();

		if (message != messageInMemory) {
			messageInMemory = message;
		}
		demandTypeInMemory = demandType;

		if (debug) {
			Debug.Log ("Ear: demandType is '" + demandType + "'.");
			Debug.Log ("Ear: name is '" + parameters.GetUserName () + "'.");
			Debug.Log ("Ear: message is '" + message + "'.");
			Debug.Log ("Ear: url is '" + url + "'.");
		}

		form.AddField ("username", parameters.GetUserName ());
		form.AddField( "demand", demandType );
		form.AddField( "message", message );

		UnityWebRequest www = UnityWebRequest.Post(url, form);
		www.chunkedTransfer = false;
		// www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

		serverError = false;

		yield return www.SendWebRequest();

		if (www.error != null) {
			serverError = true;
			serverResponse = www.error;
		}
		else {
			serverResponse = www.downloadHandler.text;
			if (string.IsNullOrEmpty (serverResponse)) {
				serverResponse = "EmptyResponse";
				serverError = true;
			}
		}

		isOccupied = false;
	}

	public bool HasResponse () {

		// Return if client recieves a response.
		if (!string.IsNullOrEmpty (serverResponse)) {

			if (debug) {
				Debug.Log ("Ear: Got response from server.");
			}

			if (serverError == false) {

				responseParts = serverResponse.Split (new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

				if (responseParts.Length > 1 && responseParts [0] == "reply") {

					return true;

				} else {

					serverResponse = "";
					StartCoroutine (RetryDemand ());
					return false;
				}

			} else {
				// In case of error consider that no response has been received.
				if (debug) {
					Debug.Log ("Ear: I got error '" + serverResponse + "' and will retry the same demand.");
				}

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
		StartCoroutine (AskServer (messageInMemory, demandTypeInMemory));
	}

	// ----------------------------------------------------- //

	public string GetServerResponse () {
		// Return sever response and erase it just 'after' (in reality just before).
		serverResponse = "";
		return responseParts[1];
	}

	public string[] GetServerResponseByParts () {
		serverResponse = "";
		return responseParts;
	}

	public bool IsOccupied (){
		return isOccupied;
	}
}
