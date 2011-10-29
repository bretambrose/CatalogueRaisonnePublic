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