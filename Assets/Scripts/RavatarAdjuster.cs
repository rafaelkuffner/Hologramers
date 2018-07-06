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
    private Vector3 _remoteHoloSurface;

    private UdpClient _udp;
    private IPEndPoint _holoSurfaceRequester;
    private bool _haveReceivedARemoteHoloSurface;

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
        _holoSurfaceRequester = null;
        _haveReceivedARemoteHoloSurface = false;
    }
	
    public void processHoloSurfaceRequestMessage(RemoteHoloSurfaceRequestMessage msg)
    {
        _udp = new UdpClient();
        _holoSurfaceRequester = new IPEndPoint(IPAddress.Parse(msg.ipaddress), msg.port);
    }

    void sendMyHoloSurface()
    {
        Vector3 holoSurface = _origin.transform.position;
        string message = RemoteHoloSurfaceMessage.createMessage(holoSurface);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        _udp.Send(data, data.Length, _holoSurfaceRequester);
    }

    void sendHoloSurfaceRequest()
    {
       
        UdpClient udp = new UdpClient();
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.Remote_HoloSurfaceListenPort);
        string message = RemoteHoloSurfaceRequestMessage.createRequestMessage(TrackerProperties.Instance.Local_avatarReceivePort);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        udp.Send(data, data.Length, remoteEndPoint);
    }

    public void processHoloSurfaceMessage(RemoteHoloSurfaceMessage msg)
    {
        _remoteHoloSurface = msg.holoSurface;
        _haveReceivedARemoteHoloSurface = true;
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
        
        //Calculating forward
        Vector3 fw = _trackerClientRemote.spineBase.localPosition - _remoteHoloSurface;
        //Translate it back to center of surface
        _trackercharRemote.transform.localPosition = new Vector3(-_trackerClientRemote.spineBase.localPosition.x, 0, -_trackerClientRemote.spineBase.localPosition.z);
        
        //Adjusting orientation
        fw.y = 0;
        Vector3 diff = _trackerClientLocal.GetHeadPos() - _origin.transform.position;
        diff.y = 0;
        _origin.transform.Rotate(Vector3.Cross(fw, diff), Vector3.Angle(fw, diff));

        ////Adjusting scale
        //my scale related to the remote guy
        float ratio = (_trackerClientLocal.GetHeadPos().y - _origin.transform.position.y) / _trackerClientRemote.GetHeadPos().y;
        //if ratio > 1, means I'm the big guy, i can't upscale, so i do nothing.
        if (ratio <= 1)
        {
            float remoteRatio = (_trackerClientRemote.GetHeadPos().y - _remoteHoloSurface.y) / _trackerClientLocal.GetHeadPos().y;
            //if his ratio is <= 1, means he can downscale me over there, so its fine
            //if not....
            if (remoteRatio > 1)
            {
                Vector3 myHeadRemotePos = new Vector3(_remoteHoloSurface.x, _remoteHoloSurface.y + _trackerClientLocal.GetHeadPos().y, _remoteHoloSurface.z);
                Vector3 hisHeadRemotePos = _trackerClientRemote.GetHeadPos();
                //Vector from my head to his
                Vector3 viewVec = hisHeadRemotePos - myHeadRemotePos;
                hisHeadRemotePos.y = myHeadRemotePos.y;
                Vector3 planevec = hisHeadRemotePos - myHeadRemotePos;
                //view angle
                float angle = Vector3.Angle(planevec, viewVec);
                //now in my space, i get a vector to the ideal eye to eye head position for him
                Vector3 hisHeadOrigin = _origin.transform.position;
                hisHeadOrigin.y = _trackerClientLocal.GetHeadPos().y;
                Vector3 catetoAdjacente = hisHeadOrigin - _trackerClientLocal.GetHeadPos();
                float heightDiffLocal = catetoAdjacente.sqrMagnitude* Mathf.Tan(angle);
                ratio = (_trackerClientLocal.GetHeadPos().y - _origin.transform.position.y+ heightDiffLocal) / hisHeadOrigin.y ;
            }
            //use ratio to scale
            _origin.transform.localScale = new Vector3(ratio, ratio, ratio);
        }




        if(_holoSurfaceRequester != null)
        {
            sendMyHoloSurface();
        }

        if (!_haveReceivedARemoteHoloSurface)
        {
            sendHoloSurfaceRequest();
        }
        //foreach (KeyValuePair<string, GameObject> cloudobj in _cloudGameObjects)
        //{
        //    cloudobj.Value.transform.localPosition = pai.position;
        //    cloudobj.Value.transform.rotation = pai.rotation;
        //    cloudobj.Value.transform.parent = pai;
        //}
    }
}
