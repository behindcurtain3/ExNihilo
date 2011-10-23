using System;

namespace Ants {
	public abstract class Bot {

        public abstract void doSetup(GameState state);
		public abstract void doTurn(GameState state);
        public abstract void doEnd(GameState state);
		
		protected void issueOrder(Location loc, char direction) {
			System.Console.Out.WriteLine("o {0} {1} {2}", loc.row, loc.col, direction);
		}
	}
}