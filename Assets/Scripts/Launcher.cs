using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    string _gameVersion = "1";
    public PunLogLevel Loglevel = PunLogLevel.Informational;
    public byte MaxPlayersPerRoom = 2;
    public GameObject controlPanel;
    public GameObject progressLabel;

    private bool isConnecting;

    private void Awake()
    {
        //PhotonNetwork.lobby = true;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LogLevel = Loglevel;
    }

    private void Start()
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }

    public void Connect()
    {
        isConnecting = true;
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);
    }
    public override void OnConnectedToMaster()
    {
        if(isConnecting)
        {
            Debug.Log("OnConnectedToMaster() was called by PUN");
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No random room available.");
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("OnDisconnected() was called by PUN");
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("You are in room now.");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("We load the 'Room for 1' ");
            PhotonNetwork.LoadLevel("Room for 1");
        }
    }

}
