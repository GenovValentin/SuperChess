using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;

public class NetGetOpponentRating : NetMessage
{
    public int teamNumber;

    public int opponentRating;

    public NetGetOpponentRating()
    {
        Code = OpCode.GET_OPPONENT_RATING;
    }

    public NetGetOpponentRating(DataStreamReader reader)
    {
        Code = OpCode.GET_OPPONENT_RATING;
        Deserialize (reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt (teamNumber);
        writer.WriteInt (opponentRating);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        teamNumber = reader.ReadInt();
        opponentRating = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_GET_OPPONENT_RATING?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_GET_OPPONENT_RATING?.Invoke(this, cnn);
    }
}
