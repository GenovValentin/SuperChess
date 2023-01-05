using Unity.Networking.Transport;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    #region Singleton implementation

    public static Client Instance{set; get;}
    private void Awake() {
        Instance = this;
        SetActive(false);
    }

    #endregion

    public NetworkDriver driver;
    private NetworkConnection connection;

    private bool isActive;

    public Action connectionDropped;

    // Methods
    public void Init(string ip, ushort port) {
        Debug.Log("Init active " + isActive);
        if(isActive && connection != null)
            Debug.Log("Init " + connection.GetState(driver));
        if (!isActive) {
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

    private void SetActive (bool active) {
        Debug.Log("Setting active to " + active);
        isActive = active;
    }

    public void Shutdown() {
        if (!isActive) {
            return;
        }

        Debug.Log("Dispose isActive " + isActive);
        UnRegisterToEvent();
        connection = default(NetworkConnection);
        driver.Dispose();
        SetActive(false);
    }

    public void OnDestroy() {
        Shutdown();
    }

    public void Update() {
        if (!isActive) {
            return;
        }
        // Debug.Log("Pumping " +isActive);

        driver.ScheduleUpdate().Complete();
        CheckAlive();
        UpdateMessagePump();
    }

    private void CheckAlive() {
        if (!connection.IsCreated && isActive) {
            Debug.Log("Something went wrong, lost connection to server");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }

    private void UpdateMessagePump() {
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty) {
            if (cmd == NetworkEvent.Type.Connect) {
                SendToServer(new NetWelcome());
                Debug.Log("We are connected!");
            }
            else if (cmd == NetworkEvent.Type.Data) {
                NetUtility.OnData(stream, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Client got disconnected from server");
                connection = default(NetworkConnection);
                connectionDropped?.Invoke();
                Shutdown();
            }
        }
    }

    public void SendToServer(NetMessage msg) {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    // Event parsing
    private void RegisterToEvent() {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }

    private void UnRegisterToEvent() {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }

    private void OnKeepAlive(NetMessage nm) {
        // Send it back, to keep both sides alive
        SendToServer(nm);
    }
}
