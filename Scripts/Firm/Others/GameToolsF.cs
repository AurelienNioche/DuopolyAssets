using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AssemblyCSharp;
using UnityEngine.Networking;


public class GameTools {

	static float radiusForTesting = 0.25f;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		
	}

	// --------------  For tutorial  -------------- //

	public static int NewFakeOpponentPrice (int initialOpponentPrice) {
		int newPrice = initialOpponentPrice - 2;
		int opponentPrice = Mathf.Max (Mathf.Min(GameFeatures.maximumPrice, newPrice), GameFeatures.minimumPrice);
		return opponentPrice;
	}

	public static int NewFakeOpponentPosition (int initialOpponentPosition) {
		int pos = initialOpponentPosition + 2;
		int opponentPosition = Mathf.Max (Mathf.Min (GameFeatures.maximumPosition, pos), GameFeatures.minimumPosition);
		return opponentPosition;
	}

	public static int[] NewFakeConsumerChoices (int[,] consumersFieldOfView, int position, int opponentPosition, int price, int opponentPrice) {

		int[] fakeConsumerChoices = new int[GameFeatures.nPositions];

		for (int i = 0; i < GameFeatures.nPositions; i++) {

			List<int> pos = PositionsSeenByAConsumer (i, consumersFieldOfView);

			List<int> seen = FirmsSeenByAConsumer (pos, position, opponentPosition);

			int target = GetConsumerTarget (seen, price, opponentPrice);

			fakeConsumerChoices [i] = target;
		};

		return fakeConsumerChoices;
	}

	// -------- //

	public static int GetConsumerTarget (List<int> firmsSeen, int price, int opponentPrice) {

		int target;

		if (firmsSeen.Contains (GameRole.opponent) && firmsSeen.Contains (GameRole.player)) {

			if (price < opponentPrice) {
				target = GameRole.player;

			} else if (opponentPrice < price) {
				target = GameRole.opponent;

			} else {
				int r = UnityEngine.Random.Range (0, 2);
				if (r == 1) {
					target = GameRole.player;
				} else {
					target = GameRole.opponent;
				}
			}

		} else if (firmsSeen.Contains (GameRole.player)) {
			target = GameRole.player;	

		} else if (firmsSeen.Contains (GameRole.opponent)) {
			target = GameRole.opponent;

		} else {
			target = GameRole.noOne;
		}

		return target;
	}

	public static List<int> PositionsSeenByAConsumer (int consumerPosition, int [,] consumersFieldOfView) {

		List<int> positionsSeen = new List<int> ();

		for (int i = 0; i < GameFeatures.nPositions; i++) {
			if (consumersFieldOfView [consumerPosition, i] == 1) {
				positionsSeen.Add (i);
			}
		}
			
		return positionsSeen;
	}

	public static List<int> FirmsSeenByAConsumer (List<int> positionsSeen, int playerPosition, int opponentPosition) { 

		List<int> firmsSeen = new List<int> ();

		if (positionsSeen.Contains (playerPosition)) {
			firmsSeen.Add (GameRole.player);
		}

		if (positionsSeen.Contains (opponentPosition)) {
			firmsSeen.Add (GameRole.opponent);
		}

		return firmsSeen;
	}

	public static int[,] TranslateFromFieldOfView (string strFov) {

		int[, ] array = new int[GameFeatures.nPositions, GameFeatures.nPositions];

		string [] parts = strFov.Split (new char[] {'$'}, StringSplitOptions.RemoveEmptyEntries);

		int i = 0; // Would be a consumer idx
		foreach (string part in parts) {
			int j = 0; // Would be a position idx
			string [] littleParts = part.Split (new char[] {'!'}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string littlePart in littleParts) {
				array [i, j] = int.Parse (littlePart);
				j++;
			}
			i++;
		}
			
		return array;
	}

	public static int[,] GenerateConsumersFieldOfView () {

		int radius = (int) Math.Round(radiusForTesting*GameFeatures.nPositions);

		int[, ] array = new int[GameFeatures.nPositions, GameFeatures.nPositions];
		for (int i=0; i<GameFeatures.nPositions; i++) { // Iterate over n consumers
			for (int j = 0; j < GameFeatures.nPositions; j++) {
				if (i - radius <= j && i + radius >= j) {
					array [i, j] = 1;
				} else {
					array [i, j] = 0;
				}
			}
		}
		return array;
	}
}
