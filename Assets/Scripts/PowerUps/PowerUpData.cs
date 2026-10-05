using UnityEngine;

public enum PowerUpType
{
    Damage,
    ProjectileSpeed,
    ProjectileSize,
    Heal,
    InvertedControls,
    Invisibility,
    FuturePowerUp1,
    FuturePowerUp2
}

[CreateAssetMenu(
    fileName = "NewPowerUp",
    menuName = "PowerUps/Power Up"
)]
public class PowerUpData : ScriptableObject
{
    [Header("Basic Information")]
    public string powerUpName;

    [TextArea(2, 4)]
    public string description;

    [Header("Visuals")]
    public Sprite cardImage;
    public Sprite infoImage;

    [Header("Power Up")]
    public PowerUpType powerUpType;

    [Header("Plot Twist")]
    public bool isTwisted;

    [Header("Effect")]
    public float effectValue = 1f;

    [Header("Cost")]
    [Min(0)]
    public int diamondCost = 5;
}