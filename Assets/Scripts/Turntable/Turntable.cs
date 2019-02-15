using System.Collections;
using System.Collections.Generic;
using Controls;
using UnityEngine;

public class Turntable : MonoBehaviour
{
    public GameObject handlePrefab;
    public Transform childObject;

    public int elementCount = 12;
    public float radius = .5f;
    public float height = 0.5f;

    private void Awake()
    {
        childObject = transform.GetChild(0);
    }

    public float GetMaxYWithBounds()
    {
        return transform.position.y + height + 0.02f;
    }
    public float GetMaxY()
    {
        return transform.position.y + height;
    }

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < elementCount; i++)
        {
            float delta = i * 2 * Mathf.PI / elementCount;
            var go = Instantiate(handlePrefab);
            go.transform.localPosition = new Vector3(Mathf.Sin(delta) * radius, 0, Mathf.Cos(delta) * radius);
            go.transform.localRotation = Quaternion.Euler(0, 180 + i * 360.0f / elementCount, 0);
            go.GetComponent<TurntableController>()._turntable = this;
            go.transform.parent = this.transform;
        }
    }

}
