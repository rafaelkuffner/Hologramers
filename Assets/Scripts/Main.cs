using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MainBACKUP : MonoBehaviour {

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

    public Transform localOrigin;
    public Transform remoteOrigin;

    public Dictionary<string, GameObject> _sensors;

    private string localTrackerAddress;
    private string remoteTrackerAddress;
    private int localTrackerListenPort;
    private int remoteTrackerListenPort;
    private int localTrackerSurfaceRequestPort;
    private int remoteTrackerSurfaceRequestPort;
    private int localTrackerSurfaceListenerPort;
    private int remoteTrackerSurfaceListenerPort;

    public Transform ARCameraRig;
    public Transform LocalHumanHead;

    private bool _everythingIsConfigured = false;

    void Awake () {

        Application.runInBackground = true;

        ConfigFile = Application.dataPath + "/config.txt";
        ConfigProperties.save(ConfigFile, "last.run", DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

        setupLocation = (SetupLocation) Enum.Parse(enumType: typeof(SetupLocation), value: ConfigProperties.load(ConfigFile, "setup.type"));
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

    }

    internal void setRemoteSurface(SurfaceRectangle s)
    {
        
    }

    internal void setLocalSurface(SurfaceRectangle s)
    {
        
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


    void Update () {

        if (!_everythingIsConfigured)
        {
            ravatarManagerTracker.Init(
            remoteTrackerListenPort,
            int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".client.avatar.listen.port")),
            remoteOrigin
            );

            _everythingIsConfigured = true;
        }

        //if (Input.GetKey(KeyCode.Space))
       // {
           // ARCameraRig.position = LocalHumanHead.position;
        //}

    }

    /*private void _configWorkspaces()
    {
        Debug.Log("NOW I CAN DO ALL THE STUFFS!!");

        LocalTable.set(_localSurface);
        RemoteTable.set(_remoteSurface);

        _deploySensors(_localSurface.sensors, localOrigin);
        _deploySensors(_remoteSurface.sensors, remoteOrigin);

        localWorkspaceCenter = new GameObject();
        localWorkspaceCenter.name = "localWorkspaceCenter";
        localWorkspaceCenter.transform.position = LocalTable.transform.position;
        localWorkspaceCenter.transform.rotation = LocalTable.transform.rotation;
        localOrigin.parent = localWorkspaceCenter.transform;


        remoteWorkspaceCenter = new GameObject();
        remoteWorkspaceCenter.name = "remoteWorkspaceCenter";
        remoteWorkspaceCenter.transform.position = RemoteTable.transform.position;
        remoteWorkspaceCenter.transform.rotation = RemoteTable.transform.rotation;
        remoteOrigin.parent = remoteWorkspaceCenter.transform;

        remoteWorkspaceCenter.transform.position = localWorkspaceCenter.transform.position;
        remoteWorkspaceCenter.transform.rotation = localWorkspaceCenter.transform.rotation;

        if (formation == Formation.FACE_TO_FACE)
        {

            remoteWorkspaceCenter.transform.rotation = Quaternion.LookRotation(-localWorkspaceCenter.transform.forward, localWorkspaceCenter.transform.up);
        }


        workspace.gameObject.SetActive(true);
        workspace.transform.position = localWorkspaceCenter.transform.position;
        workspace.transform.rotation = localWorkspaceCenter.transform.rotation;
        workspace.__init__();

    }
    */

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
}
