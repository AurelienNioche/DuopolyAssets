using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using AssemblyCSharp;


public class GameControllerF : MonoBehaviour {

    UIControllerF uiController;
    FogControllerF fogController;

	MenuF menu;
	RoundF round;
	TutorialF tutorial;

	// Timeline
	TLGeneralF stateGeneral;

	Parameters parameters;

    bool isOccupied;

	string currentStep;

    // -------------- Overloaded methods from MonoBehavior ---------------------------- //

    void Awake () {

        // Get components
        uiController = GetComponent<UIControllerF> ();
        fogController = GetComponent<FogControllerF> ();
		menu = GetComponent<MenuF> ();
		round = GetComponent<RoundF> ();
		tutorial = GetComponent<TutorialF> ();

        GetParameters ();
    }

    void Start () {

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        isOccupied = false;

		currentStep = parameters.GetCurrentStep ();

		// TimeLine 
		stateGeneral = TLGeneralF.Menu; //TLGeneralF.Tuto;

        fogController.NoFog ();
    }
        
    void Update () {

        if (!isOccupied) {

            isOccupied = true;
        
            switch (stateGeneral) {

            case TLGeneralF.Menu:

				menu.ManageState ();
                break;

            case TLGeneralF.Tuto:
                
				tutorial.ManageState ();
                break;

            case TLGeneralF.Game:
                
				round.ManageState ();
                break;
            }

            isOccupied = false;
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

    // --------------------------------- //

    IEnumerator ActionWithDelay(Action methodName, float seconds) {
        yield return new WaitForSeconds(seconds);

        methodName ();
    }

    // ----------------- Called from uiController ---------------------- //

	public void UserGotIt () {
		tutorial.UserGotIt ();
	}
		
    public void UserChangePosition () {
		if (stateGeneral == TLGeneralF.Tuto) {
			tutorial.UserChangePosition ();
		} else {
			uiController.AuthorizeValidation (true);
		}
    }

    public void UserChangePrice () {

		tutorial.UserChangePrice ();
    }

    public void UserValidate () {

        if (stateGeneral == TLGeneralF.Tuto) {
			tutorial.UserValidate ();
        } else if (stateGeneral == TLGeneralF.Game) {
			round.UserValidate ();
        }
    }

    public void UserCollect () {

        if (stateGeneral == TLGeneralF.Tuto) {
			tutorial.UserCollect ();
        } else if (stateGeneral == TLGeneralF.Game) {
			round.UserCollect ();
        }
    }

    public void UserPushedButtonMenu () {
    
		if (stateGeneral == TLGeneralF.Menu) {
			menu.UserPushedButtonMenu ();
        }
        
    }

    // ----------------- Called from populationController ---------------------- //

    public void ConsumersAreArrived () {

        Debug.Log ("GameController: Consumers are arrived.");

        if (stateGeneral == TLGeneralF.Tuto) {
			tutorial.ConsumersAreArrived ();

        } else if (stateGeneral == TLGeneralF.Game) {    
			round.ConsumersAreArrived ();
        }
    }

    public void OpponentIsArrived () {

        Debug.Log ("GameController: Opponent is arrived.");
        if (stateGeneral == TLGeneralF.Tuto) {
			tutorial.OpponentIsArrived ();
        
		} else if (stateGeneral == TLGeneralF.Game) {    
			round.OpponentIsArrived ();
        }
    }

    // -------------  Begin game  ------------- //

    public void BeginTheGame () {

        Debug.Log ("GameController: BeginTheGame.");

        stateGeneral = TLGeneralF.Game;

		round.Begin ();
    }

    // -------------  Begin tutorial  ------------- //

    public void BeginTheTutorial () {

        Debug.Log ("GameController: BeginTheTutorial.");

		if (parameters.GetSkipTutorial ()) {
			tutorial.SetState (TLTutoF.End);
		}

        stateGeneral = TLGeneralF.Tuto;
        
    }

    public void EndTutorial () {

        Debug.Log ("GameController: EndTutorial.");

		currentStep = GameStep.pve;

        stateGeneral = TLGeneralF.Menu;
		menu.Begin ();
    }

	public void EndOfRound () {

		if (currentStep == GameStep.pve) {
			InterRound();
		} else {
			EndOfGame ();
		}
	}

	void EndOfGame () {
		Debug.Log ("GC: End of game");
		uiController.ShowMessageFinal ();
		stateGeneral = TLGeneralF.None;
	}

	public void PlayerDisconnected () {
		Debug.Log ("GC: Player disconnected");
		uiController.ShowMessagePLayerDisconnected ();
		stateGeneral = TLGeneralF.None;
	}
		
	// ------------ Inter rounds ------------------------ //

	void InterRound () {
		Debug.Log ("GC: InterRound");
		currentStep = GameStep.pvp;
		stateGeneral = TLGeneralF.Menu;
		menu.Begin ();
	}
        
    // --------------- Relative to parameters ------------- //

    public string GetUrl () {
        return parameters.GetUrl ();
    }

    public float GetTimeBeforeRetryingDemand () {
        return parameters.GetTimeBeforeRetryingDemand ();
    }

	public string GetUserName () {
		return parameters.GetUserName ();
	}

	public string GetPlayerId () {
		return parameters.GetPlayerId ();
	}

	public int[,] GetConsumersFieldOfView () {
		return parameters.GetConsumersFieldOfView ();
	}

	// ----------------------------------- //
	public string GetCurrentStep () {
		return currentStep;
	}    

	public int GetScore() {
		return round.GetScore ();
	}
}
