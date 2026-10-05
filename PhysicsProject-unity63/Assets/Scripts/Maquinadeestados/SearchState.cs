using UnityEngine;

public class SearchState : State
{
    private Vector3 destinoBusca;

    public SearchState(AITank ai)
        : base(ai)
    {
    }

    protected override void Enter()
    {
        // =====================================================
        // DEFINE O DESTINO DA BUSCA
        // =====================================================

        if (ai.temUltimaPosicaoConhecida)
        {
            destinoBusca =
                ai.ultimaPosicaoConhecida;
        }
        else if (ai.enemy != null)
        {
            destinoBusca =
                ai.enemy.transform.position;
        }
        else
        {
            ChangeState(
                new PatrolState(ai)
            );

            return;
        }

        ai.LimparCaminho();

        base.Enter();
    }

    protected override void Update()
    {
        // =====================================================
        // JOGADOR FOI ENCONTRADO NOVAMENTE
        // =====================================================

        if (
            ai.enemy != null &&
            ai.PlayerVisivel()
        )
        {
            ai.ultimaPosicaoConhecida =
                ai.enemy.transform.position;

            ai.tempoUltimaDeteccao =
                Time.time;

            ai.temUltimaPosicaoConhecida =
                true;

            ChangeState(
                new PursueState(ai)
            );

            return;
        }

        // =====================================================
        // VAI ATÉ A ÚLTIMA POSIÇÃO CONHECIDA
        // =====================================================

        float distancia =
            Vector3.Distance(
                ai.transform.position,
                destinoBusca
            );

        if (distancia > 1.5f)
        {
            ai.PerseguirDestino(
                destinoBusca
            );

            return;
        }

        // =====================================================
        // CHEGOU NA ÚLTIMA POSIÇÃO
        // =====================================================

        ai.LimparCaminho();

        ai.temUltimaPosicaoConhecida =
            false;

        // =====================================================
        // AGGRESSIVE
        // =====================================================

        if (
            ai.behavior ==
            AITank.AIBehavior.Aggressive
        )
        {
            // Somente agora, depois de chegar
            // na última posição conhecida,
            // cria uma nova região.
            ai.CriarNovaPatrulhaAggressive();

            ChangeState(
                new PatrolState(ai)
            );

            return;
        }

        // =====================================================
        // DEFENSIVE
        // =====================================================

        if (
            ai.behavior ==
            AITank.AIBehavior.Defensive
        )
        {
            // Defensive nunca cria região.
            //
            // Se não tiver uma rota válida,
            // procura uma região predefinida.
            if (ai.rotaAtual == null)
            {
                ai.EscolherRotaDefensiva();
            }

            ChangeState(
                new PatrolState(ai)
            );

            return;
        }

        // =====================================================
        // PATROL
        // =====================================================

        // Patrol simplesmente retorna para
        // sua região original.
        //
        // A região não é liberada e não é criada
        // uma nova região.
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