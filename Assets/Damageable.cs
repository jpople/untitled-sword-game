using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    AudioSource audio;
    [SerializeField] AudioClip hitSound;

    [SerializeField] Animator anim;

    private void Awake() {
        audio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SetHP(maxHP);
        SetPosture(maxPosture);
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
    }

    public void GetHit() {
        recoveringPosture = false;
        timeSinceEngagement = 0f;
        audio.PlayOneShot(hitSound);
        ChangeHP(-10);
        ChangePosture(-20);
        anim.CrossFade("Dummy_Hit", 0.0f);
    }

    public void GetHitBlocked() {
        timeSinceEngagement = 0f;

    }

    public void GetHitParried() {
        timeSinceEngagement = 0f;
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
