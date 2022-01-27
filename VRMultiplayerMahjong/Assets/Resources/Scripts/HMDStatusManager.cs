using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HMDStatusManager : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        Debug.Log("HMD Name: " + XRSettings.loadedDeviceName);
    }
}
