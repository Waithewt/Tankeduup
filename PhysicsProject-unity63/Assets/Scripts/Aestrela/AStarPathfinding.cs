using UnityEngine;
using System.Collections.Generic;

public class AStarPathfinding : MonoBehaviour
{
    public List<AStarNode> openList;
    public List<AStarNode> closedList;
    public List<AStarNode> path;
    public List<AStarNode> grid;

    public AStarNode startNode;
    public AStarNode endNode;
    public AStarNode currentNode;

    public AStarGrid gridGenerator;

    void Start()
    {
        grid = new List<AStarNode>(
            FindObjectsByType<AStarNode>(
                FindObjectsSortMode.None
            )
        );

        if (gridGenerator == null)
        {
            gridGenerator =
                FindFirstObjectByType<AStarGrid>();
        }

        Debug.Log(
            "Quantidade de Nodes: " +
            grid.Count
        );
    }

    public List<AStarNode> FindPath(
        AStarNode start,
        AStarNode end
    )
    {
        if (start == null || end == null)
        {
            Debug.LogError(
                "A*: Start ou End está vazio."
            );

            return null;
        }

        if (gridGenerator == null)
        {
            Debug.LogError(
                "A*: AStarGrid não foi encontrado."
            );

            return null;
        }

        startNode = start;
        endNode = end;

        openList = new List<AStarNode>();
        closedList = new List<AStarNode>();
        path = new List<AStarNode>();

        // Reseta todos os Nodes
        foreach (AStarNode node in grid)
        {
            node.ResetNode();
        }

        // Começamos pelo Node inicial
        currentNode = startNode;

        currentNode.gCost = 0f;

        currentNode.hCost =
            Vector3.Distance(
                currentNode.transform.position,
                endNode.transform.position
            );

        currentNode.fCost =
            currentNode.gCost +
            currentNode.hCost;

        openList.Add(currentNode);

        while (openList.Count > 0)
        {
            // Ordena pelo menor F
            openList.Sort(
                (node1, node2) =>
                    node1.fCost.CompareTo(
                        node2.fCost
                    )
            );

            currentNode = openList[0];

            // Chegamos ao destino
            if (currentNode == endNode)
            {
                ReconstructPath();

                return path;
            }

            openList.Remove(currentNode);

            closedList.Add(currentNode);

            // Procura os vizinhos
            foreach (
                AStarNode neighbor
                in GetNeighbors(currentNode)
            )
            {
                // Ignora obstáculos
                if (
                    neighbor.status ==
                    NodeStatus.Obstacle
                )
                {
                    continue;
                }

                // Ignora Nodes já processados
                if (closedList.Contains(neighbor))
                {
                    continue;
                }

                float newGCost =
                    currentNode.gCost +
                    Vector3.Distance(
                        currentNode.transform.position,
                        neighbor.transform.position
                    );

                bool alreadyInOpen =
                    openList.Contains(neighbor);

                // Se encontramos um caminho melhor
                if (
                    !alreadyInOpen ||
                    newGCost < neighbor.gCost
                )
                {
                    neighbor.parent =
                        currentNode;

                    neighbor.gCost =
                        newGCost;

                    neighbor.hCost =
                        Vector3.Distance(
                            neighbor.transform.position,
                            endNode.transform.position
                        );

                    neighbor.fCost =
                        neighbor.gCost +
                        neighbor.hCost;

                    if (!alreadyInOpen)
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning(
            "A*: Não foi possível encontrar um caminho."
        );

        return null;
    }

    List<AStarNode> GetNeighbors(
        AStarNode node
    )
    {
        List<AStarNode> neighbors =
            new List<AStarNode>();

        foreach (AStarNode other in grid)
        {
            if (other == node)
                continue;

            float deltaX =
                Mathf.Abs(
                    other.transform.position.x -
                    node.transform.position.x
                );

            float deltaZ =
                Mathf.Abs(
                    other.transform.position.z -
                    node.transform.position.z
                );

            // Verifica Nodes adjacentes
            // usando o tamanho definido no Grid
            if (
                Mathf.Approximately(
                    deltaX + deltaZ,
                    gridGenerator.nodeSize
                )
            )
            {
                neighbors.Add(other);
            }
        }

        return neighbors;
    }

    void ReconstructPath()
    {
        path.Clear();

        AStarNode current =
            endNode;

        while (current != null)
        {
            path.Add(current);

            if (current == startNode)
                break;

            current =
                current.parent;
        }

        path.Reverse();

        Debug.Log(
            "A*: Caminho encontrado com " +
            path.Count +
            " Nodes."
        );
    }
}
