using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Animator animator;

    float horizontalVelocity = 0f;
    float horizAcceleration = 0.01f;
    int movementDirection;

    float verticalVelocity = 0f;
    float jumpForce = 0.15f;
    float gravity = 0.01f;

    const float MAX_HORIZONTAL_SPEED = 0.08f;

    const float ATTACK_ANIM_DURATION = 0.8f;
    bool isAttacking;

    [SerializeField] AttackData firstAttack;

    bool isGrounded = false;
    
    [SerializeField] PolygonCollider2D attackHitbox;
    [SerializeField] BoxCollider2D groundedBox;
    [SerializeField] AudioSource attackSound;
    [SerializeField] AudioClip[] footsteps;

    Combatant executionTarget;
    Vector3 EXECUTION_OFFSET = new Vector3(0.5f, 0, 0);

    public UnityEvent AttackWindup; 

    // Start is called before the first frame update
    void Start()
    {
        if (AttackWindup == null) {
            AttackWindup = new UnityEvent();
        }
    }

    // Update is called once per frame
    void Update()
    {   
        ExecutionTargetCheck();
    }

    void FixedUpdate() {
        GroundedCheck();
        HandleAcceleration();
        transform.position += new Vector3(horizontalVelocity, verticalVelocity, 0);
        
        animator.SetFloat("Speed", Mathf.Abs(horizontalVelocity));
        animator.SetFloat("VertSpeed", verticalVelocity);
    }

    private IEnumerator Attack() {
        yield return new WaitForSeconds(0.8f);
        isAttacking = false;
    }

    void Bump() {
        horizontalVelocity = MAX_HORIZONTAL_SPEED * transform.localScale.x * 0.5f;
    }

    #region GameLogic

    void CheckHitbox() {
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D noFilter = new ContactFilter2D();
        int allHitEntities = attackHitbox.OverlapCollider(noFilter.NoFilter(), results);
        if (allHitEntities != 0) {
            foreach (Collider2D c in results) {
                if (c.gameObject.name != "Player")
                {
                    Combatant enemy = c.gameObject.GetComponent<Combatant>();
                    if (enemy != null) {
                        enemy.HandleReceiveAttack(firstAttack);
                    }
                }
            }
        }
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

    void ExecutionTargetCheck() {
        LayerMask mask = LayerMask.GetMask("Enemy");
        RaycastHit2D h = Physics2D.Raycast(transform.position, new Vector2(transform.localScale.x, 0), 1f, mask);
        if (h.collider != null) {
            Combatant frontEnemy = h.collider.gameObject.GetComponent<Combatant>();
            if (frontEnemy.currentStatus == Combatant.Status.BROKEN) {
                if (executionTarget == null) {
                    executionTarget = frontEnemy;
                    frontEnemy.targetedForExecution = true;
                }
                else if (executionTarget != frontEnemy) {
                    executionTarget.targetedForExecution = false;
                    frontEnemy.targetedForExecution = true;
                    executionTarget = frontEnemy;
                }
            }
        }
        else {
            if (executionTarget != null) {
                executionTarget.targetedForExecution = false;
                executionTarget = null;
            }
        }
    }

    #endregion

    #region ActionHandling

    public void HandleAcceleration() {
        // surely this can be made nicer somehow
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

        if (!isGrounded) {
            verticalVelocity -= gravity;
        }
        else {
            verticalVelocity = 0;
        }
    }

    public void HandleAttack() {
        if (isGrounded && !isAttacking) {
            if (executionTarget != null) {
                HandleExecute();
            }
            else {
                animator.CrossFade("Player_Attack", 0f);
                isAttacking = true;
                AttackWindup.Invoke();
                StartCoroutine(Attack());
            }
        }
    }

    public void HandleExecute() {
        transform.position = executionTarget.gameObject.transform.position - (EXECUTION_OFFSET * transform.localScale.x);
        animator.CrossFade("Player_Execute", 0.0f);
    }

    public void HandleJump() {
        if(isGrounded) {
            verticalVelocity = jumpForce;
        }
    }

    public void HandleGetParried() {
        Debug.Log("attack was parried!");
        animator.CrossFade("Player_Get_Parried", 0.0f);
    }
    
    #endregion

    #region InputHandling

    public void OnMoveInput(InputAction.CallbackContext c) {
        float movementX = c.ReadValue<Vector2>().x;
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

    void PlayExecuteSound() {
        attackSound.Play();
    }

    void ExecuteTarget() {
        executionTarget.HandleReceiveExecution();
        executionTarget = null;
    }

    void PlayFootstepSound() {
        if (isGrounded) attackSound.PlayOneShot(footsteps[Random.Range(0, 3)]);
    }
}
