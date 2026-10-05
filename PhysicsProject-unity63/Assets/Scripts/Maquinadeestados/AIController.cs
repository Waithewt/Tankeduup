using UnityEngine;

public class AIController : MonoBehaviour
{
    public AITank aiTank;

    private State currentState;

    void Start()
    {
        // Procura o AITank no mesmo GameObject
        // caso o campo esteja vazio no Inspector.
        if (aiTank == null)
        {
            aiTank =
                GetComponent<AITank>();
        }

        if (aiTank == null)
        {
            Debug.LogError(
                "AIController: AITank não encontrado."
            );

            return;
        }

        // Todo tanque começa patrulhando.
        currentState =
            new PatrolState(aiTank);
    }

    void Update()
    {
        if (currentState == null)
        {
            return;
        }

        // Processa o estado atual.
        State nextState =
            currentState.Process();

        // Se o estado pediu uma mudança,
        // troca para o novo estado.
        if (nextState != null)
        {
            currentState =
                nextState;
        }
    }
}