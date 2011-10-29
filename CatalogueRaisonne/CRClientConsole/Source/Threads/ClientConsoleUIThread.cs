using System;
using System.Reflection;
using System.Threading;

using CRShared;
using CRClientShared;

namespace CRClientConsole
{
	public class CClientConsoleUIThread : CConsoleUIThread
	{
		static CClientConsoleUIThread() {}
		protected CClientConsoleUIThread() : 
			base() 
		{
			ScreenState = EUIScreenState.Startup;
		}

		// Methods
		// Public interface
		public static Thread Allocate_Thread()
		{
			return new Thread( Instance.Start );
		}

		// Non-public interface
		protected override void Initialize()
		{
			base.Initialize();

			Register_Generic_Handlers();
		}

		protected override bool Process_Console_Input( string input_string )
		{
			bool processed = base.Process_Console_Input( input_string );

			if ( !processed && input_string.Length > 0 )
			{
				CUIInputChatRequest request = new CUIInputChatRequest( input_string );		
				Add_Input_Request( request );					
			}

			return true;
		}

		private void Register_Generic_Handlers()
		{
			CGenericHandlerManager.Instance.Find_Handlers< CUILogicNotification >( Assembly.GetAssembly( typeof( CUILogicNotification ) ) );
			CGenericHandlerManager.Instance.Find_Handlers< CUILogicNotification >( Assembly.GetExecutingAssembly() );
		}

		[GenericHandler]
		private void Handle_Screen_State_Notification( CUIScreenStateNotification notice )
		{
			ScreenState = notice.ScreenState;
		}

		// Properties
		public static CClientConsoleUIThread Instance { get { return m_Instance; } }

		public EUIScreenState ScreenState { get; private set; }

		// Fields
		private static CClientConsoleUIThread m_Instance = new CClientConsoleUIThread();
	}
}

