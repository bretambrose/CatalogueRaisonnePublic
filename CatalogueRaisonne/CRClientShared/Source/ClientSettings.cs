/*

	ClientSettings.cs

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
using System.Xml;
using System.IO;
using System.Net;

namespace CRClientShared
{
	[DataContract(Name="ClientSettings",Namespace="http://www.bambrose.com")]
	public sealed class CClientSettings
	{
		// Construction
		static CClientSettings() {}
		CClientSettings() { }

		// Methods
		// Public interface
		static public void Initialize( string filename )
		{
			FileName = filename;
		}
	
		static public void Load_Settings()
		{
			DataContractSerializer serializer = new DataContractSerializer( typeof( CClientSettings ) );
			using ( Stream stream = File.OpenRead( FileName ) )
			{
				m_Instance = serializer.ReadObject( stream ) as CClientSettings;
			}
			
			m_Instance.IsDirty = false;			
		}
		
		static public void Save_Settings()
		{
			if ( !m_Instance.IsDirty )
			{
				return;
			}
			
			try
			{
				DataContractSerializer serializer = new DataContractSerializer( typeof( CClientSettings ) );
				XmlWriterSettings output_settings = new XmlWriterSettings() { Indent = true };
				
				using ( XmlWriter writer = XmlWriter.Create( FileName, output_settings ) )
				{
					serializer.WriteObject( writer, m_Instance );
				}
				
				m_Instance.IsDirty = false;
			}
			catch ( Exception e )
			{
				CClientResource.Output_Text( EClientTextID.Client_Error_Saving_Settings, e.Message );
			}
		}
		
		// Private interface
		[OnDeserialized]
		private void On_Post_Load( StreamingContext context )
		{
			m_ServerAddress = IPAddress.Parse( m_ServerAddressAsString );
		}
		
		[OnSerializing]
		private void On_Pre_Save( StreamingContext context )
		{
			m_ServerAddressAsString = m_ServerAddress.ToString();
		}
		
		// Properties
		public static CClientSettings Instance { get { return m_Instance; } }

		public bool IsDirty { get { return m_IsDirty; } private set { m_IsDirty = value; } }
		
		public IPAddress ServerAddress { 
			get { return m_ServerAddress; } 
			set { 
				if ( !m_ServerAddress.Equals( value ) )
				{
					m_ServerAddress = value;
					IsDirty = true;
				}
			} 
		}

				
		// Fields
		private static CClientSettings m_Instance = new CClientSettings();
		
		private static string FileName = null;
		
		[DataMember(Name="ServerAddress",IsRequired=true)]
		private string m_ServerAddressAsString = null;
		private IPAddress m_ServerAddress = null;			
		private bool m_IsDirty = false;
	}	
}
