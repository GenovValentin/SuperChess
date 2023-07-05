using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public enum OpCode
{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    REMATCH = 5,
    RESIGN = 6,
    DRAW = 7,
    DECLINE = 8,
    GET_OPPONENT_NAME = 9
}

public static class NetUtility
{
    // Net messages
    public static Action<NetMessage> C_KEEP_ALIVE;

    public static Action<NetMessage> C_WELCOME;

    public static Action<NetMessage> C_START_GAME;

    public static Action<NetMessage> C_MAKE_MOVE;

    public static Action<NetMessage> C_REMATCH;

    public static Action<NetMessage> C_RESIGN;

    public static Action<NetMessage> C_DRAW;

    public static Action<NetMessage> C_DECLINE;

    public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;

    public static Action<NetMessage, NetworkConnection> S_WELCOME;

    public static Action<NetMessage, NetworkConnection> S_START_GAME;

    public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;

    public static Action<NetMessage, NetworkConnection> S_REMATCH;

    public static Action<NetMessage, NetworkConnection> S_RESIGN;

    public static Action<NetMessage, NetworkConnection> S_DRAW;

    public static Action<NetMessage, NetworkConnection> S_DECLINE;

    public static NetworkConnection
        EMPTY_CONNECTION = default(NetworkConnection);

    public static void OnData(
        DataStreamReader stream,
        NetworkConnection cnn,
        Server server = null
    )
    {
        try
        {
            NetMessage msg = CreateMessage(stream);
            if (msg == null)
            {
                return;
            }

            if (server == null)
            {
                Debug.Log("no server");
                msg.ReceivedOnClient();
                return;
            }

            msg.ReceivedOnServer (cnn);
        }
        catch (Exception e)
        {
            Debug.Log("Error on data in server" + e);
        }
    }

    private static NetMessage CreateMessage(DataStreamReader stream)
    {
        try
        {
            var opCode = (OpCode) stream.ReadByte();
            switch (opCode)
            {
                case OpCode.KEEP_ALIVE:
                    return new NetKeepAlive(stream);
                case OpCode.WELCOME:
                    return new NetWelcome(stream);
                case OpCode.START_GAME:
                    return new NetStartGame(stream);
                case OpCode.MAKE_MOVE:
                    return new NetMakeMove(stream);
                case OpCode.REMATCH:
                    return new NetRematch(stream);
                case OpCode.RESIGN:
                    return new NetResign(stream);
                case OpCode.DRAW:
                    return new NetDraw(stream);
                case OpCode.DECLINE:
                    return new NetDecline(stream);
                case OpCode.GET_OPPONENT_NAME:
                    return new NetGetOpponentName(stream);
                default:
                    NetMessage msg = null;
                    Debug.LogError("Message received had no OpCode");
                    return msg;
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error creating message on server" + e);
            NetMessage msg = null;

            return msg;
        }
    }
}
