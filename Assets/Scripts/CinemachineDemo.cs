using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineDemo : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector3 v = transform.position;
        if (Input.GetKey(KeyCode.LeftArrow))
            v.x -= 1.0f;
        if (Input.GetKey(KeyCode.RightArrow))
            v.x += 1.0f;
        if (Input.GetKey(KeyCode.UpArrow))
            v.z -= 1.0f;
        if (Input.GetKey(KeyCode.DownArrow))
            v.z += 1.0f;
        transform.position = v;
    }
}
