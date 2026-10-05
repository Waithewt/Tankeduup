using UnityEngine;

public abstract class State
{
    protected AITank ai;

    protected State nextState;

    protected enum StateStage
    {
        Enter,
        Update,
        Exit
    }

    protected StateStage stage;

    public State(AITank ai)
    {
        this.ai = ai;

        stage =
            StateStage.Enter;
    }

    public State Process()
    {
        switch (stage)
        {
            case StateStage.Enter:

                Enter();

                break;

            case StateStage.Update:

                Update();

                break;

            case StateStage.Exit:

                Exit();

                return nextState;
        }

        return null;
    }

    protected virtual void Enter()
    {
        stage =
            StateStage.Update;
    }

    protected virtual void Update()
    {
    }

    protected virtual void Exit()
    {
        stage =
            StateStage.Enter;
    }

    protected void ChangeState(
        State newState
    )
    {
        nextState =
            newState;

        stage =
            StateStage.Exit;
    }
}