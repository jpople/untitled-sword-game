using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;

    public float horizontalVelocity = 0f;
    public float horizAcceleration;
    int movementDirection;

    public float verticalVelocity = 0f;
    public float jumpForce;
    public float gravity;

    const float MAX_HORIZONTAL_SPEED = 0.08f;

    const float ATTACK_ANIM_DURATION = 0.8f;
    bool isAttacking;

    bool isGrounded = false;
    
    [SerializeField] 
    PolygonCollider2D attackHitbox;
    [SerializeField]
    BoxCollider2D groundedBox;
    [SerializeField]
    AudioSource attackSound;
    [SerializeField]
    AudioClip[] footsteps;

    // Start is called before the first frame update
    void Start()
    {
        animator.ResetTrigger("Attack");
    }

    // Update is called once per frame
    void Update()
    {   
        HandleMove();

        animator.SetFloat("Speed", Mathf.Abs(horizontalVelocity));
        animator.SetFloat("VertSpeed", verticalVelocity);
    }

    void FixedUpdate() {
        GroundedCheck();
        if (!isGrounded) {
            verticalVelocity -= gravity;
        }
        else {
            verticalVelocity = 0;
        }
        transform.position += new Vector3(horizontalVelocity, verticalVelocity, 0);
    }

    private IEnumerator Attack() {
        yield return new WaitForSeconds(0.8f);
        isAttacking = false;
    }

    void Bump() {
        horizontalVelocity = MAX_HORIZONTAL_SPEED * transform.localScale.x;
    }

    void CheckHitbox() {
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D noFilter = new ContactFilter2D();
        int allHitEntities = attackHitbox.OverlapCollider(noFilter.NoFilter(), results);
        if (allHitEntities != 0) {
            foreach (Collider2D c in results) {
                if (c.gameObject.name != "Player")
                {
                    Damageable enemy = c.gameObject.GetComponent<Damageable>();
                    if (enemy != null) {
                        enemy.GetHit();
                    }
                }
            }
        }
    }

    #region ActionHandling

    public void HandleMove() {
        if (movementDirection == 1 && !isAttacking) {
            horizontalVelocity = Mathf.Clamp(horizontalVelocity + horizAcceleration, 0, MAX_HORIZONTAL_SPEED);
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (movementDirection == -1 && !isAttacking) {
            horizontalVelocity = Mathf.Clamp(horizontalVelocity - horizAcceleration, -MAX_HORIZONTAL_SPEED, 0);
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else {
            if (horizontalVelocity < 0) {
                horizontalVelocity = Mathf.Clamp(horizontalVelocity + horizAcceleration, -MAX_HORIZONTAL_SPEED, 0);
            }
            else if (horizontalVelocity > 0) {
                horizontalVelocity = Mathf.Clamp(horizontalVelocity - horizAcceleration, 0, MAX_HORIZONTAL_SPEED);
            }
        }
    }

    public void HandleAttack() {
        animator.SetTrigger("Attack");
        isAttacking = true;
        StartCoroutine(Attack());
    }

    public void HandleJump() {
        if(isGrounded) {
            verticalVelocity = jumpForce;
        }
    }
    
    #endregion

    #region InputHandling

    public void OnMoveInput(InputAction.CallbackContext c) {
        float movementX = c.ReadValue<Vector2>().x;
        Debug.Log($"moving x = {movementX}");
        if (movementX > 0.1) {
            movementDirection = 1;
        }
        else if (movementX < -0.1) {
            movementDirection = -1;
        }
        else {
            movementDirection = 0;
        }
    }

    public void OnJumpInput(InputAction.CallbackContext c) {
        if (c.started) {
            HandleJump();
        }
    }

    public void OnAttackInput(InputAction.CallbackContext c) {
        if (c.started) {
            HandleAttack();
        }
    }

    #endregion

    void PlayAttackSound() {
        attackSound.Play();
    }

    void PlayFootstepSound() {
        if (isGrounded) attackSound.PlayOneShot(footsteps[Random.Range(0, 3)]);
    }

    void GroundedCheck() {
        bool startedGrounded = isGrounded;
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D noFilter = new ContactFilter2D();
        int hitGround = groundedBox.OverlapCollider(noFilter.NoFilter(), results);
        isGrounded = hitGround > 1 && verticalVelocity <= 0;
        if (!startedGrounded) {
            PlayFootstepSound();
        }
    }
}
