using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;

namespace CRShared
{
	[DataContract(Name="SharedSettings",Namespace="http://www.bambrose.com")]
	public sealed class CSharedSettings
	{
		// Construction
		static CSharedSettings() {}
		CSharedSettings() {}

		// Methods
		// Public interface
		static public void Initialize( string filename )
		{
			m_FileName = filename;
		}
	
		static public void Load_Settings()
		{
			DataContractSerializer serializer = new DataContractSerializer( typeof ( CSharedSettings ) );
			using ( Stream stream = File.OpenRead( m_FileName ) )
			{
				m_Instance = serializer.ReadObject( stream ) as CSharedSettings;
			}			
		}
		
		// Properties
		public static CSharedSettings Instance { get { return m_Instance; } }

		public int ListenerPort { get { return m_ListenerPort; } }
		
		// Fields
		static CSharedSettings m_Instance = new CSharedSettings();

		static private string m_FileName = null;
		
		[DataMember(Name="ListenerPort",IsRequired=true)]
		int m_ListenerPort = 0;
	}	
}
