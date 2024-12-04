using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Object/Enemy")]
public class EnemySO : ScriptableObject
{
    public GameObject BulletPrefab;

    public GameObject HealthOrbPrefab;

    public Sprite Sprite;

    public float Height;

    public float Distance;

    public float MaxHealth;

    public float Cooldown;

    public float Damage;

    public AudioClip DeathSound;

    public AudioClip HitSound;

    public AudioClip ShootSound;
}
