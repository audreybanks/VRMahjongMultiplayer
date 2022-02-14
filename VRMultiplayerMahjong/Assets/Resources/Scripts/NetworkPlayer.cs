using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using Photon.Pun;
using Photon.Realtime;

public class NetworkPlayer : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks {
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private Transform headDevice;
    private Transform leftHandDevice;
    private Transform rightHandDevice;

    private MahjongGameManager gameManager;

    // Start is called before the first frame update
    void Start() {
        XROrigin rig = FindObjectOfType<XROrigin>();
        headDevice = rig.transform.Find("Camera Offset/Main Camera");
        leftHandDevice = rig.transform.Find("Camera Offset/LeftHand Controller");
        rightHandDevice = rig.transform.Find("Camera Offset/RightHand Controller");
        gameManager = FindObjectOfType<MahjongGameManager>();
    }

    // Update is called once per frame
    void Update() {
        if (photonView.IsMine) {
            setDeviceTransform(headDevice, head);
            setDeviceTransform(leftHandDevice, leftHand);
            setDeviceTransform(rightHandDevice, rightHand);
        }
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer) {
        Debug.Log("OnOwnershipRequest() called");

        if (targetView.IsMine && !gameManager.isShuffling()) {
            targetView.TransferOwnership(requestingPlayer);
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

    private void setDeviceTransform(Transform inputDevice, Transform mappingTransform) {
        mappingTransform.SetPositionAndRotation(inputDevice.position, inputDevice.rotation);
    }
}
