using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandShiftTransform : MonoBehaviour {

    public Transform primaryHand;

    public Vector3 offset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = primaryHand.position + offset;
        transform.rotation = primaryHand.rotation;

    }
}
