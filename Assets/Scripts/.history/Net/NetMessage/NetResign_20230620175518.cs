using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetResign : NetMessage
{
    public int teamID;

    public byte hasResigned;

    public NetResign()
    {
        Code = OpCode.REMATCH;
    }

    public NetResign(DataStreamReader reader)
    {
        Code = OpCode.RESIGN;
        Deserialize (reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt (teamID);
        writer.WriteByte (hasResigned);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        teamID = reader.ReadInt();
        hasResigned = reader.ReadByte();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_RESIGN?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_RESIGN?.Invoke(this, cnn);
    }
}
