using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

namespace cakeslice
{
    public class OutlineAnimation : MonoBehaviour
    {

		public float speed = 1.0f;
		public float alphaMin = 0.0f;
		public float alphaMax = 1.0f;

        bool pingPong = false;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Color c = GetComponent<OutlineEffect>().lineColor0;

            if (pingPong) {
				c.a += Time.deltaTime * speed;

				if (c.a >= alphaMax) {
					pingPong = false;
				}
            } else {
                c.a -= Time.deltaTime * speed;

				if (c.a <= alphaMin) {
					pingPong = true;
				}
            }

            c.a = Mathf.Clamp01(c.a);
            GetComponent<OutlineEffect>().lineColor0 = c;
            GetComponent<OutlineEffect>().UpdateMaterialsPublicProperties();
        }
    }
}
