using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp;

public class ScoresManagerF : MonoBehaviour {

	int scoreCumulative;
	int opponentScoreCumulative;
	int scoreTurn;
	int opponentScoreTurn;
	int nClients;
	int opponentNClients;

	// Use this for initialization
	void Start () {
		scoreCumulative = 0;
		opponentScoreCumulative= 0;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ComputeScores (int[] consumerChoices, int price, int opponentPrice) {

		nClients = 0;
		opponentNClients = 0;

		for (int i = 0; i < GameFeatures.nPositions; i++) {
			if (consumerChoices [i] == GameRole.player) {
				nClients += 1;
			} else if (consumerChoices [i] == GameRole.opponent) {
				opponentNClients += 1;
			}
		}

		scoreTurn = price * nClients;
		opponentScoreTurn = opponentPrice * opponentNClients;

		scoreCumulative += scoreTurn;
		opponentScoreCumulative += opponentScoreTurn;

		Debug.Log ("Player: Scores for this turn are: Player: " + scoreTurn + ", Opponent: " + opponentScoreTurn + ".");
	}

	public void ResetScores() {
		scoreCumulative = 0;
		opponentScoreCumulative = 0;
	}

	public void SetCumulativeScores (int score, int opponentScore) {
		scoreCumulative = score;
		opponentScoreCumulative = opponentScore;
	}

	public int GetScoreCumulative () {
		return scoreCumulative;
	}

	public int GetOpponentScoreCumulative () {
		return opponentScoreCumulative;
	}

	public int GetScoreTurn () {
		return scoreTurn;
	}

	public int GetOpponentScoreTurn() {
		return opponentScoreTurn;
	}

	public int GetNClients() {
		return nClients;
	}

}
