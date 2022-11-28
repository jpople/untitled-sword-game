using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] Transform player;
    const float LOOK_OFFSET_X = 0.7f;
    const float SMOOTH_FACTOR = 3f;

    private void FixedUpdate() {
        Follow();
    }

    void Follow() {
        Vector3 targetPosition = player.position + new Vector3(LOOK_OFFSET_X * player.localScale.x, 0, -10);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, SMOOTH_FACTOR * Time.fixedDeltaTime);
        transform.position = smoothedPosition;
    }

}
