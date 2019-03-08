using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SetupLocation
{
    LEFT,
    RIGHT
}

public enum Formation
{
    SIDE_TO_SIDE,
    FACE_TO_FACE
}

public class NewMain : MonoBehaviour {

    public string ConfigFile;
    public SetupLocation setupLocation;
    public Formation formation;
    private string _localPrefix;
    private string _remotePrefix;

    public BodiesManager localBodiesManager;
    public UdpBodiesListener localUdpListener;
    public BodiesManager remoteBodiesManager;
    public UdpBodiesListener remoteUdpListener;

    public TrackerMesh ravatarManagerTracker;

    private string localTrackerAddress;
    private string remoteTrackerAddress;
    private int localTrackerListenPort;
    private int remoteTrackerListenPort;
    private int localTrackerSurfaceRequestPort;
    private int remoteTrackerSurfaceRequestPort;
    private int localTrackerSurfaceListenerPort;
    private int remoteTrackerSurfaceListenerPort;

    public Transform ARCameraRig;
    public Transform RemoteARCameraRig;

    public Transform localCreepyTrackerOrigin;
    public Transform remoteCreepyTrackerOrigin;
    public Transform remoteCreepyTrackerOriginDelta;

    public Transform hologramPivot;
    public Vector3 remoteCreepyTrackerPosition;

    public Dictionary<string, GameObject> _sensors;
    private SurfaceRectangle _localSurface;
    private SurfaceRectangle _remoteSurface;

    private bool _everythingIsConfigured = false;

    public Transform localWorkspaceOrigin;
    public Transform remoteWorkspaceOrigin;
    public Transform leftRigidBody;
    public Transform rightRigidBody;

    public Transform workspaceTransform;

    void Start()
    {

        Application.runInBackground = true;

        ConfigFile = Application.dataPath + "/config.txt";
        ConfigProperties.save(ConfigFile, "last.run", DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

        setupLocation = (SetupLocation)Enum.Parse(enumType: typeof(SetupLocation), value: ConfigProperties.load(ConfigFile, "setup.type"));
        if (setupLocation == SetupLocation.LEFT)
        {
            _localPrefix = "left"; _remotePrefix = "right";
        }
        else
        {
            _localPrefix = "right"; _remotePrefix = "left";
        }

        formation = (Formation)Enum.Parse(enumType: typeof(Formation), value: ConfigProperties.load(ConfigFile, "start.formation"));

        localTrackerListenPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.listen.port"));
        remoteTrackerListenPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.listen.port"));

        localTrackerAddress = ConfigProperties.load(ConfigFile, _localPrefix + ".setup.address");
        int localTrackerBroadcastPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.broadcast.port"));
        localTrackerSurfaceRequestPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.surface.request.port"));
        localTrackerSurfaceListenerPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.surface.listener.port"));
        int localAvatarListenPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".client.avatar.listen.port"));

        remoteTrackerAddress = ConfigProperties.load(ConfigFile, _remotePrefix + ".setup.address");
        int remoteTrackerBroadcastPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.broadcast.port"));
        remoteTrackerSurfaceRequestPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.surface.request.port"));
        remoteTrackerSurfaceListenerPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.surface.listener.port"));
        int remoteAvatarListenPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".client.avatar.listen.port"));

        GetComponent<CreepyTrackerSurfaceRequestListener>().StartReceive(localTrackerSurfaceListenerPort, remoteTrackerSurfaceListenerPort);
        localUdpListener.startListening(localTrackerBroadcastPort);
        remoteUdpListener.startListening(remoteTrackerBroadcastPort);

        localWorkspaceOrigin.transform.position = _getPositionFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".workspaceCenter.transform.position"));
        localWorkspaceOrigin.transform.rotation = _getRotationFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".workspaceCenter.transform.rotation"));
        remoteWorkspaceOrigin.transform.position = _getPositionFromConfig(ConfigProperties.load(ConfigFile, _remotePrefix + ".workspaceCenter.transform.position"));
        remoteWorkspaceOrigin.transform.rotation = _getRotationFromConfig(ConfigProperties.load(ConfigFile, _remotePrefix + ".workspaceCenter.transform.rotation"));

        _sensors = new Dictionary<string, GameObject>();
        _surfaceRequest();

    }

    internal void setRemoteSurface(SurfaceRectangle s)
    {
        Debug.Log("] REMOTE SURFACE " + s.ToString());
        _remoteSurface = s;
    }

    internal void setLocalSurface(SurfaceRectangle s)
    {
        Debug.Log("] LOCAL SURFACE " + s.ToString());
        _localSurface = s;
    }

    internal void setupSensors(GameObject[] sensors)
    {

        if (sensors.Length > 0 && _sensors.Values.Count > 0)
        {
            foreach (GameObject g in sensors)
            {
                g.transform.parent = _sensors[g.name].transform;
                g.transform.localPosition = Vector3.zero;
                g.transform.localRotation = Quaternion.identity;
            }
        }
        else
        {
            Debug.LogError("lokl");
        }
    }

    private void _surfaceRequest()
    {
        if (_localSurface == null || _remoteSurface == null)
        {
            Debug.Log("[" + this.ToString() + "] Surface Request Sent...");
            GetComponent<CreepyTrackerSurfaceRequest>().Request(localTrackerListenPort, localTrackerSurfaceListenerPort, remoteTrackerListenPort, remoteTrackerSurfaceListenerPort);
        }
        else
        {
            Debug.Log("[" + this.ToString() + "] WE ALREADY HAVE ALL SURFACES");

        }
    }

    private void calibrateOptiTrackAndCreepyTracker()
    {

        _deploySensors(_localSurface.sensors, localCreepyTrackerOrigin);
        Vector3 locpos = _getPositionFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".rigidBodyCalibration.transform.position"));
        Quaternion locrot = _getRotationFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".rigidBodyCalibration.transform.rotation"));
        string locKinectName = ConfigProperties.load(ConfigFile, _localPrefix + ".trackedKinect.name");

        _sensors[locKinectName].transform.parent = null;
        localCreepyTrackerOrigin.parent = _sensors[locKinectName].transform;
        _sensors[locKinectName].transform.position = locpos;
        _sensors[locKinectName].transform.rotation = locrot;
        _sensors[locKinectName].transform.forward = -_sensors[locKinectName].transform.forward;
        localCreepyTrackerOrigin.parent = null;
        _sensors[locKinectName].transform.parent = localCreepyTrackerOrigin.transform;

        _deploySensors(_remoteSurface.sensors, remoteCreepyTrackerOrigin);
        Vector3 rempos = _getPositionFromConfig(ConfigProperties.load(ConfigFile, _remotePrefix + ".rigidBodyCalibration.transform.position"));
        Quaternion remrot = _getRotationFromConfig(ConfigProperties.load(ConfigFile, _remotePrefix + ".rigidBodyCalibration.transform.rotation"));
        string remKinectName = ConfigProperties.load(ConfigFile, _remotePrefix + ".trackedKinect.name");

        _sensors[remKinectName].transform.parent = null;
        remoteCreepyTrackerOrigin.parent = _sensors[remKinectName].transform;
        _sensors[remKinectName].transform.position = rempos;
        _sensors[remKinectName].transform.rotation = remrot;
        _sensors[remKinectName].transform.forward = -_sensors[remKinectName].transform.forward;
        remoteCreepyTrackerOrigin.parent = null;
        _sensors[remKinectName].transform.parent = remoteCreepyTrackerOrigin.transform;

        Vector3 deltapos = _getPositionFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".remoteCreepyTrackerDelta.position"));
        Quaternion deltarot = _getRotationFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".remoteCreepyTrackerDelta.rotation"));
        remoteCreepyTrackerOriginDelta = new GameObject("RemoteCreepyTrackerOriginPivot").transform;
        remoteCreepyTrackerOrigin.transform.parent = remoteCreepyTrackerOriginDelta;
        remoteCreepyTrackerOriginDelta.position = deltapos;
        remoteCreepyTrackerOriginDelta.rotation = deltarot;
    }

    private void Update()
    {
        if (!_everythingIsConfigured && _localSurface != null && _remoteSurface != null)
        {
            calibrateOptiTrackAndCreepyTracker();

            ravatarManagerTracker.Init(
            remoteTrackerListenPort,
            int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".client.avatar.listen.port")),
            remoteCreepyTrackerOrigin
            );

            _configureWorkspace();
            _everythingIsConfigured = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _adjustHologramSize();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            string p = _gameObjectPositionToString(leftRigidBody.transform.position);
            string r = _gameObjectRotationToString(leftRigidBody.transform.rotation);
            ConfigProperties.save(ConfigFile, "left.workspaceCenter.transform.position", p);
            ConfigProperties.save(ConfigFile, "left.workspaceCenter.transform.rotation", r);

            p = _gameObjectPositionToString(rightRigidBody.transform.position);
            r = _gameObjectRotationToString(rightRigidBody.transform.rotation);
            ConfigProperties.save(ConfigFile, "right.workspaceCenter.transform.position", p);
            ConfigProperties.save(ConfigFile, "right.workspaceCenter.transform.rotation", r);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            string p = _gameObjectPositionToString(leftRigidBody.transform.position);
            string r = _gameObjectRotationToString(leftRigidBody.transform.rotation);
            ConfigProperties.save(ConfigFile, "left.rigidBodyCalibration.transform.position", p);
            ConfigProperties.save(ConfigFile, "left.rigidBodyCalibration.transform.rotation", r);

            p = _gameObjectPositionToString(rightRigidBody.transform.position);
            r = _gameObjectRotationToString(rightRigidBody.transform.rotation);
            ConfigProperties.save(ConfigFile, "right.rigidBodyCalibration.transform.position", p);
            ConfigProperties.save(ConfigFile, "right.rigidBodyCalibration.transform.rotation", r);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            GameObject pivot = GameObject.Find("RemoteCreepyTrackerOriginPivot");
            if (pivot != null)
            {
                string p = _gameObjectPositionToString(pivot.transform.position);
                string r = _gameObjectRotationToString(pivot.transform.rotation);
                ConfigProperties.save(ConfigFile, _localPrefix + ".remoteCreepyTrackerDelta.position", p);
                ConfigProperties.save(ConfigFile, _localPrefix + ".remoteCreepyTrackerDelta.rotation", r);


            }
            else
            {
                Debug.LogError("NO PIVOT FOUND");
            }
        }
    }

    private void _configureWorkspace()
    {
        //PUT THE GUY ON TOP OF HIS SURFACE
        //RemoteARCameraRig.transform.parent = remoteCreepyTrackerOrigin;
        //remoteWorkspaceOrigin.transform.parent = remoteCreepyTrackerOrigin;
        remoteCreepyTrackerPosition = remoteCreepyTrackerOriginDelta.position;

        GameObject g = new GameObject("Pivot");
        hologramPivot = g.transform;
        hologramPivot.parent = localWorkspaceOrigin.transform;
        hologramPivot.localPosition = Vector3.zero;
        hologramPivot.localRotation = Quaternion.identity;
        remoteCreepyTrackerOriginDelta.parent = hologramPivot;
        remoteCreepyTrackerOriginDelta.localPosition = Vector3.zero;
        remoteCreepyTrackerOriginDelta.localRotation = Quaternion.identity;

    }

    private void _adjustHologramSize()
    {
        ////Calculating forward
        Vector3 fw = RemoteARCameraRig.position - remoteWorkspaceOrigin.position;
        //Debug.DrawLine(_trackerClientRemote.spineBase.localPosition, _remoteHoloSurface, Color.cyan);

        //Translate it back to center of surface
        Vector3 holoPos = new Vector3(RemoteARCameraRig.position.x - remoteCreepyTrackerPosition.x , 0, RemoteARCameraRig.position.z - remoteCreepyTrackerPosition.z );
        remoteCreepyTrackerOriginDelta.localPosition = holoPos;
        //_trackercharRemote.transform.localPosition = new Vector3(-_trackerClientRemote.spineBase.localPosition.x, 0, -_trackerClientRemote.spineBase.localPosition.z);

        //Adjusting orientation
        fw.y = 0;
        Vector3 diff = localWorkspaceOrigin.position - ARCameraRig.position;
        diff.y = 0;
        hologramPivot.localRotation = Quaternion.identity;
        hologramPivot.Rotate(Vector3.Cross(fw, diff), Vector3.Angle(fw, diff),Space.Self);

        float localHeadY = ARCameraRig.position.y;
        float remoteHeadY = RemoteARCameraRig.position.y;

        float ratio = (localHeadY - localWorkspaceOrigin.transform.position.y) / remoteHeadY;

        print("ratio " + ratio);
        //if ratio > 1, means I'm the big guy, i can't upscale, so i do nothing.
        if (ratio <= 1)
        {
            float remoteRatio = (remoteHeadY - remoteWorkspaceOrigin.position.y) / localHeadY;
              //if his ratio is <= 1, means he can downscale me over there, so its fine
                //if not....
              print("remoteRatio" + remoteRatio);
            if (remoteRatio > 1)
            {
                Vector3 myHeadRemotePos = new Vector3(remoteWorkspaceOrigin.position.x, remoteWorkspaceOrigin.position.y + localHeadY, remoteWorkspaceOrigin.position.z);
                Vector3 hisHeadRemotePos = RemoteARCameraRig.position;
                //Vector from my head to his
                Vector3 viewVec = hisHeadRemotePos - myHeadRemotePos;
                Debug.DrawLine(hisHeadRemotePos, myHeadRemotePos, Color.green);

                hisHeadRemotePos.y = myHeadRemotePos.y;
                Vector3 planevec = hisHeadRemotePos - myHeadRemotePos;
                Debug.DrawLine(hisHeadRemotePos, myHeadRemotePos, Color.green);
                //view angle
                float angle = Vector3.Angle(planevec, viewVec);
                print("angle " + angle);
                //now in my space, i get a vector to the ideal eye to eye head position for him
                Vector3 hisHeadOrigin = localWorkspaceOrigin.position;
                hisHeadOrigin.y = localHeadY;
                Vector3 localHeadPos = ARCameraRig.position;
                Vector3 catetoAdjacente = hisHeadOrigin - localHeadPos;

                float heightDiffLocal = catetoAdjacente.magnitude * Mathf.Tan(Mathf.Deg2Rad * angle);
                print("diffLocal " + heightDiffLocal);
                ratio = (localHeadY - localWorkspaceOrigin.position.y + heightDiffLocal) / remoteHeadY;
                Vector3 debugremotehead = localWorkspaceOrigin.position;
                debugremotehead.y = localHeadY + heightDiffLocal;
                Debug.DrawLine(localHeadPos, debugremotehead, Color.red);
            }
            //use ratio to scale
            if (!float.IsInfinity(ratio) && !float.IsNaN(ratio) && ratio != 0)
            {
                print("Scaling");
                hologramPivot.localScale = new Vector3(ratio, ratio, ratio);
            }
        }
    }

    private void _deploySensors(Sensor[] sensors, Transform parent)
    {
        foreach (Sensor s in sensors)
        {
            GameObject sensor = new GameObject
            {
                name = s.id
            };
            sensor.transform.parent = parent;
            sensor.transform.localPosition = s.position;
            sensor.transform.localRotation = s.rotation;
            GameObject cube = LittleCube(sensor.transform, sensor.name + "cube");
            _sensors[s.id] = sensor;
        }

    }

    public GameObject LittleCube(Transform parent, string name)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        cube.transform.parent = parent;
        cube.transform.localPosition = Vector3.zero;
        cube.transform.rotation = Quaternion.identity;
        cube.name = name;
        return cube;
    }

    private string _gameObjectRotationToString(Quaternion rotation)
    {
        return "" + rotation.x + ":" + rotation.y + ":" + rotation.z + ":" + rotation.w;
    }

    private string _gameObjectPositionToString(Vector3 position)
    {
        return "" + position.x + ":" + position.y + ":" + position.z;
    }

    private Quaternion _getRotationFromConfig(string v)
    {
        // x:y:z:w
        string[] values = v.Split(':');
        return new Quaternion(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])); 
    }

    private Vector3 _getPositionFromConfig(string v)
    {
        // x:y:z
        string[] values = v.Split(':');
        return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
    }

    
}
