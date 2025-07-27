using UnityEngine;
using System.Collections.Generic;

public class TurretScript : MonoBehaviour
{
    [System.Serializable]
    public class Buff
    {
        public float damageMultiplier = 1f;
        public float rangeMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public float duration;
        public float endTime;
        public GameObject specialPrefab; // For Turret D
    }

    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject machineBullet;
    [SerializeField] private GameObject flame;
    [SerializeField] private GameObject upgradedFlame; // For Turret D range buff
    [SerializeField] private Transform firingPort;
    [SerializeField] private LineRenderer rayLine;
    public GameObject gunA;
    public GameObject gunB;
    public GameObject gunC;
    public GameObject gunD;

    [Header("Audio")]
    [SerializeField] private AudioClip lasSound;
    [SerializeField] private AudioClip gunSound;
    [SerializeField] private AudioClip machinegunSound;
    [SerializeField] private float volume = 1f;

    [Header("Attributes")]
    [SerializeField] private float targetingRange = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float AChargeTime = 1.5f;
    [SerializeField] private float ARayDuration = 0.3f;
    [SerializeField] private float ARayRange = 10f;
    [SerializeField] private float ADamage = 5f;
    [SerializeField] private float BFireRate = 5f;
    [SerializeField] private float BBulletSpeed = 5f;
    [SerializeField] private float windUpTime = 1.5f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private int bulletsPerShot = 5;
    [SerializeField] private float machineBulletSpeed = 8f;
    [SerializeField] private float spreadAngle = 30f;
    [SerializeField] private float spawnDuration = 2f;
    [SerializeField] private float fireCooldown = 3f;

    [Header("Field of View")]
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private float returnRotationSpeed = 2f;

    [Header("Turret Type")]
    [SerializeField] private TurretType selectedType;

    // Base values for resetting buffs
    private float baseTargetingRange;
    private float baseAChargeTime;
    private float baseADamage;
    private float baseBFireRate;
    private float baseFireRate;
    private int baseBulletsPerShot;
    private float baseSpreadAngle;
    private float baseSpawnDuration;
    private float baseFireCooldown;
    private GameObject baseFlamePrefab;

    private float chargeTimer;
    private bool isCharging;
    private bool isRayActive;
    private float fireTimer;
    private float windUpTimer;
    private bool isWindingUp;
    private float cooldownTimer;
    private bool isOnCooldown;
    private GameObject currentSpawnedObj;

    private List<Buff> activeBuffs = new List<Buff>();

    public enum TurretType
    {
        TurretA,
        TurretB,
        TurretC,
        TurretD
    }

    private Transform target;
    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = turretRotationPoint.localRotation;

        // Store base values
        baseTargetingRange = targetingRange;
        baseAChargeTime = AChargeTime;
        baseADamage = ADamage;
        baseBFireRate = BFireRate;
        baseFireRate = fireRate;
        baseBulletsPerShot = bulletsPerShot;
        baseSpreadAngle = spreadAngle;
        baseSpawnDuration = spawnDuration;
        baseFireCooldown = fireCooldown;
        baseFlamePrefab = flame;
    }

    private void Update()
    {
        // Handle buff expiration
        HandleBuffExpiration();
        switch (selectedType)
        {
            case TurretType.TurretA:
                gunA.SetActive(true);
                gunB.SetActive(false);
                gunC.SetActive(false);
                gunD.SetActive(false);
                break;
            case TurretType.TurretB:
                gunA.SetActive(false);
                gunB.SetActive(true);
                gunC.SetActive(false);
                gunD.SetActive(false);
                break;
            case TurretType.TurretC:
                gunA.SetActive(false);
                gunB.SetActive(false);
                gunC.SetActive(true);
                gunD.SetActive(false);
                break;
            case TurretType.TurretD:
                gunA.SetActive(false);
                gunB.SetActive(false);
                gunC.SetActive(false);
                gunD.SetActive(true);
                break;
        }
    }

    private void FixedUpdate()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0) isOnCooldown = false;

        if (currentSpawnedObj != null)
        {
            currentSpawnedObj.transform.position = firingPort.position;
            currentSpawnedObj.transform.rotation = firingPort.rotation;
        }

        if (target == null)
        {
            if (isRayActive) DisableRay();
            isWindingUp = false;
            FindTarget();
        }
        else
        {
            RotateTowardTarget();
            if (!CheckTargetInRange())
            {
                target = null;
            }
            else
            {
                switch (selectedType)
                {
                    case TurretType.TurretA:
                        HandleTurretA();
                        break;
                    case TurretType.TurretB:
                        HandleTurretB();
                        break;
                    case TurretType.TurretC:
                        HandleTurretC();
                        break;
                    case TurretType.TurretD:
                        HandleTurretD();
                        break;
                }
            }
        }

        if (target == null)
        {
            ReturnToInitialRotation();
        }
    }

    private void HandleTurretA()
    {
        if (!isCharging && !isRayActive)
        {
            chargeTimer = AChargeTime;
            isCharging = true;
        }

        if (isCharging)
        {
            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0)
            {
                SoundFXManager.Instance.PlaySoundFXClip(lasSound, transform, volume);
                FireRay();
                isCharging = false;
            }
        }
        else if (isRayActive)
        {
            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0) DisableRay();
        }
    }

    private void HandleTurretB()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            SoundFXManager.Instance.PlaySoundFXClip(gunSound, transform, volume);
            Shoot();
            fireTimer = 1f / BFireRate;
        }
    }

    private void HandleTurretC()
    {
        if (!isWindingUp)
        {
            windUpTimer = windUpTime;
            isWindingUp = true;
            fireTimer = 0;
        }

        windUpTimer -= Time.deltaTime;
        if (windUpTimer <= 0)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0)
            {
                SoundFXManager.Instance.PlaySoundFXClip(machinegunSound, transform, volume);
                FireSpread();
                fireTimer = 1f / fireRate;
            }
        }
    }

    private void HandleTurretD()
    {
        if (currentSpawnedObj == null && !isOnCooldown)
        {
            SpawnObject();
        }
        else if (currentSpawnedObj != null)
        {
            currentSpawnedObj.transform.position = firingPort.position;
            currentSpawnedObj.transform.rotation = firingPort.rotation;
        }
    }

    public void ApplyBuff(float damageBoostPercent, float rangeBoostPercent,
                          float fireRateBoostPercent, float duration)
    {
        Buff newBuff = new Buff
        {
            damageMultiplier = 1 + damageBoostPercent / 100f,
            rangeMultiplier = 1 + rangeBoostPercent / 100f,
            fireRateMultiplier = 1 + fireRateBoostPercent / 100f,
            duration = duration,
            endTime = Time.time + duration
        };

        // Special handling for Turret D range buff
        if (selectedType == TurretType.TurretD && rangeBoostPercent > 0)
        {
            newBuff.specialPrefab = upgradedFlame;
        }

        activeBuffs.Add(newBuff);
        ApplyBuffEffects();
    }

    private void ApplyBuffEffects()
    {
        // Reset to base values
        targetingRange = baseTargetingRange;
        AChargeTime = baseAChargeTime;
        ADamage = baseADamage;
        BFireRate = baseBFireRate;
        fireRate = baseFireRate;
        bulletsPerShot = baseBulletsPerShot;
        spreadAngle = baseSpreadAngle;
        spawnDuration = baseSpawnDuration;
        fireCooldown = baseFireCooldown;
        flame = baseFlamePrefab;

        // Apply all active buffs
        foreach (Buff buff in activeBuffs)
        {
            switch (selectedType)
            {
                case TurretType.TurretA:
                    // Damage buff increases ray damage
                    ADamage *= buff.damageMultiplier;
                    // Fire rate buff decreases charge time
                    AChargeTime /= buff.fireRateMultiplier;
                    break;

                case TurretType.TurretB:
                    // Damage buff increases bullet damage
                    // (Bullet damage is applied when created)
                    // Fire rate buff increases fire rate
                    BFireRate *= buff.fireRateMultiplier;
                    break;

                case TurretType.TurretC:
                    // Damage buff increases bullet damage
                    // (Bullet damage is applied when created)
                    // Fire rate buff increases fire rate
                    fireRate *= buff.fireRateMultiplier;
                    // Range buff increases range and decreases spread
                    targetingRange *= buff.rangeMultiplier;
                    spreadAngle /= buff.rangeMultiplier;
                    // Bullets per shot buff
                    bulletsPerShot = Mathf.RoundToInt(bulletsPerShot * buff.damageMultiplier);
                    break;

                case TurretType.TurretD:
                    // Damage buff increases flame damage
                    // (Flame damage is applied when created)
                    // Fire rate buff increases spawn duration and decreases cooldown
                    spawnDuration *= buff.fireRateMultiplier;
                    fireCooldown /= buff.fireRateMultiplier;
                    // Range buff increases range and changes prefab
                    targetingRange *= buff.rangeMultiplier;
                    if (buff.specialPrefab != null)
                    {
                        flame = buff.specialPrefab;
                    }
                    break;
            }
        }
    }

    private void HandleBuffExpiration()
    {
        bool buffsChanged = false;

        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (Time.time >= activeBuffs[i].endTime)
            {
                activeBuffs.RemoveAt(i);
                buffsChanged = true;
            }
        }

        if (buffsChanged)
        {
            ApplyBuffEffects();
        }
    }

    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position,
            targetingRange,
            Vector2.zero,
            0f,
            enemyMask
        );

        foreach (RaycastHit2D hit in hits)
        {
            Vector2 directionToTarget = (hit.transform.position - transform.position).normalized;
            float angleToTarget = Vector2.Angle(transform.up, directionToTarget);

            if (angleToTarget <= fieldOfViewAngle / 2f)
            {
                target = hit.transform;
                return;
            }
        }
    }

    private bool CheckTargetInRange()
    {
        if (target == null) return false;

        float distance = Vector2.Distance(target.position, transform.position);
        if (distance > targetingRange) return false;

        Vector2 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector2.Angle(transform.up, directionToTarget);
        return (angleToTarget <= fieldOfViewAngle / 2f);
    }

    private void RotateTowardTarget()
    {
        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        turretRotationPoint.rotation = Quaternion.RotateTowards(
            turretRotationPoint.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void ReturnToInitialRotation()
    {
        turretRotationPoint.localRotation = Quaternion.RotateTowards(
            turretRotationPoint.localRotation,
            initialRotation,
            returnRotationSpeed * Time.deltaTime
        );
    }

    void FireRay()
    {
        rayLine.enabled = true;
        isRayActive = true;
        chargeTimer = ARayDuration;

        Vector2 rayDirection = firingPort.up;
        RaycastHit2D[] hits = Physics2D.RaycastAll(firingPort.position, rayDirection, ARayRange, enemyMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                // Apply buffed damage
                //hit.collider.GetComponent<Health>()?.TakeDamage(ADamage);
                Debug.Log("DAMAGEBLYAT");
                hit.collider.GetComponent<Enemy>()?.TakeDamage(ADamage * GetDamageMultiplier());
            }
        }

        rayLine.SetPosition(0, firingPort.position);
        rayLine.SetPosition(1, firingPort.position + (Vector3)rayDirection * ARayRange);
    }

    void DisableRay()
    {
        rayLine.enabled = false;
        isRayActive = false;
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firingPort.position, firingPort.rotation);
        Projectile projectileScript = bullet.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDamage(ADamage * GetDamageMultiplier());
            projectileScript.SetDirection(firingPort.up);
            projectileScript.SetSpeed(BBulletSpeed);
        }
        // Apply buffed damage
        //Bullet bulletScript = bullet.GetComponent<Bullet>();
        //if (bulletScript != null)
        //{
        //    bulletScript.damage *= activeBuffs.Count > 0 ? GetDamageMultiplier() : 1f;
        //}

        //bullet.GetComponent<Rigidbody2D>().linearVelocity = firingPort.up * BBulletSpeed;
        Destroy(bullet, 3f);
    }

    void FireSpread()
    {
        float angleStep = spreadAngle / (bulletsPerShot - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            Quaternion rotation = firingPort.rotation * Quaternion.Euler(0, 0, currentAngle);
            GameObject bullet = Instantiate(machineBullet, firingPort.position, rotation);

            //// Apply buffed damage
            //Bullet bulletScript = bullet.GetComponent<Bullet>();
            //if (bulletScript != null)
            //{
            //    bulletScript.damage *= activeBuffs.Count > 0 ? GetDamageMultiplier() : 1f;
            //}

            bullet.GetComponent<Rigidbody2D>().linearVelocity = bullet.transform.up * machineBulletSpeed;
            Destroy(bullet, 4f);
        }
    }

    void SpawnObject()
    {
        currentSpawnedObj = Instantiate(flame, firingPort.position, firingPort.rotation);

        // Apply buffed damage
        //Flame flameScript = currentSpawnedObj.GetComponent<Flame>();
        //if (flameScript != null)
        //{
        //    flameScript.damage *= activeBuffs.Count > 0 ? GetDamageMultiplier() : 1f;
        //}

        Destroy(currentSpawnedObj, spawnDuration);
        isOnCooldown = true;
        cooldownTimer = fireCooldown;
    }

    private float GetDamageMultiplier()
    {
        float multiplier = 1f;
        foreach (Buff buff in activeBuffs)
        {
            multiplier *= buff.damageMultiplier;
        }
        return multiplier;
    }

    // For debugging buffs in editor
    private void OnDrawGizmosSelected()
    {
        // Targeting range
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, targetingRange);

        // Field of view
        Vector3 forward = transform.up;
        Vector3 leftBound = Quaternion.AngleAxis(-fieldOfViewAngle / 2f, Vector3.forward) * forward;
        Vector3 rightBound = Quaternion.AngleAxis(fieldOfViewAngle / 2f, Vector3.forward) * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBound * targetingRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBound * targetingRange);
    }
}