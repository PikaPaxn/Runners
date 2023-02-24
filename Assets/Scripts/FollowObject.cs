using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    Transform _target;
    float deltaZ;

    public void SetTarget(Transform target) {
        deltaZ = transform.position.z - target.position.z;
        _target = target;
    }

    // Update is called once per frame
    void Update()
    {
        if (_target != null) {
            transform.position = new Vector3(_target.position.x, _target.position.y, _target.position.z + deltaZ);
        }
    }
}
