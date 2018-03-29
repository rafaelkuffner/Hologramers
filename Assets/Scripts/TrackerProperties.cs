using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackerProperties : MonoBehaviour {

    private static TrackerProperties _singleton;
    private static bool loaded = false;
    private int remote_trackerBroadcastPort = 55555;
    private int remote_trackerListenPort = 55556;
    private int local_avatarReceivePort = 55557;
    private int local_trackerBroadcastPort = 55558;

    public string configFilename = "configSettings.txt";

    public int Remote_trackerBroadcastPort
    {
        get{
            return remote_trackerBroadcastPort;
        }
        set{
            remote_trackerBroadcastPort = value;
        }
    }

    public int Remote_trackerListenPort
    {
        get
        {
            return remote_trackerListenPort;
        }
        set
        {
            remote_trackerListenPort = value;
        }
    }

    public int Local_avatarReceivePort
    {
        get
        {
            return local_avatarReceivePort;
        }
        set
        {
            local_avatarReceivePort = value;
        }
    }

    public int Local_trackerBroadcastPort
    {
        get
        {
            return local_trackerBroadcastPort;
        }
        set
        {
            local_trackerBroadcastPort = value;
        }
    }
    private TrackerProperties()
    {
        _singleton = this;
    }

    public static TrackerProperties Instance
    {
        get
        {
            if (!loaded)
            {
                _singleton.loadConfig();
                loaded = true;
            }
            return _singleton;
        }
    }

    public void loadConfig()
    {
        string filePath = Application.dataPath + "/" + _singleton.configFilename;

        string port = ConfigProperties.load(filePath, "remote.trackerBroadcastPort");
        if (port != "")
        {
            this.remote_trackerBroadcastPort = int.Parse(port);
        }
        

        port = ConfigProperties.load(filePath, "remote.trackerListenPort");
        if (port != "")
        {
            this.remote_trackerListenPort = int.Parse(port);
        }

        port = ConfigProperties.load(filePath, "local.avatarReceivePort");
        if (port != "")
        {
            this.local_avatarReceivePort = int.Parse(port);
        }

        port = ConfigProperties.load(filePath, "local.trackerBroadcastPort");
        if (port != "")
        {
            this.local_trackerBroadcastPort = int.Parse(port);
        }
    }
    void Start()
    {
        //_singleton = this;
    }
}
