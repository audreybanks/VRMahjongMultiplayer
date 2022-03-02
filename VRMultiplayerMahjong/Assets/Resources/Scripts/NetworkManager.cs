using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks {

    private GameObject playerPrefab;
    private GameObject gameManager;

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
        base.OnJoinedRoom();
        Debug.Log("Joined Room.");
        playerPrefab = PhotonNetwork.Instantiate("Prefabs/NetworkPlayer", transform.position, transform.rotation);
        if (PhotonNetwork.IsMasterClient) { //multiple game managers created without this
            gameManager = PhotonNetwork.InstantiateRoomObject("Prefabs/MahjongGameManager", transform.position, transform.rotation);
        }
    }

    public override void OnLeftRoom() {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(playerPrefab);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        Debug.Log("New player has joined.");
        base.OnPlayerEnteredRoom(newPlayer);
    }
}

