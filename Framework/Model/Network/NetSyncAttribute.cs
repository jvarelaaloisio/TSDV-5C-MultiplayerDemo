using System;

namespace Model.Network
{
    /// <summary>
    /// Attribute meant to be added to each field in a custom packet
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NetSyncAttribute : Attribute
    {
        private readonly int _uniqueId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueId">The identification for the field, must be unique among all the synced fields in a class.</param>
        public NetSyncAttribute(int uniqueId)
        {
            _uniqueId = uniqueId;
        }
    }
}