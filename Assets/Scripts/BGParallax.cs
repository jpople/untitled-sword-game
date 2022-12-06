using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGParallax : MonoBehaviour
{
    [SerializeField] float parallaxFactor;
    [SerializeField] Camera cam;
    float startPositionX;

    void Start()
    {
        startPositionX = transform.position.x;
    }

    void FixedUpdate()
    {
        float dist = (cam.transform.position.x * parallaxFactor);

        transform.position = new Vector3(startPositionX + dist, transform.position.y, transform.position.z);
    }
}
