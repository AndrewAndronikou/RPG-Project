using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCFollow : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float targetDistance;
    [SerializeField] float allowedDistance = 5f;
    [SerializeField] GameObject theNPC;
    [SerializeField] float followSpeed;
    RaycastHit hit;

    NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //navMeshAgent.
        transform.LookAt(player.transform);

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit))
        {
            targetDistance = hit.distance;

            if (targetDistance >= allowedDistance)
            {
                navMeshAgent.speed = 10f;
                //followSpeed = 0.2f;
                //theNPC.GetComponent<Animation>().Play("")
                navMeshAgent.SetDestination(player.transform.position);
                //transform.position = Vector3.MoveTowards(transform.position, player.transform.position, followSpeed);
            }
            else
            {
                //navMeshAgent.Stop();
                navMeshAgent.speed = 0f;
              //  followSpeed = 0f;
              //  theNPC.GetComponent<Animation>().Play("idle");
            }
        }
    }
}
