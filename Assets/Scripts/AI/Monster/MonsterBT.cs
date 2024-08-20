using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviourTrees;

public class MonsterBT : BehaviourTree
{
    public float attackRangeMin = 2f;
    public float attackRangeMax = 6f;
    public float attackTimer = 0.75f;
    public float trackingTime = 3f;
    //public static int tLayerMask = LayerMask.NameToLayer("Player");
    public float fovDistance = 25f;
    public float fovAngleH = 120f;
    public float fovAngleV = 90f;
    public Transform[] waypoints;
    public Transform target;
    public Transform eye;
    private NavMeshAgent agent;
    private BTNode _haunting;

    private void OnEnable()
    {
        PlayerEntity.onPlayerDeath += ResetTarget;
    }

    private void OnDisable()
    {
        PlayerEntity.onPlayerDeath -= ResetTarget;
    }

    private void ResetTarget()
    {
        _ctx.ClearVar("Target");
    }

    protected override BTNode SetupTree()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!agent)
        {
            return null;
        }

        _ctx = new BTContext(new List<KeyValuePair<string, object>>{
            new("Route", waypoints),
            new("TrackingTime", trackingTime),
            new("Agent", agent),
            new("Transform", eye),
            new("Target", target),
            new("FOVDistance", fovDistance),
            new("FOVHorAngle", fovAngleH),
            new("AttackRangeMin", attackRangeMin),
            new("AttackRangeMax", attackRangeMax),
            });

        BTNode root = new Selector(_ctx);

        Sequence haunting = new Sequence(_ctx);
        _haunting = haunting;
        //haunting.SetData("target", target);

        Selector targetSearch = new Selector(_ctx, new List<BTNode>
        {
            new IsTargetInSight(_ctx),
            new GoWhereSeenLast(_ctx),
        }); ;

        root.AttachChild(haunting);

        List<BTNode> hauntingList = new List<BTNode>
        {
            targetSearch,
            //new GoWhereSeenLast(_ctx),
            new FollowTarget(_ctx),
            new IsInAttackRange(_ctx),
            new PerformAttack(_ctx)
        };
        haunting.AttachChildren(hauntingList);

        root.AttachChild(new Patrol(_ctx));

        return root;
    }
}
