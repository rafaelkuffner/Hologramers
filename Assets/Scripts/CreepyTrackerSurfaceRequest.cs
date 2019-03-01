using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Sensor
{
    public string id = null;
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
}

public class SurfaceRectangle
{
    private Vector3 _bl;
    public Vector3 SurfaceBottomLeft { get { return _bl; } }
    private Vector3 _br;
    public Vector3 SurfaceBottomRight { get { return _br; } }
    private Vector3 _tl;
    public Vector3 SurfaceTopLeft { get { return _tl; } }
    private Vector3 _tr;
    public Sensor[] sensors = null;

    public Vector3 SurfaceTopRight { get { return _tr; } }

    public Vector3 Center { get { return (_bl + _tr) * 0.5f; } }

    public Vector3 Normal
    {
        get
        {
            Vector3 up = _tl - _bl;
            Vector3 right = _br - _bl;
            return Vector3.Cross(up, right);
        }
    }

    public Quaternion rotation
    {
        get
        {
            Vector3 f = _tl - _bl;
            Vector3 r = _br - _bl;
            Vector3 u = Vector3.Cross(f, r);

            return Quaternion.LookRotation(f, u);
        }
    }

    public GameObject CenterGameObject { get; internal set; }

    public SurfaceRectangle(Vector3 BL, Vector3 BR, Vector3 TL, Vector3 TR)
    {
        _bl = BL;
        _br = BR;
        _tl = TL;
        _tr = TR;
    }

    public SurfaceRectangle(string value)
    {
        string[] values = value.Split(MessageSeparators.L0)[1].Split(MessageSeparators.L1)[0].Split(MessageSeparators.L2);

        string name = values[0];
        sensors = new Sensor[0];

        _bl = CommonUtils.networkStringToVector3(values[1], MessageSeparators.L3);
        _br = CommonUtils.networkStringToVector3(values[2], MessageSeparators.L3);
        _tl = CommonUtils.networkStringToVector3(values[3], MessageSeparators.L3);
        _tr = CommonUtils.networkStringToVector3(values[4], MessageSeparators.L3);
    }

    public override string ToString()
    {
        return "BL" + _bl.ToString() + ", BR" + _br.ToString() + ", TL" + _tl.ToString() + ", TR" + _tr.ToString();
    }
}

public class SurfaceMessage
{
    public static string createRequestMessage(int port)
    {
        return "SurfaceMessage" + MessageSeparators.L0 + Network.player.ipAddress + MessageSeparators.L1 + port;
    }

    public static bool isMessage(string value)
    {
        if (value.Split(MessageSeparators.L0)[0] == "SurfaceMessage")
        {
            return true;
        }
        return false;
    }
}

public class CreepyTrackerSurfaceRequest : MonoBehaviour
{
    public SurfaceRectangle localSurface = null;
    public SurfaceRectangle remoteSurface = null;
    
    private NewMain _main;

    private DateTime lastTry;
    public int requestInterval = 100;

    public void Request(int localSurfaceRequestPort, int localSurfaceListenPort, int remoteSurfaceRequestPort, int remoteSurfaceListenPort)
    {
        Debug.Log("[" + this.ToString() + "] Requesting local surface to " + localSurfaceRequestPort + " to receive in " + localSurfaceListenPort);
        Debug.Log("[" + this.ToString() + "] Requesting remote surface to " + remoteSurfaceRequestPort + " to receive in " + remoteSurfaceListenPort);

        _request(localSurfaceRequestPort, localSurfaceListenPort);
        _request(remoteSurfaceRequestPort, remoteSurfaceListenPort);

        lastTry = DateTime.Now;
    }

    private void _request(int trackerPort, int receivePort)
    {
        UdpClient udp = new UdpClient();
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, trackerPort);
        string message = SurfaceMessage.createRequestMessage(receivePort);
        byte[] data = Encoding.UTF8.GetBytes(message);
        udp.Send(data, data.Length, remoteEndPoint);
    }

    void Update()
    {
        /*
        if (_main.LocalSurfaceReceived && _main.RemoteSurfaceReceived) return;

        if (lastTry != null && DateTime.Now > lastTry.AddMilliseconds(requestInterval))
        {
            if (!_main.LocalSurfaceReceived)
            {
                _requestLocal();
            }

            if (!_main.RemoteSurfaceReceived)
            {
                _requestRemote();
            }

            lastTry = DateTime.Now;
        }
        */
    }
}
