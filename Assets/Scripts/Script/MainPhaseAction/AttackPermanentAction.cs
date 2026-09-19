using ExitGames.Client.Photon;

public class AttackPermanentAction : MainPhaseAction
{
    int PermanentIndex;
    int AttackTargetPermanentIndex;

    public AttackPermanentAction(byte[] bytes)
    {
        PermanentIndex = -1;
        AttackTargetPermanentIndex = -1;

        Deserialize(bytes);
    }

    public AttackPermanentAction(int permanentIndex, int attackTargetPermanentIndex)
    {
        PermanentIndex = permanentIndex;
        AttackTargetPermanentIndex = attackTargetPermanentIndex;
    }

    public override void Execute(TurnStateMachine stateMachine)
    {
        stateMachine.SetAttackingPermaent(PermanentIndex, AttackTargetPermanentIndex);
    }

    public override void Deserialize(byte[] bytes)
    {
        int index = 0;

        Protocol.Deserialize(out PermanentIndex, bytes, ref index);
        Protocol.Deserialize(out AttackTargetPermanentIndex, bytes, ref index);
    }

    public override byte[] Serialize()
    {
        byte[] bytes = new byte[sizeof(int) * 2];
        int index = 0;

        Protocol.Serialize(PermanentIndex, bytes, ref index);
        Protocol.Serialize(AttackTargetPermanentIndex, bytes, ref index);

        return bytes;
    }
}
