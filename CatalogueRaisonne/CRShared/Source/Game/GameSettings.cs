/*

	GameSettings.cs

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
	[Flags]
	public enum EPlayerMode
	{
		Invalid			= 0,

		Player			= 1 << 0,
		Observer			= 1 << 1
	}

	[Flags]
	public enum EGameModeType
	{
		Invalid				= 0,

		Two_Players			= 1 << 0,
		Four_Players		= 1 << 1
	}

	public enum EGameSide
	{
		Side1,
		Side2
	}

	public static class CGameModeUtils
	{
		public static uint Player_Count_For_Game_Mode( EGameModeType mode )
		{
			switch ( mode )
			{
				case EGameModeType.Two_Players:
					return 2;

				case EGameModeType.Four_Players:
					return 4;
			}

			return 0;
		}
	}


}
