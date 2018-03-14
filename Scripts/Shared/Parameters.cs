using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters : MonoBehaviour {

	public string url = "http://127.0.0.1:8000/";
	public float delayNewDemand = 1f;
	public float delayNewDemandMessenger = 1f;

	public string serverName = "Admin";

	public bool skipTutorial = false;

	string userName = "azer=:;,";
	string password = "azer=:;,";

	string playerId = "0";

	string currentStep = "tutorial";

	string clientRequest = "client_request/";
	string messenger = "messenger/";

	int[, ] consumersFieldOfView;

	bool displayOpponentScore = false;

	// Use this for initialization
	void Start () {

		DontDestroyOnLoad (this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool GetSkipTutorial () {
		return skipTutorial;
	}

	public void SetPlayerId (string value) {
		playerId = value;
	}

	public string GetPlayerId () {
		return playerId;
	}

	public void SetCurrentStep (string value) {
		currentStep = value;
	}

	public string GetCurrentStep () {
		return currentStep;
	}

	public void SetUserName (string value) {
		userName = value;
	}

	public string GetUserName () {
		return userName;
	}

	public void SetPassword (string value) {
		password = value;
	}

	public string GetPassword () {
		return password;
	}

	public string GetUrl() {
		return url + clientRequest;
	}

	public float GetTimeBeforeRetryingDemand() {
		return delayNewDemand;
	}

	public string GetUrlMessenger() {
		return url + messenger;
	}

	public float GetTimeBeforeRetryingDemandMessenger() {
		return delayNewDemandMessenger;
	}

	public void SetConsumersFieldOfView (int[,] value) {
		consumersFieldOfView = value;
	}

	public int[, ] GetConsumersFieldOfView () {
		return consumersFieldOfView;
	}

	public string GetServerName () {
		return serverName;
	}

	public bool GetDisplayOpponentScore () {
		return displayOpponentScore;
	}

	public void SetDisplayOpponentScore (bool value) {
		displayOpponentScore = value;
	}

}
