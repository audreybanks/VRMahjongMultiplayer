using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class ResetButtonInteractable : XRGrabInteractable {

    public MahjongGameManager mahjongGameManager;

    // Start is called before the first frame update
    void Start() {
        // Debug.Log("Reset button created");
        // Debug.Log("Reset button: " + mahjongGameManager.isShuffling());
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args) {
        mahjongGameManager.resetTiles();
        base.OnSelectEntered(args);
    }
}
