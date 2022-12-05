using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TargetDummyFX : MonoBehaviour
{
    [SerializeField] AudioClip hitSoundDefault;
    [SerializeField] AudioClip hitSoundBlocked;
    [SerializeField] AudioClip hitSoundParried;

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
    }

    void HandleGetHit(AttackData a) {
        source.PlayOneShot(hitSoundDefault);
        anim.CrossFade("Dummy_Hit", 0);
    }

    void HandleBlock(AttackData a) {
        source.PlayOneShot(hitSoundBlocked);
    }
    
    void HandleParry(AttackData a) {
        source.PlayOneShot(hitSoundBlocked);
    }

    void HandleDie() {
        source.PlayOneShot(hitSoundDefault);
        anim.CrossFade("Dummy_Death", 0);
    }
}
