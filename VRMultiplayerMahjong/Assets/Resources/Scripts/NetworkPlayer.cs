using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using Photon.Pun;
using Photon.Realtime;
using Wolf3D.ReadyPlayerMe.AvatarSDK;

public class NetworkPlayer : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks, IPunInstantiateMagicCallback, IPunObservable {

    public Vector3 headBodyOffset;
    private Transform head;
    private Transform leftHand;
    private Transform rightHand;

    private Transform headDevice;
    private Transform leftHandDevice;
    private Transform rightHandDevice;
    private XROrigin rig;

    public string avatarURL;
    private Animator handAnimator;
    private GameObject avatar;
    private int loadedAvatar;

    private Vector3[] avatarComponentPositions;
    private Quaternion[] avatarComponentRotations;
    private Transform[] avatarComponents;

    private MahjongGameManager gameManager;

    [SerializeField] private MapTransforms headMapping;
    [SerializeField] private MapTransforms leftHandMapping;
    [SerializeField] private MapTransforms rightHandMapping;

    private GameObject lastHoveredObject;

    private InputDevice leftInput;
    private InputDevice rightInput;
    private GameObject locomotonManager;

    private Transform leftAttachPoint;
    private Transform rightAttachPoint;

    ///<summary>Class to map the network Transform and the device Transform</summary>
    [System.Serializable]
    private class MapTransforms {
        public Transform deviceTransform;
        public Transform networkTransform;

        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        public MapTransforms(Transform deviceTransform, Transform networkTransform, Vector3 positionOffset, Vector3 rotationOffset) {
            this.deviceTransform = deviceTransform;
            this.networkTransform = networkTransform;
            this.positionOffset = positionOffset;
            this.rotationOffset = rotationOffset;
        }

        public void mapTransforms() {
            networkTransform.position = deviceTransform.TransformPoint(positionOffset);
            networkTransform.rotation = deviceTransform.rotation * Quaternion.Euler(rotationOffset);
        }
    }

    // Start is called before the first frame update
    void Start() {
        rig = FindObjectOfType<XROrigin>();
        Transform[] avatarObjects = GetComponentsInChildren<Transform>();
        headDevice = rig.GetComponentInChildren<Camera>().gameObject.transform;
        ActionBasedController[] hands = rig.GetComponentsInChildren<ActionBasedController>();
        if (hands[0].gameObject.name == "LeftHand Controller") {
            leftHandDevice = hands[0].gameObject.transform;
            rightHandDevice = hands[1].gameObject.transform;
        } else {
            leftHandDevice = hands[1].gameObject.transform;
            rightHandDevice = hands[0].gameObject.transform;
        }

        leftHandDevice.GetComponentInChildren<MeshRenderer>().enabled = false;
        rightHandDevice.GetComponentInChildren<MeshRenderer>().enabled = false;

        leftHandDevice.GetComponentInChildren<BoxCollider>().enabled = true;
        rightHandDevice.GetComponentInChildren<BoxCollider>().enabled = true;

        gameManager = FindObjectOfType<MahjongGameManager>();
        locomotonManager = FindObjectOfType<LocomotionSystem>().gameObject;

        leftInput = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightInput = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    // Update is called once per frame
    void Update() {

        //If avatar isn't loaded yet, don't sync 
        if (photonView.IsMine && avatar != null) {
            transform.position = head.position + headBodyOffset;
            transform.forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;

            headMapping.mapTransforms();
            leftHandMapping.mapTransforms();
            rightHandMapping.mapTransforms();

            updateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), "RightGrip");
            updateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), "LeftGrip");

            Vector2 leftJoystick;
            leftInput.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftJoystick);

            Vector2 rightJoystick;
            rightInput.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightJoystick);

            
            foreach (Transform transform in rig.GetComponentsInChildren<Transform>()) {
                if (transform.name == "LeftHandAttachPoint") {
                    leftAttachPoint = transform;
                } else if (transform.name == "RightHandAttachPoint") {
                    rightAttachPoint = transform;
                }
            }

            foreach (XRDirectInteractor device in rig.GetComponentsInChildren<XRDirectInteractor>()) {
                if (device.hasSelection) {
                    locomotonManager.GetComponent<ContinuousTurnProviderBase>().enabled = false;
                    //Debug.Log(device.transform.name + " hasSelection: " + device.hasSelection);
                    if (device.transform.name == "RightHand Controller") {
                        rotateInteractable(device, rightJoystick, rightAttachPoint);
                    } else {
                        rotateInteractable(device, leftJoystick, leftAttachPoint);
                    }
                    disableHandRenderer(device.transform.name);
                } else {
                    //Debug.Log(device.transform.name + " hasSelection: " + device.hasSelection);
                    locomotonManager.GetComponent<ContinuousTurnProviderBase>().enabled = true;
                    enableHandRenderer(device.transform.name);
                }

                //Highlight the first interactable being hovered by this player
                if (device.hasHover) {
                    highlightInteractable(device);
                } else if (lastHoveredObject != null) {
                    unhighlightInteractable(lastHoveredObject);
                    lastHoveredObject = null;
                }
            }
        }

        //Debug.Log("Update called for Player " + photonView.OwnerActorNr);
    }

    private void rotateInteractable(XRDirectInteractor interactor, Vector2 joystickInput, Transform attachPoint) {
        Debug.Log("interactable: " + interactor.interactablesSelected[0].transform.name);
        Debug.Log("interactable rotation: " + interactor.interactablesSelected[0].transform.rotation);

        GameObject interactable = interactor.interactablesSelected[0].transform.gameObject;

        if (joystickInput[0] < -0.8 && joystickInput[1] < -0.8) {
            interactable.transform.Rotate(0, -5, 0);
            attachPoint.transform.Rotate(0, -5, 0);
        } else if (joystickInput[0] > 0.8 && joystickInput[1] > 0.8) {
            interactable.transform.Rotate(0, 5, 0);
            attachPoint.transform.Rotate(0, 5, 0);
        }

        if (joystickInput[0] < -0.8) {
            interactable.transform.Rotate(0, 0, -5);
            attachPoint.transform.Rotate(0, 0, -5);
        } else if (joystickInput[0] > 0.8) {
            interactable.transform.Rotate(0, 0, 5);
            attachPoint.transform.Rotate(0, 0, 5);
        }

        if (joystickInput[1] < -0.8) {
            interactable.transform.Rotate(5, 0, 0);
            attachPoint.transform.Rotate(5, 0, 0);
        } else if (joystickInput[1] > 0.8) {
            interactable.transform.Rotate(-5, 0, 0);
            attachPoint.transform.Rotate(-5, 0, 0);
        }
    }

    //Used to update the position and rotations of each avatar child.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

        int listCount = 0;
        avatarComponentPositions = new Vector3[GetComponentsInChildren<Transform>().Length];
        avatarComponentRotations = new Quaternion[GetComponentsInChildren<Transform>().Length];
        avatarComponents = GetComponentsInChildren<Transform>();

        if (photonView.IsMine) {
            listCount = avatarComponents.Length;

            for (int i = 0; i < avatarComponentPositions.Length; i++) {
                avatarComponentPositions[i] = avatarComponents[i].position;
                avatarComponentRotations[i] = avatarComponents[i].rotation;
            }
            photonView.RPC("updateAvatarTransforms", RpcTarget.Others, avatarComponentPositions,
                avatarComponentRotations, listCount);
        }
    }

    [PunRPC]
    private void updateAvatarTransforms(Vector3[] positions, Quaternion[] rotations, int listCount) {
        if ((avatarComponents != null) && listCount == avatarComponents.Length) {
            for (int i = 0; i < listCount; i++) {
                avatarComponents[i].position = positions[i];
                avatarComponents[i].rotation = rotations[i];
            }
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (photonView.IsMine) {
            loadAvatar();
        }
    }

    ///<summary>Loads the avatar from the provided URL</sumamry>
    private void loadAvatar() {
        Debug.Log("Avatar loading...");
        AvatarLoader avatarLoader = new AvatarLoader();
        avatarLoader.LoadAvatar(avatarURL, OnAvatarImported, OnAvatarLoaded);
    }

    private void OnAvatarImported(GameObject avatar) {
        Debug.Log("Avatar is imported...");
    }

    private void OnAvatarLoaded(GameObject avatar, AvatarMetaData metaData) {
        Debug.Log("Avatar loaded.");
        if (photonView.IsMine) {
            avatar.transform.parent = gameObject.transform;
            setAvatarComponents(avatar);
            this.avatar = avatar;
            photonView.RPC("loadAvatarRPC", RpcTarget.OthersBuffered, photonView.OwnerActorNr, avatarURL);
        } else if (!photonView.IsMine && loadedAvatar == photonView.OwnerActorNr) {
            avatar.transform.parent = gameObject.transform;
        }
    }

    [PunRPC]
    private void loadAvatarRPC(int actorID, string URL) {
        loadedAvatar = actorID;
        avatarURL = URL;
        if (loadedAvatar == photonView.OwnerActorNr) {
            loadAvatar();
        }
    }

    private void setAvatarComponents(GameObject avatar) {
        Transform[] avatarComponents = avatar.GetComponentsInChildren<Transform>();
        foreach (Transform component in avatarComponents) {
            if (component.gameObject.name.EndsWith("_EyeLeft") || component.gameObject.name.EndsWith("_EyeRight") ||
        component.gameObject.name.EndsWith("_Glasses") || component.gameObject.name.EndsWith("_Hair") ||
        component.gameObject.name.EndsWith("_Head") || component.gameObject.name.EndsWith("_Teeth") ||
        component.gameObject.name.EndsWith("_Facewear") || component.gameObject.name.EndsWith("_Shirt") ||
        component.gameObject.name.EndsWith("_Headwear")) {
                component.gameObject.layer = LayerMask.NameToLayer("PlayerHead");
            }

            if (component.gameObject.name == "Neck") {
                head = component;
            }

            if (component.gameObject.name == "LeftHand") {
                leftHand = component;

            }

            if (component.gameObject.name == "RightHand") {
                rightHand = component;
            }

            if (component.gameObject.name == "Spine") {
                handAnimator = component.gameObject.AddComponent<Animator>();
                handAnimator.runtimeAnimatorController = Resources.Load("Animations/HandAnimator") as RuntimeAnimatorController;

                PhotonAnimatorView photonAnimatorView = component.gameObject.AddComponent<PhotonAnimatorView>();
                photonAnimatorView.SetLayerSynchronized(0, PhotonAnimatorView.SynchronizeType.Continuous);
                photonAnimatorView.SetLayerSynchronized(1, PhotonAnimatorView.SynchronizeType.Continuous);

                photonAnimatorView.SetParameterSynchronized("RightGrip", PhotonAnimatorView.ParameterType.Float,
                    PhotonAnimatorView.SynchronizeType.Continuous);
                photonAnimatorView.SetParameterSynchronized("LeftGrip", PhotonAnimatorView.ParameterType.Float,
                    PhotonAnimatorView.SynchronizeType.Continuous);
            }
        }

        headMapping = new MapTransforms(headDevice, head, new Vector3(0.0f, -0.15f, 0.0f),
            new Vector3(0.0f, 0.0f, 0.0f));
        leftHandMapping = new MapTransforms(leftHandDevice, leftHand, new Vector3(0.0f, -0.06f, -0.15f),
            new Vector3(0.0f, 90.0f, 90.0f));
        rightHandMapping = new MapTransforms(rightHandDevice, rightHand, new Vector3(0.0f, -0.06f, -0.15f),
            new Vector3(0.0f, -90.0f, -90.0f));
    }

    private void highlightInteractable(XRDirectInteractor device) {

        if (lastHoveredObject != null) {
            Material[] lastHoverMats = lastHoveredObject.GetComponent<Renderer>().materials;
            foreach (Material mat in lastHoverMats) {
                mat.SetColor("_Color", mat.GetColor("_Color") - new Color(0.3f, 0.3f, 0.3f, 1.0f));
            }
        }

        lastHoveredObject = device.interactablesHovered[0].transform.gameObject;

        Material[] tileMats = lastHoveredObject.GetComponent<Renderer>().materials;
        foreach (Material mat in tileMats) {
            mat.SetColor("_Color", mat.GetColor("_Color") + new Color(0.3f, 0.3f, 0.3f, 1.0f));
        }
    }

    private void unhighlightInteractable(GameObject interactable) {
        Material[] tileMats = interactable.GetComponent<Renderer>().materials;
        foreach (Material mat in tileMats) {
            mat.SetColor("_Color", mat.GetColor("_Color") - new Color(0.3f, 0.3f, 0.3f, 1.0f));
        }
    }


    ///<summary>Disables the hand renderer, used when grabbing an object</summary>
    public void disableHandRenderer(string interactor) {
        if (photonView.IsMine) {
            if (interactor == "RightHand Controller") {
                rightHand.transform.localScale = Vector3.zero;
            } else {
                leftHand.transform.localScale = Vector3.zero;
            }
        }
    }

    ///<summary>Enables the hand renderer, used when letting go of an object</summary>
    public void enableHandRenderer(string interactor) {
        if (photonView.IsMine) {
            if (interactor == "RightHand Controller") {
                rightHand.transform.localScale = Vector3.one;
            } else {
                leftHand.transform.localScale = Vector3.one;
            }
        }
    }

    ///<summary>Updates the hand animations based on the grip input strength</summary>
    private void updateHandAnimation(InputDevice handDevice, string parameter) {
        if (handDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue)) {
            //Debug.Log(handDevice.name + "'s grip value: " + gripValue);
            if (gripValue >= 1.0f) {
                handAnimator.SetFloat(parameter, 0.9999f);
            } else {
                handAnimator.SetFloat(parameter, gripValue);
            }
        } else {
            handAnimator.SetFloat(parameter, 0.0f);
        }
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer) {
        Debug.Log("OnOwnershipRequest() called");
        Vector3 position = targetView.gameObject.transform.position;
        Quaternion rotation = targetView.gameObject.transform.rotation;

        //TODO: tiles not picked up by the owner don't go back to their original positions if request denied.
        if (targetView.IsMine && !gameManager.isShuffling()) {
            targetView.TransferOwnership(requestingPlayer);
        } else if (gameManager.isShuffling()) {
            Debug.Log(targetView.gameObject.GetComponent<Tile>().tileName + " is being shuffled");
        }
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner) {
        return;
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest) {
        return;
    }

}
