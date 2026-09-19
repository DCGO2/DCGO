using ExitGames.Client.Photon;

public class PassAction : MainPhaseAction
{
    
    public PassAction()
    {
    }

    public PassAction(byte[] bytes)
    {
        Deserialize(bytes);
    }

    public override void Execute(TurnStateMachine stateMachine)
    {
        stateMachine.PassTurn();
    }

    public override void Deserialize(byte[] bytes)
    {
    }

    public override byte[] Serialize()
    {
        return null;
    }
}
