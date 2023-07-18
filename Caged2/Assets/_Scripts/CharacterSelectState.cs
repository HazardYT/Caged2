using System;
using Unity.Collections;
using Unity.Netcode;
public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public FixedString32Bytes name;

    public CharacterSelectState(ulong clientId, FixedString32Bytes steamName = default, int characterId = -1)
    {
        name = steamName;
        ClientId = clientId;
        CharacterId = characterId;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
    }
    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId &&
            CharacterId == other.CharacterId && 
            name == other.name;
    }
}
