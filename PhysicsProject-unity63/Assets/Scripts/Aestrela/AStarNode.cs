using UnityEngine;
using System.Collections.Generic;

public class AStarNode : MonoBehaviour
{
    public AStarNode parent;

    public List<AStarNode> neighbors =
        new List<AStarNode>();

    public float gCost;
    public float hCost;
    public float fCost;

    public NodeStatus status;

    public void ResetNode()
    {
        parent = null;

        gCost = 0f;
        hCost = 0f;
        fCost = 0f;

        neighbors.Clear();
    }
}