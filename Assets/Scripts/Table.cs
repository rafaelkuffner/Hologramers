using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour {

    public GameObject BL;
    public GameObject BR;
    public GameObject TL;
    public GameObject TR;

	void Start () {
		
	}
	
	void Update () {
        Debug.DrawLine(BL.transform.position, BR.transform.position, Color.white);
        Debug.DrawLine(TL.transform.position, TR.transform.position, Color.white);
        Debug.DrawLine(BL.transform.position, TL.transform.position, Color.white);
        Debug.DrawLine(BR.transform.position, TR.transform.position, Color.white);
    }

    public void set(SurfaceRectangle surface)
    {

        
        transform.localPosition = (surface.SurfaceBottomLeft + surface.SurfaceTopRight) / 2;
        transform.localRotation = surface.rotation;



        BL.transform.position = surface.SurfaceBottomLeft;
        BR.transform.position = surface.SurfaceBottomRight;
        TL.transform.position = surface.SurfaceTopLeft;
        TR.transform.position = surface.SurfaceTopRight;
    }
}
