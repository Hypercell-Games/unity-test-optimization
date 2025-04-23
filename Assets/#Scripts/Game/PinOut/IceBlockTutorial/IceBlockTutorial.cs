using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    public class IceBlockTutorial : MonoBehaviour
    {
        [SerializeField] private bool _showOnlyHint = true;
        [SerializeField] private List<IceBlockTutorialItem> _tutorialList;

        public List<IceBlockTutorialItem> tutorialSequence => _tutorialList;

        public bool ShowOnlyHint => _showOnlyHint;
    }

    [Serializable]
    public class IceBlockTutorialItem
    {
        public HookController pin;
        public HookController iceBlock;
    }
}
