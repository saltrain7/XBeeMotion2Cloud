using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Runtime.Serialization;

namespace WCFServiceWebRole1
{
    [DataContract]
    public class ConfRoomDto
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string TimeS { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public int Motion { get; set; }
    }
}

