using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float MovementSpeed = 5f;
    public float JumpForce = 10f;
    public float BulletSpeed = 5f;
    public float BulletDamage = 1;
    public int BulletPenetration = 1;
    public float BulletSize = .2f;
    public float ShootCooldown = 1f;
    public float Health;
    public float MaxHealth = 20;
    public float CriticalChance = 0f;
    public float CriticalDamage = 1.5f;
    public float Shield = 1f;
    public float LeechLevel = 0;

    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _wandGameObject;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObject _bulletsParent;
    [SerializeField] private Transform _wandEnd;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TextMeshProUGUI _healthText;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip jumpSound;
    public AudioClip hitSound;

    private bool _canShoot = true;

    void Start()
    {
        Health = MaxHealth;
    }

    void Update()
    {
        Move();
        WandAimAndShoot();

        _healthSlider.maxValue = MaxHealth;
        _healthSlider.value = Health;
        _healthText.text = $"{Health}/{MaxHealth}";
    }

    void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal") * MovementSpeed;
        bool isGrounded = IsGrounded();

        if (isGrounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, JumpForce);

            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }
        else
        {
            _rigidbody.velocity = new Vector2(horizontal, _rigidbody.velocity.y);
        }
    }

    void WandAimAndShoot()
    {
        // Aim script
        Vector2 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        float angle = (Mathf.Atan2(mousePosition.y - _wandGameObject.transform.position.y, mousePosition.x - _wandGameObject.transform.position.x) * Mathf.Rad2Deg);

        _wandGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Shoot Script
        if (Input.GetMouseButton(0) && _canShoot)
        {
            StartCoroutine(ShootingCooldown());

            GameObject bullet = Instantiate(_bulletPrefab, _wandEnd.position, _wandGameObject.transform.rotation, _bulletsParent.transform);
            var bulletScript = bullet.GetComponent<Bullet>();
            bulletScript.BulletSpeed = BulletSpeed;

            bullet.transform.localScale = new Vector3(BulletSize, BulletSize, 0);

            var bulletDamage = BulletDamage;
            if (CriticalChance >= Random.Range(0, 100))
            {
                bulletScript.IsCritical = true;
                bulletDamage *= CriticalDamage;
            }

            bulletScript.Penetration = BulletPenetration;
            bulletScript.Damage = bulletDamage;
            bulletScript.Initialize(gameObject);

            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }
    }

    bool IsGrounded()
    {
        return Physics2D.CircleCast(_groundCheck.position, .03f, Vector2.down, .1f, _groundLayer);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Health -= collision.gameObject.GetComponent<Bullet>().Damage;
            audioSource.PlayOneShot(hitSound, .3f);
        }
    }

    IEnumerator ShootingCooldown()
    {
        _canShoot = false;
        yield return new WaitForSeconds(ShootCooldown);
        _canShoot = true;
    }
}
