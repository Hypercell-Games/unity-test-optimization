using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PinOutLevelDataElement
{
    [SerializeField] public PinType type;

    [SerializeField] public Vector3 pos;

    [SerializeField] public Vector3 rot;

    [SerializeField] public bool isFrzn;

    [SerializeField] public bool isGhost;

    [SerializeField] public bool isFire;

    [SerializeField] public bool colOvrd;

    [SerializeField] public bool matOvrd;

    [SerializeField] public PinMaterialId matId;

    [SerializeField] public PinColorID colId;

    [SerializeField] public bool hsKey;

    [SerializeField] public bool hsStr;

    [SerializeField] public bool scaled;

    [SerializeField] public Vector2 pinScl;

    [SerializeField] public string chains;

    [SerializeField] public string chainsScl;

    [SerializeField] public string chainRot;

    [SerializeField] public List<int> blckPins;

    [NonSerialized] public int sIsDestroyed = 0;

    [NonSerialized] public int sRevealed = 0;
}
