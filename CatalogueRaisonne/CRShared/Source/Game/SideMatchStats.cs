/*

	SideMatchStats.cs

	(c) Copyright 2010-2011, Bret Ambrose (mailto:bretambrose@gmail.com).

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
 
*/

using System;

namespace CRShared
{
	public class CSideMatchStats : ISimpleClonable< CSideMatchStats >
	{
		// Nested Types
		public class CSideMatchStatsDelta : CAbstractMatchDelta
		{
			public CSideMatchStatsDelta( EGameSide side, int score, uint games_lost, uint games_won, uint games_drawn )
			{
				Side = side;
				Score = score;
				GamesLost = games_lost;
				GamesWon = games_won;
				GamesDrawn = games_drawn;
			}

			// Public interface
			public override void Apply( CMatchState match_state )
			{
				CSideMatchStats side_stats = match_state.Get_Side_Data( Side );

				side_stats.Score = Score;
				side_stats.GamesWon = GamesWon;
				side_stats.GamesLost = GamesLost;
				side_stats.GamesDrawn = GamesDrawn;
			}

			// Properties
			public override EMatchDeltaType Type { get { return EMatchDeltaType.Side_Stats; } }

			public EGameSide Side { get; private set; }
			public int Score { get; private set; }
			public uint GamesWon { get; private set; }
			public uint GamesLost { get; private set; }
			public uint GamesDrawn { get; private set; }
		}

		// Construction
		public CSideMatchStats()
		{
			Score = 0;
			GamesWon = 0;
			GamesLost = 0;
			GamesDrawn = 0;
		}

		// Public Interface
		public CSideMatchStats Clone()
		{
			CSideMatchStats clone = new CSideMatchStats();

			clone.Score = Score;
			clone.GamesLost = GamesLost;
			clone.GamesWon = GamesWon;
			clone.GamesDrawn = GamesDrawn;

			return clone;
		}

		public override string ToString()
		{
			return String.Format( "( [CSideMatchStats] Score = {0}, GamesWon = {1}, GamesLost = {2}, GamesDrawn = {3} )", Score, GamesWon, GamesLost, GamesDrawn );
		}

		// Properties
		public int Score { get; private set; }
		public uint GamesWon { get; private set; }
		public uint GamesLost { get; private set; }
		public uint GamesDrawn { get; private set; }
	}
}
