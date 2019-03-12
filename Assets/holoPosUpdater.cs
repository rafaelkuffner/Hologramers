using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class holoPosUpdater : MonoBehaviour {

    public GameObject optiTrackObejct;
	
	// Update is called once per frame
	void Update () {
        this.transform.localPosition = optiTrackObejct.transform.localPosition;
	}
}
