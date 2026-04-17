public abstract class MainPhaseAction : IGamePacket
{
    public abstract void Execute(TurnStateMachine stateMachine);
    public abstract void Deserialize(byte[] bytes);
    public abstract byte[] Serialize();
}
