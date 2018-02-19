using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using AssemblyCSharp;


public class GameControllerSi : MonoBehaviour {

	Parameters parameters;

	ClientSi client;
	UIControllerSi uiController;

	TimeLineSi state;

	string sceneMain = "Scenes/Main";
	string sceneLogIn = "Scenes/LogIn";

	string sceneToLoad;
	bool occupied = false;

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake () {

		client = GetComponent<ClientSi> ();
		uiController = GetComponent<UIControllerSi> ();

		GetParameters ();
	}

	void Start () {

		state = TimeLineSi.WaitingUserFillIn;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	void Update () {

		if (!occupied) {

			occupied = true;
			ManageState ();
			occupied = false;
		}
	}

	// ------------------------ Parameters -------------------------------- //

	void GetParameters () {

		GameObject[] gos = GameObject.FindGameObjectsWithTag ("Parameters");

		if (gos.Length == 0) {
			gameObject.AddComponent<Parameters> ();
			parameters = GetComponent<Parameters> ();

		} else {
			parameters = gos [0].GetComponent<Parameters> ();
		}
	}

	// ------------------- Between user and client --------------------------- //

	void ManageState () {

		switch (state) {

		case TimeLineSi.WaitingUserFillIn:
			break;
		
		case TimeLineSi.UserSubmit:

			client.SendForm (uiController.GetUserData ());
			state = TimeLineSi.SendForm;
			LogState ();
			break;
		
		case TimeLineSi.UserSendAgain:

			client.SendAgain ();
			state = TimeLineSi.SendAgain;
			LogState ();
			break;

		case TimeLineSi.SendForm:
		case TimeLineSi.SendAgain:
			// Wait for client to got a response from server
			break;

		case TimeLineSi.FormSent:
		case TimeLineSi.SendAgainGotReply:

			uiController.ProposeToLogin ();
			state = TimeLineSi.WaitUserLogIn;
			LogState ();
			break;

		case TimeLineSi.SendingFailed:
			
			uiController.SendingFailed ();
			state = TimeLineSi.WaitingUserFillIn;
			LogState ();
			break;

		case TimeLineSi.AlreadyExists:

			uiController.AlreadyExists ();
			state = TimeLineSi.WaitingUserFillIn;
			LogState ();
			break;

		case TimeLineSi.WaitUserLogIn:
			break;
		
		case TimeLineSi.ComeBackHome:

			sceneToLoad = sceneMain;
			uiController.QuitScene ();
			state = TimeLineSi.WaitEndAnimationQuitScene;
			LogState ();
			break;
		
		case TimeLineSi.LogIn:

			sceneToLoad = sceneLogIn;
			uiController.QuitScene ();
			state = TimeLineSi.WaitEndAnimationQuitScene;
			LogState ();
			break;
		
		case TimeLineSi.WaitEndAnimationQuitScene:
			break;
		
		case TimeLineSi.EndAnimationQuitScene:

			LoadNewScene ();
			state = TimeLineSi.Dead;
			LogState ();
			break;

		case TimeLineSi.Dead:
			break;

		default:
			throw new System.Exception ("GameController: Bad state.");
		}
	}

	// ----------- From UIManager ---------------- //

	public void EndAnimationQuitScene () {
		state = TimeLineSi.EndAnimationQuitScene;
	}
		
	public void UserChooseLogIn () {
		state = TimeLineSi.LogIn;
	}

	public void UserChooseSubmit () {
		state = TimeLineSi.UserSubmit;
	}

	public void UserChooseSendAgain () {
		state = TimeLineSi.UserSendAgain;
	}

	public void SendAgainGotReply () {
		state = TimeLineSi.SendAgainGotReply;
	}

	public void UserChooseHome() {
	
		state = TimeLineSi.ComeBackHome;
	}

	public void FormSent () {
		state = TimeLineSi.FormSent;
	}

	public void SendingFailed () {
		state = TimeLineSi.SendingFailed;
	}

	public void AlreadyExists() {
		state = TimeLineSi.AlreadyExists;
	}

	// ------------- Scene management --------------- //

	public void LoadNewScene () {
		SceneManager.LoadScene (sceneToLoad);
	}

	public void ReloadScene ()
	{
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}

	// ------------------------------ //

	void LogState() {
		Debug.Log ("GameController: My state is '" + state + "'.");
	}

	IEnumerator ActionWithDelay (Action methodName, float seconds) {
		yield return new WaitForSeconds(seconds);

		methodName ();
	}

	// --------------- Relative to parameters ------------- //

	public string GetUrl() {
		return parameters.GetUrl ();
	}

	public float GetTimeBeforeRetryingDemand () {
		return parameters.GetTimeBeforeRetryingDemand ();
	}
}
