using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Wolf3D.ReadyPlayerMe.AvatarSDK;

public class PlayerSpawner : MonoBehaviourPun {

    private GameObject avatar;

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() {}

    public void spawnPlayer(string avatarURL) {
        loadAvatar(avatarURL);
    }

    private void loadAvatar(string avatarURL) {

    }

    //Check is photonView IsMine before invoking
    ///<summary>Returns the </summary>
    public GameObject getAvatar() {
        return avatar;
    }
}
