using System;
using System.Collections.Generic;

public static class GamePacketFactory
{
    static readonly Dictionary<byte, Func<byte[], IGamePacket>> Factories = new();
    static readonly Dictionary<Type, byte> IdLookup = new();

    static byte NextID = 0;

    public static void Register<T>(Func<byte[], T> factory) where T : IGamePacket
    {
        Type type = typeof(T);

        if (IdLookup.ContainsKey(type))
        {
            return;
        }

        byte id = NextID++;

        IdLookup[type] = id;
        Factories[id] = bytes => factory(bytes);
    }

    public static IGamePacket Create(byte id, byte[] bytes)
    {
        if (!Factories.TryGetValue(id, out var factory))
        {
            return null;
        }

        return factory(bytes);
    }

    public static byte GetId(Type type)
    {
        if (!IdLookup.TryGetValue(type, out byte id))
        {
            throw new InvalidOperationException($"{type.Name} is not a registered packet");
        }

        return id;
    }
}