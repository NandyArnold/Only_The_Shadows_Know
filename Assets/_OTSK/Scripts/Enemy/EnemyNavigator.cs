// EnemyNavigator.cs

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigator : MonoBehaviour
{
    private NavMeshAgent _agent;
    public bool HasReachedDestination => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;

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
        _agent.isStopped =true;
    }

    public void Resume()
    {
        _agent.isStopped = false;
    }

    public void ResetPath()
    {
        if (_agent.hasPath)
        {
            _agent.ResetPath();
        }
    }
    public void SetStoppingDistance(float distance)
    {
        _agent.stoppingDistance = distance;
    }
    public bool CalculatePath(Vector3 target, NavMeshPath path)
    {
        return _agent.CalculatePath(target, path);
    }
}
