using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetDraw : NetMessage
{
    public int teamId;

    public byte wantDraw;

    public NetDraw()
    {
        Code = OpCode.DRAW;
    }

    public NetDraw(DataStreamReader reader)
    {
        Code = OpCode.DRAW;
        Deserialize (reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt (teamId);
        writer.WriteByte (wantDraw);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        teamId = reader.ReadInt();
        wantDraw = reader.ReadByte();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_DRAW?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_DRAW?.Invoke(this, cnn);
    }
}
