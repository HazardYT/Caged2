using System;
using Unity.Collections;
using Unity.Netcode;
public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public FixedString32Bytes SteamName;

    public CharacterSelectState(ulong clientId, FixedString32Bytes steamName, int characterId = -1)
    {
        ClientId = clientId;
        CharacterId = characterId;
        SteamName = steamName;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref SteamName);
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
    }
    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId &&
            CharacterId == other.CharacterId;
    }
}
