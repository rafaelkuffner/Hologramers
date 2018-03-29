﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;


public class Tracker : MonoBehaviour
{

	private Dictionary<string, PointCloudDepth> _clouds;
    private Dictionary<string, GameObject> _cloudGameObjects;
    private GameObject _origin;
    private GameObject _trackercharLocal;
    private GameObject _trackercharRemote;
    private TrackerClient _trackerClientRemote;
    private TrackerClient _trackerClientLocal;

    void Awake ()
	{
        Debug.Log("Hello Tracker");
		_clouds = new Dictionary<string, PointCloudDepth> ();
        _cloudGameObjects = new Dictionary<string, GameObject>();
        resetListening();
        _trackerClientRemote = null;
        _trackerClientLocal = null;
        _trackercharRemote = null;

        UdpClient udp = new UdpClient();
        string message = AvatarMessage.createRequestMessage(1, TrackerProperties.Instance.Local_avatarReceivePort);
        byte[] data = Encoding.UTF8.GetBytes(message);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.Remote_trackerListenPort);
        Debug.Log("Sent request to port" + TrackerProperties.Instance.Remote_trackerListenPort + " with content " + message); 
        udp.Send(data, data.Length, remoteEndPoint);
        _origin = GameObject.Find("RemoteOrigin");
        _trackercharRemote = GameObject.Find("Trackerchar Remote");
        _trackercharLocal = GameObject.Find("Trackerchar");

    }



    //FOR TCP DEPTH
    internal void setNewDepthCloud(string KinectID, byte[] colorData, byte[] depthData, uint id,bool compressed,int sizec,int scale)
    {
       
        // tirar o id da mensagem que é um int
        if (_clouds.ContainsKey(KinectID))
        {
            _clouds[KinectID].setPoints(colorData,depthData,compressed, sizec,scale);
            _clouds[KinectID].show();
        }
    }

  
    
	public void resetListening ()
	{
		gameObject.GetComponent<UdpListener> ().udpRestart ();
	}

	public void hideAllClouds ()
	{
		foreach (PointCloudDepth s in _clouds.Values) {
			s.hide ();
		}
		UdpClient udp = new UdpClient ();
		string message = CloudMessage.createRequestMessage (2,Network.player.ipAddress, TrackerProperties.Instance.Local_avatarReceivePort); 
		byte[] data = Encoding.UTF8.GetBytes(message);
		IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.Remote_trackerListenPort);
		udp.Send(data, data.Length, remoteEndPoint);
	}

	public void broadCastCloudRequests (bool continuous)
	{
		UdpClient udp = new UdpClient ();
		string message = CloudMessage.createRequestMessage (continuous ? 1 : 0, Network.player.ipAddress, TrackerProperties.Instance.Local_avatarReceivePort); 
		byte[] data = Encoding.UTF8.GetBytes (message);
		IPEndPoint remoteEndPoint = new IPEndPoint (IPAddress.Broadcast, TrackerProperties.Instance.Remote_trackerListenPort);
		udp.Send (data, data.Length, remoteEndPoint);
	}
    
    public void processAvatarMessage(AvatarMessage av)
    {
        foreach (string s in av.calibrations)
        {
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
            cloudobj.transform.parent = _trackercharRemote.transform;
            cloudobj.transform.localPosition = new Vector3(px,py,pz);
            cloudobj.transform.localRotation = new Quaternion(rx,ry,rz,rw);
            cloudobj.transform.localScale = new Vector3(-1, 1, 1);
            cloudobj.AddComponent<PointCloudDepth>();
            PointCloudDepth cloud = cloudobj.GetComponent<PointCloudDepth>();
            _clouds.Add(id, cloud);
            _cloudGameObjects.Add(id, cloudobj);

        }
        Camera.main.GetComponent<MouseOrbitImproved>().target = _cloudGameObjects.First().Value.transform;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Transform pai = GameObject.Find("Surface 1").transform;
            _origin.transform.position = pai.position;
            _origin.transform.rotation = pai.rotation;
            _origin.transform.parent = pai;
           
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Transform pai = GameObject.Find("Surface 2").transform;
            _origin.transform.position = pai.position;
            _origin.transform.rotation = pai.rotation;
            _origin.transform.parent = pai;
        }

        if(_trackerClientRemote == null)
        {
            _trackerClientRemote = _trackercharRemote.GetComponent<TrackerClient>();
        }
        if (_trackerClientLocal == null)
        {
            _trackerClientLocal = _trackercharLocal.GetComponent<TrackerClient>();
        }

        _trackercharRemote.transform.localPosition = new Vector3(-_trackerClientRemote.spineBase.localPosition.x, 0, -_trackerClientRemote.spineBase.localPosition.z);
        Vector3 fw = _trackerClientRemote.GetForward();
        fw.y = 0;
        Vector3 diff = _trackerClientLocal.GetHeadPos() - _origin.transform.position;
        diff.y = 0;
        _origin.transform.Rotate(Vector3.Cross(fw, diff), Vector3.Angle(fw, diff));

        //foreach (KeyValuePair<string, GameObject> cloudobj in _cloudGameObjects)
        //{
        //    cloudobj.Value.transform.localPosition = pai.position;
        //    cloudobj.Value.transform.rotation = pai.rotation;
        //    cloudobj.Value.transform.parent = pai;
        //}
    }
}
