using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Combatant : MonoBehaviour {
    // HP damage handling
    bool isInvulnerable;
    [SerializeField] StatusBar hpBar;
    float maxHP = 100;
    float currentHP;
    // posture damage handling
    [SerializeField] StatusBar postureBar;
    float maxPosture = 100;
    float currentPosture;   
    float postureBreakDuration = 4f;
    float timeLeftBroken = 0f; // this and "timeSinceEngagement" are weirdly asymmetrical
    // posture recovery handling
    float basePostureRecovery = 1f;
    float postureRecoveryModifier = 1f; // this value slows down recovery as HP decreases
    bool isRecoveringPosture;
    float postureRecoveryDelay = 1f;
    float timeSinceEngagement;
    // execution marker
    [SerializeField] ExecutionMark executionMark;
    public bool targetedForExecution;
    // events
    public AttackImpactEvent onGetHit;
    public AttackImpactEvent onGetParried;
    public AttackImpactEvent onBlock;
    public AttackImpactEvent onParry;
    public UnityEvent onPostureBroken;
    public UnityEvent onPostureUnbroken;
    public UnityEvent onGetExecuted;
    public UnityEvent onDie;
    // status
    public enum Status { NONE, BLOCKING, PARRYING, BROKEN }
    public Status currentStatus = Status.NONE;

    public void ChangeStatus(Status newStatus) {
        currentStatus = newStatus;
    }

    #region MonoBehaviourStuff

    void Awake() {
        if (onGetHit == null) {
            onGetHit = new AttackImpactEvent();
        }
        if (onGetParried == null) {
            onGetParried = new AttackImpactEvent();
        }
        if (onBlock == null) {
            onBlock = new AttackImpactEvent();
        }
        if (onParry == null) {
            onParry = new AttackImpactEvent();
        }
        if (onPostureBroken == null) {
            onPostureBroken = new UnityEvent();
        }
        if (onPostureUnbroken == null) {
            onPostureUnbroken = new UnityEvent();
        }
        if (onGetExecuted == null) {
            onGetExecuted = new UnityEvent();
        }
        if (onDie == null) {
            onDie = new UnityEvent();
        }
    }

    void Start() {
        SetHP(maxHP);
        SetPosture(maxPosture);

        onGetHit.AddListener(HandleGetHit);
        onGetParried.AddListener(HandleGetParried);
        onBlock.AddListener(HandleBlock);
        onParry.AddListener(HandleParry);
    }

    void Update() {
        // handle posture recovery
        if (isRecoveringPosture) {
            postureRecoveryModifier = 1 - (currentHP / (maxHP * 2));
            SetPosture(currentPosture += (basePostureRecovery * postureRecoveryModifier * Time.deltaTime));
            if (currentPosture >= maxPosture) {
                isRecoveringPosture = false;
                SetPosture(maxPosture);
            }
        }
        else if (currentPosture < maxPosture) {
            timeSinceEngagement += Time.deltaTime;
            isRecoveringPosture = timeSinceEngagement > postureRecoveryDelay;
        }

        // handle recovery from posture break (or not)
        if (currentStatus == Status.BROKEN) {
            timeLeftBroken -= Time.deltaTime;
            if (timeLeftBroken < 0) {
                if (currentHP < 0) {
                    Die();
                }
                else {
                    HandleUnbreakPosture();
                    onPostureUnbroken.Invoke();
                    timeLeftBroken = 0;
                }
            }
        }

        // toggle execution mark
        executionMark.targeted = targetedForExecution;
    }

    #endregion

    #region GettingHit

    public void HandleReceiveAttack(AttackData attack) {
        switch(currentStatus) {
            case Status.NONE:
                onGetHit.Invoke(attack);
                break;
            case Status.BLOCKING:
                onBlock.Invoke(attack);
                break;
            case Status.PARRYING:
                onParry.Invoke(attack);
                break;
        }
    }

    public void HandleReceiveExecution() {
        Die();
        Debug.Log("executed!");
    }

    void HandleGetHit(AttackData attack) {
        SetHP(currentHP - attack.hpDamageBase);
        SetPosture(currentPosture - attack.postureDamageBase);
    }

    void HandleGetParried(AttackData attack) {
        SetPosture(currentPosture - attack.postureDamageToAttacker);
    }

    void HandleBlock(AttackData attack) {
        SetHP(currentHP - attack.hpDamageBlocked);
        SetPosture(currentPosture - attack.postureDamageBlocked);
    }

    void HandleParry(AttackData attack) {
        SetHP(currentHP - attack.hpDamageParried);
        SetPosture(currentPosture - attack.postureDamageParried);
    }

    #endregion

    #region PostureBreaking

    void HandleBreakPosture() {
        currentStatus = Status.BROKEN;
        timeLeftBroken = postureBreakDuration;
        onPostureBroken.Invoke();
        executionMark.gameObject.SetActive(true);
    }

    void HandleUnbreakPosture() {
        currentStatus = Status.NONE;
        onPostureUnbroken.Invoke();
        executionMark.gameObject.SetActive(false);
    }

    void Die() {
        hpBar.gameObject.SetActive(false);
        postureBar.gameObject.SetActive(false);
        executionMark.gameObject.SetActive(false);
        GetComponent<BoxCollider2D>().enabled = false;
    }

    #endregion

    #region Bars

    void SetHP(float value) {
        currentHP = value;
        float scaledHP = currentHP / maxHP;
        hpBar.SetFill(scaledHP * 100);
        if (currentHP < 0) {
            HandleBreakPosture();
        }
    }

    void SetPosture(float value) {
        currentPosture = value;
        float scaledPosture = currentPosture / maxPosture;
        postureBar.SetFill(scaledPosture * 100);
        if (currentPosture < 1) {
            HandleBreakPosture();
        }
    }

    #endregion
}