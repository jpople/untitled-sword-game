using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Damageable : MonoBehaviour
{
    float currentPosture;
    float maxPosture = 100;
    float basePostureRecovery = 0.01f;
    [SerializeField] StatusBar postureBar;

    float timeSinceEngagement;
    float recoveryDelay = 1.0f;
    float recoveryModifier = 1.0f; // recovery slows when HP is low
    bool recoveringPosture;

    float maxHP = 100;
    float currentHP;
    [SerializeField] StatusBar hpBar;

    AudioSource source;
    [SerializeField] AudioClip hitSoundDefault;
    [SerializeField] AudioClip hitSoundBlocked;

    [SerializeField] Animator anim;

    enum State {IDLE, BLOCKING, PARRYING}
    State behavior;

    [SerializeField] bool isBlocking;

    private void Awake() {
        source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SetHP(maxHP);
        SetPosture(maxPosture);
        behavior = State.IDLE;
    }

    private void Update() {
        if (recoveringPosture) {
            recoveryModifier = 1 - (currentHP / (maxHP * 2)); // reexamine this
            ChangePosture(basePostureRecovery * recoveryModifier);
            if (currentPosture > maxPosture) {
                recoveringPosture = false;
                SetPosture(maxPosture);
            }
        }
        else if (currentPosture < maxPosture) {
            timeSinceEngagement += Time.deltaTime;
            recoveringPosture = timeSinceEngagement > recoveryDelay;
        }

        if (isBlocking) {
            behavior = State.BLOCKING;
        }
        else {
            behavior = State.IDLE;
        }
    }

    public void GetHit() {
        recoveringPosture = false;
        timeSinceEngagement = 0f;
        switch(behavior) {
            case State.IDLE:
                source.PlayOneShot(hitSoundDefault);
                ChangeHP(-10);
                ChangePosture(-20);
                anim.CrossFade("Dummy_Hit", 0.0f);
                break;
            case State.BLOCKING:
                source.PlayOneShot(hitSoundBlocked);
                ChangePosture(-20);
                anim.CrossFade("Dummy_Block", 0.0f);
                break;
            default:
                break;
        }
    }

    public void ChangeBehavior(int value) {
        behavior = (State)value;
        Debug.Log($"new behavior: {behavior}");
    }

    // gross

    public void ChangePosture(float change) {
        currentPosture += change;
        float scaledPosture = currentPosture / maxPosture;
        postureBar.SetFill(scaledPosture * 100);
    }

    public void SetPosture(float value) {
        currentPosture = value;
        float scaledPosture = currentPosture / maxPosture;
        postureBar.SetFill(scaledPosture * 100);
    }

    public void ChangeHP(float change) {
        currentHP += change;
        float scaledHP = currentHP / maxHP;
        hpBar.SetFill(scaledHP * 100);
    }

    public void SetHP(float value) {
        currentHP = value;
        float scaledHP = currentHP / maxHP;
        hpBar.SetFill(scaledHP * 100);
    }
}
