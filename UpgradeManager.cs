using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public int UpgradeCount;

    [SerializeField] GameObject UpgradePrefab;
    [SerializeField] GameObject UpgradeHorizontalLayout;
    [SerializeField] PlayerController Player;

    private List<Upgrade> Upgrades;
    private readonly Dictionary<UpgradeType, int> SelectedUpgradesDictionary = new();

    [Header("Audio Settings")]
    public AudioSource backgroundAudioSource;
    public AudioSource upgradeAudioSource;
    public AudioClip backgroundSound;

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
        Upgrades = new List<Upgrade>()
        {
            new("Speed Boost", "Increases the player's movement speed", UpgradeType.MovementSpeed, 1, 10, Resources.Load<Sprite>("Sprites/Swift-sharedassets0.assets-191")),
            new("Rapid Fire", "Increases the firing rate of weapons", UpgradeType.AttackSpeed, .01f, 10, Resources.Load<Sprite>("Sprites/Streamer-sharedassets0.assets-229")),
            new("Power Shot", "Enhances the damage dealt by projectiles", UpgradeType.Damage, 1, 10, Resources.Load<Sprite>("Sprites/Catalyst-sharedassets0.assets-130")),
            new("Eagle Eye", "Improves critical hit chance for attacks", UpgradeType.CriticalChance, 5, 20, Resources.Load<Sprite>("Sprites/Eyesight-sharedassets0.assets-153")),
            new("Lethal Strike", "Increases the damage of critical hits", UpgradeType.CriticalDamage, .1f, 10, Resources.Load<Sprite>("Sprites/Speculator-sharedassets0.assets-193")),
            new("Vitality Boost", "Increases maximum health points (HP)", UpgradeType.Growth, 10, 20, Resources.Load<Sprite>("Sprites/Regrowth-sharedassets0.assets-224")),
            new("High Jump", "Increases jump height", UpgradeType.Impulse, 1, 5, Resources.Load<Sprite>("Sprites/Thunderbolt-sharedassets0.assets-97")),
            new("Regeneration", "Instantly heals the player to full HP", UpgradeType.Regen, 0, 999, Resources.Load<Sprite>("Sprites/Growth-sharedassets0.assets-77")),
            new("Fortify", "Increases defense, reducing damage taken", UpgradeType.Resist, 1, 10, Resources.Load<Sprite>("Sprites/Resist-sharedassets0.assets-87")),
            new("Durability", "Increases the number of hits projectiles can take before they are destroyed", UpgradeType.Stability, 1, 10, Resources.Load<Sprite>("Sprites/Fragmentation-sharedassets0.assets-108")),
            new("Heavy Ammo", "Increases the size of projectiles", UpgradeType.Charge, .1f, 3, Resources.Load<Sprite>("Sprites/Comet-sharedassets0.assets-226")), // if the max is more than 3, the bullets collides in max attack speed
            new("Leech", "Steals life from the enemy based on damage dealth", UpgradeType.Leech, 1, 10, Resources.Load<Sprite>("Sprites/Wound-sharedassets0.assets-104"))
        };

        gameObject.SetActive(false);
    }

    public void SetUpgradeButtons()
    {
        var upgradesInScreen = new List<UpgradeType>();

        while (UpgradeHorizontalLayout.transform.childCount < UpgradeCount)
        {
            var upgrade = GetRandomAvailableUpgrade(upgradesInScreen);
            if (upgrade == null) break;

            var upgradeObject = Instantiate(UpgradePrefab, UpgradeHorizontalLayout.transform);

            ConfigureUpgradeButton(upgradeObject.GetComponent<Button>(), upgrade);
        }

        gameObject.SetActive(true);
        Time.timeScale = 0;

        backgroundAudioSource.Pause();
        upgradeAudioSource.PlayOneShot(backgroundSound);
    }

    private Upgrade GetRandomAvailableUpgrade(List<UpgradeType> upgradesInScreen)
    {
        var random = new System.Random();

        var upgrade = Upgrades
            .Where(U => U.MaxLevel > (SelectedUpgradesDictionary.GetValueOrDefault(U.Type, 0))
                && !upgradesInScreen.Contains(U.Type))
            .OrderBy(x => random.Next())
            .FirstOrDefault();

        if (upgrade != null)
            upgradesInScreen.Add(upgrade.Type);

        return upgrade;
    }

    private void ConfigureUpgradeButton(Button upgradeButton, Upgrade upgrade)
    {
        upgradeButton.onClick.AddListener(() =>
        {
            var upgradeAction = upgrade.SetUpgrade(Player, SelectedUpgradesDictionary);
            upgradeAction?.Invoke();
            gameObject.SetActive(false);

            foreach (Transform button in UpgradeHorizontalLayout.transform)
            {
                Destroy(button.gameObject);
            }

            Time.timeScale = 1;

            upgradeAudioSource.Stop();
            backgroundAudioSource.UnPause();
        });

        upgradeButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = upgrade.Name;
        upgradeButton.transform.GetChild(1).GetComponent<Image>().sprite = upgrade.Sprite;
        upgradeButton.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = upgrade.Description;
    }
}

public class Upgrade
{
    public Upgrade(string name, string description, UpgradeType type, float value, int maxLevel, Sprite sprite)
    {
        Name = name;
        Sprite = sprite;
        Description = description;
        Type = type;
        UpgradeValue = value;
        MaxLevel = maxLevel;
    }

    public string Name { get; private set; }

    public Sprite Sprite { get; private set; }

    public string Description { get; private set; }

    public UpgradeType Type { get; private set; }

    public float UpgradeValue { get; private set; }

    public int MaxLevel { get; private set; }

    public UnityAction SetUpgrade(PlayerController player, Dictionary<UpgradeType, int> playerUpgrades)
    {
        if (playerUpgrades.TryGetValue(Type, out var currentLevel))
        {
            if (currentLevel >= MaxLevel)
            {
                return null;
            }
        }
        else
        {
            currentLevel = 0;
        }

        playerUpgrades[Type] = currentLevel + 1;

        return Type switch
        {
            UpgradeType.MovementSpeed => new UnityAction(() => player.MovementSpeed += UpgradeValue),
            UpgradeType.AttackSpeed => new UnityAction(() => player.ShootCooldown -= UpgradeValue),
            UpgradeType.Damage => new UnityAction(() => player.BulletDamage += UpgradeValue),
            UpgradeType.CriticalChance => new UnityAction(() => player.CriticalChance += UpgradeValue),
            UpgradeType.CriticalDamage => new UnityAction(() => player.CriticalDamage += UpgradeValue),
            UpgradeType.Growth => new UnityAction(() => player.MaxHealth += UpgradeValue),
            UpgradeType.Impulse => new UnityAction(() => player.JumpForce += UpgradeValue),
            UpgradeType.Regen => new UnityAction(() => player.Health = player.MaxHealth),
            UpgradeType.Resist => new UnityAction(() => player.Shield += UpgradeValue),
            UpgradeType.Stability => new UnityAction(() => player.BulletPenetration += (int)UpgradeValue),
            UpgradeType.Charge => new UnityAction(() => player.BulletSize += UpgradeValue),
            UpgradeType.Leech => new UnityAction(() => player.LeechLevel += UpgradeValue),
            _ => null,
        };
    }
}

public enum UpgradeType
{
    MovementSpeed,
    AttackSpeed,
    Damage,
    CriticalChance,
    CriticalDamage,
    Growth,
    Impulse,
    Regen,
    Resist,
    Stability,
    Charge,
    Leech
}