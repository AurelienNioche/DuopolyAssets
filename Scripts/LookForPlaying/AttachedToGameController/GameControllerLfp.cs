using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using AssemblyCSharp;


public class GameControllerLfp : MonoBehaviour {

	Parameters parameters;

	ClientLfp client;
	UIControllerLfp uiController;

	TimeLineLfp state;

	string sceneFirm = "Scenes/Firm";
	string tagParameters = "Parameters";

	string sceneToLoad;
	bool occupied = false;

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake () {

		GetParameters ();

		client = GetComponent<ClientLfp> ();
		uiController = GetComponent<UIControllerLfp> ();
	}

	void Start () {

		state = TimeLineLfp.RegisteredAsPlayerAsk;
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

		GameObject[] gos = GameObject.FindGameObjectsWithTag (tagParameters);
		if (gos.Length == 0) {
			gameObject.AddComponent<Parameters> ();
			parameters = GetComponent<Parameters> ();
		} else {
			parameters = gos[0].GetComponent<Parameters> ();
		}
	}

	// ------------------- Between user and client --------------------------- //

	void ManageState () {

		switch (state) {

		case TimeLineLfp.RegisteredAsPlayerAsk:

			client.SetUserName (parameters.GetUserName());
			client.SetState (TimeLineClientLfp.RegisteredAsPlayerAsk);

			state = TimeLineLfp.RegisteredAsPlayerWaitReply;
			LogState ();
			break;
		
		case TimeLineLfp.RegisteredAsPlayerWaitReply:

			if (client.IsState (TimeLineClientLfp.RegisteredAsPlayerGotAnswer)) {

				if (client.GetRegisteredAsPlayer ()) {

					if (client.GetCurrentStep() == GameStep.end) {

						uiController.ShowMessageAlreadyPlayed ();
						state = TimeLineLfp.Dead;
					
					} else {

						parameters.SetPlayerId (client.GetPlayerId ());
						state = TimeLineLfp.MissingPlayersAsk;
					
					}
				} else {
					state = TimeLineLfp.RoomAvailableAsk; 
				}
			}
			break;
		
		case TimeLineLfp.RoomAvailableAsk:

			client.SetState (TimeLineClientLfp.RoomAvailableAsk);
		
			state = TimeLineLfp.RoomAvailableWaitReply;

			LogState ();
			break;

		case TimeLineLfp.RoomAvailableWaitReply:

			if (client.IsState (TimeLineClientLfp.RoomAvailableGotAnswer)) {

				if (client.GetRoomAvailable ()) {
					uiController.Participation ();

					state = TimeLineLfp.WaitingForUserToParticipate;
					LogState ();
					
				} else {
					uiController.NoRoomAvailable ();

					client.SetState (TimeLineClientLfp.WaitingCommand);
					StartCoroutine (SetClientStateWithDelay (TimeLineClientLfp.RoomAvailableAsk));
				}
			
			}
			break;

		case TimeLineLfp.WaitingForUserToParticipate:
			
			if (client.IsState (TimeLineClientLfp.RoomAvailableGotAnswer)) {

				if (client.GetRoomAvailable ()) {
					if (uiController.GotUserParticipaton ()) {

						state = TimeLineLfp.GotUserParticipation;
						LogState ();

					} else {

						client.SetState (TimeLineClientLfp.WaitingCommand);
						StartCoroutine (SetClientStateWithDelay (TimeLineClientLfp.RoomAvailableAsk));

					}
				} else {
					uiController.NoRoomAvailable ();
					client.SetState (TimeLineClientLfp.RoomAvailableAsk);
					state = TimeLineLfp.RoomAvailableWaitReply;
					LogState ();
				}
			}
				
			break;

		case TimeLineLfp.GotUserParticipation:

			client.SetState (TimeLineClientLfp.ProceedToRegistrationAsPlayerAsk);

			state = TimeLineLfp.ProceedToRegistrationAsPlayerWaitReply;
			LogState ();
			break;

		case TimeLineLfp.ProceedToRegistrationAsPlayerWaitReply:

			if (client.IsState (TimeLineClientLfp.ProceedToRegistrationAsPlayerGotAnswer)) {

				if (client.GetRegisteredAsPlayer ()) {

					state = TimeLineLfp.MissingPlayersAsk;
					LogState ();

				} else {

					uiController.NoRoomAvailable ();

					state = TimeLineLfp.RoomAvailableAsk;
					LogState ();
				}
			}
			break;

		case TimeLineLfp.MissingPlayersAsk:

			client.SetState (TimeLineClientLfp.MissingPlayersAsk);
			state = TimeLineLfp.MissingPlayersWaitReply;
			LogState ();
			break;

		case TimeLineLfp.MissingPlayersWaitReply:

			if (client.IsState (TimeLineClientLfp.MissingPlayersGotAnswer)) {

				if (client.GetErrorRaised ()) {

					if (client.GetError () == CodeErrorLfp.playerDisconnected) {
						uiController.ShowMessagePlayerDisconnected ();
					} else {
						uiController.ShowMessageOpponentDisconnected ();
					}

					state = TimeLineLfp.Dead;
					LogState ();

				} else if (client.GetMissingPlayers () == 0) {

					uiController.WaitingForPlay ();

					state = TimeLineLfp.PrepareNewScene;
					LogState ();
				
				} else {

					uiController.UpdateMissingPlayers (client.GetMissingPlayers ());

					client.SetState (TimeLineClientLfp.WaitingCommand);
					StartCoroutine (SetClientStateWithDelay (TimeLineClientLfp.MissingPlayersAsk));
				}
			} 
			break;

		case TimeLineLfp.PrepareNewScene:
			
			parameters.SetPlayerId (client.GetPlayerId ());
			parameters.SetCurrentStep (client.GetCurrentStep ());

			// Load next scene
			sceneToLoad = sceneFirm;

			uiController.QuitScene ();

			state = TimeLineLfp.WaitEndAnimationQuitScene;
			LogState ();
			break;
		
		case TimeLineLfp.WaitEndAnimationQuitScene:
			break;

		case TimeLineLfp.EndAnimationQuitScene:

			LoadNewScene ();
			state = TimeLineLfp.Dead;
			LogState ();
			break;

		case TimeLineLfp.Dead:
			break;

		default:
			throw new System.Exception ("GC (LookForPlaying): Bad state ('" + state + "').");
		}
	}

	// ----------- From UIManager ---------------- //

	public void EndAnimationQuitScene () {
		state = TimeLineLfp.EndAnimationQuitScene;
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

	void LogState () {
		Debug.Log ("GC (LookForPlaying): My state is '" + state + "'.");
	}

	// ------------------------------------------------ //

	IEnumerator ActionWithDelay (Action methodName, float seconds) {
		yield return new WaitForSeconds(seconds);

		methodName ();
	}

	IEnumerator SetClientStateWithDelay (TimeLineClientLfp state) {
		float seconds = parameters.GetTimeBeforeRetryingDemand ();
		yield return new WaitForSeconds(seconds);
		client.SetState (state);
	}

	// --------------- Relative to parameters ------------- //

	public string GetUrl () {
		return parameters.GetUrl ();
	}

	public float GetTimeBeforeRetryingDemand () {
		return parameters.GetTimeBeforeRetryingDemand ();
	}

	public void SetConsumersFieldOfView (int[,] value) {
		parameters.SetConsumersFieldOfView (value);
	}
}
