using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unpuzzle
{
    public static class PinOutLevelSolverHelper
    {
        public static List<HookNode> GetHooksList(GameObject gameObject)
        {
            var hooks = gameObject.GetComponentsInChildren<HookController>();
            var tutorialNodes = hooks.Select(h => new HookNode { hook = h }).ToList();
            for (var i = 0; i < tutorialNodes.Count; i++)
            {
                var node = tutorialNodes[i];
                var handleHooks = node.hook.GetNextHooks();
                node.nexts = handleHooks.Select(hhs => new DataHolder<HookController> { data = hhs }).ToList();
            }

            return tutorialNodes;
        }
    }

    [Serializable]
    public class HookNode
    {
        public HookController hook;

        public List<DataHolder<HookController>> nexts = new();
    }
}
