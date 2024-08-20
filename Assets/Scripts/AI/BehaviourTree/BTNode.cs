using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public enum NodeState
    {
        Running,
        Success,
        Failed
    }

    public class BTNode
    {
        protected NodeState state;

        public BTNode _parent;
        protected List<BTNode> _children = new List<BTNode>();

        //private Dictionary<string, object> _dataContext = new Dictionary<string, object>();
        protected BTContext _ctx;

        public BTNode(BTContext ctx)
        {
            _ctx = ctx;
            _parent = null;
        }

        public BTNode(BTContext ctx, List<BTNode> children)
        {
            _ctx = ctx;
            AttachChildren(children);
        }

        public void AttachChild(BTNode node)
        {
            node._parent = this;
            _children.Add(node);
        }

        public void AttachChildren(List<BTNode> nodeList)
        {
            foreach(BTNode node in nodeList)
            {
                AttachChild(node);
            }
        }

        public virtual NodeState Evaluate() => NodeState.Failed;

        //public void SetData(string key, object value)
        //{
        //    _dataContext[key] = value;
        //}

        //public object GetData(string key)
        //{
        //    object value = null;
        //    if (_dataContext.TryGetValue(key, out value))
        //    {
        //        return value;
        //    }

        //    BTNode node = _parent;
        //    while (node != null)
        //    {
        //        value = node.GetData(key);
        //        if (value != null)
        //            return value;
        //        node = node._parent;
        //    }
        //    return null;
        //}

        //public bool ClearData(string key)
        //{
        //    object value = null;
        //    if (_dataContext.TryGetValue(key, out value))
        //    {
        //        _dataContext.Remove(key);
        //        return true;
        //    }

        //    BTNode node = _parent;
        //    while (node != null)
        //    {
        //        bool cleared = node.ClearData(key);
        //        if (cleared)
        //            return true;
        //        node = node._parent;
        //    }
        //    return false;
        //}
    }

    public class Sequence : BTNode
    {
        public Sequence(BTContext ctx) : base(ctx) { }
        public Sequence(BTContext ctx, List<BTNode> children) : base(ctx, children) { }

        public override NodeState Evaluate()
        {
            //bool anyChildRunning = false;

            foreach (BTNode child in _children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.Running:
                        //anyChildRunning = true;
                        state = NodeState.Running;
                        return state;
                        //continue;
                    case NodeState.Failed:
                        state = NodeState.Failed;
                        return state;
                    case NodeState.Success:
                        continue;
                    default:
                        state = NodeState.Success;
                        return state;
                }
            }

            state = NodeState.Success;// anyChildRunning ? NodeState.Running : NodeState.Success;
            return state;
        }
    }

    public class Selector : BTNode
    {
        public Selector(BTContext ctx) : base(ctx) { }
        public Selector(BTContext ctx, List<BTNode> children) : base(ctx, children) { }
        public override NodeState Evaluate()
        {
            foreach (BTNode child in _children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;
                    case NodeState.Failed:
                        continue;
                    case NodeState.Success:
                        state = NodeState.Success;
                        return state;
                    default:
                        continue;
                }
            }

            state = NodeState.Failed;
            return state;
        }
    }
}