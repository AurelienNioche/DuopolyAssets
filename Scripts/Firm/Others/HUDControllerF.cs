using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDControllerF : MonoBehaviour {

	public float offsetRelativeY = 0.1f;

	Camera cam;
	UIControllerF uiController;

	Text text;

	// -------------- Inherited from MonoBehavior ---------------------------- //

	void Start () {

		// Get components
		uiController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<UIControllerF> ();
		cam = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
	}

	void Update () {

		// Follow the avatar it has been made for.
		Vector3 target = uiController.GetTarget (tag);
		Vector3 screenPos = cam.WorldToScreenPoint (target);
		Vector3 offset = new Vector3(0, offsetRelativeY * Screen.height, 0);
		Vector3 wantedPos = screenPos + offset;
		transform.position = wantedPos;
	}
}
