using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Model.Network
{
    public class PacketProvider
    {
        private Dictionary<string, TypeGroup> _groups;

        private record TypeGroup
        {
            public Dictionary<int, Type> TypesById = new();
        }

        public void Initialize()
        {
            _groups = new();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var packet in assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(PacketAttribute), inherit: false).Length != 0))
                {
                    var packetAttribute = packet.GetCustomAttribute<PacketAttribute>();
                    var group = packetAttribute.Group;
                    var id = packetAttribute.UniqueId;
                    if (_groups.TryGetValue(group, out var classGroup))
                    {
                        if (!classGroup.TypesById.TryAdd(id, packet))
                        {
                            throw new
                                DuplicateNameException($"Found more than one type with unique Id: {id} in the group: {group}");
                        }
                    }
                    else
                        _groups.Add(group, new TypeGroup { TypesById = new Dictionary<int, Type> {{id, packet}}});
                }
            }
        }

        public bool TryGetType(string typeGroup, int uniqueId, out Type type)
        {
            type = default;
            return _groups.TryGetValue(typeGroup, out var group)
                   && group.TypesById.TryGetValue(uniqueId, out type);
            //TODO: Remove below code if the one above works.
            // if (_groups.TryGetValue(typeGroup, out var group))
            //     return group.TypesById.TryGetValue(uniqueId, out type);
            // type = default;
            // return false;
        }
    }
}