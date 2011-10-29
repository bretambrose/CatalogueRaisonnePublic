using System;
using System.Reflection;
using System.Threading;

using CRShared;

namespace CRServer
{
	public class CServerConsoleUIThread : CConsoleUIThread
	{
		static CServerConsoleUIThread() {}
		protected CServerConsoleUIThread() : base() {}

		// Methods
		// Public interface
		public static Thread Allocate_Thread()
		{
			return new Thread( Instance.Start );
		}

		// Protected interface and overrides
		protected override void Initialize()
		{
			base.Initialize();

			Register_Generic_Handlers();
		}

		// Private interface
		private void Register_Generic_Handlers()
		{
			CGenericHandlerManager.Instance.Find_Handlers< CUILogicNotification >( Assembly.GetAssembly( typeof( CUILogicNotification ) ) );
			CGenericHandlerManager.Instance.Find_Handlers< CUILogicNotification >( Assembly.GetExecutingAssembly() );
		}

		// Properties
		public static CServerConsoleUIThread Instance { get { return m_Instance; } }

		// Fields
		private static CServerConsoleUIThread m_Instance = new CServerConsoleUIThread();
	}
}

