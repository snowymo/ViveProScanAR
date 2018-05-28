using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveTest : MonoBehaviour {

    public Transform trans1, trans2;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<UnityEngine.UI.Text>().text = (trans1.position - trans2.position).ToString("F4");
	}
}
