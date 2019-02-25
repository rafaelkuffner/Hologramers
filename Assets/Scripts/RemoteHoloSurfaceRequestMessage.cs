using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteHoloSurfaceRequestMessage  {

    public string ipaddress;
    public int port;

    public RemoteHoloSurfaceRequestMessage(string message)
    {
        string[] tokens = message.Split(MessageSeparators.L1);
        ipaddress = tokens[0]; 
        port = int.Parse(tokens[1]);
    }

    public static string createRequestMessage(int port)
    {
		return "RemoteHoloSurfaceMessage" + MessageSeparators.L0 +  IPManager.GetIP(ADDRESSFAM.IPv4)+ MessageSeparators.L1 + port;
    }
}
