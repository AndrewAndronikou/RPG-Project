using RPG.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    private Transform target;
    Health targetHealth;

    [Header("Attributes")]

    [SerializeField] float range = 15f;
    [SerializeField] float fireRate = 1f;
    private float fireCountdown = 0f;

    [Header("Unity setup fields")]
    public string enemyTag = "Enemy";
    public Transform partToRotate;
    public float turnSpeed = 10f;

    public GameObject bulletPrefab;
    public Transform firePoint;

    RaycastHit hit;
    Ray ray;
    int targetableMaskLayer;

    bool autoTarget = true;

    // Use this for initialization
    void Start()
    {
        targetableMaskLayer = LayerMask.GetMask("Targetable");
        InvokeRepeating("Targetting", 0f, 0.5f);

    }

    void Targetting()
    {
        if (autoTarget == true)
            UpdateTarget();
        if (autoTarget == false)
            ManualTarget();
    }

    void ManualTarget()
    {
        if (targetHealth == null)
            return;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, targetableMaskLayer))
        {
            target = hit.transform.gameObject.transform;

            if (targetHealth.GetPercentage() <= 0)
            {
                target = null;
                autoTarget = true;
            }
        }
    }

    //Auto targetting
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
            targetHealth = target.GetComponent<Health>();

        }
        else
        {
            target = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ManualTarget();
        }
        if (targetHealth == null)
            return;

        if (target == null)
            return;

        //Used to rotate a part towards the target (target lock on)
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void Shoot()
    {
        GameObject bulletGO = (GameObject)Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.Seek(target);
        }
    }

    void OnDrawGizmosSelected() //allows me to show range in scene view
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
