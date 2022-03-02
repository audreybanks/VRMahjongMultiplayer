using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class NetworkInteractable : XRGrabInteractable {

    private PhotonView photonView;
    private Material[] tileMats;

    // Start is called before the first frame update
    void Start() {
        photonView = GetComponent<PhotonView>();
        tileMats = gameObject.GetComponent<Renderer>().materials;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args) {
        Debug.Log("RequestOwnership() called");
        photonView.RequestOwnership();
        base.OnSelectEntered(args);
    }

    //TODO: Have tiles highlight when hovered over
    protected override void OnHoverEntered(HoverEnterEventArgs args) {
        Debug.Log(gameObject.GetComponent<Tile>().name + " is being hovered over");
        foreach (Material mat in tileMats) {
            mat.SetColor("_EmissiveColor", mat.GetColor("_Color") * 1);
        }
        base.OnHoverEntered(args);
    }

    protected override void OnHoverExited(HoverExitEventArgs args) {
        base.OnHoverExited(args);
    }
}
