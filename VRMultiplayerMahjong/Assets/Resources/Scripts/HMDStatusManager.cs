using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HMDStatusManager : MonoBehaviour {

    public GameObject XRDeviceSimulator;

    // Start is called before the first frame update
    void Start() {
        if (XRSettings.loadedDeviceName == "MockHMD Display") {
            XRDeviceSimulator.SetActive(true);
        }
    }
}
