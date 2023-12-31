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

    public NetDecline(Unity.Collections.DataStreamReader reader)
    {
        Code = OpCode.DECLINE;
        Deserialize (reader);
    }

    public override void Serialize(ref Unity.Collections.DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt (teamNr);
        writer.WriteByte (wantDecline);
    }

    public override void Deserialize(Unity.Collections.DataStreamReader reader)
    {
        teamNr = reader.ReadInt();
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
