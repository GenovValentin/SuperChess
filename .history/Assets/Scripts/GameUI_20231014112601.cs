using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum CameraAngle
{
    menu = 0,
    whiteTeam = 1,
    blackTeam = 2
}

public class GameUI : MonoBehaviour
{
    string DEFAULT_IP = "127.0.0.1";

    ushort DEFAULT_PORT = 9000;

    public static GameUI Instance { set; get; }

    public Server server;

    public Client client;

    // private Chessboard chessboardInstance;
    [SerializeField]
    private Animator menuAnimatior;

    [SerializeField]
    private TMP_InputField addressInput;

    [SerializeField]
    private GameObject[] cameraAngles;

    public Action<bool> SetLocalGame;

    private bool isInSettingsMenu = false;

    private void Awake()
    {
        Instance = this;

        RegisterEvents();
    }

    // Cameras
    public void ChangeCamera(CameraAngle index)
    {
        for (int i = 0; i < cameraAngles.Length; i++)
        {
            cameraAngles[i].SetActive(false);
        }

        cameraAngles[(int) index].SetActive(true);
    }

    // Buttons
    public void OnLocalGameButton()
    {
        menuAnimatior.SetTrigger("InGameMenu");
        SetLocalGame?.Invoke(true);
        server.Init (DEFAULT_PORT);
        client.Init (DEFAULT_IP, DEFAULT_PORT);
    }

    public void OnOnlineGameButton()
    {
        menuAnimatior.SetTrigger("OnlineMenu");
    }

    public void OnSettingsButton()
    {
        isInSettingsMenu = true;
        menuAnimatior.SetTrigger("SettingsMenu");
    }

    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);
        server.Init (DEFAULT_PORT);
        client.Init (DEFAULT_IP, DEFAULT_PORT);
        menuAnimatior.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        SetLocalGame?.Invoke(false);
        string ip = addressInput.text == "" ? DEFAULT_IP : addressInput.text;

        client.Init (DEFAULT_IP, DEFAULT_PORT);
    }

    public void OnOnlineBackButton()
    {
        menuAnimatior.SetTrigger("StartMenu");
    }

    public void OnHostBackButton()
    {
        server.Shutdown();
        client.Shutdown();
        menuAnimatior.SetTrigger("OnlineMenu");
    }

    public void OnSettingsBackButton()
    {
        menuAnimatior.SetTrigger("StartMenu");
    }

    public void OnLeaveGameMenu()
    {
        ChangeCamera(CameraAngle.menu);
        menuAnimatior.SetTrigger("StartMenu");
    }


#region

    private void RegisterEvents()
    {
        NetUtility.C_START_GAME += OnStartGameClient;
    }

    private void UnRegisterEvents()
    {
        NetUtility.C_START_GAME -= OnStartGameClient;
    }

    private void OnStartGameClient(NetMessage msg)
    {
        menuAnimatior.SetTrigger("InGameMenu");
    }


#endregion
}
