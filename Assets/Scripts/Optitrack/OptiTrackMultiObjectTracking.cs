using UnityEngine;
using OptitrackManagement;

/// <summary>
/// Sets a GameObject's position and rotation to a RigidBody
/// tracked via OptiTrack, specified with an ID.
/// </summary>
public class OptiTrackMultiObjectTracking : MonoBehaviour
{
    public NewMain main;

    public bool useOrientation = true;
    public bool applyCorrection;

    public float lastTrackedTime { get; private set; }

    public int RigidBody_index_1_ID;
    public GameObject RigidBody_index_1;

    public int RigidBody_index_2_ID;
    public GameObject RigidBody_index_2;

    public GameObject localCameraRig;
    public GameObject remoteCameraRig;

    ~OptiTrackMultiObjectTracking()
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
            if (rigidBodies[i].ID == RigidBody_index_1_ID)
            {
                ApplyOptiTrackTransformToObject(rigidBodies[i], useOrientation, RigidBody_index_1);
            }
            if (rigidBodies[i].ID == RigidBody_index_2_ID)
            {
                ApplyOptiTrackTransformToObject(rigidBodies[i], useOrientation, RigidBody_index_2);
            }

            int localIndex = main.setupLocation == SetupLocation.LEFT ? 1 : 2;
            if (rigidBodies[i].ID == localIndex)
            {
                ApplyOptiTrackTransformToObject(rigidBodies[i], useOrientation, localCameraRig);
            }
            int remoteIndex = main.setupLocation == SetupLocation.LEFT ? 2 : 1;
            if (rigidBodies[i].ID == remoteIndex)
            {
                ApplyOptiTrackTransformToObject(rigidBodies[i], useOrientation, remoteCameraRig);
            }

        }
        lastTrackedTime = Time.time;

        return false;
    }
    
    void ApplyOptiTrackTransformToObject(RigidBody rigidBody, bool useOrientation, GameObject o)
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
    }

}
