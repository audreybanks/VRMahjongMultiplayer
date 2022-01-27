using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour {

    private PhotonView photonView;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private Transform headDevice;
    private Transform leftHandDevice;
    private Transform rightHandDevice;


    // Start is called before the first frame update
    void Start() {
        photonView = GetComponent<PhotonView>();
        XROrigin rig = FindObjectOfType<XROrigin>();
        headDevice = rig.transform.Find("Camera Offset/Main Camera");
        leftHandDevice = rig.transform.Find("Camera Offset/LeftHand Controller");
        rightHandDevice = rig.transform.Find("Camera Offset/RightHand Controller");
    }

    // Update is called once per frame
    void Update() {
        if (photonView.IsMine) {
            setDeviceTransform(headDevice, head);
            setDeviceTransform(leftHandDevice, leftHand);
            setDeviceTransform(rightHandDevice, rightHand);
        }
    }
    private void setDeviceTransform(Transform inputDevice, Transform mappingTransform) {
        mappingTransform.SetPositionAndRotation(inputDevice.position, inputDevice.rotation);
    }
}
