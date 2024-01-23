using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetResign : NetMessage
{
    public int teamNumber;

    public byte hasResigned;

    public NetResign()
    {
        Code = OpCode.RESIGN;
    }

    public NetResign(Unity.Collections.DataStreamReader reader)
    {
        Code = OpCode.RESIGN;
        Deserialize (reader);
    }

    public override void Serialize(
        ref Unity.Collections.DataStreamWriter writer
    )
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt (teamNumber);
        writer.WriteByte (hasResigned);
    }

    public override void Deserialize(Unity.Collections.DataStreamReader reader)
    {
        teamNumber = reader.ReadInt();
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
