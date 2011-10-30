/*

	SharedSettings.cs

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
