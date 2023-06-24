using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetDecline : NetMessage
{
    public int teamNr;

    public byte wantDecline;

    public NetDecline()
    {
        Code = OpCode.DECLINE;
    }

    public NetDecline(DataStreamReader reader)
    {
        Code = OpCode.DECLINE;
        Deserialize (reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteByte (wantDecline);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        wantDecline = reader.ReadByte();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_DECLINE?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_DECLINE?.Invoke(this, cnn);
    }
}
