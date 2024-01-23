using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;

public class NetGetOpponentName : NetMessage
{
    public int teamNumber;

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
        writer.WriteInt (teamNumber);
        byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(opponentName);
        writer.WriteInt(nameBytes.Length);
        writer.WriteBytes(new NativeArray<byte>(nameBytes, Allocator.Temp));
    }

    public override void Deserialize(DataStreamReader reader)
    {
        teamNumber = reader.ReadInt();
        int nameLength = reader.ReadInt();
        byte[] nameBytes = new byte[nameLength];
        for (int i = 0; i < nameLength; i++)
        {
            nameBytes[i] = reader.ReadByte();
        }
        opponentName = System.Text.Encoding.ASCII.GetString(nameBytes);
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
