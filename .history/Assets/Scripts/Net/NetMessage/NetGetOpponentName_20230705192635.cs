using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking;
using Unity.Networking.Transport;

public class NetGetOpponentName : NetMessage
{
    public int teamNUMBER;

    public string opponentName;

    public NetGetOpponentName()
    {
        Code = OpCode.GET_OPPONENT_NAME;
    }

    public NetGetOpponentName(DataStreamReader reader)
    {
        Code = OpCode.GET_OPPONENT_NAME;
        Deserialize (reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt (teamNUMBER);
        writer.WriteString (opponentName);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        teamNUMBER = reader.ReadInt();
        opponentName = reader.ReadString();
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
