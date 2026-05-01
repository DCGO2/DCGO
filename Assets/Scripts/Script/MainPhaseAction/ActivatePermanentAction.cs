using ExitGames.Client.Photon;

public class ActivatePermanentAction : MainPhaseAction
{
    int PermanentIndex;
    int SkillIndex;

    public ActivatePermanentAction(int permanentIndex, int skillIndex)
    {
        PermanentIndex = permanentIndex;
        SkillIndex = skillIndex;
    }

    public ActivatePermanentAction(byte[] bytes)
    {
        PermanentIndex = -1;
        SkillIndex = -1;
        Deserialize(bytes);
    }

    public override void Execute(TurnStateMachine stateMachine)
    {
        stateMachine.SetActSkill(PermanentIndex, SkillIndex);
    }

    public override void Deserialize(byte[] bytes)
    {
        int index = 0;

        Protocol.Deserialize(out PermanentIndex, bytes, ref index);
        Protocol.Deserialize(out SkillIndex, bytes, ref index);
    }

    public override byte[] Serialize()
    {
        byte[] bytes = new byte[sizeof(int) * 2];
        int index = 0;

        Protocol.Serialize(PermanentIndex, bytes, ref index);
        Protocol.Serialize(SkillIndex, bytes, ref index);

        return bytes;
    }
}
