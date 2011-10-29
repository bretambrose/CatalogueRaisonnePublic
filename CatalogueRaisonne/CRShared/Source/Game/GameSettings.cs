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
