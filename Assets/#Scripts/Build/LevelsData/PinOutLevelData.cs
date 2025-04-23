using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PinOutLevelData
{
    [SerializeField] public string name;

    [SerializeField] public bool customPivot;

    [SerializeField] public Vector3 offset;

    [SerializeField] public List<PinOutLevelDataElement> elements = new();

    [SerializeField] public List<IceBlockElementLevelData> iceBlocks = new();

    [SerializeField] public List<PinBoltPlankData> pinBoltsPlanks = new();

    [SerializeField] public string hintsOrder;

    [SerializeField] public string hintFrozenBlockPairOrder;
}

[Serializable]
public class IceBlockElementLevelData
{
    [SerializeField] public IceBlockType blockType;

    [SerializeField] public Vector3 position;

    [SerializeField] public Vector3 rotation;

    [SerializeField] public Vector3 size;

    [SerializeField] public int punchCount;

    [NonSerialized] public int sIsDestroyed = 0;
}

[Serializable]
public class PinBoltPlankData
{
    [SerializeField] public Vector3 pos;

    [SerializeField] public Vector3 rot;

    [SerializeField] public Vector3 scl;

    [NonSerialized] public int sIsDestroyed = 0;
}
