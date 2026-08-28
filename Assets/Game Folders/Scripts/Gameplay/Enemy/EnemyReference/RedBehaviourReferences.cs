using System;
using UnityEngine;

[Serializable]
public class RedBehaviourReferences
{
    [Header("Flee")]
    public EnemyFlee flee;

    [Header("Movement")]
    public EnemyCornerDetector cornerDetector;
    public EnemyObstacleDetector obstacleDetector;
    public EnemyJump jump;
    public EnemyLowGapDetector lowGapDetector;
    public EnemyCrouch crouch;

    [Header("Combat")]
    public EnemyVulnerable vulnerable;
}