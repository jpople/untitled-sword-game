using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyBehavior : MonoBehaviour
{
    PlayerMovement playerInLOS;
    Animator anim;
    [SerializeField] AnimationClip blockAnimation;

    private void Start() {
        // is it okay to directly access animator here?
        anim = GetComponent<Animator>();
    }

    private void Update() {
        CheckForPlayer();
    }

    void CheckForPlayer() { 
        LayerMask mask = LayerMask.GetMask("Player");
        RaycastHit2D h = Physics2D.Raycast(transform.position, Vector2.left, 1f, mask);
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
        p.AttackWindup.AddListener(ReactToAttack);
    }

    void ForgetPlayer() {
        playerInLOS.AttackWindup.RemoveListener(ReactToAttack);
        playerInLOS = null;

    }

    void ReactToAttack() {
        StartCoroutine(BlockIncomingAttack());
    }

    private IEnumerator BlockIncomingAttack() {
        yield return new WaitForSeconds(0.3f);
        anim.CrossFade(blockAnimation.name, 0f);
    }
}
