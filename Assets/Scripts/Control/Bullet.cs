using RPG.Attributes;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;

    public float speed = 70f;
    public GameObject impactEffect;
    Vector3 offset = new Vector3(0f,2f,0f);

    Health hitTarget;

    private void Start()
    {
        hitTarget = target.GetComponent<Health>();
    }
    public void Seek(Transform _target)
    {
        target = _target;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        GameObject effectIns = Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectIns, 0.1f);
        hitTarget = GetComponent<Health>();
       // hitTarget.TakeDamage(gameObject, 150f );
        //Debug.Log("Turret: ");
        //TARGET NOT TAKING DAMAGE NEED TO FIX
       // hitTarget.TakeDamage(target.gameObject, 1000f);
        //Destroy(target.gameObject);
        Destroy(gameObject);
    }
}
