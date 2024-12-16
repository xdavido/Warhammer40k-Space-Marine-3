using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemiesIA : MonoBehaviour
{
    public enum State { Patrol, Chasing, Attack}
    public State currentState = State.Patrol;

    [SerializeField] float attackRange = 10f;
    [SerializeField] float visionRange = 15f;
    [SerializeField] float attackCoolDown = 2f;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform barret;
    Transform nexo;


    GameObject[] players;
    NavMeshAgent agent;
    float lastAttackTiem = 0f;

    private Transform target = null;

    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        nexo = GameObject.FindGameObjectWithTag("Nexo").transform;
        agent = GetComponent<NavMeshAgent>();
        currentState = State.Patrol;

    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chasing:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    void Patrol()
    {
        target = FindNearestPlayerInRange(visionRange);
        agent.destination = target.position;
        

        // Si encuentra un jugador, cambia a Chasing.
        if (target != null)
        {
            currentState = State.Chasing;
        }
    }

    void Chase()
    {
        agent.destination = target.position;

        if (Vector3.Distance(nexo.position, transform.position) < attackRange)
        {
            currentState = State.Attack;
        }

        if (Vector3.Distance(nexo.position, transform.position) > visionRange)
        {
            currentState = State.Patrol;
        }
    }

    void Attack()
    {
        agent.isStopped = true;

        if(Time.time >lastAttackTiem + attackCoolDown)
        {
            Shoot();
            lastAttackTiem = Time.time;
        }

        if (Vector3.Distance(target.position, transform.position) > attackRange)
        {
            currentState = State.Patrol;
        }
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, barret.position, Quaternion.identity);
    }


    Transform FindNearestPlayerInRange(float range)
    {
        Transform nearestPlayer = nexo;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < range && distanceToPlayer < nearestDistance)
            {
                nearestDistance = distanceToPlayer;
                nearestPlayer = player.transform;
            }
        }

        return nearestPlayer;
    }

}
