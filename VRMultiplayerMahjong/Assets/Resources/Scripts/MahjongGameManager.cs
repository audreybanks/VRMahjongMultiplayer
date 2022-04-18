using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;

public class MahjongGameManager : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback {

    private List<GameObject> tiles;
    private List<TransformData> tilePositions;
    private bool shuffling;
    private GameObject TilePositions;
    private GameObject resetButton;

    // Start is called before the first frame update
    void Start() {
        //Debug.Log(tilePositions.Count);
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        shuffling = false;
        tilePositions = new List<TransformData>();
        tiles = new List<GameObject>();

        foreach (Transform child in GetComponentsInChildren<Transform>()) {
            if (child.gameObject.name == "TilePositions") {
                TilePositions = child.gameObject;
            }
        }

        foreach (Tile tile in TilePositions.GetComponentsInChildren<Tile>()) {
            //TODO: Tiles show up in the wrong place for players besides the master client
            tilePositions.Add(new TransformData(tile.gameObject.transform.position, tile.gameObject.transform.rotation));
            Destroy(tile.gameObject);
        }

        if (PhotonNetwork.IsMasterClient) {
            resetButton = PhotonNetwork.InstantiateRoomObject("Prefabs/ButtonStand", new Vector3(0.75f, 0.49400003f, 1.90999997f), Quaternion.identity);
            resetButton.GetComponentInChildren<PushButton>().onPressed.AddListener(resetTiles);
            resetButton.transform.parent = transform;
            //After instatiating the reset button as the Master Client, use an rpc to set it for the other clients.
            photonView.RPC("setResetButton", RpcTarget.OthersBuffered, resetButton.GetComponent<PhotonView>().ViewID);
        }
        buildWall();
    }

    ///<summary>Returns true if the tiles are currently being shuffled.</summary>
    public bool isShuffling() {
        return shuffling;
    }

    ///<summary>Randomizes the positons of the tiles</summary>
    private void shuffleTilePositions() {
        for (int i = 0; i < tilePositions.Count - 1; i++) {
            int randomIndex = Random.Range(i, tilePositions.Count);
            TransformData temp = tilePositions[i];
            tilePositions[i] = tilePositions[randomIndex];
            tilePositions[randomIndex] = temp;
        }
    }

    ///<summary>Instantiates the wall of tiles and adds them to the list of tiles.</summary>
    public void buildWall() {
        photonView.RPC("updateShuffleState", RpcTarget.AllBuffered, true);
        if (PhotonNetwork.IsMasterClient) {
            shuffleTilePositions();
            Object[] tilePrefabs = Resources.LoadAll("Prefabs/Tiles", typeof(GameObject));
            Object[] tileSpecialPrefabs = Resources.LoadAll("Prefabs/TilesSpecial", typeof(GameObject));
            // Debug.Log(tileSpecialPrefabs[0].name);
            // Debug.Log("buildWall() called");
            for (int i = 0; i < 4; i++) {
                for (int j = (31 * i); j < (31 * i) + 31; j++) {
                    GameObject tile = PhotonNetwork.InstantiateRoomObject("Prefabs/Tiles/" + tilePrefabs[j % 31].name, tilePositions[j].position,
                        tilePositions[j].rotation);
                    //tile.transform.parent = TilePositions.transform;
                    int tileID = tile.GetComponent<PhotonView>().ViewID;
                    photonView.RPC("addTile", RpcTarget.AllBuffered, tileID);
                }
            }

            for (int i = 0; i < 2; i++) {
                for (int j = 124 + (6 * i); j < 124 + (6 * i) + 6; j++) {
                    GameObject tile = PhotonNetwork.InstantiateRoomObject("Prefabs/TilesSpecial/" + tileSpecialPrefabs[(j - 124) % 6].name, tilePositions[j].position,
                        tilePositions[j].rotation);
                    //tile.transform.parent = TilePositions.transform;
                    int tileID = tile.GetComponent<PhotonView>().ViewID;
                    photonView.RPC("addTile", RpcTarget.AllBuffered, tileID);
                }
            }
        }
        photonView.RPC("updateShuffleState", RpcTarget.AllBuffered, false);
    }

    [PunRPC]
    private void addTile(int tileID) {
        tiles.Add(PhotonNetwork.GetPhotonView(tileID).gameObject);
        //PhotonNetwork.GetPhotonView(tileID).gameObject.transform.parent = TilePositions.transform;
    }

    ///<summary>Sets the reset button for all players besides the Master Client</summary>
    [PunRPC]
    private void setResetButton(int resetButtonID) {
        resetButton = PhotonNetwork.GetPhotonView(resetButtonID).gameObject;
        resetButton.GetComponentInChildren<PushButton>().onPressed.AddListener(resetTiles);
    }

    ///<summary>Changes the color of the reset button, used when tiles are being shuffled to turn the button gray.</summary>
    [PunRPC]
    private void changeButtonColor(float r, float g, float b, float a) {
        resetButton.GetComponent<Renderer>().material.color = new Color(r, g, b, a);
    }

    ///<summary>Updates the shuffle state for each user</summary>
    [PunRPC]
    private void updateShuffleState(bool state) {
        shuffling = state;
    }

    //TODO: Reset tiles by reinstantiating them
    ///<summary>Shuffles and resets the tile positions. Can only be activated by the Master Client.</summary>
    public void resetTiles() {
        shuffling = true;
        // Debug.Log("Clicked reset button");
        // Debug.Log("isShuffling: " + shuffling);

        foreach (GameObject tile in tiles) {
            tile.GetComponent<PhotonView>().RequestOwnership();
            tile.GetComponent<PhotonTransformView>().enabled = false;
        }

        Debug.Log("tile count: " + tiles.Count);
        Debug.Log("tilePositions count: " + tilePositions.Count);

        shuffleTilePositions();

        Debug.Log("tile count: " + tiles.Count);
        Debug.Log("tilePositions count: " + tilePositions.Count);
        for (int i = 0; i < tilePositions.Count; i++) {
            tiles[i].transform.position = tilePositions[i].position;
            tiles[i].transform.rotation = tilePositions[i].rotation;
        }

        foreach (GameObject tile in tiles) {
            tile.GetComponent<PhotonTransformView>().enabled = true;
        }
        shuffling = false;
    }
}
