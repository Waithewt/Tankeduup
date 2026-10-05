using UnityEngine;

public class AttackState : State
{
    public AttackState(AITank ai)
        : base(ai)
    {
    }

    protected override void Enter()
    {
        // Ao entrar no ataque,
        // não precisa mais seguir um caminho.
        ai.LimparCaminho();

        base.Enter();
    }

    protected override void Update()
    {
        // =====================================================
        // SEM JOGADOR
        // =====================================================

        if (ai.enemy == null)
        {
            ChangeState(
                new PatrolState(ai)
            );

            return;
        }

        // =====================================================
        // PERDEU A LINHA DE VISÃO
        // =====================================================

        if (!ai.PlayerVisivel())
        {
            ChangeState(
                new SearchState(ai)
            );

            return;
        }

        // =====================================================
        // JOGADOR SAIU DA DISTÂNCIA DE PARADA
        // =====================================================

        if (!ai.EstaDentroDaDistanciaDeParada())
        {
            ChangeState(
                new PursueState(ai)
            );

            return;
        }

        // =====================================================
        // PARADO E ATIRANDO
        // =====================================================

        // O tanque não chama PerseguirDestino aqui.
        // Portanto, permanece parado.
        ai.MirarCanhao();

        // Só dispara se estiver dentro
        // do alcance máximo de ataque.
        if (ai.EstaDentroDoAlcanceDeAtaque())
        {
            ai.Atacar();
        }
    }

    protected override void Exit()
    {
        ai.LimparCaminho();

        base.Exit();
    }
}