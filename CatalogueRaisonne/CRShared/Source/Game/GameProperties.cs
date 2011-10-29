using System;
using System.Collections.Generic;

namespace CRShared
{
	public static class CGameProperties
	{
		static CGameProperties() 
		{
			m_CardColorSet.Add( ECardColor.Blue );
			m_CardColorSet.Add( ECardColor.Red );
			m_CardColorSet.Add( ECardColor.Green );
			m_CardColorSet.Add( ECardColor.Yellow );
			m_CardColorSet.Add( ECardColor.White );
		}

		public static IEnumerable< ECardColor > Get_Card_Colors() { return m_CardColorSet; }

		public static uint StartingHandSize { get { return 8; } }

		private static List< ECardColor > m_CardColorSet = new List< ECardColor >();
	}
}
