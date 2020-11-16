using GameDevTV.Utils;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using RPG.Attributes;
using System.Collections;
using UnityEngine;
using System;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        //Configuration parameters
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float suspicionTime = 3f;
        [SerializeField] float aggroCooldownTime = 5f;
        [SerializeField] PatrolPath[] patrolPaths;
        [SerializeField] float wayPointTolerance = 1f;
        [SerializeField] float wayPointDwellTime = 3f;
        [Range(0, 1)]
        [SerializeField] float patrolSpeedFraction = 0.2f;
        [SerializeField] float EnemyRunDistance = 10f;
        [SerializeField] WeaponConfig rangedWeapon = null;
        [SerializeField] float fuzzyTimer = 3f;
        [SerializeField] float fuzzyProb = 0.5f;
        [SerializeField] float shoutDistance = 5;

        public enum AIState { WANDERING, SUSPICION, ATTACK, FLEE, RUNTOWARDS }
        public enum AttackStates { DEFAULT, RANGED }

        [SerializeField] public AIState state = AIState.WANDERING;
        [SerializeField] public AttackStates attackState = AttackStates.DEFAULT;

        //References
        Fighter fighter;
        Health health;
        Mover mover;
        GameObject player;

        LazyValue<Vector3> guardPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceArrivedAtWaypoint = Mathf.Infinity;
        float timeSinceAggrevated = Mathf.Infinity;
        int currentWaypointIndex = 0;

        float fleeHealth = 40f;
        bool fleeingOver = false;
        bool alreadyFled = false;
        bool fleeRand = true;
        int index;

        private void Awake()
        {
            //Initalize references
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            player = GameObject.FindWithTag("Player");

            guardPosition = new LazyValue<Vector3>(GetGuardPosition);

            if (patrolPaths != null)
            {
                index = UnityEngine.Random.Range(0, patrolPaths.Length);
            }
        }

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }

        private void Start()
        {
            guardPosition.ForceInit(); ;
        }

        //Every X seconds attempt to change to melee or ranged attacks
        private void FuzzyStateTimer()
        {
            if (fuzzyTimer <= 0)
            {
                fuzzyTimer = UnityEngine.Random.Range(4f, 10f);
                if (UnityEngine.Random.Range(0f, 1f) < fuzzyProb)
                {
                    attackState = AttackStates.DEFAULT;
                    chaseDistance = 10f;
                }
                else
                {
                    attackState = AttackStates.RANGED;
                    chaseDistance = 15f;
                }
            }
        }

        private void Update()
        {
            if (health.IsDead()) return;

            //Fuzzy logic for fleeing health
            if (health.GetPercentage() < 40 && fleeRand == true)
            {
                fleeRand = false;
                fleeHealth = UnityEngine.Random.Range(0f, 40f);
            }

            if (health.GetPercentage() < fleeHealth && alreadyFled == false)
                state = AIState.FLEE;

            //Runtowards player if hit
            //if (health.beenHit == true && !IsAggrevated())
            //{
            //    state = AIState.RUNTOWARDS;
            //}

           
            FuzzyStateTimer();

            switch (state)
            {
                case AIState.WANDERING:
                    PatrolBehaviour();
                    gameObject.name = "Wandering";
                    if (IsAggrevated() && fighter.CanAttack(player))
                        state = AIState.ATTACK;
                    break;

                //-----------------------------------------//
                case AIState.SUSPICION:
                    SuspicionBehaviour();
                    gameObject.name = "Suspicion";
                    if (timeSinceLastSawPlayer > suspicionTime)
                        state = AIState.WANDERING;
                    if (IsAggrevated() && fighter.CanAttack(player))
                        state = AIState.ATTACK;
                        attackState = AttackStates.DEFAULT;
                    break;

                //-----------------------------------------//
                //case AIState.RUNTOWARDS:
                //    if (!IsAggrevated())
                //        mover.StartMoveAction(player.transform.position, 1f);
                //    else
                //    {
                //        health.beenHit = false;
                //        state = AIState.ATTACK;
                //    }
                //    break;

                //-----------------------------------------//
                case AIState.ATTACK:
                    if (!IsAggrevated())
                        state = AIState.SUSPICION;

                    switch (attackState)
                    {
                        case AttackStates.DEFAULT: //Melee (equip melee weapon)
                            AttackBehaviour();
                            fighter.EquipedWeapon(fighter.defaultWeapon);
                            gameObject.name = "Attacking Default";
                            break;

                        //-----------------------------------------//
                        case AttackStates.RANGED: //Ranged (equip bow weapon)
                            AttackBehaviour();
                            fighter.EquipedWeapon(rangedWeapon);
                            gameObject.name = "Attacking Ranged";
                            break;
                    }
                    break;

                //-----------------------------------------//
                case AIState.FLEE:
                    FleeBehaviour();
                    gameObject.name = "Fleeing";
                    break;
            }
            UpdateTimers();
        }

        public void Aggrevate()
        {
            timeSinceAggrevated = 0;
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceArrivedAtWaypoint += Time.deltaTime;
            fuzzyTimer -= Time.deltaTime;
            timeSinceAggrevated += Time.deltaTime;
        }

        //Guard State patrols waypoints
        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition.value;
            
            if (patrolPaths[index] != null)
            {
                if (AtWayPoint())
                {
                    timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();
            }

            if (timeSinceArrivedAtWaypoint > wayPointDwellTime)
            {
                mover.StartMoveAction(nextPosition, patrolSpeedFraction);
            }
        }

        private bool AtWayPoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint < wayPointTolerance;
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPaths[index].GetNextIndex(currentWaypointIndex);
        }

        private Vector3 GetCurrentWaypoint()
        {

            return patrolPaths[index].GetWaypoint(currentWaypointIndex);
        }

        //Suspicion state
        private void SuspicionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        //Attack Player state
        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);

            AggrevateNearbyEnemies();
        }

        private void AggrevateNearbyEnemies()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, shoutDistance, Vector3.up, 0);

            foreach (RaycastHit hit in hits)
            {
                AIController ai = hit.collider.GetComponent<AIController>();
                if (ai == null) continue;

                ai.Aggrevate();
            }
        }

        //Flee state
        private void FleeBehaviour()
        {
            StartCoroutine(timeFled());
            alreadyFled = true;
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < EnemyRunDistance)
            {
                //Vector to player
                Vector3 disToPlayer = transform.position - player.transform.position;
                Vector3 newPos = transform.position + disToPlayer;
                mover.StartMoveAction(newPos, 5f);
            }
            if (fleeingOver == true)
            {
                state = AIState.RUNTOWARDS;
            }
        }

        IEnumerator timeFled()
        {
            yield return new WaitForSeconds(5);
            fleeingOver = true;
        }

        private bool IsAggrevated()
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            return distanceToPlayer < chaseDistance || timeSinceAggrevated < aggroCooldownTime;
        }

        //Called by Unity
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}
