using System.Collections;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public static EnemySpawn Instance { get; private set; }

    [SerializeField] EnemySO[] Enemies;
    [SerializeField] GameObject EnemyPrefab;

    private GameObject EnemyParent;
    private GameObject BulletParent;

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
        
    }

    public void NextWave(int wave)
    {
        // clear bullets in screen for next wave
        foreach (Transform bullet in BulletParent.transform)
        {
            if (bullet.TryGetComponent<Bullet>(out var bulletScript))
            {
                bulletScript.DestroyWithoutExplosion();
            }
        }

        // instantiane new enemies
        StartCoroutine(SpawnEnemiesWithCooldown(wave));
    }

    private IEnumerator SpawnEnemiesWithCooldown(int wave)
    {
        int enemyCount = (int)Mathf.Pow(wave, 1.3f);
        int enemyIndex = 0;

        for (int i = 0; i < enemyCount; i++)
        {
            // Determine the enemy type based on the wave
            if (wave < 3)
            {
                enemyIndex = 0;
            }
            else if (wave < 10)
            {
                enemyIndex = Random.Range(0, 2);
            }

            // Random spawn position
            Vector3 spawnPos = new Vector3(Random.Range(-10, 10), Random.Range(6, 8));

            // Instantiate the enemy
            GameObject enemy = Instantiate(EnemyPrefab, spawnPos, Quaternion.identity, EnemyParent.transform);
            enemy.transform.localScale = new Vector3(2f, 3f, 1f);

            // Assign the enemy's script and settings
            EnemyAI enemy_script = enemy.GetComponent<EnemyAI>();
            enemy_script.EnemySO = Enemies[enemyIndex];

            // Wait for the cooldown before spawning the next enemy
            yield return new WaitForSeconds(.25f);
        }
    }
}
