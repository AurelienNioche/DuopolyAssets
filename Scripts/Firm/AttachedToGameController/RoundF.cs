using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using AssemblyCSharp;


public class RoundF : MonoBehaviour {

	TLRoundF stateGame;

	ClientF client;
	PopulationControllerF populationController;
	UIControllerF uiController;
	GameControllerF gameController;
	ScoresManagerF scoreManager;
	ACF ac;

	float timeOfInit;
	float timeForInitialAnimation = 1.0f;

	int haveToWait = -1;
	int playerDisconnected = -3;


	// Use this for initialization
	void Start () {
		stateGame = TLRoundF.Init;
	}
	
	// Update is called once per frame
	void Update () {
	}

	void Awake () {
		// Get components
		client = GetComponent<ClientF> ();
		populationController = GetComponent<PopulationControllerF> ();
		uiController = GetComponent<UIControllerF> ();
		gameController = GetComponent<GameControllerF> ();
		scoreManager = GetComponent<ScoresManagerF> ();
		ac = GetComponent<ACF> ();
	}

	public void ManageState () {

		switch (stateGame) {

		case TLRoundF.Init:

			//client.AskInit (parameters.GetRoomId (), parameters.GetPlayerId ());
			client.AskInit ();

			timeOfInit = Time.time;

			RoundNextStep ();
			break;

		case TLRoundF.WaitingInfo:

			if (client.IsState (TLClientF.GotInit)) {

				if (client.GetTime () == haveToWait) {
					client.AskInit ();
					uiController.ShowMessageProgressOtherPlayer (client.GetProgressOtherPlayer ());
				} else if (client.GetTime() == playerDisconnected) {
					stateGame = TLRoundF.EndOfGame;
					gameController.PlayerDisconnected ();
				} else {
	
					float delta = Time.time - timeOfInit;
					if (delta > timeForInitialAnimation) { 
						RoundNextStep ();
					}
				
				}
			}
			break;

		case TLRoundF.GotInfo:

			// Instruction to scoreManager
			scoreManager.SetCumulativeScores (client.GetScore (), client.GetOpponentScore ());

			// Instructions for uiController
			uiController.Initialize (client.GetPosition (), client.GetPrice (), client.GetOpponentPrice (), 
				client.GetScore (), client.GetOpponentScore ());

			uiController.ChangeSelectedTurn (Turn.none);

			uiController.HideMenu ();

			ac.HUDOpponent.SetBool(Bool.visible, true);
			ac.HUDPlayer.SetBool(Bool.visible, true);

			ac.turns.SetBool(Bool.visible, true);

			ac.cumulativeScore.SetBool(Bool.visible, true);
			ac.scores.SetBool(Bool.visible, true);

			// Instructions for populationController
			populationController.PlaceAgentsToInitialPosition (client.GetPosition (), client.GetOpponentPosition ());

			populationController.MakePlayerAppear ();
			populationController.MakeOpponentAppear ();
			populationController.MakeAllConsumerAppear ();

			RoundNextStep ();
			break;

		case TLRoundF.Preparation:

			if (client.GetInitialStateOfPlay () == "active") {

				// Update state
				stateGame = TLRoundF.ActiveBeginningOfTurn;

			} else {

				uiController.EnableChoice (false);

				// Update state
				stateGame = TLRoundF.PassiveBeginningOfTurn;
			}

			uiController.TurnSelectionAnimation (true);

			LogGameState ();
			break;

		case TLRoundF.PassiveBeginningOfTurn:

			client.AskFirmPassiveOpponentChoice ();

			// Instructions for UIController
			uiController.ChangeSelectedTurn (Turn.opponent);
			uiController.ResetScoreTexts ();
			uiController.FloatingConsumerAndCurrentScoreAnimation ();

			// Update state
			RoundNextStep ();
			break;

		case TLRoundF.PassiveWaitingOpponent:

			if (client.IsState (TLClientF.GotPassiveOpponentChoice)) {

				if (client.GetTime () == playerDisconnected) {
					stateGame = TLRoundF.EndingGame;
					break;
				}

				// Instructions for populationController
				StartCoroutine (populationController.MoveOpponentAndSendSignal (client.GetOpponentPosition ()));

				// Update state
				RoundNextStep ();

			} else if (!populationController.OpponentIsMoving ()) {
				populationController.MoveOpponent (UnityEngine.Random.Range (0, GameFeatures.nPositions - 1));
			}

			break;

		case TLRoundF.PassiveOpponentHasMoved:

			// Instructions for client
			client.AskFirmPassiveConsumerChoices ();

			// Instructions for UIController
			uiController.SetOpponentPrice (client.GetOpponentPrice ());
			uiController.ChangeSelectedTurn (Turn.consumers2);

			// Update state
			RoundNextStep ();
			break;

		case TLRoundF.PassiveWaitingConsumers:

			if (client.IsState (TLClientF.GotPassiveConsumerChoices)) {

				if (client.GetTime () == playerDisconnected) {
					stateGame = TLRoundF.EndingGame;
					break;
				}

				scoreManager.ComputeScores (client.GetConsumerChoicesPassive (), client.GetPrice (), client.GetOpponentPrice ());

				// Instructions to populationController
				StartCoroutine (populationController.MoveConsumers (client.GetConsumerChoicesPassive ()));

				// Update state
				RoundNextStep ();
			}
			break;

		case TLRoundF.PassiveConsumersHaveMoved:

			// Instructions to UIController
			uiController.UpdateScoreTurnAndCumulative (
				scoreManager.GetNClients (),
				scoreManager.GetScoreTurn (), 
				scoreManager.GetOpponentScoreTurn (),
				scoreManager.GetScoreCumulative (),
				scoreManager.GetOpponentScoreCumulative ()
			);

			uiController.AuthorizeCollect (true);
			ac.buttonCashRegister.SetBool(Bool.visible, true);
			uiController.TurnSelectionAnimation (false);
			uiController.GlowingCurrentScoreAnimation ();

			// Update state
			RoundNextStep ();
			break;

		case TLRoundF.PassiveNextTurn:

			// Instructions for uiController
			uiController.AddScoreAnimation ();
			uiController.TurnSelectionAnimation (true);

			ac.buttonCashRegister.SetBool(Bool.visible, false);

			// Instructions for populationController
			StartCoroutine (populationController.MoveBackConsumers ());

			// Update state
			RoundNextStep ();
			break;

		case TLRoundF.PassiveEndOfTurn:

			if (client.GetEndGame () == 1) {

				stateGame = TLRoundF.EndingGame;

			} else {

				// Update state
				stateGame = TLRoundF.ActiveBeginningOfTurn;

			}

			LogGameState ();
			break;

		case TLRoundF.ActiveBeginningOfTurn:

			// Instructions to UIController 
			uiController.AuthorizeDeplacement (true);
			uiController.ResetScoreTexts ();
			uiController.FloatingScoreAnimaton ();
			uiController.EnableChoice (true);
			uiController.ChangeSelectedTurn (Turn.player);

			// Animations
			ac.buttonYes.SetBool(Bool.visible, true);
			ac.strategicButtons.SetBool(Bool.visible, true);


			// Instructions for populationController
			populationController.ShowConsumersOutline ();

			// Update state
			RoundNextStep ();
			break;

		case TLRoundF.ActiveChoiceMade:

			// Inform client that the user made his choice.
			client.AskFirmActiveChoiceRecording (uiController.GetSelectedPosition (), uiController.GetSelectedPrice ());

			// Instructions for UIController
			uiController.FloatingConsumerAndCurrentScoreAnimation ();
			uiController.ChangeSelectedTurn (Turn.consumers1);

			// Animations
			ac.buttonYes.SetBool(Bool.visible, false);
			ac.strategicButtons.SetBool(Bool.visible, false);

			// Instructions for populationController
			populationController.HideConsumersOutline();

			// Update state
			RoundNextStep ();
			break;

		case TLRoundF.ActiveChoiceSubmitting:

			if (client.IsState (TLClientF.GotActiveChoiceRecording)) {

				if (client.GetTime () == playerDisconnected) {
					stateGame = TLRoundF.EndingGame;
					break;
				}

				RoundNextStep ();
			}

			break;

		case TLRoundF.ActiveChoiceSubmitted:

			client.AskFirmActiveConsumerChoices ();

			RoundNextStep ();
			break;

		case TLRoundF.ActiveWaitingConsumers:

			if (client.IsState (TLClientF.GotActiveConsumerChoices)) {

				if (client.GetTime () == playerDisconnected) {
					stateGame = TLRoundF.EndingGame;
					break;
				}

				scoreManager.ComputeScores (client.GetConsumerChoicesActive (), client.GetPrice (), client.GetOpponentPrice ());

				// Instructions for populationController
				StartCoroutine (populationController.MoveConsumers (client.GetConsumerChoicesActive ()));

				// Update state
				RoundNextStep ();
			}
			break;

		case TLRoundF.ActiveConsumersHaveMoved:

			// Instructions for UIController
			uiController.UpdateScoreTurnAndCumulative (
				scoreManager.GetNClients (),
				scoreManager.GetScoreTurn (), 
				scoreManager.GetOpponentScoreTurn (),
				scoreManager.GetScoreCumulative (),
				scoreManager.GetOpponentScoreCumulative ()
			);

			uiController.TurnSelectionAnimation (false);
			uiController.AuthorizeCollect (true);
			uiController.GlowingCurrentScoreAnimation ();

			// Animations
			ac.buttonCashRegister.SetBool(Bool.visible, true);

			// Update state
			RoundNextStep ();
			break;

		case TLRoundF.ActiveNextTurn:

			// Instructions for uiController
			uiController.AddScoreAnimation ();
			ac.buttonCashRegister.SetBool(Bool.visible, false);
			uiController.TurnSelectionAnimation (true);

			// Instructions for populationController
			StartCoroutine (populationController.MoveBackConsumers ());

			// Update state
			RoundNextStep ();
			break;

		case TLRoundF.ActiveEndOfTurn:

			if (client.GetEndGame () == 1) {
				// Update state
				stateGame = TLRoundF.EndingGame;

			} else {
				// Update state
				stateGame = TLRoundF.PassiveBeginningOfTurn;
			}

			LogGameState ();

			break;

		case TLRoundF.EndingGame:

			ac.blackBackground.SetBool (Bool.visible, true);

			ac.turns.SetBool (Bool.visible, false);

			ac.HUDPlayer.SetBool (Bool.visible, false);
			ac.HUDOpponent.SetBool (Bool.visible, false);

			ac.cumulativeScore.SetBool (Bool.visible, false);
			ac.scores.SetBool (Bool.visible, false);

			populationController.MakePlayerDisappear ();
			populationController.MakeOpponentDisappear ();
			populationController.MakeAllConsumersDisappear ();


			if (client.GetTime () == playerDisconnected) {
				gameController.PlayerDisconnected ();
			} else {
				gameController.EndOfRound ();
			}

			RoundNextStep ();
			break;

		case TLRoundF.EndOfGame:
			
			break;

		default:
			break;
		}
	}

	// ------------------------------------------------------------------------------ //

	public void Begin () {
		stateGame = TLRoundF.Init;
	}

	// ----------------- Called from populationController ---------------------- //

	public void ConsumersAreArrived () {
		RoundNextStep ();
	}

	public void OpponentIsArrived () {
		RoundNextStep ();
	}

	// 

	public void UserCollect () {
		RoundNextStep ();
	}

	public void UserValidate () {
		RoundNextStep ();
	}

	public int GetScore () {
		return scoreManager.GetScoreCumulative ();
	}



	void RoundNextStep () {
		stateGame += 1;
		LogGameState ();
	}

	// -------------------------------------------------------------------------------- //

	void LogGameState () {
		Debug.Log ("RoundF: New state is '" + stateGame + "'.");
	}

	// ----------------------------------------------------------------------------//

	IEnumerator ActionWithDelay(Action methodName, float seconds) {
		yield return new WaitForSeconds(seconds);

		methodName ();
	}
}
