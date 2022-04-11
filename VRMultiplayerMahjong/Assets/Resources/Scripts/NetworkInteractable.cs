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
        foreach (Transform transforms in args.interactorObject.transform.gameObject.GetComponentsInChildren<Transform>()) {
            if (transforms.gameObject.name.EndsWith("AttachPoint")) {
                transforms.rotation = transform.rotation;
            }
        }
        photonView.RequestOwnership();
        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args) {
        foreach (Transform transforms in args.interactorObject.transform.gameObject.GetComponentsInChildren<Transform>()) {
            if (transforms.gameObject.name.EndsWith("AttachPoint")) {
                transforms.rotation = Quaternion.identity;
            }
        }
        base.OnSelectExited(args);
    }

    //tiles highlight when hovered over
    protected override void OnHoverEntered(HoverEnterEventArgs args) {
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
