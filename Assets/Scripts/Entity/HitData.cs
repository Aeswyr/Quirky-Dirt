using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HitData", menuName = "Quirky-Dirt/HitData", order = 0)]
public class HitData : ScriptableObject
{
    [SerializeField] int baseDamage;
    [SerializeField] float physicalCoeff;
    [SerializeField] float magicalCoeff;
    [SerializeField] DamageType damageType;
    [SerializeField] KnockbackStrength knockbackStrength;
    [SerializeField] KnockbackRelative knockbackRelative;

    public enum KnockbackRelative {
        ROTATION, SOURCE, ENTITY
    }
    public enum KnockbackStrength {
        NONE, SMALL, MEDIUM, LARGE, MASSIVE
    }

    public int GetDamage(int ATK, int MATK) {
        return (int)(baseDamage + physicalCoeff * ATK + magicalCoeff * MATK);
    }

    public DamageType GetDamageType() {
        return damageType;
    }

    public KnockbackStrength GetKnockback() {
        return knockbackStrength;
    }

    public KnockbackRelative GetKnockbackRelative() {
        return knockbackRelative;
    }
}

public enum DamageType {
    TRUE, PHYSICAL, MAGICAL
}
