using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;



public class CreepyTrackerSurfaceRequestListener : MonoBehaviour
{
    private NewMain _main;
    private Calibration _calibration;

    private int _portForLocal = 0;
    private int _portForRemote = 0;

    private UdpClient _udpClient_LocalSurface = null;
    private IPEndPoint _anyIP_LocalSurface;

    private UdpClient _udpClient_RemoteSurface = null;
    private IPEndPoint _anyIP_RemoteSurface;


    public void StartReceive(int localSurfaceListenPort, int remoteSurfaceListenPort)
    {

        _main = GetComponent<NewMain>();
        _calibration = GetComponent<Calibration>();

        _portForLocal = localSurfaceListenPort;
        _portForRemote = remoteSurfaceListenPort;

        Debug.Log(this.ToString() + ": Awaiting Surfaces: remote at " + _portForRemote + ", local at " + _portForLocal);

        _anyIP_LocalSurface = new IPEndPoint(IPAddress.Any, _portForLocal);
        _udpClient_LocalSurface = new UdpClient(_anyIP_LocalSurface);
        _udpClient_LocalSurface.BeginReceive(new AsyncCallback(this.ReceiveCallback_LocalSurface), null);

        _anyIP_RemoteSurface = new IPEndPoint(IPAddress.Any, _portForRemote);
        _udpClient_RemoteSurface = new UdpClient(_anyIP_RemoteSurface);
        _udpClient_RemoteSurface.BeginReceive(new AsyncCallback(this.ReceiveCallback_RemoteSurface), null);

    }

    public void ReceiveCallback_LocalSurface(IAsyncResult ar)
    {
        Byte[] receiveBytes = _udpClient_LocalSurface.EndReceive(ar, ref _anyIP_LocalSurface);
        string result = System.Text.Encoding.UTF8.GetString(receiveBytes);
        string[] trackermessage = result.Split(MessageSeparators.L0);
        result = trackermessage[0] + MessageSeparators.L0 + trackermessage[1];

        Sensor[] sensors = _retrieveSensors(trackermessage[2]);
        foreach (Sensor s in sensors)
        {
            Debug.Log("_LOCAL_SENSOR______________________________________ " + s.id);
        }


        if (SurfaceMessage.isMessage(result))
        {
            SurfaceRectangle s = new SurfaceRectangle(result);
            Debug.Log("SURFACE_________________________________________ " + s.ToString());
            s.sensors = sensors;
            if(_main != null)
                _main.setLocalSurface(s);
            if (_calibration != null)
                _calibration.setLocalSurface(s);
            _udpClient_LocalSurface.Close();
        }
        else
        {
            _udpClient_LocalSurface.BeginReceive(new AsyncCallback(this.ReceiveCallback_LocalSurface), null);
        }
    }

    public void ReceiveCallback_RemoteSurface(IAsyncResult ar)
    {
        Byte[] receiveBytes = _udpClient_RemoteSurface.EndReceive(ar, ref _anyIP_RemoteSurface);
        string result = System.Text.Encoding.UTF8.GetString(receiveBytes);
        string[] trackermessage = result.Split(MessageSeparators.L0);
        result = trackermessage[0] + MessageSeparators.L0 + trackermessage[1];
        Sensor[] sensors = _retrieveSensors(trackermessage[2]);
        foreach (Sensor s in sensors)
        {
            Debug.Log("_REMOTE_SENSOR______________________________________ " + s.id);
        }


        if (SurfaceMessage.isMessage(result))
        {
            SurfaceRectangle s = new SurfaceRectangle(result);
            Debug.Log("SURFACE_________________________________________ " + s.ToString());
            s.sensors = sensors;
            if (_main != null)
                _main.setRemoteSurface(s);
            if (_calibration != null)
                _calibration.setRemoteSurface(s);
            _udpClient_RemoteSurface.Close();
        }
        else
        {
            _udpClient_RemoteSurface.BeginReceive(new AsyncCallback(this.ReceiveCallback_RemoteSurface), null);
        }
    }

    private Sensor[] _retrieveSensors(string v)
    {
        List<Sensor> sensors = new List<Sensor>();
        string[] sensorsInfo = v.Split(MessageSeparators.L2);
        foreach (string s in sensorsInfo)
        {

            if (s.Length > 0)
            {
                string[] values = s.Split(MessageSeparators.L3);
                Sensor sensor = new Sensor();
                sensor.id = values[0];
                sensor.position = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                sensor.rotation = new Quaternion(float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]), float.Parse(values[7]));

                sensors.Add(sensor);
            }
        }

        return sensors.ToArray();
    }

    void Update()
    {

    }

    void OnApplicationQuit()
    {
        if (_udpClient_LocalSurface != null) _udpClient_LocalSurface.Close();
        if (_udpClient_RemoteSurface != null) _udpClient_RemoteSurface.Close();
    }

    void OnQuit()
    {
        OnApplicationQuit();
    }
}
