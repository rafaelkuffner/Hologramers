using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteForwardRequestMessage  {

    public string ipaddress;
    public int port;

    public RemoteForwardRequestMessage(string message)
    {
        string[] tokens = message.Split(MessageSeparators.L1);
        ipaddress = tokens[0]; 
        port = int.Parse(tokens[1]);
    }

    public static string createRequestMessage(int port)
    {
        return "RemoteForwardMessage" + MessageSeparators.L0 + Network.player.ipAddress + MessageSeparators.L1 + port;
    }
}
