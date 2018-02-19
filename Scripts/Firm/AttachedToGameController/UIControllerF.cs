﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using AssemblyCSharp;


public class UIControllerF : MonoBehaviour {

	PopulationControllerF populationController;
	GameControllerF gameController;

	List<Button> positionButtons;
	Dictionary<string, Button> buttons;

	TextsF texts;
	ACF ac;

	int score;
	int scoreOpponent;

	int selectedPosition;
	int selectedPrice;

	bool scoreUpdated;

	string tagHUDPlayer = "HUDPlayer";
	string tagHUDOpponent = "HUDOpponent";

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Awake () {
	
	}

	void Start () {

		// Get components
		populationController = GetComponent<PopulationControllerF> ();
		gameController = GetComponent<GameControllerF> ();
		texts = GetComponent<TextsF> ();
		ac = GetComponent<ACF> ();

		GetPushButtons ();
		GetPositionButtons ();
		
		scoreUpdated = true;
	}

	void Update () {

	}

	// ------------ Get components / objects ------------------- //

	void GetPositionButtons () {

		positionButtons = new List<Button> ();

		for (int i = 0; i < GameFeatures.nPositions; i++) {
			GameObject go = GameObject.FindGameObjectWithTag (string.Concat (ButtonNames.position, i));
			Button btn = go.GetComponent<Button> ();

			int captured = i;
			btn.onClick.AddListener (delegate {
				ButtonPosition (captured);
			});
			btn.interactable = false;
			positionButtons.Add (btn);
		}
	}

	void GetPushButtons () {

		buttons = new Dictionary<string, Button> ();

		Dictionary<string, UnityAction> buttonAssociations = new Dictionary<string, UnityAction> () {

			{ButtonNames.left, ButtonLeft},
			{ButtonNames.right, ButtonRight},
			{ButtonNames.plus, ButtonPlus},
			{ButtonNames.minus, ButtonMinus},
			{ButtonNames.yes, ButtonYes},
			{ButtonNames.cashRegister, ButtonCashRegister},
			{ButtonNames.menu, ButtonMenu}
            
		};

		foreach(KeyValuePair<string, UnityAction> entry in buttonAssociations) {
			GameObject go = GameObject.FindGameObjectWithTag (entry.Key);
			Button btn = go.GetComponent<Button> ();
			btn.onClick.AddListener (entry.Value);
			btn.interactable = false;
			buttons [entry.Key] = btn;
		}
	}
		
	// ------------ Called once player got init information from server -------- //

	public void Initialize (int playerPosition, int playerPrice, int opponentPrice, int initScore, int initScoreOpponent) {

		// Initialize scores
		ResetScoreTexts ();

		//Initialize selected price and position
		selectedPosition = playerPosition;
		selectedPrice = playerPrice;

		// Update displaying
		UpdatePriceDisplaying ();

		// Set opponentHUD
		SetOpponentPrice (opponentPrice);

		// Initialize Score
		score = initScore;
		scoreOpponent = initScoreOpponent;

		UpdateScore ();
	}
		
	// --------------------- Click button -------------------------------- //

	public void EnableChoice (bool value) {			

		MakeButtonsInteractible (value);
		AuthorizeValidation (value);
	}

	void MakeButtonsInteractible (bool value) {
		
		AuthorizeDeplacement (true);
		AuthorizePriceSelection (true);
	}

	public void AuthorizeDeplacement (bool value) {

		buttons[ButtonNames.right].interactable = value;
		buttons[ButtonNames.left].interactable = value;
		foreach (Button button in positionButtons) {
			button.interactable = value;
		}
	}

	public void AuthorizePriceSelection (bool value) {

		buttons[ButtonNames.plus].interactable = value;
		buttons[ButtonNames.minus].interactable = value;
	}

	public void AuthorizeValidation (bool value) {

		buttons[ButtonNames.yes].interactable = value;
	}

	public void AuthorizeCollect (bool value) {

		buttons[ButtonNames.cashRegister].interactable = value;
	}

	public void AuthorizeButtonMenu (bool value) {
    
		buttons[ButtonNames.menu].interactable = value;
    }

	// ------------ Click on... --------------- //

	public void ButtonYes () {

		Debug.Log ("UIController: User validates his choice.");
		Debug.Log ("UIController: Selected position is " + selectedPosition.ToString () + " .");
		Debug.Log ("UIController: Selected price is " + selectedPrice.ToString () + " .");

		AuthorizeValidation (false);
		AuthorizeDeplacement (false);
		AuthorizePriceSelection (false);

		gameController.UserValidate ();
	}

	public void ButtonCashRegister () {
		
		Debug.Log ("UIController: Click on 'ButtonCashRegister'.");

		AuthorizeCollect (false);

		gameController.UserCollect ();
	}

	public void ButtonMinus () {
		
		Debug.Log ("UIController: Click on 'ButtonMinus'.");
		if (selectedPrice > GameFeatures.minimumPrice) {
			selectedPrice -= 1;
			UpdatePriceDisplaying ();

			gameController.UserChangePrice ();
		}
	}

	public void ButtonPlus () {
		
		Debug.Log ("UIController: Click on 'ButtonPlus'.");
		if (selectedPrice < GameFeatures.maximumPrice) {
			selectedPrice += 1;
			UpdatePriceDisplaying ();

			gameController.UserChangePrice ();
		}
	}

	public void ButtonLeft () {
		
		Debug.Log ("UIController: Click on 'ButtonLeft'.");
		if (!populationController.PlayerIsMoving () && selectedPosition > GameFeatures.minimumPosition) {

			selectedPosition -= 1;
			MoveAction ();
		}
	}

	public void ButtonRight () {
		
		Debug.Log ("UIController: Click on 'ButtonRight'.");
		if (!populationController.PlayerIsMoving () && selectedPosition < GameFeatures.maximumPosition) {

			selectedPosition += 1;
			MoveAction ();
		}
	}

	public void ButtonPosition (int position) {

		Debug.Log ("UIController: Click on 'ButtonPosition' of '" + position + "'.");

		if (position >= GameFeatures.minimumPosition &&
			position <= GameFeatures.maximumPosition) {

			selectedPosition = position;
			MoveAction ();
		}				
	}

	public void ButtonIndicator (string buttonName) {

		Debug.Log ("ButtonIndicator with name '" + buttonName + "' has been pushed.");
		gameController.UserGotIt ();
	}

	public void ButtonMenu () {

		buttons [ButtonNames.menu].interactable = false;

		Debug.Log ("UIController: Click on 'ButtonMenu'.");

		gameController.UserPushedButtonMenu ();
	}

	// -------------------------------- //

	void MoveAction () {
		populationController.MovePlayer (selectedPosition);

		AuthorizeValidation (false);
		StartCoroutine (ActionsAfterMoveAction ());
	}
		

	IEnumerator ActionsAfterMoveAction () {
		while (populationController.PlayerIsMoving ()) {
			yield return new WaitForEndOfFrame ();
		}
		populationController.ShowConsumersOutline ();
		gameController.UserChangePosition ();
	}

	// ------------------------------------------- //

	public void SetButtonInteractable (string buttonName, bool value) {
		buttons [buttonName].interactable = value;
	}

	// ------------------ HUD -------------------------------- //

	void UpdatePriceDisplaying () 
	{
		texts.HUDPlayer.text = "$ " + selectedPrice.ToString ();
		texts.price.text = selectedPrice.ToString ();
	}

	public void SetOpponentPrice (int newValue) 
	{
		texts.HUDOpponent.text = "$ " + newValue.ToString ();
	}

	public Vector3 GetTarget (string hudTag) 
	{
		if (hudTag == tagHUDPlayer) {
			return populationController.GetPlayerPosition ();
		} else if (hudTag == tagHUDOpponent) {
			return populationController.GetOpponentPosition ();
		} else {
			throw new System.Exception ("UIController: Error with HUD tag (tag given: '" + hudTag + ").");	
		}
	}

	// ----------------- Score -------------------- //
		
	public void UpdateScoreTurnAndCumulative (
			int nClients, 
			int scoreTurn, int scoreTurnOpponent, 
			int scoreCumulative, int scoreCumulativeOpponent) {

		texts.consumer.text = nClients.ToString ();
		texts.currentScore.text = scoreTurn.ToString();

		score = scoreCumulative;
		scoreOpponent = scoreCumulativeOpponent;
	}

	public void UpdateScore () {
		
		texts.cumulativeScore.text = score.ToString ();
		texts.cumulativeScoreOpponent.text = scoreOpponent.ToString ();
	}

	public void ResetScoreTexts () {
		
		texts.consumer.text = "?";
		texts.currentScore.text = "?";
	}

	public void ResetCumulativeScore () {
		
		score = 0;
		texts.cumulativeScore.text = score.ToString ();
	}

	// -------------- Turn Icons --------------------//

	public void ChangeSelectedTurn (string newTurn)
	{
		if (newTurn == Turn.player) {
			SetSelectedTurn (true, false, false, false);
		} else if (newTurn == Turn.consumers1) {
			SetSelectedTurn (false, true, false, false);
		} else if (newTurn == Turn.opponent) {
			SetSelectedTurn (false, false, true, false);
		} else if (newTurn == Turn.consumers2) {
			SetSelectedTurn (false, false, false, true);
		} else if (newTurn == Turn.none) {
			SetSelectedTurn (false, false, false, false);
		} else {
			throw new System.Exception ("UIController: Error with 'ChangeSelectedTurn' (got '" + newTurn + "' as arg).");
		}
	}

	void SetSelectedTurn (bool player, bool consumers1, bool opponent, bool consumers2) {

		ac.turnPlayer.SetBool (Bool.visible, player);
		ac.turnConsumers1.SetBool (Bool.visible, consumers1);
		ac.turnOpponent.SetBool (Bool.visible, opponent);
		ac.turnConsumers2.SetBool (Bool.visible, consumers2);
	}

	// ----------- Animations ------------------- //

	public void AddScoreAnimation () {
		scoreUpdated = false; 
		ac.scores.SetTrigger (Trigger.addScore);
	}

	public void FloatingScoreAnimaton () {
		ac.scores.SetTrigger (Trigger.floatScore);
	}

	public void FloatingConsumerAndCurrentScoreAnimation () {
		ac.scores.SetTrigger (Trigger.floatConsumerAndCurrentScore);
	}

	public void GlowingCurrentScoreAnimation () {
		ac.scores.SetTrigger (Trigger.glowCurrentScore);
	}

	public void EndAddingScore () {
		scoreUpdated = true;
	}

	public bool IsScoreUpdated () {
		return scoreUpdated;
	}

	public void TurnSelectionAnimation (bool value) {
		ac.turnPlayer.SetBool (Bool.glow, value);
		ac.turnConsumers1.SetBool (Bool.glow, value);
		ac.turnOpponent.SetBool (Bool.glow, value);
		ac.turnConsumers2.SetBool (Bool.glow, value);
	}
		
	public void HideObjects () {

		Animator[] toHide = {

			ac.HUDPlayer,
			ac.HUDOpponent, 

			ac.turns,
			ac.turnPlayer,
			ac.turnOpponent,

			ac.turnConsumers1,
			ac.turnConsumers2,
			ac.scores,
			ac.cumulativeScore,
			ac.buttonYes,
			ac.buttonCashRegister,
			ac.strategicButtons,

			// All the indicators
			ac.indicatorMessenger,
			ac.indicatorYou,
			ac.indicatorOpponent,
			ac.indicatorConsumer,
			ac.indicatorCentral,
			ac.indicatorScore,
			ac.indicatorScoreTurn,
			ac.indicatorTurnPlayer,
			ac.indicatorChangePosition,
			ac.indicatorChangePrice,
			ac.indicatorValidation,
			ac.indicatorTurnConsumers1,
			ac.indicatorTurnOpponent,
			ac.indicatorTurnConsumers2
		};

		foreach (Animator anim in toHide) {
			anim.SetBool (Bool.visible, false);
		}

		ac.blackBackground.SetBool (Bool.visible, true);

		string[]  scoreTriggers = {
			Trigger.addScore, 
			Trigger.floatScore,
			Trigger.floatConsumerAndCurrentScore,
			Trigger.glowCurrentScore
		};

		foreach (string name in scoreTriggers) {
			ac.scores.ResetTrigger (name);
		}

		Animator[] turnAC = {
			ac.turnPlayer,
			ac.turnOpponent,
			ac.turnConsumers1,
			ac.turnConsumers2,
		};

		foreach (Animator anim in turnAC) {
			anim.SetBool (Bool.visible, false);
			anim.SetBool (Bool.glow, false);
		}
	}

	// --------------------------------------------- //

	public void ShowMenu () {

		Debug.Log ("UIController: Show menu");

		ac.logo.SetBool (Bool.visible, true);
		ac.textMenuCentral.SetBool (Bool.visible, true);
		ac.textMenuCentral.SetBool (Bool.glow, false);

		ac.buttonMenu.SetBool (Bool.visible, true);
		ac.buttonMenu.SetBool (Bool.glow, true);

		buttons [ButtonNames.menu].interactable = true;
	}

	public void HideMenu () {

		Debug.Log ("UIController: Hide menu");

		ac.blackBackground.SetBool (Bool.visible, false);
		ac.textMenuCentral.SetBool (Bool.visible, false);
		ac.logo.SetBool (Bool.visible, false);
	}

	// ------------ Getters for user choice ------------- //

	public int GetSelectedPosition () {
		return selectedPosition;
	}

	public int GetSelectedPrice () {
		return selectedPrice;
	}

	// -------------------------------------------------- //

	public void ShowMessageFinal () {
		ac.textMenuCentral.SetBool (Bool.glow, false);
		texts.menuCentral.text = "You finished the final round! Well done!\n" +
			"Score: " + gameController.GetScore().ToString() + "\n\n" + 
			"Your HIT will be accepted within three days \n" +
			"with a bonus corresponding to your scores on the two rounds.";

		ac.textMenuCentral.SetBool(Bool.visible, true);
	}

	public void ShowMessageAlreadyPlayed () {
		ac.textMenuCentral.SetBool (Bool.glow, false);
		texts.menuCentral.text = "We know that this game is the best game you ever played, \\nbut it is a single shot experiment!"; 
		ac.textMenuCentral.SetBool (Bool.visible, true);
	}

	public void ShowMessageWaiting () {
		ac.textMenuCentral.SetBool (Bool.glow, true);
		texts.menuCentral.text = "Waiting for the other player...\n";
		ac.textMenuCentral.SetBool (Bool.visible, true);
	}

	public void ShowMessageTutorial () {
		ac.textMenuCentral.SetBool (Bool.glow, false);
		texts.menuCentral.text = "Let's begin with a small tutorial!";
		ac.textMenuCentral.SetBool (Bool.visible, true);
	}

	public void ShowMessagePVE () {
		ac.textMenuCentral.SetBool (Bool.glow, false);
		texts.menuCentral.text = "It's time to play the round against the computer!";
		ac.textMenuCentral.SetBool (Bool.visible, true);
	}

	public void ShowMessagePVP () {
		ac.textMenuCentral.SetBool (Bool.glow, false);
		int score = gameController.GetScore ();
		if (score > 0) {
			texts.menuCentral.text = "You finished the first round!\n" +
			"Score: " + score.ToString () + "\n\n" +
			"It's now the time for the final round against a HUMAN player!";
		} else { // In case of deconnection
			texts.menuCentral.text = "You finished the first round!\n" +
				"\n\n" + // Do not display score
				"It's now the time for the final round against a HUMAN player!";
		}
		ac.textMenuCentral.SetBool (Bool.visible, true);
	}

	public void ShowMessageProgressOtherPlayer (string progressOtherPlayer) {
		ac.textMenuCentral.SetBool (Bool.glow, true);
		texts.menuCentral.text = "Waiting for the other player...\n" +
			"Progress of the other player on the previous phase: " + progressOtherPlayer + "%";
		ac.textMenuCentral.SetBool (Bool.visible, true);
	}

	public void ShowMessagePLayerDisconnected () {
		ac.textMenuCentral.SetBool (Bool.glow, false);
		texts.menuCentral.text = "Unfortunately, the other player is disconnected\n" +
			"There is no choice but to put an end to the game\n\n" +
			"Your HIT will be accepted within three days \n" +
			"with a bonus corresponding to your score";

		ac.textMenuCentral.SetBool(Bool.visible, true);
	}

}
