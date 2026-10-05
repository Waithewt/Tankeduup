using UnityEngine;

public class PatrolState : State
{
    private int waypointAtual;

    public PatrolState(AITank ai)
        : base(ai)
    {
    }

    protected override void Enter()
    {
        ai.LimparCaminho();

        // Começa pelo waypoint mais próximo.
        waypointAtual = EncontrarWaypointMaisProximo();

        ai.waypointAtual =
            waypointAtual;

        base.Enter();
    }

    protected override void Update()
    {
        // =====================================================
        // SEM ROTA
        // =====================================================

        if (ai.waypoints == null ||
            ai.waypoints.Length == 0)
        {
            // Defensive procura uma região livre.
            if (
                ai.behavior ==
                AITank.AIBehavior.Defensive
            )
            {
                ai.EscolherRotaDefensiva();
            }
            else
            {
                // Patrol e Aggressive procuram
                // uma região somente se não tiverem uma.
                if (ai.regiaoPatrulha == null)
                {
                    ai.EscolherRegiaoInicial();
                }
            }

            return;
        }

        // =====================================================
        // VERIFICAR JOGADOR
        // =====================================================

        if (ai.PlayerVisivel())
        {
            ChangeState(
                new PursueState(ai)
            );

            return;
        }

        // =====================================================
        // VERIFICAR WAYPOINT
        // =====================================================

        if (
            waypointAtual >=
            ai.waypoints.Length
        )
        {
            waypointAtual = 0;
        }

        Transform waypoint =
            ai.waypoints[waypointAtual];

        if (waypoint == null)
        {
            AvancarWaypoint();

            return;
        }

        // =====================================================
        // MOVIMENTO
        // =====================================================

        ai.PerseguirDestino(
            waypoint.position
        );

        float distancia =
            Vector3.Distance(
                ai.transform.position,
                waypoint.position
            );

        // =====================================================
        // CHEGOU NO WAYPOINT
        // =====================================================

        if (distancia < 1.5f)
        {
            // DEFENSIVE
            //
            // Diferente dos outros, não fica
            // repetindo a mesma rota.
            //
            // Quando termina todos os waypoints,
            // libera a região e escolhe outra.
            if (
                ai.behavior ==
                AITank.AIBehavior.Defensive
            )
            {
                if (
                    waypointAtual >=
                    ai.waypoints.Length - 1
                )
                {
                    ai.LimparCaminho();

                    if (ai.rotaAtual != null)
                    {
                        ai.rotaAtual.Liberar(ai);

                        ai.rotaAtual = null;
                    }

                    ai.waypoints = null;

                    ai.EscolherRotaDefensiva();

                    waypointAtual = 0;

                    ai.waypointAtual =
                        waypointAtual;

                    return;
                }
            }

            // PATROL e AGGRESSIVE
            //
            // Continuam circulando dentro
            // da região atual.
            AvancarWaypoint();
        }
    }

    int EncontrarWaypointMaisProximo()
    {
        if (
            ai.waypoints == null ||
            ai.waypoints.Length == 0
        )
        {
            return 0;
        }

        int melhorWaypoint = 0;

        float menorDistancia =
            Mathf.Infinity;

        for (
            int i = 0;
            i < ai.waypoints.Length;
            i++
        )
        {
            if (ai.waypoints[i] == null)
            {
                continue;
            }

            float distancia =
                Vector3.Distance(
                    ai.transform.position,
                    ai.waypoints[i].position
                );

            if (distancia < menorDistancia)
            {
                menorDistancia =
                    distancia;

                melhorWaypoint =
                    i;
            }
        }

        return melhorWaypoint;
    }

    void AvancarWaypoint()
    {
        waypointAtual++;

        if (
            ai.waypoints == null ||
            ai.waypoints.Length == 0
        )
        {
            waypointAtual = 0;
        }
        else if (
            waypointAtual >=
            ai.waypoints.Length
        )
        {
            waypointAtual = 0;
        }

        ai.waypointAtual =
            waypointAtual;

        ai.LimparCaminho();
    }

    protected override void Exit()
    {
        ai.LimparCaminho();

        base.Exit();
    }
}