using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {

		int cj = 0;
		int cj2 = 0;

		for (int j = 0; j< 100; j ++) {

			//Random.InitState(Random.Range (0, 100000000));

			int len = 21;
			int[] c = new int[len];
			for (int i = 0; i < len; i++) {
				// bool a = Random.Range (0, 2) ==0;// Random.Range (0f, 1f) < 0.5;
				// int r = a ? 1 : 0;
				int r = Random.Range (0, 2);
				c [r] += 1;
			}

			// Debug.Log (c[0] + " " + c[1]);
			if (c [0] > c [1]) {
				cj += 1;
			} else if (c[0] < c[1]) {
				cj2 += 1;
			}
	
		}
		Debug.Log (cj + " " + cj2);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
