using System;

namespace CRShared
{
	public class CPlayerInfo
	{
		// Construction
		public CPlayerInfo( EPersistenceID persistence_id )
		{
			PersistenceID = persistence_id;
			Name = null;
		}

		public CPlayerInfo( EPersistenceID persistence_id, string name )
		{
			PersistenceID = persistence_id;
			Name = name;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( Name = {0}, PersistenceID = {1} )", Name, (long) PersistenceID );
		}

		// Properties
		public EPersistenceID PersistenceID { get; set; }
		public string Name { get; set; }
	}
}