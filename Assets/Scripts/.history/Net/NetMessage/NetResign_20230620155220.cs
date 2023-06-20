using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetResign : NetMessage
{
    public int teamId;

    public byte wantRematch;

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
        writer.WriteInt (teamId);
        writer.WriteByte (wantRematch);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        teamId = reader.ReadInt();
        wantRematch = reader.ReadByte();
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
