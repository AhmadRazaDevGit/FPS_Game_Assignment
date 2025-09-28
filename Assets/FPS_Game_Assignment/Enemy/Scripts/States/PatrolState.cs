using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IState
{
    private readonly IEnemyContext _context;
    private IState _nextState; // usually IdleState
    private int _currentWaypointIndex = -1;
    private readonly List<Transform> _waypoints;

    public string Name => "Patrol";

    public PatrolState(IEnemyContext context)
    {
        _context = context;
        _waypoints = _context.Waypoints;
    }

    public void SetNextState(IState next)
    {
        _nextState = next;
    }

    public void Enter()
    {
        var d = _context.EnemyData;

        if (_context.Animator != null)
        {
            if (!string.IsNullOrEmpty(d.patrolAnimationName))
                _context.Animator.CrossFade(d.patrolAnimationName, 0.5f);
        }

        // Configure agent
        if (_context.Agent != null)
        {
            _context.Agent.isStopped = false;
            _context.Agent.speed = d.patrolSpeed;
            _context.Agent.stoppingDistance = d.stoppingDistance;
        }

        // Immediately pick a target and move
        TrySetRandomWaypointDestination();
    }

    public void Tick()
    {
        if (_waypoints == null || _waypoints.Count == 0)
        {
            _nextState?.Exit();
            _context.SwitchState(_nextState);
            return;
        }

        if (_context.Agent != null)
        {
            // If destination reached -> go to Idle
            if (!_context.Agent.pathPending && _context.Agent.remainingDistance <= (_context.Agent.stoppingDistance + 0.1f))
            {
                // arrived
                _context.Agent.isStopped = true;
                if (_nextState != null)
                {
                    _context.SwitchState(_nextState);
                }
            }
            else
            {
                // still moving
                if (_context.Agent.hasPath == false)
                {
                    // pick new point if agent somehow lost path
                    TrySetRandomWaypointDestination();
                }
            }
        }
    }

    public void Exit()
    {
        if (_context.Agent != null)
        {
            _context.Agent.isStopped = true;
        }
    }

    private void TrySetRandomWaypointDestination()
    {
        if (_waypoints == null || _waypoints.Count == 0) return;

        int nextIndex = Random.Range(0, _waypoints.Count);
        // Avoid repeating the same waypoint consecutively when possible
        if (_waypoints.Count > 1)
        {
            int tries = 0;
            while (nextIndex == _currentWaypointIndex && tries++ < 5)
            {
                nextIndex = Random.Range(0, _waypoints.Count);
            }
        }

        _currentWaypointIndex = nextIndex;
        var target = _waypoints[_currentWaypointIndex];
        if (target != null && _context.Agent != null)
        {
            _context.Agent.SetDestination(target.position);
        }
    }
}
