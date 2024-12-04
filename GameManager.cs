using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] GameObject DeathMenu;

    public int Wave;
    [SerializeField] PlayerController Player;

    GameObject EnemyParent;
    GameObject BulletParent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        EnemyParent = GameObject.FindWithTag("EnemiesParent");
        BulletParent = GameObject.FindWithTag("BulletParent");
    }

    void Update()
    {
        if (Instance == null) return;

        if (Player.Health <= 0)
        {
            HandlePlayerDeath();
            return;
        }

        if (EnemyParent.transform.childCount == 0)
        {
            Wave += 1;

            if (Wave > 1)
            {
                UpgradeManager.Instance.SetUpgradeButtons();
            }

            EnemySpawn.Instance.NextWave(Wave);
        }
    }

    void HandlePlayerDeath()
    {
        // clear bullets in screen for next wave
        foreach (Transform bullet in BulletParent.transform)
        {
            if (bullet.TryGetComponent<Bullet>(out var bulletScript))
            {
                bulletScript.DestroyWithoutExplosion();
            }
        }

        Time.timeScale = 0;

        DeathMenu.SetActive(true);

        Destroy(gameObject);
    }
}
