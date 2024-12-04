using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemySO EnemySO;
    public GameObject PopUpPrefab;
    public float Health;
    public float Height;
    public float ShootCooldown;
    
    GameObject Player;
    GameObject BulletParent;
    GameObject OrbParent;
    SpriteRenderer SpriteRenderer;

    private float _distanceFromPlayer;
    private float _bulletSpeed = 2;
    private bool _canShoot = false;
    private Color _defaultColor;

    [Header("Audio Settings")]
    AudioSource audioSource;
    public AudioClip criticalDamageSound;

    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        BulletParent = GameObject.FindWithTag("BulletParent");
        OrbParent = GameObject.FindWithTag("OrbParent");
        SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        SpriteRenderer.sprite = EnemySO.Sprite;
        Player.TryGetComponent<AudioSource>(out audioSource);

        Health = EnemySO.MaxHealth;
        Height = Random.Range(EnemySO.Height - .5f, EnemySO.Height + .5f);
        ShootCooldown = EnemySO.Cooldown;
        _defaultColor = new Color(SpriteRenderer.color.r, SpriteRenderer.color.g, SpriteRenderer.color.b, Mathf.Pow(Health / EnemySO.MaxHealth, 0.7f));

        StartCoroutine(ShootingInitialize());
    }

    void Update()
    {
        RotateTowardsPlayer();
        Move();
        TryShoot();
    }

    private void RotateTowardsPlayer()
    {
        float angle = Mathf.Atan2(
            Player.transform.position.y - transform.position.y,
            Player.transform.position.x - transform.position.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
    }

    private void Move()
    {
        _distanceFromPlayer = Vector2.Distance(transform.position, Player.transform.position);

        var targetCompensation = Mathf.Pow((_distanceFromPlayer - Height + 2) * .019f, 3);

        if (targetCompensation <= 0.0001) return; // This avoid enemy flickering

        transform.position = new Vector3(
            GetMoveXPosition(targetCompensation),
            GetMoveYPosition(targetCompensation),
            0
        );
    }

    private float GetMoveXPosition(float compensation)
    {
        var distanceTolerance = 2;

        if (Player.transform.position.x > transform.position.x + distanceTolerance)
        {
            return transform.position.x + compensation;
        }
        else if (Player.transform.position.x < transform.position.x - distanceTolerance)
        {
            return transform.position.x - compensation;
        }

        return transform.position.x;
    }

    private float GetMoveYPosition(float compensation)
    {
        var distanceTolerance = 1;

        if (transform.position.y > Height)
        {
            return transform.position.y - compensation;
        }
        else if (transform.position.y < Height - distanceTolerance)
        {
            return transform.position.y + compensation;
        }
        else
        {
            return transform.position.y;
        }
    }

    private void TryShoot()
    {
        if (_distanceFromPlayer <= EnemySO.Distance && _canShoot)
        {
            StartCoroutine(ShootingCooldown());

            var bulletPosition = new Vector3(transform.position.x, transform.position.y);

            GameObject bullet = Instantiate(EnemySO.BulletPrefab, bulletPosition, Quaternion.Euler(new Vector3(0, 0, transform.rotation.eulerAngles.z - 90)), BulletParent.transform);
            var bulletScript = bullet.GetComponent<Bullet>();
            bulletScript.BulletSpeed = _bulletSpeed;

            var playerShield = Player.GetComponent<PlayerController>().Shield;

            bulletScript.Damage = EnemySO.Damage - playerShield >= 0 ? EnemySO.Damage - playerShield : 0;
            bulletScript.Initialize(gameObject);
            audioSource.PlayOneShot(EnemySO.ShootSound, .2f);
        }
    }

    private IEnumerator UpdateHealth()
    {
        SpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        SpriteRenderer.color = _defaultColor;
    }

    private void Die()
    {
        if (Random.Range(0, 100) <= 10)
        {
            var orbPosition = new Vector3(transform.position.x, transform.position.y + .5f);
            Instantiate(EnemySO.HealthOrbPrefab, orbPosition, Quaternion.Euler(new Vector3(0, 0, 0)), OrbParent.transform);

        }

        audioSource.PlayOneShot(EnemySO.DeathSound);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            var bullet = collision.gameObject.GetComponent<Bullet>();

            if (Health >= bullet.Damage)
            {
                if (bullet.IsCritical == true)
                {
                    audioSource.PlayOneShot(criticalDamageSound);
                }
                else
                {
                    audioSource.PlayOneShot(EnemySO.HitSound);
                }
            }

            Health -= bullet.Damage;
            StartCoroutine(UpdateHealth());

            // PopUp
            GameObject popUp = Instantiate(PopUpPrefab, new Vector3(collision.transform.position.x, collision.transform.position.y + EnemySO.Height, collision.transform.position.z), Quaternion.identity);
            popUp.GetComponentInChildren<TMP_Text>().text = bullet.Damage.ToString();

            if (popUp.TryGetComponent<PopUpDamage>(out var popUpDamage))
            {
                popUpDamage.IsCritical = bullet.IsCritical;
            }

            // Leech
            var playerComponent = Player.GetComponent<PlayerController>(); // Assuming the player script is called "Player"
            if (playerComponent != null && playerComponent.LeechLevel > 0)
            {
                float healAmount = (bullet.Damage * playerComponent.LeechLevel) / 10;
                playerComponent.Health += healAmount; // Adjust according to how player health is managed
                playerComponent.Health = Mathf.Min(playerComponent.Health, playerComponent.MaxHealth); // Prevent overhealing
            }
        }

        if (Health <= 0)
        {
            Die();
        }
    }

    IEnumerator ShootingInitialize()
    {
        yield return new WaitForSeconds(1);
        _canShoot = true;
    }

    IEnumerator ShootingCooldown()
    {
        _canShoot = false;
        yield return new WaitForSeconds(ShootCooldown);
        _canShoot = true;
    }
}
