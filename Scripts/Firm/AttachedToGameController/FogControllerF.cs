using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // For method 'Contains' of arrays
using AssemblyCSharp;

public class FogControllerF : MonoBehaviour {

	List<ParticleSystem> fogControllers;

	void Awake () {
		GetFogControl ();
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void GetFogControl () {

		fogControllers = new List<ParticleSystem> ();

		for (int i = 0; i < GameFeatures.nPositions; i++) {
			GameObject go = GameObject.FindGameObjectWithTag (string.Concat ("Fog", i));
			fogControllers.Add (go.GetComponent<ParticleSystem> ());
		}
	}

	void MakeFogDisappear (int i) {

		// Debug.Log ("FogController: I will remove fog "+ i.ToString() + ".");
		fogControllers [i].Stop () ;
	}

	void MakeFogAppear (int i) {

		//Debug.Log ("FogController: I will make fog "+ i.ToString() + " appearing.");
		fogControllers [i].Play ();
	}

	public void RevealPositions (List<int> positions) {

		for (int i = 0; i < GameFeatures.nPositions; i++) {
			if (positions.Contains (i)) {
				MakeFogDisappear (i);
			} else {
				if (!fogControllers[i].isPlaying) {
					MakeFogAppear (i);
				}
			}
		}
	}

	public void FogOnEveryPosition () {
		for (int i = 0; i < GameFeatures.nPositions; i++) {
			MakeFogAppear (i);
		}
	}

	public void NoFog () {
		for (int i = 0; i < GameFeatures.nPositions; i++) {
			MakeFogDisappear (i);
		}
	}
}
