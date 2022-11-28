using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionMark : MonoBehaviour
{
    [SerializeField] Animator anim;
    public bool targeted = false;

    void Update() {
        anim.SetBool("Activated", targeted);
    }
}
