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

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "PowerUps/Power Up")]
public class PowerUpData : ScriptableObject
{
    public string powerUpName;

    [TextArea]
    public string description;

    public Sprite cardImage;

    public PowerUpType powerUpType;

    public bool isTwisted;
}