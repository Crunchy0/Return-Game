using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public class BTContext
    {
        private Dictionary<string, object> _ctx;

        public BTContext()
        {
            _ctx = new Dictionary<string, object>();
        }

        public BTContext(IEnumerable<KeyValuePair<string, object>> collection)
        {
            _ctx = new Dictionary<string, object>(collection);
        }

        public void SetVar(string key, object value)
        {
            _ctx[key] = value;
        }

        public object GetVar(string key)
        {
            object value;
            if (_ctx.TryGetValue(key, out value))
                return value;
            return null;
        }

        public void ClearVar(string key)
        {
            _ctx.Remove(key);
        }
    }

    public abstract class BehaviourTree : MonoBehaviour
    {
        protected BTContext _ctx = null;
        private BTNode _root = null;
        // Start is called before the first frame update
        void Start()
        {
            _root = SetupTree();
        }

        // Update is called once per frame
        void Update()
        {
            if (_root != null)
            {
                _root.Evaluate();
            }
        }

        protected abstract BTNode SetupTree();
    }
}