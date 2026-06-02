using ExitGames.Client.Photon;

public class CheatAction : MainPhaseAction
{
    public enum Type
    {
        None,
        Draw,
        TrashCard,
        PlaceCardOnDeck,
        PlaceCardInSecurity,
        PlaceCardInSecurityFaceup,
        GainMemory,
        LoseMemory,
        RemoveFromSecurity,
    }

    Type CheatType;
    int PlayerID;

    public CheatAction(int playerID, Type cheatType)
    {
        PlayerID = playerID;
        CheatType = cheatType;
    }

    public CheatAction(byte[] bytes)
    {
        CheatType = Type.None;
        PlayerID = -1;
        Deserialize(bytes);
    }

    public override void Execute(TurnStateMachine stateMachine)
    {
        GManager gameManager = GManager.instance;

        Player player = gameManager.GetPlayerFromID(PlayerID);

        if (player == null)
        {
            return;
        }

        if (gameManager.AllowCheats())
        {
            switch (CheatType)
            {
                case Type.Draw:
                    gameManager.StartCoroutine(gameManager.DrawCard(player));
                    break;
                case Type.TrashCard:
                    gameManager.StartCoroutine(gameManager.TrashCard(player));
                    break;
                case Type.PlaceCardOnDeck:
                    gameManager.StartCoroutine(gameManager.TopDeckCard(player));
                    break;
                case Type.PlaceCardInSecurity:
                    gameManager.StartCoroutine(gameManager.PlaceInSecurity(player, false));
                    break;
                case Type.PlaceCardInSecurityFaceup:
                    gameManager.StartCoroutine(gameManager.PlaceInSecurity(player, true));
                    break;
                case Type.GainMemory:
                    gameManager.StartCoroutine(gameManager.AlterMemory(player, 1));
                    break;
                case Type.LoseMemory:
                    gameManager.StartCoroutine(gameManager.AlterMemory(player, -1));
                    break;
                case Type.RemoveFromSecurity:
                    gameManager.StartCoroutine(gameManager.RemoveFromSecurity(player));
                    break;
                default:
                    break;
            }
        }
    }

    public override void Deserialize(byte[] bytes)
    {
        int index = 0;

        Protocol.Deserialize(out int cheatTypeInt, bytes, ref index);
        CheatType = (Type)cheatTypeInt;

        Protocol.Deserialize(out PlayerID, bytes, ref index);
    }


    public override byte[] Serialize()
    {
        byte[] bytes = new byte[sizeof(int) * 2];
        int index = 0;

        Protocol.Serialize((int)CheatType, bytes, ref index);
        Protocol.Serialize(PlayerID, bytes, ref index);

        return bytes;
    }
}
