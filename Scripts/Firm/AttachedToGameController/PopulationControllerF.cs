using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AssemblyCSharp;
using cakeslice; // For outriles


public class PopulationControllerF : MonoBehaviour
{

    public bool debug = false;

    public Vector3 offsetIfSameLocation = new Vector3(0f, 0f, -2f);

    // GameController
    GameControllerF gameController;

    // Avatars
    GameObject playerAvatar;
    GameObject opponentAvatar;

    List<GameObject> consumerAvatar = new List<GameObject>();

    // Controllers of avatars
    AvatarFirmController playerController;
    AvatarFirmController opponentController;
    List<AvatarConsumerController> consumerController = new List<AvatarConsumerController>();

    // List of outline objects for each consumer
    List<Outline> consumerOutlines = new List<Outline>();

    // Locations
    List<Vector3> firmLocations = new List<Vector3>();
    List<Vector3> consumerLocations = new List<Vector3>();
    List<Vector3> consumerNapLocations = new List<Vector3>();
    List<Vector3> outsideLocations = new List<Vector3>();

    int playerPosition;
    int opponentPosition;

    int initialPlayerPosition;
    int initialOpponentPosition;

    bool playerShifted = false;
    bool opponentShifted = false;

	int[, ] consumersFieldOfView;


    void Awake()
    {
        gameController = GetComponent<GameControllerF>();

        // Retrieve coordinates of locations and instantiate avatars (getting their scripts)
        GetLocationPositions();
        GetAgentsControl();
        GetAgentsOutline();

    }


    // Use this for initialization
    void Start() {

		consumersFieldOfView = gameController.GetConsumersFieldOfView ();
		if (consumersFieldOfView == null) {
			Debug.Log("Generate fake 'consumerFieldOfView'");
			consumersFieldOfView = GameTools.GenerateConsumersFieldOfView ();
		}

    }

    // Update is called once per frame
    void Update()
    {
    }

    // -------------------------- Called at start ----------------  // 

    public void GetLocationPositions()
    {

        for (int i = 0; i < GameFeatures.nPositions; i++)
        {
            GameObject firmLocation = GameObject.FindGameObjectWithTag(string.Concat("FirmLocation", i));
            firmLocations.Add(firmLocation.transform.position);

            GameObject consumerLocation = GameObject.FindGameObjectWithTag(string.Concat("ConsumerLocation", i));
            consumerLocations.Add(consumerLocation.transform.position);

            GameObject consumerNapLocation = GameObject.FindGameObjectWithTag(string.Concat("ConsumerNapLocation", i));
            consumerNapLocations.Add(consumerNapLocation.transform.position);
        }

        for (int i = 0; i < 2; i++)
        {
            GameObject outsideLocation = GameObject.FindGameObjectWithTag(string.Concat("OutsideLocation", i));
            outsideLocations.Add(outsideLocation.transform.position);
        }

    }

    void GetAgentsControl()
    {

        // Get game objects

        for (int i = 0; i < GameFeatures.nPositions; i++)
        {
            GameObject ca = GameObject.FindGameObjectWithTag(string.Concat("AvatarConsumer", i));
            consumerAvatar.Add(ca);
        }

        playerAvatar = GameObject.FindGameObjectWithTag("AvatarPlayer");
        opponentAvatar = GameObject.FindGameObjectWithTag("AvatarOpponent");

        // Get access to controllers (scripts)

        playerController = playerAvatar.GetComponent<AvatarFirmController>();
        opponentController = opponentAvatar.GetComponent<AvatarFirmController>();
        for (int i = 0; i < GameFeatures.nPositions; i++)
        {
            consumerController.Add(consumerAvatar[i].GetComponent<AvatarConsumerController>());
        }

    }

    void GetAgentsOutline()
    {
        for (int i = 0; i < GameFeatures.nPositions; i++)
        {
            Outline outline = consumerController[i].transform.Find("SP2_Character_SurferDude").GetComponent<Outline>();
            consumerOutlines.Add(outline);
            consumerOutlines[i].enabled = false;

        }

    }


    public void PlaceAgentsToInitialPosition(int playerPosition, int opponentPosition)
    {

        this.playerPosition = playerPosition;
        this.opponentPosition = opponentPosition;

        MovePlayerStraightforward(playerPosition);
        MoveOpponentStraightforward(opponentPosition);
        for (int i = 0; i < GameFeatures.nPositions; i++)
        {
            consumerAvatar[i].transform.position = consumerNapLocations[i];

        }
		HideConsumersOutline ();

    }

    // --------------------- Are player or consumers moving -------------------------- //

    public bool PlayerIsMoving()
    {
        return playerController.GetIsWalking();
    }

    public bool OpponentIsMoving()
    {
        return opponentController.GetIsWalking();
    }

    public bool ConsumersAreMoving()
    {
        bool consumerMoving = false;
        for (int i = 0; i < GameFeatures.nPositions; i++)
        {
            if (consumerController[i].GetIsWalking())
            {
                consumerMoving = true;
                break;
            }
        }
        return consumerMoving;
    }

    // ------------------ Move avatars ------------------------------------------ //

    public void MovePlayer(int playerPosition)
    {
        if (debug)
        {
            Debug.Log("I move player to position " + playerPosition + ".");
        }
        playerController.SetActive(true);
        opponentController.SetActive(false);

        if (this.playerPosition != playerPosition)
        {

            this.playerPosition = playerPosition;
            Vector3 newPosition = firmLocations[playerPosition];
            if (playerPosition == opponentPosition && !opponentShifted)
            {
                newPosition += offsetIfSameLocation;
                playerShifted = true;
            }
            else
            {
                playerShifted = false;
            }

            playerController.SetGoal(newPosition);
        }
    }

    public void MoveOpponent(int opponentPosition)
    {

        if (debug)
        {
            Debug.Log("I move opponent to position " + opponentPosition + ".");
        }
        playerController.SetActive(false);
        opponentController.SetActive(true);

        if (this.opponentPosition != opponentPosition)
        {
            this.opponentPosition = opponentPosition;
            Vector3 newPosition = firmLocations[opponentPosition];
            if (playerPosition == opponentPosition && !playerShifted)
            {
                newPosition += offsetIfSameLocation;
                opponentShifted = true;
            }
            else
            {
                opponentShifted = false;
            }
            opponentController.SetGoal(newPosition);
        }
    }


    public IEnumerator MoveOpponentAndSendSignal(int opponentPosition) {

        MoveOpponent(opponentPosition);

        while (opponentController.GetIsWalking())
        {
            yield return new WaitForEndOfFrame();
        }

        gameController.OpponentIsArrived();
    }

	public void MovePlayerStraightforward(int playerPosition) {

        opponentController.SetActive(false);

        if (debug)
        {
            Debug.Log("I move player to position " + playerPosition + ".");
        }
        this.playerPosition = playerPosition;
        Vector3 newPosition = firmLocations[playerPosition];
        playerController.transform.position = newPosition;

        if (playerPosition == opponentPosition)
        {
            if (debug)
            {
                Debug.Log("PopulationController: Same position for player and opponent!");
            }
            playerController.transform.position += offsetIfSameLocation;

        }        
    }

    public void MoveOpponentStraightforward(int opponentPosition)
    {

        playerController.SetActive(false);

        if (debug)
        {
            Debug.Log("I move opponent to position " + opponentPosition + ".");
        }

        this.opponentPosition = opponentPosition;
        Vector3 newPosition = firmLocations[opponentPosition];
        opponentController.transform.position = newPosition;
    }

    public IEnumerator MoveConsumers(int[] consumerChoices)
    {

        if (debug)
        {
            Debug.Log("populationController: Moving consumers.");
        }

        int goalPosition = -1;

        for (int i = 0; i < GameFeatures.nPositions; i++)
        {

            int choice = consumerChoices[i];

            if (choice == GameRole.opponent)
            {
                goalPosition = opponentPosition;
            }
            else if (choice == GameRole.player)
            {
                goalPosition = playerPosition;
            }

            if (choice == GameRole.opponent || choice == GameRole.player)
            {

                consumerController[i].MoveToFirm(consumerLocations[goalPosition]);
                if (debug)
                {
                    Debug.Log("Consumer " + i + " will move to position " + goalPosition + ".");
                }

            }
            else
            {
                if (debug)
                {
                    Debug.Log("Consumer " + i + " will not move.");
                }
            }

        }

        while (ConsumersAreMoving())
        {
            yield return new WaitForEndOfFrame();
        }

        gameController.ConsumersAreArrived();

    }

    public IEnumerator MoveBackConsumers()
    {

        if (debug)
        {
            Debug.Log("populationController: Moving back consumers.");
        }

        for (int i = 0; i < GameFeatures.nPositions; i++)
        {
            consumerController[i].ComeBack(consumerNapLocations[i]);
        }

        while (ConsumersAreMoving())
        {
            yield return new WaitForEndOfFrame();
        }

        gameController.ConsumersAreArrived ();
        //MakeConsumerRadiusAppear ();
    }

    public IEnumerator MoveExampleConsumer(int consumerPosition, int consumerChoice)
    {

        int i = consumerPosition;
        int goalPosition = -1;

        if (consumerChoice == GameRole.opponent)
        {

            goalPosition = opponentPosition;
            if (debug)
            {
                Debug.Log("I'm moving 'example consumer' towards opponent.");
            }

        }
        else if (consumerChoice == GameRole.player)
        {

            goalPosition = playerPosition;
            if (debug)
            {
                Debug.Log("I'm moving 'example consumer' towards player.");
            }

        }
        else
        {
            throw new System.ArgumentException("PopulationController: 'consumerChoice' arg should correspond to opponent or player.");
        }

        consumerController[i].MoveToFirm(consumerLocations[goalPosition]);

        while (consumerController[i].GetIsWalking())
        {
            yield return new WaitForEndOfFrame();
        }

        consumerController[i].ComeBack(consumerNapLocations[i]);

        while (consumerController[i].GetIsWalking())
        {
            yield return new WaitForEndOfFrame();
        }

        gameController.ConsumersAreArrived();
    }

    // ---------------------- Get positions (coordinates) ----------------------- //

    public Vector3 GetPlayerPosition()
    {
        return playerAvatar.transform.position;
    }

    public Vector3 GetOpponentPosition()
    {
        return opponentAvatar.transform.position;
    }

    // ------------------- Make avatar appearing ---------------------------- //

    public void MakePlayerAppear () {
        playerController.Appear ();
    }

	public void MakeOpponentAppear () {
        opponentController.Appear ();
    }

    public void MakeConsumerAppear (int i) {
        consumerController[i].Appear();
    }

    public void MakeAllConsumerAppear () {
        for (int i = 0; i < GameFeatures.nPositions; i++)
        {
            consumerController[i].Appear();
        }
    }

    public void MakePlayerDisappear () {
        playerController.Disappear ();
    }

    public void MakeOpponentDisappear () {
        opponentController.Disappear ();
    }

    public void MakeAllConsumersDisappear () {
		
        for (int i = 0; i < GameFeatures.nPositions; i++) {
            consumerController[i].Disappear();
        }
    }


    // --------------------  Consumer outline management ------------------------ //

    public void ShowConsumersOutline () {

        for (int i = 0; i < GameFeatures.nPositions; i++) {

			List<int> positions = GameTools.PositionsSeenByAConsumer(
                i,
				consumersFieldOfView
            );

			List<int> firms = GameTools.FirmsSeenByAConsumer(
                positions, 
                playerPosition,
				opponentPosition
            );

            if (firms.Contains(GameRole.player)) {

                if (debug) {
                    Debug.Log("Consumer " + i + " is reachable.");
                }
                
                consumerOutlines[i].enabled = true;

            } else {
 
                consumerOutlines[i].enabled = false;
            }

        }

    }

    public void HideConsumersOutline() {
		
        for (int i = 0; i < GameFeatures.nPositions; i++) {    
            consumerOutlines[i].enabled = false;
        }

    }
}

