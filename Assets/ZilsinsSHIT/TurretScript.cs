using UnityEngine;
using UnityEditor;


public class TurretScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject machineBullet;
    [SerializeField] private GameObject flame;
    [SerializeField] private Transform firingPort;
    [SerializeField] private LineRenderer rayLine;

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
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float fireCooldown = 3f;

    [Header("Field of View")]
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private float returnRotationSpeed = 2f;

    [Header("Turret Type")]
    [SerializeField] private TurretType selectedType;

    private float chargeTimer;
    private bool isCharging;
    private bool isRayActive;
    private float fireTimer;
    private float windUpTimer;
    private bool isWindingUp;
    private float cooldownTimer;
    private bool isOnCooldown;
    private GameObject currentSpawnedObj;

    private enum TurretType
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
    }

    private void FixedUpdate()
    {

        if (target == null)
        {
            if (isRayActive) DisableRay();
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0) isOnCooldown = false;
            isWindingUp = false;
            //if (isOnCooldown)
            //{
            //    if (currentSpawnedObj != null)
            //    {
            //        Destroy(currentSpawnedObj);
            //        currentSpawnedObj = null;
            //    }
            //    return;

            //}
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
                if(selectedType == TurretType.TurretA)
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
                else if (selectedType == TurretType.TurretB)
                {
                    fireTimer -= Time.deltaTime;
                    if (fireTimer <= 0)
                    {
                        Shoot();
                        fireTimer = 1f / BFireRate;
                    }
                }
                else if (selectedType == TurretType.TurretC)
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
                            FireSpread();
                            fireTimer = 1f / fireRate;
                        }
                    }
                }
                else if (selectedType == TurretType.TurretD)
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

            }
        }

       
        if (target == null)
        {
            turretRotationPoint.localRotation = Quaternion.RotateTowards(
                turretRotationPoint.localRotation,
                initialRotation,
                returnRotationSpeed * Time.deltaTime
            );
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


    private void OnDrawGizmosSelected()
    {
        
        Handles.color = Color.white;
        Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);

        
        Vector3 forward = transform.up;
        Vector3 leftBound = Quaternion.AngleAxis(-fieldOfViewAngle / 2f, Vector3.forward) * forward;
        Vector3 rightBound = Quaternion.AngleAxis(fieldOfViewAngle / 2f, Vector3.forward) * forward;

        Handles.color = Color.yellow;
        Handles.DrawLine(transform.position, transform.position + leftBound * targetingRange);
        Handles.DrawLine(transform.position, transform.position + rightBound * targetingRange);
        Handles.DrawWireArc(
            transform.position,
            Vector3.forward,
            rightBound,
            -fieldOfViewAngle,
            targetingRange
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
                //hit.collider.GetComponent<Health>()?.TakeDamage(ADamage);
                Debug.Log("Hit");
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
        bullet.GetComponent<Rigidbody2D>().linearVelocity = firingPort.up * BBulletSpeed;
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
            bullet.GetComponent<Rigidbody2D>().linearVelocity = bullet.transform.up * BBulletSpeed;
            Destroy(bullet, 4f);
        }
    }
    void SpawnObject()
    {
        currentSpawnedObj = Instantiate(flame, firingPort.position, firingPort.rotation);
        Destroy(currentSpawnedObj, spawnDuration);
        isOnCooldown = true;
        cooldownTimer = fireCooldown;
    }

    
}