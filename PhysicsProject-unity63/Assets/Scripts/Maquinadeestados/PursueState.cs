using UnityEngine;

public class PursueState : State
{
    public PursueState(AITank ai)
        : base(ai)
    {
    }

    protected override void Enter()
    {
        ai.LimparCaminho();

        base.Enter();
    }

    protected override void Update()
    {
        // =====================================================
        // SEM PLAYER
        // =====================================================

        if (ai.enemy == null)
        {
            ChangeState(
                new PatrolState(ai)
            );

            return;
        }

        // =====================================================
        // PLAYER VISÍVEL
        // =====================================================

        if (ai.PlayerVisivel())
        {
            // Atualiza constantemente a última posição
            // enquanto consegue enxergar o jogador.
            ai.ultimaPosicaoConhecida =
                ai.enemy.transform.position;

            ai.tempoUltimaDeteccao =
                Time.time;

            ai.temUltimaPosicaoConhecida =
                true;

            // Se chegou a 8 metros,
            // para de perseguir e começa a atacar.
            if (
                ai.EstaDentroDaDistanciaDeParada()
            )
            {
                ChangeState(
                    new AttackState(ai)
                );

                return;
            }

            // Player visível:
            // PerseguirDestino() vai direto para ele
            // se estiver em área livre.
            ai.PerseguirDestino(
                ai.enemy.transform.position
            );

            return;
        }

        // =====================================================
        // PLAYER NÃO ESTÁ VISÍVEL
        // =====================================================

        if (ai.temUltimaPosicaoConhecida)
        {
            float tempoPerdido =
                Time.time -
                ai.tempoUltimaDeteccao;

            // Ainda temos memória da posição.
            if (
                tempoPerdido <
                ai.tempoMemoria
            )
            {
                // Continua indo para a última posição
                // conhecida.
                //
                // Se houver obstáculo no caminho,
                // PerseguirDestino() usa A*.
                ai.PerseguirDestino(
                    ai.ultimaPosicaoConhecida
                );

                return;
            }

            // Memória acabou.
            ChangeState(
                new SearchState(ai)
            );

            return;
        }

        // =====================================================
        // NÃO TEM POSIÇÃO CONHECIDA
        // =====================================================

        ChangeState(
            new PatrolState(ai)
        );
    }

    protected override void Exit()
    {
        ai.LimparCaminho();

        base.Exit();
    }
}