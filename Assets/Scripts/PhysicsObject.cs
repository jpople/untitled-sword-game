using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicsObject : MonoBehaviour
{
    [SerializeField] float mass;
    [SerializeField] BoxCollider2D groundedBox;
    bool isGrounded;

    float horizontalVelocity = 0f;
    float verticalVelocity = 0f;
    float horizontalAcceleration = 0.01f;

    int movementInputDirection = 0;

    float gravity = 0.01f;

    const float MAX_HORIZONTAL_SPEED = 0.08f;
    const float MAX_VERTICAL_SPEED = 0.2f;

    void Start() {
        GetComponent<Combatant>().onGetHit.AddListener(HandleGetHit);
    }

    void FixedUpdate() {
        GroundedCheck();
        HandleAcceleration();
        transform.position += new Vector3 (horizontalVelocity, verticalVelocity, 0);
    }

    void GroundedCheck() {
        // bool startedGrounded = isGrounded;
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Terrain"));
        int hitGround = groundedBox.OverlapCollider(filter, results);
        isGrounded = hitGround > 0 && verticalVelocity <= 0;
        // if (!startedGrounded) {
        //     PlayFootstepSound();
        // }
    }


    void HandleAcceleration() {
        // surely, *surely* this can be made better somehow
        if (movementInputDirection == 1) {
            horizontalVelocity = Mathf.Clamp(horizontalVelocity + horizontalAcceleration, 0, MAX_HORIZONTAL_SPEED);
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (movementInputDirection == -1) {
            horizontalVelocity = Mathf.Clamp(horizontalVelocity - horizontalAcceleration, -MAX_HORIZONTAL_SPEED, 0);
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else {
            if (isGrounded) {
                if (horizontalVelocity < 0) {
                    horizontalVelocity = Mathf.Clamp(horizontalVelocity + horizontalAcceleration, -MAX_HORIZONTAL_SPEED, 0);
                }
                else if (horizontalVelocity > 0) {
                    horizontalVelocity = Mathf.Clamp(horizontalVelocity - horizontalAcceleration, 0, MAX_HORIZONTAL_SPEED);
                }
            }
        }

        if (!isGrounded) {
            verticalVelocity -= gravity;
        }
        else {
            verticalVelocity = 0;
        }
    }

    public void HandleGetHit(AttackData attack) {
        ApplyForce(attack.force);
    }

    void ApplyForce(Vector3 value) {
        horizontalVelocity += value.x / mass;
        verticalVelocity += value.y / mass;
    }
}
