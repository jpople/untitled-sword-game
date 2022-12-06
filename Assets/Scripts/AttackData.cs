using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct AttackData {
    public float hpDamageBase;
    public float postureDamageBase;

    public float hpDamageBlocked;
    public float postureDamageBlocked;

    public float hpDamageParried;
    public float postureDamageParried;

    public Combatant attacker;
    public float postureDamageToAttacker; // when attack is parried

    public Vector3 force; // assuming the attacker is facing right (towards positive x)
    // also the value this represents is technically momentum, don't @ me
}
[System.Serializable]
public class AttackImpactEvent : UnityEvent<AttackData> {
    // nothing
}