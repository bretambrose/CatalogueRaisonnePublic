/*

	ServerSettings.cs

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
