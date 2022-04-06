using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks {

    private GameObject playerPrefab;
    private GameObject gameManager;
    public GameObject table;
    public GameObject plane;
    public Button startButton;
    public GameObject startingTiles;

    void Start() {
        table.SetActive(false);
        plane.SetActive(false);
        startingTiles.SetActive(false);
    }

    public void connectToServer() {
        startButton.interactable = false;
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
        startButton.GetComponentInParent<Canvas>().gameObject.SetActive(false);
        table.SetActive(true);
        plane.SetActive(true);
        startingTiles.SetActive(true);
        Debug.Log("Joined Room.");
        playerPrefab = PhotonNetwork.Instantiate("Prefabs/NetworkPlayer", transform.position, transform.rotation);
        gameManager = PhotonNetwork.InstantiateRoomObject("Prefabs/MahjongGameManager", transform.position, transform.rotation);
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

