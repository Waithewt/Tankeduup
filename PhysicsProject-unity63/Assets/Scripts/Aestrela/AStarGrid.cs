using UnityEngine;

public class AStarGrid : MonoBehaviour
{
    public GameObject nodePrefab;

    public int width = 34;
    public int height = 34;

    public float nodeSize = 3f;

    [Header("Obstáculos")]
    public LayerMask obstacleMask;

    void Start()
    {
        float offsetX =
            (width - 1) * nodeSize / 2f;

        float offsetZ =
            (height - 1) * nodeSize / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(
                    x * nodeSize - offsetX,
                    0f,
                    z * nodeSize - offsetZ
                );

                GameObject nodeObject =
                    Instantiate(
                        nodePrefab,
                        position,
                        Quaternion.Euler(90f, 0f, 0f),
                        transform
                    );

                AStarNode node =
                    nodeObject.GetComponent<AStarNode>();

                if (node == null)
                {
                    Debug.LogError(
                        "O NodePrefab não possui AStarNode!",
                        nodeObject
                    );

                    continue;
                }

                Collider[] obstacles =
                    Physics.OverlapBox(
                        position,
                        new Vector3(
                            nodeSize / 2f,
                            1f,
                            nodeSize / 2f
                        ),
                        Quaternion.identity,
                        obstacleMask
                    );

                if (obstacles.Length > 0)
                {
                    node.status =
                        NodeStatus.Obstacle;
                }
                else
                {
                    node.status =
                        NodeStatus.None;
                }
            }
        }

        Debug.Log(
            "Grid criado com " +
            (width * height) +
            " Nodes."
        );
    }
}