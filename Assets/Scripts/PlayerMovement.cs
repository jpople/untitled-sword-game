using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public class PlayerMovement : PhysicsObject
{
    const float JUMP_FORCE = 0.15f;
    const float ATTACK_ANIM_DURATION = 0.8f;

    bool isAttacking;
    bool isBlocking;
    Vector2 lastMovementInput;

    [SerializeField] AttackData firstAttack;
    [SerializeField] PolygonCollider2D attackHitbox;
    [SerializeField] AudioSource attackSound;
    [SerializeField] Animator animator;

    Combatant executionTarget;
    float EXECUTION_X_OFFSET = 0.5f;

    public UnityEvent AttackWindup;
    public TextMeshProUGUI debugText;

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
        HandleMove();
        ExecutionTargetCheck();
        animator.SetFloat("Speed", Mathf.Abs(horizontalVelocity));
        animator.SetFloat("VertSpeed", verticalVelocity);
    }

    private IEnumerator Attack() {
        yield return new WaitForSeconds(0.8f);
        isAttacking = false;
        
    }

    void Bump() {
        ApplyForce(new Vector3(MAX_HORIZONTAL_SPEED * transform.localScale.x, 0, 0));
    }

    #region GameLogic

    void CheckHitbox() {
        // update this to use LayerMasks now that we know what those are
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D noFilter = new ContactFilter2D();
        int allHitEntities = attackHitbox.OverlapCollider(noFilter.NoFilter(), results);
        if (allHitEntities != 0) {
            foreach (Collider2D c in results) {
                if (c.gameObject.name != "Player")
                {
                    Combatant enemy = c.gameObject.GetComponent<Combatant>();
                    if (enemy != null) {
                        if (enemy.currentStatus == Combatant.Status.PARRYING) {
                            HandleGetParried(firstAttack);
                        }
                        // this is pretty ugly, huh
                        if (transform.localScale.x == -1) {
                            firstAttack.force.x = -firstAttack.force.x;
                            enemy.HandleReceiveAttack(firstAttack);
                            firstAttack.force.x = -firstAttack.force.x;
                        }
                        else {
                            enemy.HandleReceiveAttack(firstAttack);
                        }
                    }
                }
            }
        }
    }

    void ExecutionTargetCheck() {
        LayerMask mask = LayerMask.GetMask("Enemy");
        Vector3 bottom = new Vector3(transform.position.x, GetComponent<BoxCollider2D>().bounds.min.y, transform.position.z);
        Vector3 mid = transform.position; 
        RaycastHit2D h = Physics2D.Raycast(bottom, new Vector2(transform.localScale.x, 0), 1f, mask);
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

    public void HandleAttack() {
        movementDirection = 0;
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
        float destinationX = executionTarget.gameObject.transform.position.x - (EXECUTION_X_OFFSET * transform.localScale.x);
        transform.position = new Vector3(destinationX, transform.position.y, transform.position.z);
        animator.CrossFade("Player_Execute", 0.0f);
    }

    public void HandleJump() {
        if(isGrounded) {
            ApplyForce(new Vector3(0, 0.15f, 0));
        }
    }

    public void HandleGetParried(AttackData attack) {
        // at some point, add this this so player is forced to remain in isAttacking state
        // StopCoroutine(Attack());
        animator.CrossFade("Player_Get_Parried", 0.0f);
        GetComponent<Combatant>().onGetParried.Invoke(attack);
    }

    public void HandleMove() {
        if(!isAttacking && !isBlocking) {
            if (lastMovementInput.x > 0.1) {
                movementDirection = 1;
            }
            else if (lastMovementInput.x < -0.1) {
                movementDirection = -1;
            }
            else {
                movementDirection = 0;
            }
        }
    }

    public void HandleBlock() {
        animator.CrossFade("Player_Block", 0.0f);
        isBlocking = true;
    }

    public void HandleLeaveBlock() {
        animator.CrossFade("Player_Idle", 0f);
        isBlocking = false;
    }
    
    #endregion

    #region InputHandling

    public void OnMoveInput(InputAction.CallbackContext c) {
        // does this function only get called when the input *changes* or what?

        lastMovementInput = c.ReadValue<Vector2>();
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

    public void OnBlockInput(InputAction.CallbackContext c) {
        if (c.started) {
            HandleBlock();
        }
        if (c.canceled) {
            HandleLeaveBlock();
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
}
