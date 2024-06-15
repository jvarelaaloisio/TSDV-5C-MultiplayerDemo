using System;

namespace Model.Network
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class PacketAttribute : Attribute
    {
        private const short GroupLength = 6;
        public string Group { get; }
        public int UniqueId { get; }

        public PacketAttribute(string group, int uniqueId)
        {
            UniqueId = uniqueId;
            if (group.Length != GroupLength)
                throw new ArgumentOutOfRangeException(nameof(group), group,
                                                      $"Group must have a length of {GroupLength}!");
            Group = group[..GroupLength];
        }
    }
}