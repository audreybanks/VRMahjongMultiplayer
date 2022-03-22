using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using Photon.Pun;
using Photon.Realtime;
using Wolf3D.ReadyPlayerMe.AvatarSDK;

public class NetworkPlayer : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks, IPunInstantiateMagicCallback {

    public Vector3 headBodyOffset;
    private Transform head;
    private Transform leftHand;
    private Transform rightHand;

    private Transform headDevice;
    private Transform leftHandDevice;
    private Transform rightHandDevice;

    public string avatarURL;
    private Animator handAnimator;
    private GameObject avatar;
    private GameObject handMeshes;

    private MahjongGameManager gameManager;

    [SerializeField] private MapTransforms headMapping;
    [SerializeField] private MapTransforms leftHandMapping;
    [SerializeField] private MapTransforms rightHandMapping;


    //<summary>Class to map the network Transform and the device Transform</summary>
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
        XROrigin rig = FindObjectOfType<XROrigin>();
        Transform[] avatarObjects = GetComponentsInChildren<Transform>();
        headDevice = rig.GetComponentInChildren<Camera>().gameObject.transform;
        ActionBasedController[] hands = rig.GetComponentsInChildren<ActionBasedController>();
        if (hands[0].gameObject.name == "LeftHand Controller") {
            leftHandDevice = hands[0].gameObject.transform;
            rightHandDevice = hands[1].gameObject.transform;
        } else {
            leftHandDevice = hands[0].gameObject.transform;
            rightHandDevice = hands[1].gameObject.transform;
        }
        gameManager = FindObjectOfType<MahjongGameManager>();
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

            //TODO: Update animation here, add PhotonAnimatorView
            updateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), "RightGrip");
            updateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), "LeftGrip");
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        loadAvatar();
    }

    //TODO: Move avatar loading to SpawnPlayer
    //<summary>Loads the avatar from the provided URL</sumamry>
    private void loadAvatar() {
        if (photonView.IsMine) {
            Debug.Log("Avatar loading...");
            AvatarLoader avatarLoader = new AvatarLoader();
            avatarLoader.LoadAvatar(avatarURL, OnAvatarImported, OnAvatarLoaded);
        }
    }

    private void OnAvatarImported(GameObject avatar) {
        if (photonView.IsMine) {
            Debug.Log("Avatar is imported...");
        }
    }

    private void OnAvatarLoaded(GameObject avatar, AvatarMetaData metaData) {
        if (photonView.IsMine) {
            Debug.Log("Avatar loaded.");
            Transform[] avatarComponents = avatar.GetComponentsInChildren<Transform>();
        
            foreach (Transform component in avatarComponents) {

                if (component.gameObject.name.EndsWith("_EyeLeft") || component.gameObject.name.EndsWith("_EyeRight") ||
                component.gameObject.name.EndsWith("_Glasses") || component.gameObject.name.EndsWith("_Hair") ||
                component.gameObject.name.EndsWith("_Head") || component.gameObject.name.EndsWith("_Teeth") || 
                component.gameObject.name.EndsWith("_Facewear") || component.gameObject.name.EndsWith("_Shirt") || 
                component.gameObject.name.EndsWith("_Headwear")) {
                    component.gameObject.layer = LayerMask.NameToLayer("PlayerHead");
                }

                if (component.gameObject.name.EndsWith("_Hands")) {
                    handMeshes = component.gameObject;
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

            //TODO: remove when making setAvatar
            avatar.GetComponent<Transform>().parent = gameObject.transform;
            
            this.avatar = avatar;
        }
    }
    

    //<summary>Disables the hand renderer, used when grabbing an object</summary>
    public void disableHandRenderer(Transform interactor) {
        if (photonView.IsMine) {
            if (interactor.name == "RightHand Controller") {
                rightHand.transform.localScale = Vector3.zero;
            } else {
                leftHand.transform.localScale = Vector3.zero;
            }
        }
    }

    //<summary>Enables the hand renderer, used when letting go of an object</summary>
    public void enableHandRenderer(Transform interactor) {
        if (photonView.IsMine) {
            if (interactor.name == "RightHand Controller") {
                rightHand.transform.localScale = Vector3.one;
            } else {
                leftHand.transform.localScale = Vector3.one;
            }
        }
    }

    //<summary>Updates the hand animations based on the grip input strength</summary>
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
