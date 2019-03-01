using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabSphere : MonoBehaviour {


    public Transform leftHand_IndexTip;
    public Transform leftHand_ThumbTip;

    public Transform rightHand_IndexTip;
    public Transform rightHand_ThumbTip;

    public float pinchThreashold = 0.1f;

    public bool leftPinching = false;
    public bool leftGrabbing = false;

    public bool rightPinching = false;
    public bool rightGrabbing = false;

    void Start () {
		
	}
	
	void FixedUpdate () {

        Vector3 lp1 = leftHand_IndexTip.transform.position;
        Vector3 lp2 = leftHand_ThumbTip.transform.position;
        Vector3 rp1 = rightHand_IndexTip.transform.position;
        Vector3 rp2 = rightHand_ThumbTip.transform.position;

        Vector3 leftInteractionPosition = (lp1 + lp2) / 2;
        Vector3 rightInteractionPosition = (rp1 + rp2) / 2;

        leftPinching = Vector3.Distance(lp1, lp2) <= pinchThreashold;
        leftGrabbing = GetComponent<Collider>().bounds.Contains(leftInteractionPosition);

        rightPinching = Vector3.Distance(rp1, rp2) <= pinchThreashold;
        rightGrabbing = GetComponent<Collider>().bounds.Contains(rightInteractionPosition);

        if (rightGrabbing && rightPinching)
        {
            transform.position = rightInteractionPosition;
        }
        else if (leftGrabbing && leftPinching)
        {
            transform.position = leftInteractionPosition;
        }

    }
}
