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
    // also the value force represents is technically momentum, don't @ me
}

public class AttackImpactEvent : UnityEvent<AttackData> {
    // nothing
}