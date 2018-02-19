using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TextsF : MonoBehaviour {

	[HideInInspector]
	public Text cumulativeScore;
	[HideInInspector]
	public Text cumulativeScoreOpponent;
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

	public Text indicatorCentral;
	[HideInInspector]
	public Text indicatorValidation;

	// Use this for initialization
	void Start () {

		cumulativeScore = Associate("TextCumulativeScore");
		cumulativeScoreOpponent = Associate("TextCumulativeScoreOpponent");
		price = Associate("TextPrice");
		consumer = Associate("TextConsumer");
		currentScore = Associate("TextCurrentScore");

		HUDPlayer = Associate("HUDPlayer");
		HUDOpponent = Associate("HUDOpponent");

		buttonMenu = Associate("TextButtonMenu");
		menuCentral = Associate("TextMenuCentral");

		indicatorCentral = Associate("TextIndicatorCentral");
		indicatorValidation = Associate("TextIndicatorValidation");
	}

	Text Associate (string name) {
		try {
			GameObject gameObject = GameObject.FindGameObjectWithTag (name);
			Text txt = gameObject.GetComponent<Text> ();
			return txt;
		} catch (NullReferenceException e) {
			Debug.Log ("UIController: I could not find object with tag '" + name + "'");
			throw e;
		}
	}
}
