using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class InputManager : MonoBehaviour {

    VRControls vrcontrols;

    void Awake() {
        vrcontrols = new VRControls();
        vrcontrols.Enable();
    }

    private void OnEnable() {
        vrcontrols.Enable();
    }

    private void OnDisable() {

    }

    private void OnDestroy() {
        vrcontrols.Dispose();
    }
}
