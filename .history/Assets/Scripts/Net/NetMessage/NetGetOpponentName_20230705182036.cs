using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetGetOpponentName : NetMessage
{
    public int teamNumber;

    public byte wantDraw;

    public NetGetOpponentName()
    {
        Code = OpCode.DRAW;
    }

    public NetGetOpponentName(DataStreamReader reader)
    {
        Code = OpCode.DRAW;
        Deserialize (reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt (teamNumber);
        writer.WriteByte (wantDraw);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        teamNumber = reader.ReadInt();
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
