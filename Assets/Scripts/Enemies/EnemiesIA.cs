using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemiesIA : MonoBehaviour
{
    public enum State { Patrol, Chasing, Attack }
    public State currentState = State.Patrol;

    [SerializeField] float attackRange = 10f;
    [SerializeField] float visionRange = 15f;
    [SerializeField] float attackCoolDown = 2f;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform barret;

    private Animator animator; // Referencia al Animator
    private Transform nexo;
    private GameObject[] players;
    private NavMeshAgent agent;
    private float lastAttackTime = 0f;
    private Transform target = null;
    private bool isDead = false;

    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        nexo = GameObject.FindGameObjectWithTag("Nexo").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Inicializar el Animator
        currentState = State.Patrol;
    }

    private void Update()
    {
        if (isDead) return; // No hacer nada si estÅEmuerto

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

        animator.SetBool("isWalking", true); // Activar animaciÛn de caminar
        animator.SetBool("isShooting", false);

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
        else if (Vector3.Distance(nexo.position, transform.position) > visionRange)
        {
            currentState = State.Patrol;
        }
    }

    void Attack()
    {
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isShooting", true); // Activar animaciÛn de disparo

        if (Time.time > lastAttackTime + attackCoolDown)
        {
            Shoot();
            lastAttackTime = Time.time;
        }

        if (Vector3.Distance(target.position, transform.position) > attackRange)
        {
            currentState = State.Patrol;
            agent.isStopped = false;
        }
    }

    void Shoot()
    {
        Instantiate(projectilePrefab, barret.position, Quaternion.identity);
    }

    public void Die()
    {
        isDead = true;
        animator.SetBool("isDead", true); // Activar animaciÛn de muerte
        agent.isStopped = true;
        // Opcional: Destruir el objeto despuÈs de un tiempo
        Destroy(gameObject, 3f);
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