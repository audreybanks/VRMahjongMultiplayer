using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class NetworkInteractable : XRGrabInteractable {

    private PhotonView photonView;
    private Material[] tileMats;

    private NetworkPlayer player;

    // Start is called before the first frame update
    void Start() {
        photonView = GetComponent<PhotonView>();
        tileMats = gameObject.GetComponent<Renderer>().materials;
        player = GameObject.FindObjectOfType<NetworkPlayer>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args) {
        Debug.Log("RequestOwnership() called");
        player.disableHandRenderer();
        photonView.RequestOwnership();
        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args) {
        player.enableHandRenderer();
        base.OnSelectExited(args);
    }

    //tiles highlight when hovered over
    protected override void OnHoverEntered(HoverEnterEventArgs args) {
        //Debug.Log(gameObject.GetComponent<Tile>().name + " is being hovered over");
        foreach (Material mat in tileMats) {
            //Debug.Log(mat.GetColor("_Color").ToString());
            mat.SetColor("_Color", mat.GetColor("_Color") + new Color(0.3f, 0.3f, 0.3f, 1.0f));
        }
        base.OnHoverEntered(args);
    }

    protected override void OnHoverExited(HoverExitEventArgs args) {
        foreach (Material mat in tileMats) {
            //Debug.Log(mat.GetColor("_Color").ToString());
            mat.SetColor("_Color", mat.GetColor("_Color") - new Color(0.3f, 0.3f, 0.3f, 1.0f));
        }
        base.OnHoverExited(args);
    }
}
