public interface IGamePacket
{
    byte[] Serialize();
    void Deserialize(byte[] bytes);
}
