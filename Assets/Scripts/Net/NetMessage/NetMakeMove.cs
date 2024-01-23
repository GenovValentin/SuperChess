using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetMakeMove : NetMessage
{
    public int originalX;

    public int originalY;

    public int destinationX;

    public int destinationY;

    public int teamNumber;

    public int promotionPieceType;

    public NetMakeMove()
    {
        Code = OpCode.MAKE_MOVE;
    }

    public NetMakeMove(Unity.Collections.DataStreamReader reader)
    {
        Code = OpCode.MAKE_MOVE;
        Deserialize (reader);
    }

    public override void Serialize(
        ref Unity.Collections.DataStreamWriter writer
    )
    {
        writer.WriteByte((byte) Code);
        writer.WriteInt (originalX);
        writer.WriteInt (originalY);
        writer.WriteInt (destinationX);
        writer.WriteInt (destinationY);
        writer.WriteInt (teamNumber);
        writer.WriteInt (promotionPieceType);
    }

    public override void Deserialize(Unity.Collections.DataStreamReader reader)
    {
        originalX = reader.ReadInt();
        originalY = reader.ReadInt();
        destinationX = reader.ReadInt();
        destinationY = reader.ReadInt();
        teamNumber = reader.ReadInt();
        promotionPieceType = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_MAKE_MOVE?.Invoke(this, cnn);
    }
}
