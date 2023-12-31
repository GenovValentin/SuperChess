using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetMessage
{
    public OpCode Code { set; get; }

    public virtual void Serialize(ref Unity.Collections.DataStreamWriter writer)
    {
        try
        {
            writer.WriteByte((byte) Code);
        }
        catch (Exception e)
        {
            Debug.Log("Error serializing the message" + e);
        }
    }

    public virtual void Deserialize(Unity.Collections.DataStreamReader reader)
    {
    }

    public virtual void ReceivedOnClient()
    {
    }

    public virtual void ReceivedOnServer(NetworkConnection cnn)
    {
    }
}
