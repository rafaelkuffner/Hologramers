using UnityEngine;
using UnityEngine.VR;
using System.Collections;

/// <summary>
/// This class is responsible for updating the caracter head 
/// rotation according to the HMD (Head Mounted Display) rotation.
/// </summary>
public class HeadCameraController : MonoBehaviour
{

    public TrackerClient _bodies;

    
    void Start()
    {

    }

    void Update()
    {

            //this.transform.position = new Vector3(0, 1, 0); return;

        Vector3 headposition = _bodies.GetHeadPos();
            this.transform.position = headposition;
    }
}