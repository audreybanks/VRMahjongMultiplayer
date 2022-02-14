using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class NetworkInteractable : XRGrabInteractable {

    private PhotonView photonView;

    // Start is called before the first frame update
    void Start() {
        photonView = GetComponent<PhotonView>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args) {
        Debug.Log("RequestOwnership() called");
        photonView.RequestOwnership();
        base.OnSelectEntered(args);
    }
}
