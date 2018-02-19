using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AssemblyCSharp;
using UnityEngine.Networking;


public class Utils {

	public static int[] Range(int min, int max) {

		int count = max-min + 1;
		return  Enumerable.Range (min, count).ToArray ();
	}
}
