using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;

    public float horizontalVelocity = 0f;
    public float horizAcceleration;

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
        animator.ResetTrigger("Jump");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("d") && !isAttacking) {
            horizontalVelocity = Mathf.Clamp(horizontalVelocity + horizAcceleration, 0, MAX_HORIZONTAL_SPEED);
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (Input.GetKey("a") && !isAttacking) {
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

        if(Input.GetKeyDown("f")) {
            animator.SetTrigger("Attack");
            isAttacking = true;
            StartCoroutine(Attack());
        }

        if(isGrounded && Input.GetKeyDown("space")) {
            Debug.Log("jumping...");
            verticalVelocity = jumpForce;
        }

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
        horizontalVelocity = MAX_HORIZONTAL_SPEED * transform.localScale.x / 2;
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

    void PlayAttackSound() {
        attackSound.Play();
    }

    void PlayFootstepSound() {
        if (isGrounded) attackSound.PlayOneShot(footsteps[Random.Range(0, 3)]);
    }

    void GroundedCheck() {
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D noFilter = new ContactFilter2D();
        int hitGround = groundedBox.OverlapCollider(noFilter.NoFilter(), results);
        isGrounded = hitGround > 1 && verticalVelocity <= 0;
    }
}
