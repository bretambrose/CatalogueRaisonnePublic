/*

	GameProperties.cs

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
