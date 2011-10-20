using System;
using System.Collections.Generic;

namespace Ants {

	class MyBot : Bot {
        public static char NORTH = 'n';
        public static char EAST = 'e';
        public static char SOUTH = 's';
        public static char WEST = 'w';
		private GameState mState;

        private List<Location> previousPositions = new List<Location>();
        private List<Location> currentPositions = new List<Location>();
        private List<Location> currentMoves = new List<Location>();

		// doTurn is run once per turn
		public override void doTurn (GameState state) {
			mState = state;

            previousPositions = new List<Location>(currentPositions);
            currentPositions.Clear();
            currentMoves.Clear();

			// loop through all my ants and try to give them orders
			foreach (AntLoc ant in state.MyAnts) {
                if (getCloseItems(ant))
                    continue;

                // Go right then down then left then up
                Location newLoc = state.destination(ant, EAST);
                if (moveTo(ant, newLoc, EAST))
                    continue;
                newLoc = state.destination(ant, SOUTH);
                if (moveTo(ant, newLoc, SOUTH))
                    continue;
                newLoc = state.destination(ant, WEST);
                if (moveTo(ant, newLoc, WEST))
                    continue;
                newLoc = state.destination(ant, NORTH);
                if (moveTo(ant, newLoc, NORTH))
                    continue;
				
				// check if we have time left to calculate more orders
				if (state.TimeRemaining < 50) break;
			}
			
		}

        public Boolean getCloseItems(AntLoc ant)
        {
            List<Location> inRange = new List<Location>();

            foreach(Location food in mState.FoodTiles)
            {
                if (mState.distance(ant, food) < Math.Sqrt(mState.ViewRadius2))
                {
                    inRange.Add(food);
                }
            }

            foreach (HillLoc hill in mState.EnemyHills)
            {
                if (mState.distance(ant, hill) < Math.Sqrt(mState.ViewRadius2))
                {
                    inRange.Add(hill);
                }
            }

            if (inRange.Count == 0)
                return false;

            List<char> dirs = mState.getPath(ant, inRange);
            foreach (char dir in dirs)
            {
                Location newLoc = mState.destination(ant, dir);
                if (moveTo(ant, newLoc, dir))
                    return true;
            }

            return false;
        }

        public Boolean moveTo(AntLoc ant, Location dest, char dir)
        {
            if (mState.unoccupied(dest) && !previousPositions.Contains(dest) && !currentMoves.Contains(dest))
            {
                issueOrder(ant, dir);
                currentPositions.Add(ant);
                currentMoves.Add(dest);
                return true;
            }
            else
                return false;
        }
		
		public static void Main (string[] args) {
			new Ants().playGame(new MyBot());
		}

	}
	
}