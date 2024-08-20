using System.Collections;
using BehaviourTrees;
using UnityEngine;
using UnityEngine.AI;

public class Patrol : BTNode
{
    private Transform[] _waypoints;
    private Transform _nextTarget;
    private NavMeshAgent _agent;

    private int _curWpIdx = 0;

    public Patrol(BTContext ctx) : base(ctx)
    {
        _agent = (NavMeshAgent)(ctx.GetVar("Agent"));
        _waypoints = (Transform[])(ctx.GetVar("Route"));
        _nextTarget = _waypoints[_curWpIdx];
        //_agent.SetDestination(_waypoints[_curWpIdx].position);
    }

    public override NodeState Evaluate()
    {
        if (Vector3.Distance(_nextTarget.position, _agent.transform.position) < 1f)
        {
            _curWpIdx = (_curWpIdx + 1) % _waypoints.Length;
            _nextTarget = _waypoints[_curWpIdx];
        }
        if (_nextTarget.position != _agent.destination)
        {
            _agent.SetDestination(_waypoints[_curWpIdx].position);
        }
        return NodeState.Running;
    }
}

class IsTargetInSight : BTNode
{
    private Transform _transform;
    private float _fovDistance;
    private float _fovAngleHor;

    private bool _targetWasInSight = false;

    public IsTargetInSight(BTContext ctx) : base(ctx)
    {
        _transform = (Transform)(ctx.GetVar("Transform"));
        _fovDistance = (float)(_ctx.GetVar("FOVDistance"));
        _fovAngleHor = (float)(_ctx.GetVar("FOVHorAngle"));
    }

    private void SwitchTarget(Vector3 pos, bool inSight)
    {
        _targetWasInSight = inSight;
        if (!inSight)
            _ctx.SetVar("TargetLostPos", pos);
        else
            _ctx.ClearVar("TargetLostPos");
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)(_ctx.GetVar("Target"));
        if(target == null)
        {
            return NodeState.Failed;
        }
        //if(Vector3.Distance(_transform.position, target.position) < 10f)
        //{
        //    Debug.Log("Agent hears the target");
        //    return true;
        //}

        Collider[] cols = Physics.OverlapSphere(_transform.position, 2 * _fovDistance, LayerMask.GetMask("Player"));//MonsterBT.tLayerMask);

        if (cols.Length != 0 && cols[0].transform.parent == target)
        {
            Vector3 selfXZ = Vector3.ProjectOnPlane(_transform.position, Vector3.up);
            Vector3 targetXZ = Vector3.ProjectOnPlane(target.position, Vector3.up);
            Vector3 targetDirH = (targetXZ - selfXZ).normalized;

            float angleH = Vector3.Angle(_transform.forward, targetDirH);

            if (angleH > _fovAngleHor / 2)
            {
                if (_targetWasInSight)
                    SwitchTarget(target.position, false);
                return NodeState.Failed;
            }

            //Vector3 axis = Vector3.Cross(_transform.position, target.position).normalized;

            //float angle = Vector3.SignedAngle(_transform.position, target.position, Vector3.up);
            //Vector3 selfDifXZ = Quaternion.AngleAxis(angle, Vector3.up) * _transform.position;
            //Vector3 targetDirV = (target.position - selfDifXZ).normalized;

            //if (Vector3.Angle(_transform.forward, targetDirV) > MonsterBT.fovAngleV / 2)
            //{
            //    return false;
            //}

            Ray ray = new Ray(_transform.position, target.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _fovDistance, LayerMask.GetMask("Default")))
            {
                if (_targetWasInSight)
                    SwitchTarget(target.position, false);
                return NodeState.Failed;
            }

            //Debug.Log("Agent sees the target");
            if (!_targetWasInSight)
                SwitchTarget(target.position, true);
            return NodeState.Success;
        }
        if (_targetWasInSight)
            SwitchTarget(target.position, false);
        return NodeState.Failed;
    }
}

//class WaitWhereSeenLast : BTNode
//{
//    private bool _waiting = false;

//}

class GoWhereSeenLast : BTNode
{
    private Transform _transform;

    private bool _targetSpotted;
    private bool _tracking;
    private float _trackingTime;

    private Coroutine _trackingCor;
    private CoroutineManager _coroutineManager;

    public GoWhereSeenLast(BTContext ctx) : base(ctx)
    {
        _transform = (Transform)(ctx.GetVar("Transform"));
        _trackingTime = (float)(ctx.GetVar("TrackingTime"));
        _coroutineManager = CoroutineManager.getInstance();
    }

    public override NodeState Evaluate()
    {
        Debug.Log($"Evaluating {this}");
        state = NodeState.Failed;
        if(_ctx.GetVar("Target") == null)
        {
            return NodeState.Failed;
        }
        object lastPos = _ctx.GetVar("TargetLostPos");
        

        if (lastPos == null)
        {
            return state;
        }

        NavMeshAgent agent = (NavMeshAgent)(_ctx.GetVar("Agent"));
        //_ctx.ClearVar("TargetLostPos");
        if(agent.destination != (Vector3)lastPos)
        {
            agent.SetDestination((Vector3)lastPos);
            _tracking = true;
        }

        //_trackingCor = _coroutineManager.StartCoroutine(_TrackingReset());

        if (_tracking)
        {
            state = NodeState.Running;
            if (agent.remainingDistance < 1f)
            {
                _ctx.ClearVar("TargetLostPos");
                _tracking = false;
                state = NodeState.Failed;
            }
            
        }
        return state;
    }


    //public override NodeState Evaluate()
    //{
    //    state = NodeState.Failed;

    //    Transform target = (Transform)(_ctx.GetVar("Target"));
    //    Debug.Log($"Spot target: {target}");
    //    if (!target)
    //    {
    //        return state;
    //    }

    //    if (IsTargetInFov(target))
    //    {
    //        if (_tracking)
    //        {
    //            _coroutineManager.StopCoroutine(_trackingCor);
    //            _trackingCor = null;
    //            _tracking = false;
    //        }

    //        _targetSpotted = true;
    //        state = NodeState.Success;
    //    }
    //    else if (_targetSpotted)
    //    {
    //        Debug.Log("Cant see the light");
    //        _targetSpotted = false;
    //        _tracking = true;
    //        _trackingCor = _coroutineManager.StartCoroutine(_TrackingReset());
    //    }

    //    if (_tracking)
    //    {
    //        state = NodeState.Running;
    //    }

    //    return state;
    //}
}

public class FollowTarget : BTNode
{
    private NavMeshAgent _agent;

    public FollowTarget(BTContext ctx) : base(ctx)
    {
        _agent = (NavMeshAgent)(ctx.GetVar("Agent"));
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)(_ctx.GetVar("Target"));
        Vector3 direction = (target.position - _agent.transform.position).normalized;
        if (Vector3.Distance(_agent.transform.position, target.position) > (float)(_ctx.GetVar("AttackRangeMin")))
            //_agent.SetDestination(target.position - direction*(0.5f* (float)(_ctx.GetVar("AttackRangeMin"))));
            _agent.SetDestination(target.position);
        state = NodeState.Success;
        return state;
    }
}

public class IsInAttackRange : BTNode
{
    private Transform _transform;

    public IsInAttackRange(BTContext ctx) : base(ctx)
    {
        _transform = (Transform)(ctx.GetVar("Transform"));
    }

    public override NodeState Evaluate()
    {
        state = NodeState.Running;

        Transform target = (Transform)(_ctx.GetVar("Target"));
        EntityLogic targetEntity = target.GetComponent<EntityLogic>();
        if (target == null || targetEntity == null)
        {
            return state;
        }

        float distance = Vector3.Distance(_transform.position, target.position);
        if (distance < (float)(_ctx.GetVar("AttackRangeMax")))
        {
            Debug.Log("Can attack now");
            state = NodeState.Success;
        }

        return state;
    }
}

public class PerformAttack : BTNode
{
    private Transform _target;
    private EntityLogic _targetEntity = null;

    private bool _isReady;

    public PerformAttack(BTContext ctx) : base(ctx)
    {
        _isReady = true;
    }

    private IEnumerator _AttackCooldown()
    {
        yield return new WaitForSeconds(1.5f);
        _isReady = true;
    }

    public override NodeState Evaluate()
    {
        _target = (Transform)(_ctx.GetVar("Target"));
        _targetEntity = _target.GetComponent<EntityLogic>();

        if (_isReady)
        {
            _isReady = false;
            _targetEntity.TakeHit(10);
            CoroutineManager.getInstance().StartCoroutine(_AttackCooldown());
        }
        state = NodeState.Running;
        return state;
    }
}
