using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CombatantFX : MonoBehaviour
{
    [SerializeField] AudioClip hitSoundDefault;
    [SerializeField] AudioClip hitSoundBlocked;
    [SerializeField] AudioClip hitSoundParried;
    [SerializeField] AudioClip postureBreakSound;
    [SerializeField] AudioClip getExecutedSound;

    [SerializeField] AudioClip footstepSound;

    [SerializeField] AnimationClip idleAnimation;
    [SerializeField] AnimationClip hitAnimation;
    [SerializeField] AnimationClip blockAnimation;
    [SerializeField] AnimationClip deathAnimation;
    [SerializeField] AnimationClip postureBreakAnimation;

    [SerializeField] ParticleSystem ps;

    [SerializeField] GameObject burst;

    AudioSource source;
    Animator anim;
    Combatant character;

    void Awake() {
        source = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        character = GetComponent<Combatant>();
    }

    void Start() {
        character.onGetHit.AddListener(HandleGetHit);
        character.onBlock.AddListener(HandleBlock);
        character.onParry.AddListener(HandleParry);
        character.onDie.AddListener(HandleDie);
        character.onPostureBroken.AddListener(HandleBreakPosture);
        GetComponent<PhysicsObject>().onLand.AddListener(PlayFootstepSound);
    }

    void HandleGetHit(AttackData a) {
        source.PlayOneShot(hitSoundDefault);
        anim.CrossFade(hitAnimation.name, 0);
    }

    void HandleBlock(AttackData a) {
        source.PlayOneShot(hitSoundBlocked);
    }
    
    void HandleParry(AttackData a) {
        source.PlayOneShot(hitSoundParried);
    }

    void HandleDie() {
        source.PlayOneShot(getExecutedSound);
        anim.CrossFade(deathAnimation.name, 0f);
    }

    void HandleBreakPosture() {
        source.PlayOneShot(postureBreakSound);
        anim.CrossFade(postureBreakAnimation.name, 0f);
    }

    void PlayFootstepSound() {
        if (footstepSound != null) {
            source.PlayOneShot(footstepSound);
        }
    }
}
