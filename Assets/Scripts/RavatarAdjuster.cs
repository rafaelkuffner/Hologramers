using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RavatarAdjuster : MonoBehaviour {

    private GameObject _origin;
    private GameObject _trackercharLocal;
    private GameObject _trackercharRemote;
    private TrackerClient _trackerClientRemote;
    private TrackerClient _trackerClientLocal;
    private bool _isSetupDone;
    public List<SurfaceRectangle> _surfaces;
    public bool _surfacesLoaded;
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
    }
	
	// Update is called once per frame
	void Update () {
        if (!_isSetupDone)
        {
            if(GameObject.Find("Ravatar manager").GetComponent<Tracker>().setCloudParentObject("RemoteOrigin")) { 
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
