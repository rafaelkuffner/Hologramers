using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;



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
    public string _name = null;
    public Vector3 SurfaceTopRight { get { return _tr; } }
    public Vector3 Center { get { return (_bl + _tr) * 0.5f; } }

    private GameObject _go;

    
    public Vector3 Normal
    {
        get
        {
            Vector3 up = _tl - _bl;
            Vector3 right = _br - _bl;
            return Vector3.Cross(up, right);
        }
    }

    public Quaternion Perpendicular
    {
        get
        {
            Vector3 up = _tl - _bl;
            Vector3 right = _br - _bl;
            Vector3 forward = Vector3.Cross(up, right);

            return Quaternion.LookRotation(forward, up);
        }
    }

    public GameObject CenterGameObject {
        get
        {
            return _go;
        }
        set
        {
            _go = value;
        }
    }

    public SurfaceRectangle(Vector3 BL, Vector3 BR, Vector3 TL, Vector3 TR)
    {
        _bl = BL;
        _br = BR;
        _tl = TL;
        _tr = TR;
    }

    public SurfaceRectangle(string value)
    {
        string[] values = value.Split(MessageSeparators.L2);
        _name = values[0];
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

public class SurfaceRequestListener : MonoBehaviour
{
   
    private int _portForLocal = 0;
    private int _portForRemote = 0;

    private UdpClient _udpClient_LocalSurface = null;
    private IPEndPoint _anyIP_LocalSurface;

    private RavatarAdjuster _ravatarAdjuster;
    public void RequestAndStartReceive()
    {
        _ravatarAdjuster = GetComponent<RavatarAdjuster>();
        _request(TrackerProperties.Instance.Local_trackerListenPort, TrackerProperties.Instance.Local_surfaceReceivePort);
        Debug.Log(this.ToString() + ": Will request a local surface from " + TrackerProperties.Instance.Local_trackerListenPort);

        _portForLocal = TrackerProperties.Instance.Local_surfaceReceivePort;

        _anyIP_LocalSurface = new IPEndPoint(IPAddress.Any, TrackerProperties.Instance.Local_surfaceReceivePort);
        _udpClient_LocalSurface = new UdpClient(_anyIP_LocalSurface);
        _udpClient_LocalSurface.BeginReceive(new AsyncCallback(this.ReceiveCallback_LocalSurface), null);

      
        Debug.Log(this.ToString() + ": Awaiting Surfaces at " + _portForLocal);
    }


    private void _request(int trackerPort, int receivePort)
    {
        UdpClient udp = new UdpClient();
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, trackerPort);
        string message = SurfaceMessage.createRequestMessage(receivePort);
        byte[] data = Encoding.UTF8.GetBytes(message);
        udp.Send(data, data.Length, remoteEndPoint);
    }

    public void ReceiveCallback_LocalSurface(IAsyncResult ar)
    {
        Byte[] receiveBytes = _udpClient_LocalSurface.EndReceive(ar, ref _anyIP_LocalSurface);
        string result = System.Text.Encoding.UTF8.GetString(receiveBytes);
        print("Received surface message: " + result);
        string[] trackermessage = result.Split(MessageSeparators.L0);
        if (SurfaceMessage.isMessage(result))
        {
            string[] surfacesString = trackermessage[1].Split(MessageSeparators.L1);
            foreach(string str in surfacesString)
            {
                SurfaceRectangle s = new SurfaceRectangle(str);
                _ravatarAdjuster._surfaces.Add(s);
            }
            _udpClient_LocalSurface.Close();
        }
        else
            _udpClient_LocalSurface.BeginReceive(new AsyncCallback(this.ReceiveCallback_LocalSurface), null);
    }



    void OnApplicationQuit()
    {
        if (_udpClient_LocalSurface != null) _udpClient_LocalSurface.Close();
    }

    void OnQuit()
    {
        OnApplicationQuit();
    }
}
