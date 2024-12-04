using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float BulletSpeed;
    public float Damage;
    public float Penetration;
    public GameObject explosionEffect;
    public bool IsCritical;
    public bool shouldPlayExplosion = true;

    private Rigidbody2D _rigidbody;
    private GameObject _shooter;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip collideSound;

    public void Initialize(GameObject shooter)
    {
        _shooter = shooter;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.AddRelativeForce(new Vector2(BulletSpeed, 0), ForceMode2D.Impulse);
        Destroy(gameObject, 10); // Destroy bullet after 10 seconds automatically

        // Ignore collision with the shooter
        Collider2D bulletCollider = GetComponent<Collider2D>();
        if (_shooter.TryGetComponent<Collider2D>(out var shooterCollider))
        {
            Physics2D.IgnoreCollision(bulletCollider, shooterCollider);
        }

        _rigidbody.isKinematic = Penetration > 0;

        _shooter.TryGetComponent<AudioSource>(out audioSource);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Bullet") && collideSound != null)
            audioSource.PlayOneShot(collideSound, .5f);

        if (Penetration == 0 || collision.gameObject.name.Contains("Enemy"))
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Penetration--;

            if (Penetration == 0)
            {
                _rigidbody.isKinematic = false;
            }
        }

        // That's here to fix a bug where the bullet starts to spin and dont get destroyed
        if (_rigidbody.inertia > 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            Destroy(gameObject);
        }
    }

    public void DestroyWithoutExplosion()
    {
        shouldPlayExplosion = false;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (explosionEffect != null && shouldPlayExplosion)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }
    }
}
