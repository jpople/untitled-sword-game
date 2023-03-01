using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCBotBehavior : MonoBehaviour {
    PlayerMovement playerInLOS;
    Animator anim;
    Combatant combatant;
    [SerializeField] AnimationClip chargeAnimation;
    
    [SerializeField] AudioClip attackSound;
    [SerializeField] AudioClip chargeSound;
    AudioSource source;


    [SerializeField] AttackData attackInfo;
    [SerializeField] PolygonCollider2D attackHitbox;
    const float START_MIN_INTERVAL = 0.5f;
    const float START_MAX_INTERVAL = 1f;
    const float MIN_ATTACK_INTERVAL = 2;
    const float MAX_ATTACK_INTERVAL = 3;
    float timeUntilAttack;

    private void Start() {
        anim = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
        combatant = GetComponent<Combatant>();
        combatant.onDie.AddListener(HandleDie);
        combatant.onPostureBroken.AddListener(ForgetPlayer);
        timeUntilAttack = AwaUtils.FuzzyRandom(START_MIN_INTERVAL, START_MAX_INTERVAL);
    }

    private void Update() {
        CheckForPlayer();
        if (playerInLOS != null && combatant.currentStatus != Combatant.Status.BROKEN) {
            timeUntilAttack -= Time.deltaTime;
            if (timeUntilAttack <= 0) {
                anim.CrossFade(chargeAnimation.name, 0f);
                timeUntilAttack = AwaUtils.FuzzyRandom(MIN_ATTACK_INTERVAL, MAX_ATTACK_INTERVAL);
                source.PlayOneShot(chargeSound);
            }
        }
    }

    void CheckForPlayer() { 
        LayerMask mask = LayerMask.GetMask("Player");
        RaycastHit2D h = Physics2D.Raycast(transform.position, Vector2.left, 1.5f, mask);
        Debug.DrawRay(transform.position, Vector2.left);
        if (playerInLOS == null && h.collider != null) {
            FindPlayer(h.collider.gameObject.GetComponent<PlayerMovement>());
        }
        else if (playerInLOS != null && h.collider == null) {
            ForgetPlayer();
        }
    }

    void FindPlayer(PlayerMovement p) {
        playerInLOS = p;
    }

    void ForgetPlayer() {
        playerInLOS = null;
    }

    void HandleDie() {
        enabled = false;
    }

    void CheckHitbox() {
        // update this to use LayerMasks now that we know what those are
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Player"));
        int allHitEntities = attackHitbox.OverlapCollider(filter, results);
        if (allHitEntities != 0) {
            Combatant player = results[0].gameObject.GetComponent<Combatant>();
            if (player.currentStatus == Combatant.Status.PARRYING) {
                GetComponent<Combatant>().onGetParried.Invoke(attackInfo);
            }
            if (transform.localScale.x == -1) {
                attackInfo.force.x = -attackInfo.force.x;
                player.HandleReceiveAttack(attackInfo);
                attackInfo.force.x = -attackInfo.force.x;
            }
            else {
                player.HandleReceiveAttack(attackInfo);
            }
        }
        source.PlayOneShot(attackSound);
    }
}