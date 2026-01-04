using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    public Transform[] patrolPoints; // Patrol points array (move in order)
    public float waitTime = 2f;

    private int currentIndex = 0;
    private float waitCounter = 0f;

    private NavMeshAgent agent;

    private void Start() // Enemy NavMesh movement component
    {
        // Get NavMeshAgent and start first patrol
        agent = GetComponent<NavMeshAgent>();
        MoveToNextPoint();
    }

    private void Update()
    {
        if (agent.remainingDistance <= 0.1f) // Check if reached target point
        {
            waitCounter += Time.deltaTime; // Start wait timer after arrival

            if (waitCounter >= waitTime) // Move to next patrol point after waiting
            {
                MoveToNextPoint();
                waitCounter = 0;
            }
        }
    }

    // Set next patrol point and loop index
    private void MoveToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentIndex].position);

        currentIndex = (currentIndex + 1) % patrolPoints.Length;
    }
}
