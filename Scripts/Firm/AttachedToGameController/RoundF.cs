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

				if (client.GotFatalError ()) {

					stateGame = TLRoundF.EndingGame;
					LogGameState ();
					break;
				}

				if (client.GetTime () == CodeErrorF.haveToWait) {
				
					client.AskInit ();
					uiController.ShowMessageProgressOtherPlayer (client.GetProgressOtherPlayer ());
				
				} else {
	
					float delta = Time.time - timeOfInit;
					if (delta > timeForInitialAnimation) { 
						RoundNextStep ();
					}
				
				}
			}
			break;

		case TLRoundF.GotInfo:

			gameController.GotInitInfo ();
			stateGame = TLRoundF.WaitingGoCommand;
			// RoundNextStep ();
			break;

		case TLRoundF.Preparation:

			// Instruction to scoreManager
			scoreManager.SetCumulativeScores (client.GetScore (), client.GetOpponentScore ());

			// Instructions for uiController
			uiController.PrepareNewRound (client.GetPosition (), client.GetPrice (), client.GetOpponentPrice (), 
				client.GetScore (), client.GetOpponentScore ());
			uiController.SetProgress (ComputeProgress());

			// Instructions for populationController
			populationController.PrepareNewRound (client.GetPosition (), client.GetOpponentPosition ());

			if (client.GetInitialStateOfPlay () == "active") {

				// Update state
				stateGame = TLRoundF.ActiveBeginningOfTurn;

			} else {

				uiController.EnableChoice (false);

				// Update state
				stateGame = TLRoundF.PassiveBeginningOfTurn;
			}

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

				if (client.GotFatalError ()) {

					stateGame = TLRoundF.EndingGame;
					LogGameState ();
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

				if (client.GotFatalError ()) {

					stateGame = TLRoundF.EndingGame;
					LogGameState ();
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

				uiController.SetProgress (100.0f);

				stateGame = TLRoundF.EndingGame;

			} else {

				uiController.SetProgress (ComputeProgress ());

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

				if (client.GotFatalError ()) {

					stateGame = TLRoundF.EndingGame;
					LogGameState ();
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

				if (client.GotFatalError ()) {

					stateGame = TLRoundF.EndingGame;
					LogGameState ();
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

				uiController.SetProgress (100.0f);

				// Update state
				stateGame = TLRoundF.EndingGame;

			} else {

				uiController.SetProgress (ComputeProgress ());

				// Update state
				stateGame = TLRoundF.PassiveBeginningOfTurn;
			}

			LogGameState ();

			break;

		case TLRoundF.EndingGame:

			uiController.HideObjects ();

			populationController.MakePlayerDisappear ();
			populationController.MakeOpponentDisappear ();
			populationController.MakeAllConsumersDisappear ();

			stateGame = TLRoundF.EndOfGame;

			if (client.GotFatalError ()) {
				gameController.FatalError ();
			} else {
				gameController.EndOfRound ();
			}
			break;

		case TLRoundF.EndOfGame:
			
			break;

		default:
			break;
		}
	}

	// ---------------- Go to next step ----------------------------------------- //

	void RoundNextStep () {
		stateGame += 1;
		LogGameState ();
	}

	void LogGameState () {
		Debug.Log ("RoundF: New state is '" + stateGame + "'.");
	}
		
	// ---------------- For assessing progression ----------------------------------- //

	float ComputeProgress () {
		float floatT = (float) client.GetTime ();
		float floatEndingT = (float) client.GetEndingT ();
		return floatT / floatEndingT;
	}

	// ---------------- For facilitating coroutines ----------------------------------//

	IEnumerator ActionWithDelay(Action methodName, float seconds) {
		yield return new WaitForSeconds(seconds);
		methodName ();
	}

	// ---------------- Called from gameController ----------------------------- //

	public void Begin () {
		stateGame = TLRoundF.Preparation;
	}

	public void Initialize () {
		stateGame = TLRoundF.Init;
	}

	public int GetScore () {
		return scoreManager.GetScoreCumulative ();
	}

	// ----------------- Called from populationController ---------------------- //

	public void ConsumersAreArrived () {
		RoundNextStep ();
	}

	public void OpponentIsArrived () {
		RoundNextStep ();
	}

	//  -------------- Called from UI (through gameController -------------- //

	public void UserCollect () {
		RoundNextStep ();
	}

	public void UserValidate () {
		RoundNextStep ();
	}

	public void UserChangePosition () {
		uiController.AuthorizeValidation (true);
	}

}
