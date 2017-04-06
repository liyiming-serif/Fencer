using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Fencer))]
public class PlayerController : MonoBehaviour {

    private Fencer fencer;

	// Use this for initialization
	void Start () {
        fencer = gameObject.GetComponent<Fencer>();
	}

    void Update() {
        if (Input.GetButtonDown("Fire1")){
            fencer.Attack();
            return;
        }
        if (Input.GetButtonDown("Fire2")){
            StartCoroutine(fencer.Parry());
            return;
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
            fencer.Move(1);     //advancing
            return;
        }
        if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
            fencer.Move(-1);    //retreating
            return;
        }
	}
}
