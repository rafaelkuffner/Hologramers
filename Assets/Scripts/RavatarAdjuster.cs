using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

public class RavatarAdjuster : MonoBehaviour {

    private GameObject _origin;
    private GameObject _trackercharLocal;
    private GameObject _trackercharRemote;
    private TrackerClient _trackerClientRemote;
    private TrackerClient _trackerClientLocal;
    private bool _isSetupDone;
    public List<SurfaceRectangle> _surfaces;
    public bool _surfacesLoaded;
    private Vector3 _remoteForward;

    private UdpClient _udp;
    private IPEndPoint _forwardRequester;
    private bool _haveReceivedARemoteForward;

    // Use this for initialization
    void Start () {
        _surfacesLoaded = false;
        _isSetupDone = false;
        _trackerClientRemote = null;
        _trackerClientLocal = null;
        _trackercharRemote = null;
        _origin = GameObject.Find("RemoteOrigin");
        _trackercharRemote = GameObject.Find("Trackerchar Remote");
        _trackercharLocal = GameObject.Find("Trackerchar");
        _surfaces = new List<SurfaceRectangle>();
        _forwardRequester = null;
        _haveReceivedARemoteForward = false;
    }
	
    public void processForwardRequestMessage(RemoteForwardRequestMessage msg)
    {
        _udp = new UdpClient();
        _forwardRequester = new IPEndPoint(IPAddress.Parse(msg.ipaddress), msg.port);
    }

    void sendMyForward()
    {
        Vector3 forward = _trackerClientRemote.spineBase.localPosition - _origin.transform.position;
        string message = RemoteForwardMessage.createMessage(forward);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        _udp.Send(data, data.Length, _forwardRequester);
    }

    void sendForwardRequest()
    {
       
        UdpClient udp = new UdpClient();
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.Remote_ForwardListenPort);
        string message = RemoteForwardRequestMessage.createRequestMessage(TrackerProperties.Instance.Local_avatarReceivePort);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        udp.Send(data, data.Length, remoteEndPoint);
    }

    public void processForwardMessage(RemoteForwardMessage msg)
    {
        _remoteForward = msg.forward;
        _haveReceivedARemoteForward = true;
    }

	// Update is called once per frame
	void FixedUpdate () {
        if (!_isSetupDone)
        {
            if (GetComponent<Tracker>().setCloudParentObject("RemoteOrigin")) { 
               GetComponent<SurfaceRequestListener>().RequestAndStartReceive();
               _isSetupDone = true;
           }

        }

        if (_surfaces.Count > 0 && !_surfacesLoaded)
        {
            foreach(SurfaceRectangle sr in _surfaces)
            {
                GameObject localScreenCenter = new GameObject(sr._name);
                localScreenCenter.transform.position = sr.Center;
                localScreenCenter.transform.rotation = sr.Perpendicular;
                sr.CenterGameObject = localScreenCenter;
            }
            _surfacesLoaded = true;
        }
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Transform pai = _surfaces[0].CenterGameObject.transform;
            _origin.transform.position = pai.position;
            _origin.transform.rotation = pai.rotation;
            _origin.transform.parent = pai;

        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Transform pai = _surfaces[1].CenterGameObject.transform;
            _origin.transform.position = pai.position;
            _origin.transform.rotation = pai.rotation;
            _origin.transform.parent = pai;
        }

        if (_trackerClientRemote == null)
        {
            _trackerClientRemote = _trackercharRemote.GetComponent<TrackerClient>();
        }
        if (_trackerClientLocal == null)
        {
            _trackerClientLocal = _trackercharLocal.GetComponent<TrackerClient>();
        }

        _trackercharRemote.transform.localPosition = new Vector3(-_trackerClientRemote.spineBase.localPosition.x, 0, -_trackerClientRemote.spineBase.localPosition.z);
        //  Vector3 fw = _trackerClientRemote.GetForward();
        Vector3 fw = _remoteForward;
        fw.y = 0;
        Vector3 diff = _trackerClientLocal.GetHeadPos() - _origin.transform.position;
        diff.y = 0;
        _origin.transform.Rotate(Vector3.Cross(fw, diff), Vector3.Angle(fw, diff));

        if(_forwardRequester != null)
        {
            sendMyForward();
        }

        if (!_haveReceivedARemoteForward)
        {
            sendForwardRequest();
        }
        //foreach (KeyValuePair<string, GameObject> cloudobj in _cloudGameObjects)
        //{
        //    cloudobj.Value.transform.localPosition = pai.position;
        //    cloudobj.Value.transform.rotation = pai.rotation;
        //    cloudobj.Value.transform.parent = pai;
        //}
    }
}
