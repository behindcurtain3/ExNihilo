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
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }
        private ANode mParent;

        // Coord on map, not pixels
        public Location Coords
        {
            get
            {
                return mCoords;
            }
        }
        private Location mCoords;

        public float F;
        public float G;
        public float H;

        public Dictionary<ANode, Boolean> Neighbors
        {
            get
            {
                return mNeighbors;
            }
            set
            {
                mNeighbors = value;
            }
        }
        private Dictionary<ANode, Boolean> mNeighbors = new Dictionary<ANode, Boolean>();

        // Is this node passable?
        public Boolean Passable
        {
            get;
            set;
        }

        public ANode(Location l)
        {
            mCoords = l;
            Passable = true;
        }

    }
}
