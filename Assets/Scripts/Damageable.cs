using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class Damageable : MonoBehaviour
{
    bool isInvulnerable;

    float currentPosture;
    float maxPosture = 100;
    float basePostureRecovery = 0.01f;
    [SerializeField] StatusBar postureBar;

    float timeSinceEngagement;
    float recoveryDelay = 1.0f;
    float recoveryModifier = 1.0f; // recovery slows when HP is low
    bool recoveringPosture;

    float postureBreakDuration = 4f;
    float timeLeftBroken = 0f;

    float maxHP = 100;
    float currentHP;
    [SerializeField] StatusBar hpBar;

    AudioSource source;
    [SerializeField] AudioClip hitSoundDefault;
    [SerializeField] AudioClip hitSoundBlocked;
    [SerializeField] AudioClip hitSoundParried;
    [SerializeField] AudioClip postureBreakSound;

    [SerializeField] Animator anim;

    [SerializeField] ExecutionMark executionMark;
    public bool targetedForExecution;

    PlayerMovement playerInLOS;

    // [SerializeField] TextMeshProUGUI debugText;

    public enum State {IDLE, BLOCKING, PARRYING, BROKEN}
    public State behavior;

    public UnityEvent parryAttack;

    private void Awake() {
        source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SetHP(maxHP);
        SetPosture(maxPosture);
        behavior = State.IDLE;
        if (parryAttack == null) {
            parryAttack = new UnityEvent();
        }
    }

    private void Update() {
        CheckLOSForPlayer();
        if (recoveringPosture) {
            recoveryModifier = 1 - (currentHP / (maxHP * 2)); // reexamine this
            SetPosture(currentPosture += (basePostureRecovery * recoveryModifier));
            if (currentPosture > maxPosture) {
                recoveringPosture = false;
                SetPosture(maxPosture);
            }
        }
        else if (currentPosture < maxPosture) {
            timeSinceEngagement += Time.deltaTime;
            recoveringPosture = timeSinceEngagement > recoveryDelay;
        }

        if (behavior == State.BROKEN) {
            timeLeftBroken -= Time.deltaTime;
            if (timeLeftBroken < 0) {
                UnbreakPosture();
                timeLeftBroken = 0;
            }
        }

        executionMark.targeted = targetedForExecution;
    }

    void CheckLOSForPlayer() {
        LayerMask mask = LayerMask.GetMask("Player");
        RaycastHit2D h = Physics2D.Raycast(transform.position, new Vector2(-1, 0), 1f, mask);
        if (h.collider == null) {
            ForgetPlayer();
        }
        else {
            FindPlayer(h.collider.gameObject.GetComponent<PlayerMovement>());
        }
    }

    void FindPlayer(PlayerMovement player) {
        if (playerInLOS == null) {
            playerInLOS = player;
            playerInLOS.AttackWindup.AddListener(ReactToAttack);
        }
    }

    void ForgetPlayer() {
        if (playerInLOS != null) {
            playerInLOS.AttackWindup.RemoveListener(ReactToAttack);
            playerInLOS = null;
        }
    }

    void ReactToAttack() {
        if (behavior == State.IDLE) {
            StartCoroutine(reactiveBlock());
        }
    }

    IEnumerator reactiveBlock() {
        yield return new WaitForSeconds(0.35f);
        anim.CrossFade("Dummy_Block", 0f);
    }

    void ShieldUp() {
        behavior = State.PARRYING;
    }

    void ShieldDown() {
        behavior = State.IDLE;
    }

    public void GetHit() {
        if(currentHP <= 0) {
            BreakPosture();
        }
        recoveringPosture = false;
        timeSinceEngagement = 0f;
        switch(behavior) {
            case State.IDLE:
                source.PlayOneShot(hitSoundDefault);
                SetHP(currentHP - 30);
                SetPosture(currentPosture - 20);
                anim.CrossFade("Dummy_Hit", 0.0f);
                break;
            case State.BLOCKING:
                source.PlayOneShot(hitSoundBlocked);
                SetPosture(currentPosture - 20);
                break;
            case State.PARRYING:
                source.PlayOneShot(hitSoundParried);
                parryAttack.Invoke();
                Mathf.Clamp(currentPosture, 1, currentPosture - 10);
                break;
        }
        if(currentPosture < 1) {
            BreakPosture();
        }
    }

    public void GetExecuted() {
        SetPosture(maxPosture);
        SetHP(maxHP);
        source.PlayOneShot(hitSoundDefault);
        anim.CrossFade("Dummy_Death", 0.0f);
        postureBar.gameObject.SetActive(false);
        hpBar.gameObject.SetActive(false);
        GetComponent<BoxCollider2D>().enabled = false;
        UnbreakPosture();
    }

    public void BreakPosture() {
        if(behavior != State.BROKEN) {
            source.PlayOneShot(postureBreakSound);
            behavior = State.BROKEN;
            timeLeftBroken = postureBreakDuration;
            executionMark.gameObject.SetActive(true);
            anim.CrossFade("Dummy_Idle", 0f);
        }
    }

    public void UnbreakPosture() {
        behavior = State.IDLE;
        SetPosture(maxPosture);
        executionMark.gameObject.SetActive(false);
    }

    public void SetPosture(float value) {
        currentPosture = value;
        float scaledPosture = currentPosture / maxPosture;
        postureBar.SetFill(scaledPosture * 100);
    }

    public void SetHP(float value) {
        currentHP = value;
        float scaledHP = currentHP / maxHP;
        hpBar.SetFill(scaledHP * 100);
    }
}
