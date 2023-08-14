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
        if (driver.IsCreated)
        {
            driver.Dispose();
        }

        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);
        connection = driver.Connect(endpoint);

        SetActive(true);

        RegisterToEvent();
    }

    private void SetActive(bool active)
    {
        isActive = active;
    }

    public void Shutdown()
    {
        if (!isActive)
        {
            return;
        }

        UnRegisterToEvent();
        connection = default(NetworkConnection);
        driver.Dispose();
        SetActive(false);
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

            driver.ScheduleUpdate().Complete();
            CheckAlive();
            UpdateMessagePump();
        }
        catch (Exception e)
        {
            Debug.Log("Error on update in client" + e);
        }
    }

    private void CheckAlive()
    {
        if (connection.IsCreated || !isActive)
        {
            return;
        }

        if (
            connection.IsCreated &&
            connection.GetState(driver) == NetworkConnection.State.Connected
        )
        {
            driver.Disconnect (connection);
            connection.Disconnect (driver);
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

        try
        {
            while (HasEvent(out cmd, out stream))
            {
                switch (cmd)
                {
                    case NetworkEvent.Type.Connect:
                        SendToServer(new NetWelcome());
                        Debug.Log("We are connected!");
                        break;
                    case NetworkEvent.Type.Data:
                        NetUtility.OnData (stream, connection);
                        break;
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Client got disconnected from server");
                        connection.Disconnect (driver);
                        connectionDropped?.Invoke();
                        Shutdown();
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception on sending to server" + e);
        }
    }

    public void SendToServer(NetMessage msg)
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
            Debug.Log("Exception on sending to server" + e);
        }
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
