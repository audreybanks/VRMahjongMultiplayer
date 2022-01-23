using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks {

    void Start() {
        connectToServer();
    }

    private void connectToServer() {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting...");
    }

    public override void OnConnectedToMaster() {
        Debug.Log("Connected.");
        base.OnConnectedToMaster();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.MaxPlayers = 4;

        PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom() {
        Debug.Log("Joined Room.");
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        Debug.Log("New player has joined.");
        base.OnPlayerEnteredRoom(newPlayer);
    }
}

