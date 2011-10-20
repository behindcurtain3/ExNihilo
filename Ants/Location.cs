using System;
using System.Collections.Generic;

namespace Ants {
	
	public class Location {

        public Location North
        {
            get
            {
                if (row == 0)
                    return new Location(GameState.Height - 1, col);
                else
                    return new Location(row - 1, col);
            }
        }

        public Location South
        {
            get
            {
                if (row + 1 == GameState.Height)
                    return new Location(0, col);
                else
                    return new Location(row + 1, col);
            }
        }

        public Location East
        {
            get
            {
                if (col + 1 == GameState.Width)
                    return new Location(row, 0);
                else
                    return new Location(row, col + 1);
            }
        }

        public Location West
        {
            get
            {
                if (col == 0)
                    return new Location(row, GameState.Width - 1);
                else
                    return new Location(row, col - 1);
            }
        }

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

    public class LocAndDist
    {
        public AntLoc Ant { get; set; }
        public Location Dest { get; set; }
        public int Distance { get; set; }

        // Should more than one ant move towards my destination?
        public Boolean Solo { get; set; }

        public LocAndDist(AntLoc a, Location l, int d, Boolean isSolo = true)
        {
            Ant = a;
            Dest = l;
            Distance = d;
            Solo = isSolo;
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

