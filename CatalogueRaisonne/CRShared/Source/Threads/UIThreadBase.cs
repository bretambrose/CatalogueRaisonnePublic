﻿using System;
using System.Collections.Generic;

namespace CRShared
{
	public class CUIThreadBase : CTimeKeeperThread
	{
		// Construction
		protected CUIThreadBase() :
			base()
		{
		}
		
		static CUIThreadBase() {}
		
		// Methods
		// Public interface
		public void Add_Input_Request( CUIInputRequest request )
		{
			m_CurrentFrame.Add_Request( request );
		}

		// Protected interface and overrides	
		protected override void Initialize()
		{
			base.Initialize();
			base.Initialize_Time_Keeper( TimeKeeperExecutionMode.Internal, TimeKeeperTimeUnit.RealTime, TimeKeeperAuthority.Master, UI_SERVICE_INTERVAL, true, false );
			m_DataInterface = CUIFrameManager.Instance.UIInterface;	
		}

		protected override void Build_Thread_Frames() 
		{
			base.Build_Thread_Frames();

			if ( m_CurrentFrame == null )
			{
				m_CurrentFrame = new CUIInputFrame();
			}
		}

		protected override void Flush_Data_Frames( long current_time )
		{
			base.Flush_Data_Frames( current_time );

			if ( !m_CurrentFrame.Empty )
			{
				m_DataInterface.Send( m_CurrentFrame );
				m_CurrentFrame = null;
			}		
		}
				
		protected override void Service( long current_time )
		{
			base.Service( current_time );
			
			var incoming_frames = new List< CUIOutputFrame >();
			m_DataInterface.Receive( incoming_frames );

			Service_Incoming_Frames( incoming_frames );
		}
	
		// Private interface
		private void Service_Incoming_Frames( List< CUIOutputFrame > frames )
		{
			frames.Apply( frame => Service_Output_Frame( frame ) );
		}

		private void Service_Output_Frame( CUIOutputFrame frame )
		{
			foreach ( var notice in frame.Notifications )
			{
				if ( !CGenericHandlerManager.Instance.Try_Handle( notice ) )
				{
					throw new CApplicationException( "Unhandleable input notification" );
				}
			}
		}
		
		// Properties
		
		// Fields
		
		private const int UI_SERVICE_INTERVAL = 10;

		private ICrossThreadDataQueues< CUIInputFrame, CUIOutputFrame > m_DataInterface = null;
		private CUIInputFrame m_CurrentFrame = null;
	}
}
