using UnityEngine;
using OptitrackManagement;

/// <summary>
/// Sets a GameObject's position and rotation to a RigidBody
/// tracked via OptiTrack, specified with an ID.
/// </summary>
public class OptiTrackTracking : MonoBehaviour
{
    public int ID;
    public GameObject trackedObject;
    public bool useOrientation = true;
    public bool applyCorrection;

    public float lastTrackedTime { get; private set; }

    ~OptiTrackTracking()
    {
        DirectMulticastSocketClient.Close();
    }

    void Start()
    {
        DirectMulticastSocketClient.Start();
    }

    protected virtual void Update()
    {
        ProcessOptiTrackInput(useOrientation);
    }

    public bool ProcessOptiTrackInput(bool useOrientation)
    {
        RigidBody[] rigidBodies = DirectMulticastSocketClient.GetStreemData()._rigidBody;

        for (int i = 0; i < 10; i++)
        {
            if(rigidBodies[i].ID == ID) { 
                ApplyOptiTrackTransformToObject(rigidBodies[i], useOrientation,trackedObject);
                lastTrackedTime = Time.time;
            }
        }
        return false;
    }
    
    void ApplyOptiTrackTransformToObject(RigidBody rigidBody, bool useOrientation,GameObject o)
    {
        Vector3 pos = rigidBody.pos;
        pos.x *= -1;
        o.transform.localPosition = pos;

        if (applyCorrection)
            transform.localPosition += new Vector3(0, 0.1f, 0);
        // Gear VR object should not get its orientation via OptiTrack,
        // but from Gear VR's own sensors
        // Exception: in the Editor, there is no Gear VR sensors, we need the OptiTrack orientation
        //if (useOrientation || Application.isEditor)
            o.transform.localRotation = Quaternion.Inverse(rigidBody.ori);
        //o.transform.localRotation = rigidBody.ori;
    }

}
