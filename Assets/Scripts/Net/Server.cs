using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;

public class Server : MonoBehaviour
{
#region Singleton implementation

    public static Server Instance { set; get; }

    private void Awake()
    {
        Instance = this;
        isActive = false;
    }


#endregion


    private static short MAX_CONNECTIONS_COUNT = 2;

    public NetworkDriver driver;

    private NativeList<NetworkConnection> connections;

    private bool isActive;

    private const float keepAliveTickRate = 20.0f;

    private float lastKeepAlive;

    public Action connectionDropped;

    // Methods
    public void Init(ushort port)
    {
        Debug.Log("Server init " + isActive);
        if (isActive)
        {
            return;
        }

        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = InitMainEndPoint(port);
        if (IsEndpointBound(endpoint))
        {
            Debug.Log("Unable to bind on port " + endpoint.Port);
            return;
        }

        driver.Listen();
        Debug.Log("Currently listening on port " + endpoint.Port);

        connections = InitConnections();
        isActive = true;
    }

    public void Shutdown()
    {
        Debug.Log("Server shutdown " + isActive);
        if (!isActive)
        {
            return;
        }

        connections.Dispose();
        driver.Dispose();
        isActive = false;
    }

    public void OnDestroy()
    {
        Shutdown();
    }

    public void Update()
    {
        if (!isActive)
        {
            return;
        }

        KeepAlive();
        driver.ScheduleUpdate().Complete();
        CleanupConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }

    private bool IsEndpointBound(NetworkEndPoint endpoint)
    {
        return driver.Bind(endpoint) != 0;
    }

    private NetworkEndPoint InitMainEndPoint(ushort port)
    {
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;

        return endpoint;
    }

    private NativeList<NetworkConnection> InitConnections()
    {
        return new NativeList<NetworkConnection>(MAX_CONNECTIONS_COUNT,
            Allocator.Persistent);
    }

    private void KeepAlive()
    {
        if (Time.time - lastKeepAlive <= keepAliveTickRate)
        {
            return;
        }

        lastKeepAlive = Time.time;
        Broadcast(new NetKeepAlive());
    }

    private void CleanupConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].IsCreated)
            {
                continue;
            }

            connections.RemoveAtSwapBack (i);
            --i;
        }
    }

    private void AcceptNewConnections()
    {
        // Accept new connections
        NetworkConnection connection;
        while ((connection = driver.Accept()) != NetUtility.EMPTY_CONNECTION)
        {
            connections.Add (connection);
        }
    }

    private NetworkEvent.Type
    PopEvent(NetworkConnection connection, out DataStreamReader stream)
    {
        return driver.PopEventForConnection(connection, out stream);
    }

    private bool
    HasEvent(
        NetworkConnection connection,
        out NetworkEvent.Type cmd,
        out DataStreamReader stream
    )
    {
        cmd = PopEvent(connection, out stream);
        return cmd != NetworkEvent.Type.Empty;
    }

    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        for (int i = 0; i < connections.Length; i++)
        {
            while (HasEvent(connections[i], out cmd, out stream))
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    NetUtility.OnData(stream, connections[i], this);
                    return;
                }

                if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    connections[i] = NetUtility.EMPTY_CONNECTION;
                    connectionDropped?.Invoke();
                    Shutdown(); // This doesn't happen usually, it's because we're in a two person game
                }
            }
        }
    }

    // Server specific
    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend (writer);
    }

    public void Broadcast(NetMessage msg)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                continue;
            }

            if (msg.Code != OpCode.KEEP_ALIVE)
            {
                Debug
                    .Log("Sending " +
                    msg.Code +
                    " to : " +
                    connections[i].InternalId);
            }

            SendToClient(connections[i], msg);
        }
    }
}
