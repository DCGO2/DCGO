using UnityEngine;

public static class PacketRegistration
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void RegisterAll()
    {
        GamePacketFactory.Register((btyes) => new AttackPermanentAction(btyes));
        GamePacketFactory.Register((btyes) => new ActivateCardAction(btyes));
        GamePacketFactory.Register((btyes) => new ActivatePermanentAction(btyes));
        GamePacketFactory.Register((btyes) => new CheatAction(btyes));
        GamePacketFactory.Register((btyes) => new PlayCardAction(btyes));
    }
}
