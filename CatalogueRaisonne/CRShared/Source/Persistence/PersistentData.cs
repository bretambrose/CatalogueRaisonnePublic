/*

	PersistentData.cs

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
using System.Text;

namespace CRShared
{
	public class CPersistentPlayerData
	{
		// constructors
		public CPersistentPlayerData( EPersistenceID id, string name )
		{
			ID = id;
			Name = name;
			IgnoreList = new List< EPersistenceID >();
		}

		// Methods
		// Public interface
		public CPersistentPlayerData Clone()
		{
			CPersistentPlayerData clone = new CPersistentPlayerData( ID, Name );

			IgnoreList.ShallowCopy( clone.IgnoreList );

			return clone;
		}

		public override string ToString()
		{
			return String.Format( "( [CPersistentPlayerData] ID = {0}, Name = {1}, IgnoreList = {2} )", (long)ID, Name, CSharedUtils.Build_String_List( IgnoreList, n => ((long) n).ToString() ) );
		}

		// Properties
		public EPersistenceID ID { get; private set; }
		public string Name { get; private set; }
		public List< EPersistenceID > IgnoreList { get; private set; }
	}
}