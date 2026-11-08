using System.Collections;
using System;

public partial class CardEffectCommons
{
    public static bool CanTriggerWhenSendToSecurity(
    Hashtable hashtable,
    Func<Player, bool> playerCondition)
    {
        Player player = GetPlayerFromHashtable(hashtable);

        if (player != null)
        {
            if (playerCondition == null || playerCondition(player))
            {
                return true;
            }
        }

        return false;
    }
}