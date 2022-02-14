using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformData {
    private Vector3 _position;
    public Vector3 position {
        get => _position;
        set => _position = value;
    }

    private Quaternion _rotation;
    public Quaternion rotation {
        get => _rotation;
        set => _rotation = value;
    }

    public TransformData(Vector3 position, Quaternion rotation) {
        this.position = position;
        this.rotation = rotation;
    }
}
