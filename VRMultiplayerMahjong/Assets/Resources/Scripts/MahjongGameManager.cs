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

    // Start is called before the first frame update
    void Start() {
        shuffling = false;
        tilePositions = new List<TransformData>();
        tiles = new List<GameObject>();
        foreach (Tile tile in TilePositions.GetComponentsInChildren<Tile>()) {
            tilePositions.Add(new TransformData(tile.gameObject.transform.position, tile.gameObject.transform.rotation));
            Destroy(tile.gameObject);
        }
        Debug.Log(tilePositions.Count);
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
            Debug.Log(tileSpecialPrefabs[0].name);
            Debug.Log("buildWall() called");
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
    //TODO: Make button to shuffle tiles, only react to master client
}
