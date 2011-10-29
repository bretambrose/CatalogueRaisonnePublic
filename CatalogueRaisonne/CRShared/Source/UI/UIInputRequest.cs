using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CRShared
{
	public enum EUIInputRequest
	{
		Invalid,
		SlashCommand,
		Chat
	}
	
	public class CUIInputRequest
	{
		public CUIInputRequest( EUIInputRequest request_type ) 
		{
			RequestType = request_type;
		}
		
		public EUIInputRequest RequestType { get; private set; }
	}
	
	public class CUIInputSlashCommandRequest : CUIInputRequest
	{
		// Construction
		public CUIInputSlashCommandRequest( string input_string ) :
			base( EUIInputRequest.SlashCommand )
		{
			m_OriginalText = input_string;
		}
		
		// Methods
		public bool Parse( Regex command_parser, int required_params, int max_params )
		{
			Match m = command_parser.Match( m_OriginalText );
			if ( !m.Success )
			{
				return false;
			}

			for ( int i = 1; i <= max_params; i++ )
			{
				string group_name = "param" + i.ToString();
				Group group = m.Groups[ group_name ];
				if ( group == null || !group.Success )
				{
					return i > required_params;
				}

				m_Parameters.Add( group.ToString() );
			}

			return true;
		}

		// Properties
		public string Command
		{
			get
			{
				Match m = Regex.Match( m_OriginalText, @"/(\w+).*" );
				if ( !m.Success )
				{
					return null;
				}
				else
				{
					return m.Groups[ 1 ].ToString();
				}
			}
		}

		public IEnumerable< string > Parameters { get { return m_Parameters; } }
		public int Count { get { return m_Parameters.Count; } }
		public string this[ int index ] { get { return m_Parameters[ index ]; } }

		// Fields
		private string m_OriginalText;
		private List< string > m_Parameters = new List< string >();
	}

	
}
