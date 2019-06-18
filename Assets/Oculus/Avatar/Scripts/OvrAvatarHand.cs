using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Controls;

public class OvrAvatarHand : MonoBehaviour
{


    void Awake()
    {
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("hello");
        //Debug.Log("Hello I just hit vertex: " + collider.GetComponent<VertexHandleController>().AssociatedVertexID);
    }

    void OnTriggerExit(Collider collider)
    {
    }

    void Update()
    {
    }
}