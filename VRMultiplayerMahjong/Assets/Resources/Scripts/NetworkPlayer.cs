using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using Photon.Pun;
using Photon.Realtime;
using Wolf3D.ReadyPlayerMe.AvatarSDK;

public class NetworkPlayer : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks, IPunInstantiateMagicCallback {
    private Transform head;
    private Transform leftHand;
    private Transform rightHand;
    private Transform eyes;

    private Transform headDevice;
    private Transform leftHandDevice;
    private Transform rightHandDevice;

    public string avatarURL;
    private GameObject avatar;

    private MahjongGameManager gameManager;

    // Start is called before the first frame update
    void Start() {
        XROrigin rig = FindObjectOfType<XROrigin>();
        headDevice = rig.GetComponentInChildren<Camera>().gameObject.transform;
        ActionBasedController[] hands = rig.GetComponentsInChildren<ActionBasedController>();
        if (hands[0].gameObject.name == "LeftHand Controller") {
            leftHandDevice = hands[0].gameObject.transform;
            rightHandDevice = hands[1].gameObject.transform;
        } else {
            leftHandDevice = hands[0].gameObject.transform;
            rightHandDevice = hands[1].gameObject.transform;
        }
        gameManager = FindObjectOfType<MahjongGameManager>();

    }

    // Update is called once per frame
    void Update() {
        //If avatar isn't loaded yet, don't sync 
        if (photonView.IsMine && avatar != null) {
            head.SetPositionAndRotation(headDevice.position + new Vector3(0, -(eyes.position.y - head.position.y), -0.075f), headDevice.rotation);
            leftHand.SetPositionAndRotation(leftHandDevice.position, leftHandDevice.rotation);
            rightHand.SetPositionAndRotation(rightHandDevice.position, rightHandDevice.rotation);
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        loadAvatar();
    }

    //TODO: Load avatar locally to get rid of stuttering
    private void loadAvatar() {
        if (photonView.IsMine) {
            Debug.Log("Avatar loading...");
            AvatarLoader avatarLoader = new AvatarLoader();
            avatarLoader.LoadAvatar(avatarURL, OnAvatarImported, OnAvatarLoaded);
        }
    }

    private void OnAvatarImported(GameObject avatar) {
        if (photonView.IsMine) {
            Debug.Log("Avatar is imported...");
        }
    }

    private void OnAvatarLoaded(GameObject avatar, AvatarMetaData metaData) {
        if (photonView.IsMine) {
            Debug.Log("Avatar loaded.");
            Transform[] avatarComponents = avatar.GetComponentsInChildren<Transform>();

            foreach (Transform component in avatarComponents) {
                if (component.gameObject.name == "Neck") {
                    head = component;
                }

                if (component.gameObject.name == "LeftHand") {
                    leftHand = component;
                }
                
                if (component.gameObject.name == "RightHand") {
                    rightHand = component;
                }

                //eyes used to calculate distance between neck and eyes to properly place camera
                if (component.gameObject.name == "RightEye") {
                    eyes = component;
                }
            }
            avatar.GetComponent<Transform>().parent = gameObject.transform;
            this.avatar = avatar;
        }
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer) {
        Debug.Log("OnOwnershipRequest() called");
        Vector3 position = targetView.gameObject.transform.position;
        Quaternion rotation = targetView.gameObject.transform.rotation;

        //TODO: tiles not picked up by the owner don't go back to their original positions if request denied.
        if (targetView.IsMine && !gameManager.isShuffling()) {
            targetView.TransferOwnership(requestingPlayer);
        } else if (gameManager.isShuffling()) {
            Debug.Log(targetView.gameObject.GetComponent<Tile>().tileName + " is being shuffled");
        }
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner) {
        return;
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest) {
        return;
    }

    // public override void OnJoinedRoom() {
    //     base.OnJoinedRoom();
    //     PhotonNetwork.Instantiate("Prefabs/NetworkPlayer", transform.position, transform.rotation);
    //     Debug.Log("Joined Room.");
    // }
}
