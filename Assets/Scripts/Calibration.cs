using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Calibration : MonoBehaviour {


    public string ConfigFile;
    public SetupLocation setupLocation;
    private string _localPrefix;
    private string _remotePrefix;

    public TrackerMesh ravatarManagerTracker;

    private string localTrackerAddress;
    private string remoteTrackerAddress;
    private int localTrackerListenPort;
    private int remoteTrackerListenPort;
    private int localTrackerSurfaceRequestPort;
    private int remoteTrackerSurfaceRequestPort;
    private int localTrackerSurfaceListenerPort;
    private int remoteTrackerSurfaceListenerPort;

    public Transform RB1;
    public Transform RB2;
    public Transform RB3;
    public Transform RB4;

    public Transform ARCameraRig;
    public Transform localCreepyTrackerOrigin;
    public Transform remoteCreepyTrackerOrigin;

    public Dictionary<string, GameObject> _sensors;
    private SurfaceRectangle _localSurface;
    private SurfaceRectangle _remoteSurface;

    private bool _everythingIsConfigured = false;
    private bool gotClouds = false;
    public bool devCalibration;

    private Transform _Delta;

    enum CalibrationMode { Eyes, Delta}

    // Use this for initialization
    void Start () {

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

        GameObject eyes = GameObject.Find("Eyes");
        eyes.transform.position = _getPositionFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".eyes.localPosition"));
        eyes.transform.rotation = _getRotationFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".eyes.localRotation"));

        if (!devCalibration)
        {
            RB1.GetComponentInChildren<MeshRenderer>().enabled = false;
            RB2.GetComponentInChildren<MeshRenderer>().enabled = false;
            if (setupLocation ==  SetupLocation.LEFT)
            {
                RB4.GetComponentInChildren<MeshRenderer>().enabled = false;
            }
            else
            {
                RB3.GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }

        _sensors = new Dictionary<string, GameObject>();
        _surfaceRequest();

    }

    // Update is called once per frame
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

        gotClouds = true;
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

        _deploySensors(_localSurface.sensors, localCreepyTrackerOrigin,true);
        Vector3 locpos = _getPositionFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".rigidBodyCalibration.transform.position"));
        Quaternion locrot = _getRotationFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".rigidBodyCalibration.transform.rotation"));
        string locKinectName = ConfigProperties.load(ConfigFile, _localPrefix + ".trackedKinect.name");

        _sensors[locKinectName].transform.parent = null;
        localCreepyTrackerOrigin.parent = _sensors[locKinectName].transform;
        _sensors[locKinectName].transform.position = locpos;
        _sensors[locKinectName].transform.rotation = locrot;
        // _sensors[locKinectName].transform.forward = -_sensors[locKinectName].transform.forward;
        localCreepyTrackerOrigin.parent = null;
        _sensors[locKinectName].transform.parent = localCreepyTrackerOrigin.transform;

         _deploySensors(_remoteSurface.sensors, remoteCreepyTrackerOrigin, devCalibration);
      

            Vector3 rempos = _getPositionFromConfig(ConfigProperties.load(ConfigFile, _remotePrefix + ".rigidBodyCalibration.transform.position"));
        Quaternion remrot = _getRotationFromConfig(ConfigProperties.load(ConfigFile, _remotePrefix + ".rigidBodyCalibration.transform.rotation"));
        string remKinectName = ConfigProperties.load(ConfigFile, _remotePrefix + ".trackedKinect.name");

        _sensors[remKinectName].transform.parent = null;
        remoteCreepyTrackerOrigin.parent = _sensors[remKinectName].transform;
        _sensors[remKinectName].transform.position = rempos;
        _sensors[remKinectName].transform.rotation = remrot;
        //   _sensors[remKinectName].transform.forward = -_sensors[remKinectName].transform.forward;
        remoteCreepyTrackerOrigin.parent = null;
        _sensors[remKinectName].transform.parent = remoteCreepyTrackerOrigin.transform;

        //Position to center the avatar


        Vector3 deltapos = _getPositionFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".remoteCreepyTrackerDelta.position"));
        Quaternion deltarot = _getRotationFromConfig(ConfigProperties.load(ConfigFile, _localPrefix + ".remoteCreepyTrackerDelta.rotation"));

        GameObject g = new GameObject("Delta");
        _Delta = g.transform;

        remoteCreepyTrackerOrigin.parent = _Delta;
        _Delta.position = deltapos;
        _Delta.rotation = deltarot;

    }

    

    private void Update()
    {
        Transform myRig = setupLocation == SetupLocation.LEFT ? RB1 : RB2;
        ARCameraRig.transform.position = myRig.position;
        ARCameraRig.transform.rotation = myRig.rotation;

        if (!_everythingIsConfigured && _localSurface != null && _remoteSurface != null)
        {
            calibrateOptiTrackAndCreepyTracker();
            _everythingIsConfigured = true;
            if (devCalibration)
            {
                ravatarManagerTracker.Init(
                  remoteTrackerListenPort,
                  int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".client.avatar.listen.port")),
                  remoteCreepyTrackerOrigin
                  );
            }
        }


        

        if (Input.GetKeyDown(KeyCode.D))
        {
            GameObject pivot = GameObject.Find("Delta");
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


        if (Input.GetKeyDown(KeyCode.E))
        {
            GameObject eyes = GameObject.Find("Eyes");
            if (eyes != null)
            {
                string p = _gameObjectPositionToString(eyes.transform.localPosition);
                string r = _gameObjectRotationToString(eyes.transform.localRotation);
                ConfigProperties.save(ConfigFile, _localPrefix + ".eyes.localPosition", p);
                ConfigProperties.save(ConfigFile, _localPrefix + ".eyes.localRotation", r);
            }
            else
            {
                Debug.LogError("NO EYES FOUND");
            }
        }
    }

 

    private void _deploySensors(Sensor[] sensors, Transform parent,bool drawCube)
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
            if (drawCube)
            {
                GameObject cube = LittleCube(sensor.transform, sensor.name + "cube");
            }
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
