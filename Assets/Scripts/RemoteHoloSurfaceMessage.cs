using UnityEngine;
using System.Collections.Generic;
using System.Net;

public class RemoteHoloSurfaceMessage
{

    public Vector3 holoSurface;

    public RemoteHoloSurfaceMessage(string message)
    {
        string[] tokens = message.Split(MessageSeparators.L1);
        holoSurface = new Vector3(float.Parse(tokens[0]), float.Parse(tokens[1]), float.Parse(tokens[2]));
    }

    public static string createMessage(Vector3 forward)
    {
        return "HoloSurfaceMessage" + MessageSeparators.L0 + forward.x + MessageSeparators.L1 +forward.y + MessageSeparators.L1 + forward.z;
    }
}
