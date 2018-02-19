using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonConnectedToKey : MonoBehaviour {

	public KeyCode[] keys;
	Button button;

	// Use this for initialization
	void Start () {
		button = GetComponent<Button> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (button.interactable) {
			foreach (KeyCode key in keys) {
				if (Input.GetKeyUp (key)) {
					button.onClick.Invoke ();
					return;
				}
			}
		}
	}

}
