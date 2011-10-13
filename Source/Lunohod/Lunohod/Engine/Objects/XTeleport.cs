using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Lunohod.Objects
{
    [XmlType("Teleport")]
    public class XTeleport : XEdge
    {
        public XTeleport()
        {
            this.Type = XEdgeType.Teleport;
        }
    }
}
