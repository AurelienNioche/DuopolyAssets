using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using AssemblyCSharp;


public class TutorialF : MonoBehaviour {

	TLTutoF stateTuto;

	ClientF client;
	PopulationControllerF populationController;
	UIControllerF uiController;
	FogControllerF fogController;
	GameControllerF gameController;
	ScoresManagerF scoreManager;
	ACF ac;
	TextsF texts;

	// --------------------------------- //

	public int initialPlayerPosition = 15;
	public int initialPlayerPrice = 6;

	public int initialOpponentPosition = 5;
	public int initialOpponentPrice = 11;

	public int exampleConsumerPosition = 2;

	float delayForCommunicatingProgress;

	bool submitProgression;

	string roomId;
	string playerId;
	string username;
	int t;

	int position;
	int price;

	int opponentPosition;
	int opponentPrice;

	int[] fakeConsumerChoices;

	int[, ] consumersFieldOfView;

	List<int> positionsSeenByExampleConsumer;
	List<int> firmsSeenByExampleConsumer;

	List<TLTutoF> stateList;
	int nStates;


	// Use this for initialization
	void Start () {

		stateList = Enum.GetValues(typeof(TLTutoF)).Cast<TLTutoF>().ToList ();
		nStates = stateList.Count;


		consumersFieldOfView = gameController.GetConsumersFieldOfView ();
		if (consumersFieldOfView == null) {
			Debug.Log("Generate fake 'consumerFieldOfView'");
			consumersFieldOfView = GameTools.GenerateConsumersFieldOfView ();
		}

		delayForCommunicatingProgress = gameController.GetTimeBeforeRetryingDemand ();

		stateTuto = TLTutoF.Init;
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	void Awake () {

		// Get components
		client = GetComponent<ClientF> ();
		populationController = GetComponent<PopulationControllerF> ();
		uiController = GetComponent<UIControllerF> ();
		fogController = GetComponent<FogControllerF> ();
		gameController = GetComponent<GameControllerF> ();
		scoreManager = GetComponent<ScoresManagerF> ();
		ac = GetComponent<ACF> ();
		texts = GetComponent<TextsF> ();
	}

	// ----------------------------------------------------------------------------------------------------------- //

	public void ManageState () {

		// Called by game controller

		List<int> seen = new List<int> ();

		switch (stateTuto) {

		case TLTutoF.Init:
				
			position = initialPlayerPosition;
			price = initialPlayerPrice;
			opponentPosition = initialOpponentPosition;
			opponentPrice = initialOpponentPrice;

			scoreManager.ResetScores ();

			Debug.Log ("Player: Start tutorial. Player position and price are " + position + ", " + price +
			" and opponent position and price are " + opponentPosition + ", " + opponentPrice + ".");  
			
			positionsSeenByExampleConsumer = GameTools.PositionsSeenByAConsumer (exampleConsumerPosition, consumersFieldOfView);
			firmsSeenByExampleConsumer = GameTools.FirmsSeenByAConsumer (positionsSeenByExampleConsumer, position, opponentPosition);

			populationController.PlaceAgentsToInitialPosition (position, opponentPosition);

			uiController.PrepareTutorial (
				position, price, opponentPrice, 
				scoreManager.GetScoreCumulative (), 
				scoreManager.GetOpponentScoreCumulative ()			
			);

			submitProgression = true;
			StartCoroutine (SubmitProgression ());

			TutoNextStep ();
			break;

		case TLTutoF.Messenger:

			ac.indicatorMessenger.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;    

		case TLTutoF.You:

			populationController.MakePlayerAppear ();

			ac.indicatorMessenger.SetBool(Bool.visible, false);
			ac.indicatorYou.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.Opponent:

			populationController.MakeOpponentAppear ();

			ac.indicatorYou.SetBool(Bool.visible, false);
			ac.indicatorOpponent.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.Consumer:

			populationController.MakeConsumerAppear (exampleConsumerPosition);

			ac.indicatorOpponent.SetBool(Bool.visible, false);
			ac.indicatorConsumer.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.ConsumerProcess:

			ac.indicatorConsumer.SetBool(Bool.visible, false);
			texts.indicatorCentral.text = TutorialTextsF.consumerProcess;
			ac.indicatorCentral.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.ConsumerLimitedFieldOfView:

			if (firmsSeenByExampleConsumer.Contains (GameRole.opponent)) {
				ac.HUDOpponent.SetBool(Bool.visible, true);
			}

			if (firmsSeenByExampleConsumer.Contains (GameRole.player)) {
				ac.HUDPlayer.SetBool(Bool.visible, true);
			}


			fogController.RevealPositions (positionsSeenByExampleConsumer);

			ac.indicatorConsumer.SetBool(Bool.visible, false);
			texts.indicatorCentral.text = TutorialTextsF.consumerLimitedFieldOfView;
			ac.indicatorCentral.SetBool(Bool.visible, true);
			
			TutoNextStep();
			break;

		case TLTutoF.ConsumerFirmTurn:

			texts.indicatorCentral.text = TutorialTextsF.consumerFirmTurn;

			TutoNextStep();
			break;

		case TLTutoF.ConsumerSee:

			texts.indicatorCentral.text = TutorialTextsF.consumerSee;

			TutoNextStep();
			break;

		case TLTutoF.ConsumerNoOne:

			texts.indicatorCentral.text = TutorialTextsF.consumerNoOne;

			TutoNextStep();
			break;


		case TLTutoF.ConsumerNoChoice:

			texts.indicatorCentral.text = TutorialTextsF.consumerNoChoice;

			TutoNextStep();
			break;

		case TLTutoF.ConsumerLessExpensive:

			texts.indicatorCentral.text = TutorialTextsF.consumerLessExpensive;

			TutoNextStep();
			break;

		
		case TLTutoF.ConsumerMoving:

			int target = GameTools.GetConsumerTarget (firmsSeenByExampleConsumer, price, opponentPrice);

			ac.indicatorCentral.SetBool(Bool.visible, false);

			if (target == GameRole.player) {

				StartCoroutine (
					populationController.MoveExampleConsumer (exampleConsumerPosition, GameRole.player));

			} else if (target == GameRole.opponent) {
				StartCoroutine (
					populationController.MoveExampleConsumer (exampleConsumerPosition, GameRole.opponent));

			} else {
				ConsumersAreArrived ();
			}

			TutoNextStep ();
			break;

		case TLTutoF.ConsumerBot:

			seen = new List<int> ();

			seen = firmsSeenByExampleConsumer;

			if (seen.Contains (GameRole.opponent)) {
				ac.HUDOpponent.SetBool(Bool.visible, false);
			}

			if (seen.Contains (GameRole.player)) {
				ac.HUDPlayer.SetBool(Bool.visible, false);
			}

			fogController.NoFog ();

			populationController.MakeAllConsumerAppear ();

			texts.indicatorCentral.text = TutorialTextsF.consumerBot;
			ac.indicatorCentral.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.Score:

			ac.cumulativeScore.SetBool(Bool.visible, true);

			ac.indicatorCentral.SetBool(Bool.visible, false);
			ac.indicatorScore.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.ScoreTurn:

			fakeConsumerChoices = GameTools.NewFakeConsumerChoices (consumersFieldOfView, position, opponentPosition, price, opponentPrice);

			ac.indicatorScore.SetBool(Bool.visible, false);// uiController.UpdateScoreToAddAndNClients (client.GetScoreTurn(), client.GetOpponentScoreTurn(), client.GetNClients());
			ac.indicatorScoreTurn.SetBool(Bool.visible, true);

			uiController.ResetScoreTexts ();
			uiController.FloatingScoreAnimaton ();
			ac.scores.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;
		
		case TLTutoF.Training:

			ac.indicatorScoreTurn.SetBool(Bool.visible, false);

			texts.indicatorCentral.text = TutorialTextsF.training;
			ac.indicatorCentral.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.TurnPlayer:

			ac.indicatorCentral.SetBool(Bool.visible, false);
			ac.indicatorTurnPlayer.SetBool(Bool.visible, true);

			// For turn selection
			uiController.ChangeSelectedTurn (Turn.player);
			ac.turns.SetBool(Bool.visible, true);
			uiController.TurnSelectionAnimation (true);

			// For HUD
			ac.HUDOpponent.SetBool(Bool.visible, true);
			ac.HUDPlayer.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.ChangePosition:

			ac.strategicButtons.SetBool(Bool.visible, true);
			ac.buttonYes.SetBool(Bool.visible, true);

			uiController.AuthorizeDeplacement (true);
		
			ac.indicatorTurnPlayer.SetBool(Bool.visible, false);
			ac.indicatorChangePosition.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;
		
		case TLTutoF.Highlight:
			
			ac.indicatorChangePosition.SetBool(Bool.visible, false);
			texts.indicatorCentral.text = TutorialTextsF.highlight;
			ac.indicatorCentral.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.ChangePrice:

			uiController.AuthorizePriceSelection (true);

			ac.indicatorCentral.SetBool(Bool.visible, false);
			ac.indicatorChangePrice.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.Validation:

			if (!populationController.PlayerIsMoving()) {

				uiController.AuthorizeValidation (true);

				ac.indicatorChangePrice.SetBool(Bool.visible, false);
				ac.indicatorValidation.SetBool(Bool.visible, true);

				TutoNextStep ();
			}
			break;

		case TLTutoF.MovingConsumers1:
		case TLTutoF.MovingConsumers2:

			if (stateTuto == TLTutoF.MovingConsumers1) {
				ac.indicatorTurnConsumers1.SetBool(Bool.visible, false);
			} else {
				ac.indicatorTurnConsumers2.SetBool(Bool.visible, false);
			}

			position = uiController.GetSelectedPosition ();
			price = uiController.GetSelectedPrice ();

			Debug.Log ("TutorialF: Position " + position);
			Debug.Log ("TutorialF: Price " + price);
			Debug.Log ("TutorialF: Opp postion " + opponentPosition);
			Debug.Log ("TutorialF: Opp pirice " + opponentPrice);

			fakeConsumerChoices = GameTools.NewFakeConsumerChoices (consumersFieldOfView, position, opponentPosition, price, opponentPrice);

			scoreManager.ComputeScores (fakeConsumerChoices, price, opponentPrice);

			StartCoroutine (populationController.MoveConsumers (fakeConsumerChoices));

			TutoNextStep ();
			break;

		case TLTutoF.MovingBackConsumers1:
		case TLTutoF.MovingBackConsumers2:

			ac.buttonCashRegister.SetBool(Bool.visible, false);

			ac.indicatorValidation.SetBool(Bool.visible, false);

			uiController.AddScoreAnimation ();

			uiController.TurnSelectionAnimation (true);

			StartCoroutine (populationController.MoveBackConsumers ());

			TutoNextStep ();
			break;

		case TLTutoF.Collect:
		case TLTutoF.Collect2:

			uiController.TurnSelectionAnimation (false);
			uiController.GlowingCurrentScoreAnimation ();

			uiController.UpdateScoreTurnAndCumulative (
				scoreManager.GetNClients (),
				scoreManager.GetScoreTurn (), 
				scoreManager.GetOpponentScoreTurn (),
				scoreManager.GetScoreCumulative (),
				scoreManager.GetOpponentScoreCumulative ()
			);

			uiController.AuthorizeCollect (true);
			ac.buttonCashRegister.SetBool(Bool.visible, true);

			texts.indicatorValidation.text = TutorialTextsF.collect;
			ac.indicatorValidation.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.TurnConsumers1:

			uiController.ChangeSelectedTurn (Turn.consumers1);
			uiController.FloatingConsumerAndCurrentScoreAnimation ();

			uiController.AuthorizeDeplacement (false);
			ac.buttonYes.SetBool(Bool.visible, false);
			ac.strategicButtons.SetBool(Bool.visible, false);

			ac.indicatorValidation.SetBool(Bool.visible, false);
			ac.indicatorTurnConsumers1.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.TurnConsumers2:

			uiController.SetOpponentPrice (opponentPrice);

			uiController.ResetScoreTexts ();
			uiController.FloatingConsumerAndCurrentScoreAnimation ();

			uiController.ChangeSelectedTurn (Turn.consumers2);

			ac.indicatorTurnConsumers2.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.TurnOpponent:
			
			ac.indicatorTurnOpponent.SetBool(Bool.visible, true);
			uiController.ChangeSelectedTurn (Turn.opponent);

			TutoNextStep ();
			break;

		case TLTutoF.MovingOpponent:
			
			ac.indicatorTurnOpponent.SetBool(Bool.visible, false);

			opponentPosition = GameTools.NewFakeOpponentPosition (opponentPosition);
			opponentPrice = GameTools.NewFakeOpponentPrice (opponentPrice);

			StartCoroutine (populationController.MoveOpponentAndSendSignal (opponentPosition));

			TutoNextStep ();
			break;

		case TLTutoF.EndTraining:

			uiController.TurnSelectionAnimation (false);

			texts.indicatorCentral.text = TutorialTextsF.endOfTraining;
			ac.indicatorCentral.SetBool(Bool.visible, true);

			TutoNextStep ();
			break;

		case TLTutoF.Money:

			texts.indicatorCentral.text = TutorialTextsF.money;

			TutoNextStep ();
			break;

		case TLTutoF.End:

			fogController.NoFog ();

			uiController.HideObjects ();

			populationController.EndRound ();

			submitProgression = false;

			gameController.EndTutorial ();

			TutoNextStep ();
			break;

		case TLTutoF.Dead:
			break;
		}
	}

	// -----------------------------------------------------------------------------//

	public void ConsumersAreArrived() {
		
		if (stateTuto == TLTutoF.ConsumerMovingWaitAnimation ||
			stateTuto == TLTutoF.MovingConsumers1WaitAnimation ||
			stateTuto == TLTutoF.MovingConsumers2WaitAnimation ||
			stateTuto == TLTutoF.MovingBackConsumers1WaitAnimation ||
			stateTuto == TLTutoF.MovingBackConsumers2WaitAnimation
		) {
			TutoNextStep ();
		}
	}

	public void OpponentIsArrived () {
		TutoNextStep ();
	}

	// ------------------------------------------------------------------------------- //

	public void UserChangePosition () {

		TLTutoF[] blockValidation = new TLTutoF[] { 
			TLTutoF.Highlight,
			TLTutoF.HightlightWaitUser,
			TLTutoF.ChangePrice,
			TLTutoF.ChangePriceWaitUser,
		}; 

		if (stateTuto == TLTutoF.ChangePositionWaitUser) {
			TutoNextStep ();
		} else if (blockValidation.Contains(stateTuto)) {
			// ignore
		} else { 
			uiController.AuthorizeValidation (true);
		}
	}

	public void UserChangePrice () {
		if (stateTuto == TLTutoF.ChangePriceWaitUser) {
			TutoNextStep ();
		}
	}

	public void UserValidate () {

		if (stateTuto == TLTutoF.ValidationWaitUser) {
			populationController.HideConsumersOutline ();
			TutoNextStep ();
		}
	}

	public void UserCollect () {
		if (stateTuto == TLTutoF.CollectWaitUser || stateTuto == TLTutoF.Collect2WaitUser) {
			TutoNextStep ();
		}

	}

	public void UserGotIt () {
		if (stateTuto.ToString().Contains ("WaitUser")) {
			TutoNextStep ();
		}

	}

	// ------------------------------------------------------------------------------- //

	void TutoNextStep () {
		stateTuto += 1;
		uiController.SetProgress (ComputeProgress ());
		LogTutoState ();
	}

	// ----------------------------------------- Logs -------------------------------- //

	void LogTutoState () {
		Debug.Log ("TutorialF: New state is '" + stateTuto + "'.");
	}

	// ------------------------------------------------------------------------------- //

	public void End () {
		stateTuto = TLTutoF.End;
	}

	public bool IsState (TLTutoF value) {
		return stateTuto == value;
	}

	public void SetState (TLTutoF value) {
		stateTuto = value;
	}

	// ----------------------------------------------------------------------------- //

	IEnumerator SubmitProgression () {

		client.SubmitTutorialProgression (ComputeProgress ());

		while (submitProgression) {

			if (client.IsState (TLClientF.GotSubmitTutorialProgression)) {

				yield return new WaitForSeconds (delayForCommunicatingProgress);
				if (submitProgression) {
					client.SubmitTutorialProgression (ComputeProgress ());
				}		
			} else {
				yield return new WaitForEndOfFrame ();
			}
		}
	}

	float ComputeProgress () {

		int nStateInit = 2;
		int nStateToIgnore = 4 + nStateInit;

		// Compute progress
		int index = Math.Max(stateList.IndexOf (stateTuto) - nStateInit, 0);

		float indexFloat = (float) index;
		float nStatesFloat = (float) nStates;

		float progress = Math.Min(indexFloat / (nStatesFloat - nStateToIgnore), 1); 

		Debug.Log ("TutorialF: Index of current state " + index + "; N state" + nStates + "; Progress " + progress);
		return progress;
	}

}
