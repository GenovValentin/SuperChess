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

    private const float keepAliveTickRate = 200.0f;

    private float lastKeepAlive;

    public Action connectionDropped;

    // Methods
    public void Init(ushort port)
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.Log("Error on init of server" + e);
        }
    }

    public void Shutdown()
    {
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
        try
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
        catch (Exception e)
        {
            Debug.Log("Error on server update" + e);
        }
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
        try
        {
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i].IsCreated)
                {
                    continue;
                }

                Debug.Log("Removing connection " + i);
                connections.RemoveAtSwapBack (i);
                --i;
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error cleaning up connections on server" + e);
        }
    }

    private void AcceptNewConnections()
    {
        try
        {
            // Accept new connections
            NetworkConnection connection;
            while ((connection = driver.Accept()) != default(NetworkConnection))
            {
                connections.Add (connection);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error accepting connections" + e);
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
        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            try
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
                        connections[i] = default(NetworkConnection);
                        connectionDropped?.Invoke();
                        Shutdown(); // This doesn't happen usually, it's because we're in a two person game
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error on sending to client for " + i + e);
            }
        }
    }

    // Server specific
    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        try
        {
            DataStreamWriter writer;
            driver.BeginSend(connection, out writer);
            msg.Serialize(ref writer);
            driver.EndSend (writer);
        }
        catch (Exception e)
        {
            Debug.Log("Error sending to client" + e);
        }
    }

    public void Broadcast(NetMessage msg)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            try
            {
                if (connections[i].IsCreated)
                {
                    if (msg.Code != OpCode.KEEP_ALIVE)
                    {
                        Debug
                            .Log("Broadcasting to client" +
                            msg.Code +
                            " to : " +
                            i);
                    }
                    SendToClient(connections[i], msg);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error broadcasting on server" + e);
            }
        }
    }
}
