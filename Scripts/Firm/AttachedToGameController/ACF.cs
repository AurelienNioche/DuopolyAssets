using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ACF : MonoBehaviour {

	[HideInInspector]
	public Animator textMenuCentral;
	[HideInInspector]
	public Animator buttonMenu;

	[HideInInspector]
	public Animator logo;

	[HideInInspector]
	public Animator blackBackground;

	[HideInInspector]
	public Animator currentStep;

	[HideInInspector]
	public Animator HUDPlayer;
	[HideInInspector]
	public Animator HUDOpponent;

	[HideInInspector]
	public Animator turns;
	[HideInInspector]
	public Animator turnPlayer;
	[HideInInspector]
	public Animator turnOpponent;

	[HideInInspector]
	public Animator turnConsumers1;
	[HideInInspector]
	public Animator turnConsumers2;
	[HideInInspector]
	public Animator scores;
	[HideInInspector]
	public Animator cumulativeScore;
	[HideInInspector]
	public Animator buttonYes;
	[HideInInspector]
	public Animator buttonCashRegister;
	[HideInInspector]
	public Animator strategicButtons;

	// All the indicators
	[HideInInspector]
	public Animator indicatorMessenger;
	[HideInInspector]
	public Animator indicatorYou;
	[HideInInspector]
	public Animator indicatorOpponent;
	[HideInInspector]
	public Animator indicatorConsumer;
	[HideInInspector]
	public Animator indicatorCentral;
	[HideInInspector]
	public Animator indicatorScore;
	[HideInInspector]
	public Animator indicatorScoreTurn;
	[HideInInspector]
	public Animator indicatorTurnPlayer;
	[HideInInspector]
	public Animator indicatorChangePosition;
	[HideInInspector]
	public Animator indicatorChangePrice;
	[HideInInspector]
	public Animator indicatorValidation;
	[HideInInspector]
	public Animator indicatorTurnConsumers1;
	[HideInInspector]
	public Animator indicatorTurnOpponent;
	[HideInInspector]
	public Animator indicatorTurnConsumers2;

	// Use this for initialization
	void Start () {

		textMenuCentral = Associate("TextMenuCentral");

		currentStep = Associate ("CurrentStep");


		logo = Associate ("Logo");
		buttonMenu = Associate ("ButtonMenu");

		blackBackground = Associate("BlackBackground");

		HUDPlayer = Associate("HUDPlayer");
		HUDOpponent = Associate("HUDOpponent");

		turns = Associate("Turns");
		turnPlayer = Associate("TurnPlayer");
		turnOpponent = Associate("TurnOpponent");

		turnConsumers1 = Associate("TurnConsumers1");
		turnConsumers2 = Associate("TurnConsumers2");
		scores = Associate("Scores");
		cumulativeScore = Associate("CumulativeScore");
		buttonYes = Associate("ButtonYes");
		buttonCashRegister = Associate("ButtonCashRegister");
		strategicButtons = Associate("StrategicButtons");

				// All the indicators
		indicatorMessenger = Associate("IndicatorMessenger");
		indicatorYou = Associate("IndicatorYou");
		indicatorOpponent = Associate("IndicatorOpponent");
		indicatorConsumer = Associate("IndicatorConsumer");
		indicatorCentral = Associate("IndicatorCentral");
		indicatorScore = Associate("IndicatorScore");
		indicatorScoreTurn = Associate("IndicatorScoreTurn");
		indicatorTurnPlayer = Associate("IndicatorTurnPlayer");
		indicatorChangePosition = Associate("IndicatorChangePosition");
		indicatorChangePrice = Associate("IndicatorChangePrice");
		indicatorValidation = Associate("IndicatorValidation");
		indicatorTurnConsumers1 = Associate("IndicatorTurnConsumers1");
		indicatorTurnOpponent = Associate("IndicatorTurnOpponent");
		indicatorTurnConsumers2 = Associate("IndicatorTurnConsumers2");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	Animator Associate (string name) {
		try {
			GameObject gameObject = GameObject.FindGameObjectWithTag (name);
			Animator anim = gameObject.GetComponent<Animator> ();
			return anim;
		}
		catch (NullReferenceException){
			throw new Exception ("UIController: I could not find object with tag '" + name + "'");
		}
	}
}
