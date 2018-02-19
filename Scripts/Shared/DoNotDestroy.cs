using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroy : MonoBehaviour {

	void Awake () {
		DontDestroyOnLoad (transform.gameObject);
	}

	// Use this for initialization
	void Start () {
		if (GameObject.FindGameObjectsWithTag (tag).Length > 1) {
			Destroy (gameObject);
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
