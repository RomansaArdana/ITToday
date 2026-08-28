using System;
using UnityEngine;

[Serializable]
public class CyanBehaviourReferences
{
    [Header("Movement")]
    public EnemyChase chase;
    public EnemyDash dash;
    public EnemyTeleport teleport;
}