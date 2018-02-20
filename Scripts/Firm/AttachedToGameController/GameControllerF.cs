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

	RoundF round;
	TutorialF tutorial;

	ClientF client;

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
		round = GetComponent<RoundF> ();
		tutorial = GetComponent<TutorialF> ();
		client = GetComponent<ClientF> ();

        GetParameters ();
    }

    void Start () {

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        isOccupied = false;

		currentStep = parameters.GetCurrentStep ();

		// TimeLine 
		stateGeneral = TLGeneralF.Init;

        fogController.NoFog ();
    }
        
    void Update () {

        if (!isOccupied) {

            isOccupied = true;
        
            switch (stateGeneral) {

			case TLGeneralF.Init:

				if (currentStep == GameStep.tutorial) {

					if (parameters.GetSkipTutorial ()) {
						tutorial.SetState (TLTutoF.End);
					}

					PrepareTutorial ();

				} else if (currentStep == GameStep.end) {
					uiController.ShowMessageAlreadyPlayed ();

				
				} else {
					PrepareNewRound ();
				}
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

    // ----------------- Called from uiController ---------------------- //

	public void UserGotIt () {
		tutorial.UserGotIt ();
	}

	public void UserChangePrice () {
		tutorial.UserChangePrice ();
	}
		
    public void UserChangePosition () {
		if (stateGeneral == TLGeneralF.Tuto) {
			tutorial.UserChangePosition ();
		} else {
			round.UserChangePosition ();
		}
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

		if (currentStep == GameStep.tutorial) {
			stateGeneral = TLGeneralF.Tuto;
		} else {
			round.Begin ();
			stateGeneral = TLGeneralF.Game;
		}
    }

    // ----------------- Called from populationController ---------------------- //

    public void ConsumersAreArrived () {

        Debug.Log ("GC: Consumers are arrived.");

        if (stateGeneral == TLGeneralF.Tuto) {
			tutorial.ConsumersAreArrived ();

        } else if (stateGeneral == TLGeneralF.Game) {    
			round.ConsumersAreArrived ();
        }
    }

    public void OpponentIsArrived () {

        Debug.Log ("GC: Opponent is arrived.");
        if (stateGeneral == TLGeneralF.Tuto) {
			tutorial.OpponentIsArrived ();
        
		} else if (stateGeneral == TLGeneralF.Game) {    
			round.OpponentIsArrived ();
        }
    }

	// ---------- Called from round ------------- //

	public void GotInitInfo () {

		if (currentStep == GameStep.pve) {
			uiController.ShowMessagePVE ();

		} else if (currentStep == GameStep.pvp) {
			uiController.ShowMessagePVP ();
		}

		uiController.ShowButtonMenu ();
	}

    public void EndTutorial () {

        Debug.Log ("GC: EndTutorial.");

		currentStep = GameStep.pve;

		PrepareNewRound ();
    }

	public void EndOfRound () {

		if (currentStep == GameStep.pve) {
			InterRound();
		} else {
			EndOfGame ();
		}
	}

	public void FatalError () {
		Debug.Log ("GC: Fatal error");
		if (client.GetError () == CodeErrorF.opponentDisconnected) {
			uiController.ShowMessageOpponentDisconnected ();
		} else {
			uiController.ShowMessagePlayerDisconnected ();
		}
		stateGeneral = TLGeneralF.None;
	}

	// --------------- Transitions ------------------ //

	void PrepareTutorial () {

		Debug.Log("GC: Prepare tutorial");

		uiController.ShowMenu ();
		uiController.ShowMessageTutorial ();
		uiController.ShowButtonMenu ();

		stateGeneral = TLGeneralF.Waiting; // Waiting for the user (push button)
	}

	void PrepareNewRound() {

		Debug.Log ("GC: Prepare new round");
		
		uiController.ShowMessageWaiting ();

		round.Initialize ();
		uiController.ShowMenu ();

		stateGeneral = TLGeneralF.Game; // Waiting for the server (init info)
	}

	void EndOfGame () {

		Debug.Log ("GC: End of game");
		uiController.ShowMessageFinal ();
		stateGeneral = TLGeneralF.None;
	}

	void InterRound () {

		Debug.Log ("GC: InterRound");
		currentStep = GameStep.pvp;

		PrepareNewRound ();
	}
        
    // --------------- Getters relative to parameters ------------- //

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

	// ------------ Getters relative to other attributes -------------------- //

	public string GetCurrentStep () {
		return currentStep;
	}    

	public int GetScore() {
		return round.GetScore ();
	}
}
