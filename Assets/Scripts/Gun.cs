using UnityEngine;

public class Gun : WeaponBase
{
    [Header("Firing")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shootCooldown = 0.1f;

    private float shootCooldownTimer = 0f;

    [Header("Weapon Properties")]
    [SerializeField] private int bulletCount = 1;
    [SerializeField] private float bulletSpread = 0f;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private int damagePerBullet = 1;

    [Header("Modifiers (Powerups)")]
    private float fireRateMultiplier = 1f;
    private int bulletCountMultiplier = 1;
    private float bulletSpeedMultiplier = 1f;
    private float damageMultiplier = 1f;

    protected override void Update()
    {
        base.Update();

        if (shootCooldownTimer > 0f)
        {
            shootCooldownTimer -= Time.deltaTime;
        }
    }

    public override void Fire()
    {
        if (shootCooldownTimer > 0f)
            return;

        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet prefab not assigned!");
            return;
        }

        Vector2 baseDirection = aimDirection;

        int totalBullets =
            bulletCount * bulletCountMultiplier;

        for (int i = 0; i < totalBullets; i++)
        {
            float spreadAngle = 0f;

            if (totalBullets > 1)
            {
                spreadAngle =
                    (i - (totalBullets - 1) / 2f)
                    * bulletSpread;
            }

            float baseAngle =
                Mathf.Atan2(
                    baseDirection.y,
                    baseDirection.x
                ) * Mathf.Rad2Deg;

            float angle = baseAngle + spreadAngle;

            Vector2 shootDirection =
                new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );

            GameObject bullet =
                Instantiate(
                    bulletPrefab,
                    firePoint.position,
                    Quaternion.identity
                );

            Bullet bulletScript =
                bullet.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                bulletScript.SetDirection(shootDirection);

                bulletScript.SetSpeed(
                    bulletSpeed * bulletSpeedMultiplier
                );

                bulletScript.SetDamage(
                    Mathf.RoundToInt(
                        damagePerBullet * damageMultiplier
                    )
                );
            }

            bullet.transform.rotation =
                Quaternion.AngleAxis(
                    angle,
                    Vector3.forward
                );
        }

        // Fire rate multiplier works intuitively:
        // 1.0 = normal
        // 1.2 = 20% faster
        // 2.0 = twice as fast
        shootCooldownTimer =
            shootCooldown / fireRateMultiplier;
    }

    #region Powerup Modification Methods

    public void SetFireRateMultiplier(float multiplier)
    {
        fireRateMultiplier = Mathf.Max(0.01f, multiplier);
    }

    public void AddFireRateMultiplier(float amount)
    {
        fireRateMultiplier =
            Mathf.Max(0.01f, fireRateMultiplier + amount);
    }

    public void SetBulletCount(int count)
    {
        bulletCount = Mathf.Max(1, count);
    }

    public void SetBulletCountMultiplier(int multiplier)
    {
        bulletCountMultiplier =
            Mathf.Max(1, multiplier);
    }

    public void AddBulletCountMultiplier(int amount)
    {
        bulletCountMultiplier =
            Mathf.Max(
                1,
                bulletCountMultiplier + amount
            );
    }

    public void SetBulletSpread(float spreadDegrees)
    {
        bulletSpread = spreadDegrees;
    }

    public void SetBulletSpeed(float speed)
    {
        bulletSpeed = speed;
    }

    public void SetBulletSpeedMultiplier(float multiplier)
    {
        bulletSpeedMultiplier =
            Mathf.Max(0.01f, multiplier);
    }

    public void AddBulletSpeedMultiplier(float amount)
    {
        bulletSpeedMultiplier =
            Mathf.Max(
                0.01f,
                bulletSpeedMultiplier + amount
            );
    }

    public void SetDamagePerBullet(int damage)
    {
        damagePerBullet = Mathf.Max(1, damage);
    }

    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier =
            Mathf.Max(0f, multiplier);
    }

    public void AddDamageMultiplier(float amount)
    {
        damageMultiplier =
            Mathf.Max(
                0f,
                damageMultiplier + amount
            );
    }

    public void SetShootCooldown(float cooldown)
    {
        shootCooldown = Mathf.Max(0.01f, cooldown);
    }

    public void ResetAllMultipliers()
    {
        fireRateMultiplier = 1f;
        bulletCountMultiplier = 1;
        bulletSpeedMultiplier = 1f;
        damageMultiplier = 1f;
    }

    #endregion

    #region Getters

    public float GetCurrentFireRate()
    {
        return shootCooldown / fireRateMultiplier;
    }

    public int GetTotalBulletCount()
    {
        return bulletCount * bulletCountMultiplier;
    }

    public float GetCurrentBulletSpeed()
    {
        return bulletSpeed * bulletSpeedMultiplier;
    }

    public int GetCurrentDamage()
    {
        return Mathf.RoundToInt(
            damagePerBullet * damageMultiplier
        );
    }

    public float GetFireRateMultiplier()
    {
        return fireRateMultiplier;
    }

    #endregion
}