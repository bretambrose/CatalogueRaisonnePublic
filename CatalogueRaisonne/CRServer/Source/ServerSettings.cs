using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;

namespace CRServer
{
	[DataContract(Name="ServerSettings",Namespace="http://www.bambrose.com")]
	public sealed class CServerSettings
	{
		// Construction
		static CServerSettings() {}
		CServerSettings() {}

		// Methods
		// Public interface
		static public void Initialize( string filename )
		{
			FileName = filename;
		}
	
		static public bool Load_Settings()
		{
			DataContractSerializer serializer = new DataContractSerializer( typeof( CServerSettings ) );
			using ( Stream stream = File.OpenRead( FileName ) )
			{
				m_Instance = serializer.ReadObject( stream ) as CServerSettings;
			}
			
			return true;
		}

		// Properties
		public long DisconnectTimeout { get { return m_DisconnectTimeout * 1000; } }

		public static CServerSettings Instance { get { return m_Instance; } }

		// Fields
		static private CServerSettings m_Instance = new CServerSettings();
		
		static private string FileName = null;
		
		[DataMember(Name="DisconnectTimeout",IsRequired=true)]
		private long m_DisconnectTimeout = 300;

	}	
}
