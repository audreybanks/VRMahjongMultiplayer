using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MahjongGameManager : MonoBehaviourPunCallbacks {

    private List<GameObject> tiles;
    private List<TransformData> tilePositions;
    private bool shuffling;
    public GameObject TilePositions;
    public GameObject resetButton;
    private Color previousColor;

    // Start is called before the first frame update
    void Start() {
        shuffling = false;
        tilePositions = new List<TransformData>();
        tiles = new List<GameObject>();
        foreach (Tile tile in TilePositions.GetComponentsInChildren<Tile>()) {
            tilePositions.Add(new TransformData(tile.gameObject.transform.position, tile.gameObject.transform.rotation));
            Destroy(tile.gameObject);
        }
        //Debug.Log(tilePositions.Count);
    }

    // Update is called once per frame
    void Update() {

    }

    ///<summary>Returns true if the tiles are currently being shuffled.</summary>
    public bool isShuffling() {
        return shuffling;
    }

    ///<summary>Randomizes the positons of the tiles</summary>
    private void shuffleTilePositions() {
        for (int i = 0; i < tilePositions.Count - 1; i++) {
            int randomIndex = UnityEngine.Random.Range(i, tilePositions.Count);
            TransformData temp = tilePositions[i];
            tilePositions[i] = tilePositions[randomIndex];
            tilePositions[randomIndex] = temp;
        }
    }

    ///<summary>Instantiates the wall of tiles and adds them to the list of tiles.</summary>
    public void buildWall() {
        shuffling = true;
        if (PhotonNetwork.IsMasterClient) {
            shuffleTilePositions();
            Object[] tilePrefabs = Resources.LoadAll("Prefabs/Tiles", typeof(GameObject));
            Object[] tileSpecialPrefabs = Resources.LoadAll("Prefabs/TilesSpecial", typeof(GameObject));
            // Debug.Log(tileSpecialPrefabs[0].name);
            // Debug.Log("buildWall() called");
            for (int i = 0; i < 4; i++) {
                for (int j = (31 * i); j < (31 * i) + 31; j++) {
                    tiles.Add(PhotonNetwork.Instantiate("Prefabs/Tiles/" + tilePrefabs[j % 31].name, tilePositions[j].position, tilePositions[j].rotation));
                }
            }

            for (int i = 0; i < 2; i++) {
                for (int j = 124 + (6 * i); j < 124 + (6 * i) + 6; j++) {
                    tiles.Add(PhotonNetwork.Instantiate("Prefabs/TilesSpecial/" + tileSpecialPrefabs[(j - 124) % 6].name, tilePositions[j].position, tilePositions[j].rotation));
                }
            }
        }
        shuffling = false;
    }

    ///<summary>Changes the color of the reset button, used when tiles are being shuffled to turn the button gray.</summary>
    [PunRPC]
    private void changeButtonColor(float r, float g, float b, float a) {
        resetButton.GetComponent<Renderer>().material.color = new Color(r, g, b, a);
    }

    [PunRPC]
    private void moveTiles(TransformData positions) {
        
    }
    
    ///<summary>Shuffles and resets the tile positions. Can only be activated by the Master Client.</summary>
    public void resetTiles() {
        shuffling = true;
        Debug.Log("Clicked reset button");
        Debug.Log("isShuffling: " + shuffling);
        if (PhotonNetwork.IsMasterClient) {
            Debug.Log(PhotonNetwork.LocalPlayer.UserId + " is the Master Client");
            previousColor = resetButton.GetComponent<Renderer>().material.color;
            photonView.RPC("changeButtonColor", RpcTarget.AllBuffered, 0.5f, 0.5f, 0.5f, 1.0f);
            
            //Before shuffling, transfer ownership to the Master Client
            foreach (GameObject tile in tiles) {
                tile.GetComponent<PhotonView>().RequestOwnership();
                tile.GetComponent<PhotonTransformView>().enabled = false;
            }
            
            //Have the Master Client shuffle the positions then use a rpc call to have each user move the tile locally(?)
            shuffleTilePositions();
            
            //TODO: Serialize TransformData to use in RPC
            //photonView.RPC("moveTiles", RpcTarget.All, tilePositions.ToArray());


            foreach (GameObject tile in tiles) {
                tile.GetComponent<PhotonTransformView>().enabled = true;
            }
            
            photonView.RPC("changeButtonColor", RpcTarget.AllBuffered, previousColor.r, previousColor.g, previousColor.b, 1.0f);
        }
        shuffling = false;
    }
}
