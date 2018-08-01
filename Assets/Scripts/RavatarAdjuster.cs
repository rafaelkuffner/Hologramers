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


    public bool LoadDummyValues;

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

        //Added here so meta doesn't mess with them
        gameObject.AddComponent<TcpDepthListener>();
        gameObject.AddComponent<UdpListener>();
        gameObject.AddComponent<SurfaceRequestListener>();
        gameObject.AddComponent<Tracker>();

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

            if (LoadDummyValues)
            {
                GameObject dummyS = GameObject.Find("DummySurface");
                Vector3 bl,br,tl,tr;
                bl = br = tl = tr = dummyS.transform.position;
                bl.x -= 0.01f;
                bl.z += 0.01f;
                br.x += 0.01f;
                br.z += 0.01f;
                tl.x -= 0.01f;
                tl.z -= 0.01f;
                tr.x += 0.01f;
                tr.z += 0.01f;
                _surfaces.Add(new SurfaceRectangle(bl,br,tl,tr));
            }

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
                localScreenCenter.transform.rotation = Quaternion.identity;
                sr.CenterGameObject = localScreenCenter;
            }
            _surfacesLoaded = true;
            //set 1 as default right away
            Transform pai = _surfaces[0].CenterGameObject.transform;
            _origin.transform.position = pai.position;
            _origin.transform.rotation = pai.rotation;
            _origin.transform.parent = pai;
        }

        if (LoadDummyValues)
        {

            string msg = RemoteHoloSurfaceMessage.createMessage(GameObject.Find("DummySurfaceRemote").transform.position);
            string[] splitmsg = msg.Split(MessageSeparators.L0);
            RemoteHoloSurfaceMessage r = new RemoteHoloSurfaceMessage(splitmsg[1]);
            processHoloSurfaceMessage(r);
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
        Debug.DrawLine(_trackerClientRemote.spineBase.localPosition, _remoteHoloSurface,Color.cyan);
        //Translate it back to center of surface
        _trackercharRemote.transform.localPosition = new Vector3(-_trackerClientRemote.spineBase.localPosition.x, 0, -_trackerClientRemote.spineBase.localPosition.z);
        
        //Adjusting orientation
        fw.y = 0;
        Vector3 diff = _origin.transform.position - _trackerClientLocal.GetHeadPos();
        diff.y = 0;
        _origin.transform.rotation = Quaternion.identity;
        _origin.transform.Rotate(Vector3.Cross(fw, diff), Vector3.Angle(fw, diff));

        ////Adjusting scale
        //my scale related to the remote guy
        if(LoadDummyValues){
            _trackerClientLocal.AdjustAvatarHeight();
            _trackerClientRemote.AdjustAvatarHeight();
        }

        float localHeadY = LoadDummyValues ? _trackerClientLocal.dummyHeight : _trackerClientLocal.GetHeadPos().y;
        float remoteHeadY = LoadDummyValues ? _trackerClientRemote.dummyHeight : _trackerClientRemote.GetHeadPos().y;

        float ratio = (localHeadY - _origin.transform.position.y) / remoteHeadY;

        print("ratio " + ratio);
        //if ratio > 1, means I'm the big guy, i can't upscale, so i do nothing.
        if (ratio <= 1)
        {
            float remoteRatio = (remoteHeadY - _remoteHoloSurface.y) / localHeadY;
            //if his ratio is <= 1, means he can downscale me over there, so its fine
            //if not....
            print("remoteRatio" + remoteRatio);
            if (remoteRatio > 1)
            {
                Vector3 myHeadRemotePos = new Vector3(_remoteHoloSurface.x, _remoteHoloSurface.y + localHeadY, _remoteHoloSurface.z);
                Vector3 hisHeadRemotePos =LoadDummyValues? new Vector3(0,remoteHeadY,0) : _trackerClientRemote.GetHeadPos();
                //Vector from my head to his
                Vector3 viewVec = hisHeadRemotePos - myHeadRemotePos;
                Debug.DrawLine(hisHeadRemotePos, myHeadRemotePos, Color.green);

                hisHeadRemotePos.y = myHeadRemotePos.y;
                Vector3 planevec = hisHeadRemotePos - myHeadRemotePos;
                Debug.DrawLine(hisHeadRemotePos, myHeadRemotePos, Color.green);
                //view angle
                float angle = Vector3.Angle(planevec, viewVec);
                print("angle " + angle);
                //now in my space, i get a vector to the ideal eye to eye head position for him
                Vector3 hisHeadOrigin = _origin.transform.position;
                hisHeadOrigin.y = localHeadY;
                Vector3 localHeadPos = LoadDummyValues ? new Vector3(0, localHeadY, 0) :  _trackerClientLocal.GetHeadPos();
                Vector3 catetoAdjacente = hisHeadOrigin - localHeadPos;

                float heightDiffLocal = catetoAdjacente.magnitude* Mathf.Tan(Mathf.Deg2Rad*angle);
                print("diffLocal " + heightDiffLocal);
                ratio = (localHeadY - _origin.transform.position.y + heightDiffLocal) / remoteHeadY;
                Vector3 debugremotehead = _origin.transform.position;
                debugremotehead.y = localHeadY + heightDiffLocal; 
                Debug.DrawLine(localHeadPos, debugremotehead,Color.red);
            }
            //use ratio to scale
            if(!float.IsInfinity(ratio) && !float.IsNaN(ratio) && ratio != 0) {
                print("Scaling");
                _origin.transform.localScale = new Vector3(ratio, ratio, ratio);
            }
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
