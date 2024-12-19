using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using _MessageType;

public class EnemiesIA : MonoBehaviour
{
    public enum State { Patrol, Chasing, Attack, Dead }
    public State currentState = State.Patrol;
    [SerializeField] float MESSAGE_SEND_DELAY = 0.01f;

    [SerializeField] float attackRange = 10f;
    [SerializeField] float attackNexoRange = 30f;
    [SerializeField] float visionRange = 15f;
    [SerializeField] float attackCoolDown = 2f;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform barret;
    [SerializeField] int HP = 3;

    private Animator animator; 
    private Transform nexo;
    private GameObject[] players;
    private NavMeshAgent agent;
    private float lastAttackTime = 0f;
    private Transform target = null;
    private bool isDead = false;
    

    private int enemyID;


    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        nexo = GameObject.FindGameObjectWithTag("Nexo").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); 
        currentState = State.Patrol;

       
    }

    private void Update()
    {
        if (isDead) return; // No hacer nada si est・muerto

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

        animator.SetBool("isWalking", true); // Activar animaci de caminar
        animator.SetBool("isShooting", false);

        if (target != null)
        {
            currentState = State.Chasing;
        }
    }

    void Chase()
    {
        agent.destination = target.position;

        if (Vector3.Distance(target.position, transform.position) < attackRange)
        {
            currentState = State.Attack;
            return;
        }
        if (Vector3.Distance(nexo.position, transform.position) < attackNexoRange)
        {
            target = nexo;
            currentState = State.Attack;
        }


        else if (Vector3.Distance(target.position, transform.position) > visionRange)
        {
            animator.SetBool("isWalking", true); // Activar animaci de caminar
            animator.SetBool("isShooting", false);
            currentState = State.Patrol;
        }
    }


    private void OnDrawGizmosSelected()
    {

        // Cambia el color para el rango de visión
        Gizmos.color = Color.green;
        // Dibuja un círculo para el rango de visión
        Gizmos.DrawWireSphere(transform.position, attackNexoRange);
        // Cambia el color para el rango de visión
        Gizmos.color = Color.blue;
        // Dibuja un círculo para el rango de visión
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Cambia el color para el rango de ataque
        Gizmos.color = Color.red;
        // Dibuja un círculo para el rango de ataque
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void Attack()
    {
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isShooting", true); // Activar animaci de disparo

        if (Time.time > lastAttackTime + attackCoolDown)
        {
            Shoot();
            lastAttackTime = Time.time;
        }
        if (Vector3.Distance(target.position, transform.position) < attackNexoRange)
        {
            return;
        }

        if (Vector3.Distance(target.position, transform.position) > attackRange)
        {
            animator.SetBool("isWalking", true); // Activar animaci de caminar
            animator.SetBool("isShooting", false);
            currentState = State.Patrol;
            agent.isStopped = false;
        }
    }

    void Shoot()
    {
        if (target == null) return;

        // Calcula la dirección hacia el objetivo
        Vector3 direction = (target.position - barret.position).normalized;

        // Crea el proyectil y ajusta su rotación hacia el objetivo
        GameObject bullet = Instantiate(projectilePrefab, barret.position, Quaternion.LookRotation(direction));
        bulletController bulletControll = bullet.GetComponent<bulletController>();
        bulletControll.target = target.position;
        bulletControll.hit = true;
        bulletControll.original = true;
        bulletControll.isEnemy = true;
    }

    public void Die()
    {
        isDead = true;
        currentState = State.Dead;
        animator.SetBool("isDead", true ); // Activar animaci de muerte
        agent.isStopped = true;
        //MessageManager.SendMessage(new KillEnemyMessage(enemyID));
        // Opcional: Destruir el objeto despu駸 de un tiempo

        EnemyManager.instance.RemoveEnemy(gameObject);
        Destroy(gameObject, 3f);
    }

    public void TakeDMG()
    {      
        HP--;
        if(HP <= 0) { Die(); }
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

    public int GetEnemyID()
    {
        return enemyID;
    }

    
    public void SetEnemyID(int newEnemyID)
    {
        enemyID = newEnemyID;
    }

    IEnumerator SendMyState()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(MESSAGE_SEND_DELAY);
            MessageManager.SendMessage(new Position(transform.position,
                transform.rotation.eulerAngles.y));
        }
    }

    public void ResetIA()
    {
        target = null;
        if (players != null)
        {
            System.Array.Clear(players, 0, players.Length);
        }
        agent.isStopped = false;
        players = GameObject.FindGameObjectsWithTag("Player");
        currentState = State.Patrol;
    }
}