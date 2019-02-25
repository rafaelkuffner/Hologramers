﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;
using System;



public class TrackerMesh : MonoBehaviour
{

    [DllImport("RavatarPlugin")]
    private static extern IntPtr initLocal(string configLocation);
    [DllImport("RavatarPlugin")]
    private static extern void initNetwork(int myPort, int numberOfClients);
    [DllImport("RavatarPlugin")]
    private static extern bool getFrameAndNormal(string cloudID, byte[] colorFrame, byte[] depthFrame, byte[] normalFrame);
    [DllImport("RavatarPlugin")]
    private static extern void stopClouds();

    private Dictionary<string, PointCloudMesh> _clouds;
    private Dictionary<string, GameObject> _cloudGameObjects;

    public int BUFFER = 868352;
    public int DBUFFER = 868352;
    internal byte[] _colorData;
    internal byte[] _depthData;


    const int START = 1;
    const int STOP = 2;

    void Awake ()
    {
        Debug.Log("Hello Tracker");
	    _clouds = new Dictionary<string, PointCloudMesh> ();
        _cloudGameObjects = new Dictionary<string, GameObject>();
        _colorData = new byte[BUFFER];
        _depthData = new byte[DBUFFER];

        //////////LOCAL
        //IntPtr output = initLocal("C:\\Users\\rafae\\Desktop\\Data TEST\\output.ini");
        //string calib = Marshal.PtrToStringAnsi(output);
        //processCalibrationMatrix(calib);

        ////////////NETWORK
        TrackerProperties.Instance.loadConfig();
        this.gameObject.AddComponent<UdpListenerMesh>().udpRestart();
        broadCastCloudMessage(START);
        
    }


    private void Update()
    {
        foreach (KeyValuePair<string, PointCloudMesh> p in _clouds)
        {
            if(getFrameAndNormal(p.Key, _colorData, _depthData, null)) { 
                _clouds[p.Key].setPointsUncompressed(_colorData, _depthData);
                _clouds[p.Key].show();
            }
        }
        
    }

    public void hideAllClouds ()
	{
		foreach (PointCloudMesh s in _clouds.Values) {
			s.hide ();
		}
        stopClouds();
        broadCastCloudMessage(STOP);
		
	}



    public void initTCPLayer()
    {
        print("Started TCP Layer with " + _cloudGameObjects.Count);
		initNetwork(TrackerProperties.Instance.Local_avatarReceivePort,_cloudGameObjects.Count);
    }

    public void processCalibrationMatrix(string calibration)
    {
        string[] tokens = calibration.Split(MessageSeparators.L1);
        foreach (string s in tokens)
        {
            if (s == "") break;
            string[] chunks = s.Split(';');
            string id = chunks[0];

            Matrix4x4 mat = new Matrix4x4(new Vector4(float.Parse(chunks[1]), float.Parse(chunks[5]), float.Parse(chunks[9]), float.Parse(chunks[13])),
           new Vector4(float.Parse(chunks[2]), float.Parse(chunks[6]), float.Parse(chunks[10]), float.Parse(chunks[14])),
           new Vector4(float.Parse(chunks[3]), float.Parse(chunks[7]), float.Parse(chunks[11]), float.Parse(chunks[15])),
           new Vector4(float.Parse(chunks[4]), float.Parse(chunks[8]), float.Parse(chunks[12]), float.Parse(chunks[16])));

            GameObject cloudobj = new GameObject(id);
            cloudobj.transform.localPosition = new Vector3(mat[0, 3], mat[1, 3], mat[2, 3]);
            cloudobj.transform.localRotation = mat.rotation;
            cloudobj.transform.localScale = new Vector3(-1, 1, 1);
            cloudobj.AddComponent<PointCloudDepth>();

            PointCloudMesh cloud = cloudobj.GetComponent<PointCloudMesh>();
            _clouds.Add(id, cloud);
            _cloudGameObjects.Add(id, cloudobj);
        }
        if (_cloudGameObjects.Count > 0)
            Camera.main.GetComponent<MouseOrbitImproved>().target = _cloudGameObjects.First().Value.transform;

    }

    public void processCalibration(string calibration)
    {
        string[] tokens = calibration.Split(MessageSeparators.L1);
        foreach (string s in tokens)
        {
            if (s == "") break;
            string[] chunks = s.Split(';');
            string id = chunks[0];
            float px = float.Parse(chunks[1]);
            float py = float.Parse(chunks[2]);
            float pz = float.Parse(chunks[3]);
            float rx = float.Parse(chunks[4]);
            float ry = float.Parse(chunks[5]);
            float rz = float.Parse(chunks[6]);
            float rw = float.Parse(chunks[7]);

            GameObject cloudobj = new GameObject(id);
            cloudobj.transform.localPosition = new Vector3(px,py,pz);
            cloudobj.transform.localRotation = new Quaternion(rx,ry,rz,rw);
            cloudobj.transform.localScale = new Vector3(-1, 1, 1);
            cloudobj.AddComponent<PointCloudMesh>();
            PointCloudMesh cloud = cloudobj.GetComponent<PointCloudMesh>();
            _clouds.Add(id, cloud);
            _cloudGameObjects.Add(id, cloudobj);

        }
        if(_cloudGameObjects.Count > 0)
            Camera.main.GetComponent<MouseOrbitImproved>().target = _cloudGameObjects.First().Value.transform;
    }


    public void broadCastCloudMessage(int mode)
    {
        UdpClient udp = new UdpClient();
		string message = AvatarMessage.createRequestMessage(mode, TrackerProperties.Instance.Local_avatarReceivePort);
        byte[] data = Encoding.UTF8.GetBytes(message);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.Remote_trackerListenPort);
		Debug.Log("Sent request to port" + TrackerProperties.Instance.Remote_trackerListenPort + " with content " + message);
        udp.Send(data, data.Length, remoteEndPoint);

    }

    void OnApplicationQuit()
    {
        stopClouds();
    }

}
