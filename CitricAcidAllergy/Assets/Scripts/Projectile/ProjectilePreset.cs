using System;
using UnityEngine;

[Serializable]
public class ProjectilePreset
{
    // -1 means inf penetration
    protected int penetration = 1;
    protected float speed = 5.0f;
    protected float damage = 1.0f;

    public readonly int defaultPenetration = 1;
    public readonly float defaultSpeed = 5.0f;
    public readonly float defaultDamage = 1.0f;

    public float sizeModifier = 0.1f;

    public virtual int Penetration { get => penetration; set => penetration = value; }
    public virtual float Speed { get => speed; set => speed = value; }
    public virtual float Damage { get => damage; set => damage = value; }

    public ProjectilePreset(int penetration = 1, float speed = 5.0f, float damage = 1.0f)
    {
        Intialize(penetration, speed, damage);
    }

    public void Intialize(int penetration = 1, float speed = 5.0f, float damage = 1.0f)
    {
        this.Penetration = penetration;
        this.Speed = speed;
        this.Damage = damage;
    }
}
