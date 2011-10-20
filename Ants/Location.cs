using System;
using System.Collections.Generic;

namespace Ants {
	
	public class Location {
		
		public int row { get; private set; }
		public int col { get; private set; }
		
		public Location (int row, int col) {
			this.row = row;
			this.col = col;
		}

        public override string ToString()
        {
            return col.ToString() + "," + row.ToString();
        }

        public override bool Equals(object obj)
        {
            Location l = obj as Location;
            return (row == l.row && col == l.col);
        }

        public override int GetHashCode()
        {
            return row * int.MaxValue + col;
        }
	}
	
	public class AntLoc : Location {
		public int team { get; private set; }
		
		public AntLoc (int row, int col, int team) : base (row, col) {
			this.team = team;
		}
	}

    public class HillLoc : Location
    {
        public int team { get; private set; }

        public HillLoc(int row, int col, int team)
            : base(row, col)
        {
            this.team = team;
        }
    }

	public class LocationComparer : IEqualityComparer<Location> {
		public bool Equals(Location loc1, Location loc2) {
			return (loc1.row == loc2.row && loc1.col == loc2.col);
		}
	
		public int GetHashCode(Location loc) {
			return loc.row * int.MaxValue + loc.col;
		}
	}
}

