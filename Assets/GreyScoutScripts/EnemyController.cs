using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrol,
    Chase,
    Lost
}

public class EnemyController : MonoBehaviour
{
    public EnemyState currentState = EnemyState.Patrol;

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public VisionConeRotate visionRotate;
    public EnemyPatrol patrol;

    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float chaseTimeBeforeLost = 2f;

    private float chaseTimer = 0f;

    private void Start()
    {
        agent.speed = patrol.GetComponent<NavMeshAgent>().speed;
    }

    private void Update()
    {
        if (currentState == EnemyState.Chase)
        {
            agent.SetDestination(player.position);

            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseTimeBeforeLost)
            {
                ChangeState(EnemyState.Lost);
            }
        }
        else if (currentState == EnemyState.Lost)
        {
            patrol.enabled = true;
            visionRotate.enabled = true;
            ChangeState(EnemyState.Patrol);
        }
    }

    public void ChasePlayer()
    {
        currentState = EnemyState.Chase;
        chaseTimer = 0;

        patrol.enabled = false;
        visionRotate.enabled = false;

        agent.speed = chaseSpeed;
    }

    public void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }
}
