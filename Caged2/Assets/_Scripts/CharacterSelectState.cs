using System;
using Unity.Collections;
using Unity.Netcode;
public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public bool IsLockedIn;
    public FixedString32Bytes Name;

    public CharacterSelectState(ulong clientId, FixedString32Bytes steamName, int characterId = -1, bool isLockedIn = false)
    {
        Name = steamName;
        ClientId = clientId;
        CharacterId = characterId;
        IsLockedIn = isLockedIn;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref IsLockedIn);
    }
    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId &&
            CharacterId == other.CharacterId && 
            Name == other.Name &&
            IsLockedIn == other.IsLockedIn;
    }
}
