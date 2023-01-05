using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{
#region Singleton implementation

    public static Client Instance { set; get; }

    private void Awake()
    {
        Instance = this;
        SetActive(false);
    }


#endregion


    public NetworkDriver driver;

    private NetworkConnection connection;

    private bool isActive;

    public Action connectionDropped;

    // Methods
    public void Init(string ip, ushort port)
    {
        Debug.Log("Init active " + isActive);
        if (isActive && connection != null)
        {
            Debug.Log("Init " + connection.GetState(driver));
        }
        if (!isActive)
        {
            Debug.Log("Init");
            driver = NetworkDriver.Create();
        }

        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);
        connection = driver.Connect(endpoint);
        Debug.Log("Attempting to connect to Server on " + endpoint.Address);

        SetActive(true);

        // UnRegisterToEvent();
        RegisterToEvent();
    }

    private void SetActive(bool active)
    {
        Debug.Log("Setting active to " + active);
        isActive = active;
    }

    public void Shutdown()
    {
        if (!isActive)
        {
            return;
        }

        Debug.Log("Dispose isActive " + isActive);
        UnRegisterToEvent();
        connection = NetUtility.EMPTY_CONNECTION;
        driver.Dispose();
        SetActive(false);
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

        driver.ScheduleUpdate().Complete();
        CheckAlive();
        UpdateMessagePump();
    }

    private void CheckAlive()
    {
        if (connection.IsCreated || !isActive)
        {
            return;
        }

        Debug.Log("Something went wrong, lost connection to server");
        connectionDropped?.Invoke();
        Shutdown();
    }

    private bool
    HasEvent(out NetworkEvent.Type cmd, out DataStreamReader stream)
    {
        cmd = connection.PopEvent(driver, out stream);
        return cmd != NetworkEvent.Type.Empty;
    }

    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while (HasEvent(out cmd, out stream))
        {
            switch (cmd)
            {
                case NetworkEvent.Type.Connect:
                    SendToServer(new NetWelcome());
                    Debug.Log("We are connected!");
                    break;
                case NetworkEvent.Type.Data:
                    NetUtility.OnData(stream, NetUtility.EMPTY_CONNECTION);
                    break;
                case NetworkEvent.Type.Disconnect:
                    Debug.Log("Client got disconnected from server");
                    connection = NetUtility.EMPTY_CONNECTION;
                    connectionDropped?.Invoke();
                    Shutdown();
                    break;
            }
        }
    }

    public void SendToServer(NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend (writer);
    }

    // Event parsing
    private void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }

    private void UnRegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }

    private void OnKeepAlive(NetMessage nm)
    {
        // Send it back, to keep both sides alive
        SendToServer (nm);
    }
}
