using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetGetOpponentName : NetMessage
{
    public int teamNUMBER;

    public byte wantDraw;

    public NetGetOpponentName()
    {
        Code = OpCode.GetOpponentName;
    }

    public NetGetOpponentName(DataStreamReader reader)
    {
        Code = OpCode.GetOpponentName;
        Deserialize (reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt (teamNUMBER);
        writer.WriteByte (wantDraw);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        teamNUMBER = reader.ReadInt();
        wantDraw = reader.ReadByte();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_GET_OPPONENT_NAME?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_GET_OPPONENT_NAME?.Invoke(this, cnn);
    }
}
