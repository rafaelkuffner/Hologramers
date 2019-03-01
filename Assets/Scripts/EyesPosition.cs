using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyesPosition : MonoBehaviour {

    public float step = 0.005f;

	void Start () {
	
	}
	
	void Update () {

        if (Input.GetKeyDown(KeyCode.Keypad7)) transform.position = transform.position + new Vector3(step, 0, 0);
        if (Input.GetKeyDown(KeyCode.Keypad4)) transform.position = transform.position + new Vector3(-step, 0, 0);

        if (Input.GetKeyDown(KeyCode.Keypad8)) transform.position = transform.position + new Vector3(0, step, 0);
        if (Input.GetKeyDown(KeyCode.Keypad5)) transform.position = transform.position + new Vector3(0, -step, 0);

        if (Input.GetKeyDown(KeyCode.Keypad9)) transform.position = transform.position + new Vector3(0, 0, step);
        if (Input.GetKeyDown(KeyCode.Keypad6)) transform.position = transform.position + new Vector3(0, 0, -step);
    }
}
