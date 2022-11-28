using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField]
    AudioSource hitSound;

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log($"hit! other collider: {other.gameObject.name}");
        if (other.gameObject.name != "Player") {
            hitSound.Play();
        }
    }
}
