using UnityEngine;

public class WaypointRegion : MonoBehaviour
{
    public Vector2 regionSize = new Vector2(20f, 20f);
    public int waypointCount = 5;

    [Header("Obstáculos")]
    public LayerMask obstacleMask;

    public float obstacleCheckRadius = 0.5f;

    public Transform[] waypoints;

    // Tanque que está usando esta rota.
    private AITank tanqueOcupante;

    // Verifica se a rota está ocupada.
    public bool EstaOcupada
    {
        get
        {
            return tanqueOcupante != null;
        }
    }

    // Verifica se a rota está livre.
    public bool EstaLivre()
    {
        return tanqueOcupante == null;
    }

    // Tenta reservar a rota para um tanque.
    public bool TentarOcupar(AITank tanque)
    {
        if (tanqueOcupante != null)
        {
            return false;
        }

        tanqueOcupante = tanque;

        return true;
    }

    // Libera a rota.
    public void Liberar(AITank tanque)
    {
        if (tanqueOcupante == tanque)
        {
            tanqueOcupante = null;
        }
    }

    public void GenerateWaypoints()
    {
        ClearWaypoints();

        waypoints =
            new Transform[waypointCount];

        int waypointsGerados = 0;
        int tentativas = 0;

        int maxTentativas =
            waypointCount * 50;

        while (
            waypointsGerados < waypointCount &&
            tentativas < maxTentativas
        )
        {
            tentativas++;

            float x = Random.Range(
                -regionSize.x / 2f,
                regionSize.x / 2f
            );

            float z = Random.Range(
                -regionSize.y / 2f,
                regionSize.y / 2f
            );

            Vector3 localPosition =
                new Vector3(x, 0f, z);

            Vector3 worldPosition =
                transform.TransformPoint(
                    localPosition
                );

            // Verifica se existe obstáculo.
            bool temObstaculo =
                Physics.CheckSphere(
                    worldPosition,
                    obstacleCheckRadius,
                    obstacleMask
                );

            if (temObstaculo)
            {
                continue;
            }

            GameObject waypoint =
                new GameObject(
                    "Waypoint " +
                    waypointsGerados
                );

            waypoint.transform.parent =
                transform;

            waypoint.transform.position =
                worldPosition;

            waypoints[waypointsGerados] =
                waypoint.transform;

            waypointsGerados++;
        }

        if (waypointsGerados < waypointCount)
        {
            Debug.LogWarning(
                "WaypointRegion: não foi possível gerar todos os waypoints. " +
                "A região pode estar muito ocupada por obstáculos."
            );

            Transform[] novosWaypoints =
                new Transform[waypointsGerados];

            for (int i = 0; i < waypointsGerados; i++)
            {
                novosWaypoints[i] =
                    waypoints[i];
            }

            waypoints =
                novosWaypoints;
        }
    }

    public void ClearWaypoints()
    {
        for (
            int i = transform.childCount - 1;
            i >= 0;
            i--
        )
        {
            DestroyImmediate(
                transform.GetChild(i).gameObject
            );
        }

        waypoints = null;

        // Libera a região também.
        tanqueOcupante = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(
                regionSize.x,
                0.1f,
                regionSize.y
            )
        );
    }
}