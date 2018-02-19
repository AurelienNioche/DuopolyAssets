using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Mouth : MonoBehaviour {

	public bool debug = false;

	string url;
	float timeBeforeRetryingDemand; 

	string messageInMemory;
	string demandTypeInMemory;
	string serverResponse;
	string[] responseParts; 

	bool serverError;

	bool isOccupied;

	Parameters parameters;

	void Awake () {
		GetParameters ();
	}

	void Start () {
		isOccupied = false;

		url = parameters.GetUrlMessenger ();
		timeBeforeRetryingDemand = parameters.GetTimeBeforeRetryingDemandMessenger (); 
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
			Debug.Log ("Mouth: I got parameters.");
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
			Debug.Log ("Mouth: demandType is '" + demandType + "'.");
			Debug.Log ("Mouth: name is '" + parameters.GetUserName () + "'.");
			Debug.Log ("Mouth: message is '" + message + "'.");
			Debug.Log ("Mouth: url is '" + url + "'.");
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
		}

		isOccupied = false;
	}

	public bool HasResponse () {

		// Return if client recieves a response.
		if (!string.IsNullOrEmpty (serverResponse)) {

			if (debug) {
				Debug.Log ("Mouth: Got response from server.");
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
					Debug.Log ("Mouth: I got error '" + serverResponse + "' and will retry the same demand.");
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
