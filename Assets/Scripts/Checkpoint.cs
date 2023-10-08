using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script is placed on a checkpoint to store the respawn point when a player has passed through.
 * The checkpoint position is updated on each checkpoint.
*/

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Vector3 checkpointPos;

    public Vector3 GetCheckpointPos()
    {
        return checkpointPos;
    }
}
