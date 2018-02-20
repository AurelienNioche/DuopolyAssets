using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using AssemblyCSharp;


public class GameControllerM : MonoBehaviour {

	UIControllerM uiController;
	TimeLineM state;

	string sceneToLoad;
	bool occupied = false;

	string sceneLogIn = "Scenes/LogIn";
	string sceneSignIn = "Scenes/SignIn";

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake () {

		uiController = GetComponent<UIControllerM> ();
	}

	void Start () {

		state = TimeLineM.WaitingUser;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	void Update () {

		if (!occupied) {

			occupied = true;
			ManageState ();
			occupied = false;
		}
	}

	// ------------------- Between user and player --------------------------- //

	void ManageState () {

		switch (state) {

		case TimeLineM.WaitingUser:
			break;

		case TimeLineM.LogIn:
			sceneToLoad = sceneLogIn;
			uiController.QuitScene ();
			state = TimeLineM.WaitEndAnimationQuitScene;
			break;

		case TimeLineM.SignIn:
			sceneToLoad = sceneSignIn;
			uiController.QuitScene ();
			state = TimeLineM.WaitEndAnimationQuitScene;
			break;

		case TimeLineM.WaitEndAnimationQuitScene:
			break;

		case TimeLineM.EndAnimationQuitScene:
			LoadNewScene ();
			break;
		
		case TimeLineM.Dead:

			break;
		
		default:
			throw new System.Exception ("GameController: Bad state.");
		}
	}

	// ----------- From UIManager ---------------- //

	public void EndAnimationQuitScene () {
		state = TimeLineM.EndAnimationQuitScene;
	}

	public void UserChooseSignIn () {
		state = TimeLineM.SignIn;
	}

	public void UserChooseLogIn () {
		state = TimeLineM.LogIn;
	}

	// ------------- Scene management --------------- //

	public void LoadNewScene () {
		SceneManager.LoadScene (sceneToLoad);
	}

	public void ReloadScene () {
		
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
}
