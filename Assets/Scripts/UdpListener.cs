using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;


public class UdpListener : MonoBehaviour {

    private UdpClient _udpClient = null;
    private IPEndPoint _anyIP;
    private List<byte[]> _stringsToParse; // TMA: Store the bytes from the socket instead of converting to strings. Saves time.
    private byte[] _receivedBytes;
    private int number = 0;



    public void udpRestart()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
        }

        _stringsToParse = new List<byte[]>();
        
		_anyIP = new IPEndPoint(IPAddress.Any, TrackerProperties.Instance.Local_avatarReceivePort);
        
        _udpClient = new UdpClient(_anyIP);

        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);

		Debug.Log("[UDPListener] Receiving in port: " + TrackerProperties.Instance.Local_avatarReceivePort);
    }
    
    public void ReceiveCallback(IAsyncResult ar)
    {
        Byte[] receiveBytes = _udpClient.EndReceive(ar, ref _anyIP);
        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
        _stringsToParse.Add(receiveBytes);
    }

    void Update()
    {
  
        while (_stringsToParse.Count > 0)
        {
            try
            {
                byte[] toProcess = _stringsToParse.First();
                if(toProcess != null)
                {
                  if (Convert.ToChar(toProcess[0]) == 'A')
                    {
                        Debug.Log("Got Calibration Message! ");
                        string stringToParse = Encoding.ASCII.GetString(toProcess);
                        string[] splitmsg = stringToParse.Split(MessageSeparators.L0);
                        gameObject.GetComponent<Tracker>().processCalibration(splitmsg[1]);
                        gameObject.GetComponent<Tracker>().initTCPLayer();
					}
					if (Convert.ToChar(toProcess[0]) == 'F')
					{
						Debug.Log("Remote HoloSurface changed! ");
						string stringToParse = Encoding.ASCII.GetString(toProcess);
						string[] splitmsg = stringToParse.Split(MessageSeparators.L0);
						RemoteHoloSurfaceMessage rf = new RemoteHoloSurfaceMessage(splitmsg[1]);
						gameObject.GetComponent<RavatarAdjuster>().processHoloSurfaceMessage(rf);
					}
					if (Convert.ToChar(toProcess[0]) == 'R')
					{
						Debug.Log("Remote HoloSurface Request Received! ");
						string stringToParse = Encoding.ASCII.GetString(toProcess);
						string[] splitmsg = stringToParse.Split(MessageSeparators.L0);
						RemoteHoloSurfaceRequestMessage rf = new RemoteHoloSurfaceRequestMessage(splitmsg[1]);
						gameObject.GetComponent<RavatarAdjuster>().processHoloSurfaceRequestMessage(rf);
						gameObject.GetComponent<Tracker>().processCalibration(splitmsg[1]);
						gameObject.GetComponent<Tracker>().initTCPLayer();
					}
                }
                _stringsToParse.RemoveAt(0);
            }
            catch (Exception exc) { _stringsToParse.RemoveAt(0); }
        }
    }

    void OnApplicationQuit()
    {
        if (_udpClient != null) _udpClient.Close();
    }

    void OnQuit()
    {
        OnApplicationQuit();
    }
}
