using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Net
{
    [DataContract(Name = nameof(ClientInfo), Namespace = Namespace)]
    public sealed class ClientInfo : IExtensibleDataObject
    {
        public const string Namespace = "none";
        
        [DataMember]
        public readonly int ClientId;
        public ExtensionDataObject ExtensionData { get; set; }
    }
}
