using System;
using System.Collections.Generic;
using System.Text;

namespace Ants
{
    public class ANode
    {
        // Used in A*
        public ANode Parent
        {
            get;
            set;
        }
        // Coord on map, not pixels
        public Location Location
        {
            get;
            private set;
        }

        public float F;
        public float G;
        public float H;

        public List<ANode> Neighbors
        {
            get;
            private set;
        }

        // Is this node passable?
        public Boolean Passable
        {
            get;
            set;
        }

        public ANode(Location l)
        {
            Location = l;
            Passable = true;
            Neighbors = new List<ANode>();
        }

    }
}
