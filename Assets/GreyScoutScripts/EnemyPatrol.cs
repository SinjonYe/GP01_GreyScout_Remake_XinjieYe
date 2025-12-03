using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float waitTime = 2f;

    private int currentIndex = 0;
    private float waitCounter = 0f;

    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        MoveToNextPoint();
    }

    private void Update()
    {
        if (agent.remainingDistance <= 0.1f)
        {
            waitCounter += Time.deltaTime;

            if (waitCounter >= waitTime)
            {
                MoveToNextPoint();
                waitCounter = 0;
            }
        }
    }

    private void MoveToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentIndex].position);

        currentIndex = (currentIndex + 1) % patrolPoints.Length;
    }
}
