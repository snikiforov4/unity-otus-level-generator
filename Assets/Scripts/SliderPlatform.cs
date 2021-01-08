using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderPlatform : MonoBehaviour
{
    public float speed;
    public Transform begin;
    public Transform end;

    SliderJoint2D joint;

    void Start()
    {
        joint = GetComponent<SliderJoint2D>();
    }

    void Update()
    {
        Vector3 pos = transform.position;
        JointMotor2D motor = joint.motor;
        if (pos.x < begin.position.x)
            motor.motorSpeed = speed;
        else if (pos.x > end.position.x)
            motor.motorSpeed = -speed;
        joint.motor = motor;
    }
}
