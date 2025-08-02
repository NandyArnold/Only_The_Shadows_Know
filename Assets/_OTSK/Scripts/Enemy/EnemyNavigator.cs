// EnemyNavigator.cs

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigator : MonoBehaviour
{
    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public void SetSpeed(float speed)
    {
        _agent.speed = speed;
    }

    public void MoveTo(Vector3 destination)
    {
        _agent.SetDestination(destination);
    }

    public void Stop()
    {
        _agent.ResetPath();
    }
}
