using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TextsF : MonoBehaviour {

	[HideInInspector]
	public Text cumulativeScore;
	[HideInInspector]
	public Text cumulativeScoreIcon;
	[HideInInspector]
	public Text cumulativeScoreOpponent;
	[HideInInspector]
	public Text cumulativeScoreOpponentIcon;

	[HideInInspector]
	public Text price;
	[HideInInspector]
	public Text consumer;
	[HideInInspector]
	public Text currentScore;

	[HideInInspector]
	public Text HUDPlayer;
	[HideInInspector]
	public Text HUDOpponent;

	[HideInInspector]
	public Text buttonMenu;
	[HideInInspector]
	public Text menuCentral;

	[HideInInspector]
	public Text currentStep;
	[HideInInspector]
	public Text currentStepComment;
	[HideInInspector]
	public Text progression;

	[HideInInspector]
	public Text indicatorCentral;
	[HideInInspector]
	public Text indicatorValidation;

	// Use this for initialization
	void Start () {

		cumulativeScore = Associate("TextCumulativeScore");
		cumulativeScoreIcon = Associate ("TextCumulativeScoreIcon");
		cumulativeScoreOpponent = Associate("TextCumulativeScoreOpponent");
		cumulativeScoreOpponentIcon = Associate ("TextCumulativeScoreOpponentIcon");

		price = Associate("TextPrice");
		consumer = Associate("TextConsumer");
		currentScore = Associate("TextCurrentScore");

		HUDPlayer = Associate("HUDPlayer");
		HUDOpponent = Associate("HUDOpponent");

		buttonMenu = Associate("TextButtonMenu");
		menuCentral = Associate("TextMenuCentral");

		indicatorCentral = Associate("TextIndicatorCentral");
		indicatorValidation = Associate("TextIndicatorValidation");

		currentStep = Associate ("TextCurrentStep");
		currentStepComment = Associate ("TextCurrentStepComment");
		progression = Associate ("TextProgression");
	}

	Text Associate (string name) {
		try {
			GameObject gameObject = GameObject.FindGameObjectWithTag (name);
			Text txt = gameObject.GetComponent<Text> ();
			return txt;
		} catch (NullReferenceException e) {
			Debug.Log ("TextsF: I could not find game object with tag '" + name + "'");
			throw e;
		}
	}
}
