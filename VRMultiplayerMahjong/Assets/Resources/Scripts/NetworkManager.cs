using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks {

    private GameObject playerPrefab;
    public GameObject plane;
    public GameObject startButton;
    public GameObject connectingCanvas;

    void Start() {
        plane.SetActive(false);
        connectingCanvas.SetActive(false);
        //For testing, remove later
        connectToServer();
    }

    public void connectToServer() {
        startButton.SetActive(false);
        connectingCanvas.SetActive(true);
        Debug.Log("Connecting...");
        PhotonNetwork.ConnectUsingSettings();
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
        plane.SetActive(true);
        connectingCanvas.SetActive(false);
        Debug.Log("Joined Room.");
        playerPrefab = PhotonNetwork.Instantiate("Prefabs/NetworkPlayer", transform.position, transform.rotation);
        PhotonNetwork.InstantiateRoomObject("Prefabs/MahjongGame", new Vector3(0.0199999996f, 0.90200001f, 1.89999998f), 
            transform.rotation);
    }

    public override void OnLeftRoom() {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(playerPrefab);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        Debug.Log("New player has joined.");
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnDisconnected(DisconnectCause cause) {
        Debug.Log("Disconnected.");
        base.OnDisconnected(cause);
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        Debug.Log("Could not connect to room.");
        base.OnJoinRoomFailed(returnCode, message);
    }
}

