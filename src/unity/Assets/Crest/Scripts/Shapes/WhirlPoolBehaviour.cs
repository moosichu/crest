using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhirlPoolBehaviour : MonoBehaviour {

    public float speed = 1.0f;

    private MeshRenderer mf = null;

	// Use this for initialization
	void Start () {
        mf = GetComponent< MeshRenderer > ();
    }

	// Update is called once per frame
	void Update () {
        // mf.material.SetFloat("_Time", 10.0f);
	}
}
