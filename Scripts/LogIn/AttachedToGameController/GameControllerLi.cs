using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using AssemblyCSharp;


public class GameControllerLi : MonoBehaviour {

	Parameters parameters;

	ClientLi client;
	UIControllerLi uiController;

	TimeLineLi state;

	string userName;
	string password;


	string sceneLookForPlaying = "Scenes/LookForPlaying";
	string sceneToLoad;

	bool occupied = false;

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake () {

		client = GetComponent<ClientLi> ();
		uiController = GetComponent<UIControllerLi> ();

		GetParameters ();
	}

	void Start () {

		state = TimeLineLi.WaitingUserIdentification;
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

		case TimeLineLi.WaitingUserIdentification:

			if (uiController.GotUserIdentification ()) {

				state = TimeLineLi.GotUserIdentification;

				LogState ();
			}
			break;

		case TimeLineLi.GotUserIdentification:

			userName = uiController.GetUserName ();
			password = uiController.GetUserPassword ();

			parameters.SetUserName (userName);
			parameters.SetPassword (password);

			client.AskConnection (userName, password);

			state = TimeLineLi.WaitingConnection;

			LogState ();
			break;

		case TimeLineLi.WaitingConnection:

			if (client.GotConnection ()) {
				state = TimeLineLi.GotConnection;

				LogState ();

			} else if (client.GotAnErrorOfIdentification ()) {
				state = TimeLineLi.ErrorIdenfication;

				LogState ();
			}
			break;
		
		case TimeLineLi.GotConnection:

			uiController.Connected ();

			sceneToLoad = sceneLookForPlaying;

			StartCoroutine (ActionWithDelay (uiController.QuitScene, 0.5f));

			state = TimeLineLi.WaitEndAnimationQuitScene;

			LogState ();
			break;

		case TimeLineLi.ErrorIdenfication:
			
			uiController.ErrorOfConnection ();
			state = TimeLineLi.WaitingUserIdentification;

			LogState ();
			break;
		
		case TimeLineLi.ComeBackHome:

			sceneToLoad = "SceneMain";
			uiController.QuitScene ();
			state = TimeLineLi.WaitEndAnimationQuitScene;

			LogState ();
			break;

		case TimeLineLi.WaitEndAnimationQuitScene:
			break;

		case TimeLineLi.EndAnimationQuitScene:

			LoadNewScene ();
			state = TimeLineLi.Dead;

			LogState ();
			break;

		case TimeLineLi.Dead:
			break;

		default:
			throw new System.Exception ("GameController: Bad state.");
		}
	}

	// ----------- From UIManager ---------------- //

	public void EndAnimationQuitScene () {
		state = TimeLineLi.EndAnimationQuitScene;
	}

	public void UserChooseHome() {
		state = TimeLineLi.ComeBackHome;
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
