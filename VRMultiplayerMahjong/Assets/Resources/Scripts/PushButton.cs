using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PushButton : MonoBehaviour {

    public float threshold = 0.1f;
    public float deadzone = 0.025f;

    private bool isPressed;
    private Vector3 startPosition;
    private ConfigurableJoint configurableJoint;

    public UnityEvent onPressed;
    public UnityEvent onReleased;

    // Start is called before the first frame update
    void Start() {
        startPosition = transform.localPosition;
        configurableJoint = GetComponent<ConfigurableJoint>();
    }

    // Update is called once per frame
    void Update() {
        if (!isPressed && getButtonValue() + threshold >= 1) {
            pressed();
        }

        if (isPressed && getButtonValue() - threshold <= 0) {
            released();
        }
    }

    private float getButtonValue() {
        float value = Vector3.Distance(startPosition, transform.localPosition) / configurableJoint.linearLimit.limit;

        if (Mathf.Abs(value) < deadzone) {
            value = 0;
        }

        return Mathf.Clamp(value, -1f, 1f);
    }



    private void pressed() {
        isPressed = true;
        onPressed.Invoke();
        Debug.Log("Button pressed.");
    }

    private void released() {
        isPressed = false;
        onReleased.Invoke();
        Debug.Log("Button released");
    }
}
