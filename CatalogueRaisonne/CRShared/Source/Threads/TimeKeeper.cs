using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CRShared
{
	enum ETimeKeeperErrorCode
	{
		None,

		InvalidOperation,
		InvalidSetting,
		InvalidTimeUnit
	}
	
	class CTimeKeeperException : CApplicationException
	{
		// Construction
		public CTimeKeeperException( ETimeKeeperErrorCode error_code, string error_message ) :
			base( error_message )
		{
			ErrorCode = error_code;
		}

		public CTimeKeeperException( ETimeKeeperErrorCode error_code ) :
			base()
		{
			ErrorCode = error_code;
		}
		
		// methods
		public override string ToString() { return string.Format( "Time Keeper Exception - Code({0}), Message({1})", ErrorCode, ErrorMessage ); }
		
		// properties
		public ETimeKeeperErrorCode ErrorCode { get; private set; }
	}
	
	public enum TimeKeeperExecutionMode
	{
		Internal,		// time is advanced internally and execution never leaves the scope of the timekeeper until it is permanently finished
		External			// time is advanced by an external agent, and so execution leaves the timekeeper after every frame
	}
	
	public enum TimeKeeperTimeUnit
	{
		RealTime,		// advancement is based on real time
		LogicalTime		// advancement is in logical time
	}
	
	public enum TimeKeeperAuthority
	{
		Master,			
		Slave
	}
	
	public delegate void TimeKeeperCallback( long current_time );
	
	public class CTimeKeeperThread
	{
		// Construction
		public CTimeKeeperThread()
		{
			CurrentTime = 0;
			m_TimeKeeperInstance = this;
		}
		
		// Methods
		// Public interface
		public void Initialize_Time_Keeper( TimeKeeperExecutionMode execution_mode, TimeKeeperTimeUnit time_unit, TimeKeeperAuthority authority, long frame_length, bool is_thread_timer, bool should_catchup )
		{
			// can't call this after execution has started
			if ( m_ElapsedTime.IsRunning )
			{
				throw new CTimeKeeperException( ETimeKeeperErrorCode.InvalidOperation, "Cannot change timer keeper settings while it's running." );
			}
			
			if ( is_thread_timer ^ ( execution_mode == TimeKeeperExecutionMode.Internal ) )
			{
				throw new CTimeKeeperException( ETimeKeeperErrorCode.InvalidSetting, "A time keeper can execute internally if and only if it is a thread timer." );
			}
			
			ExecutionMode = execution_mode;
			TimeUnit = time_unit;
			Authority = authority;
			m_FrameLength = frame_length;
			IsDone = false;
			m_IsThreadTimer = is_thread_timer;
			m_ShouldCatchup = should_catchup;
						
			m_ElapsedTime.Start();
		}

		// Internal execution interface
		public void Run()
		{
			if ( ExecutionMode != TimeKeeperExecutionMode.Internal )
			{
				throw new CTimeKeeperException( ETimeKeeperErrorCode.InvalidOperation, "Cannot run a timekeeper unless its execution mode is internal." );
			}
			
			while ( !IsDone )
			{
				Advance_Current_Time();
				m_CurrentThreadTime = CurrentTime;			
				Execute_Single_Frame();	
				Sleep_Time_Keeper();
			}		
		}
		
		// External execution interface		
		public void Manual_Advance()
		{
			if ( ExecutionMode != TimeKeeperExecutionMode.External )
			{
				throw new CTimeKeeperException( ETimeKeeperErrorCode.InvalidOperation, "Cannot manually advance a timekeeper unless its execution mode is external." );
			}
			
			Advance_Current_Time();
			Execute_Single_Frame();
		}

		public static long Convert_Seconds_To_Internal_Time( int seconds )
		{
			return Convert_Seconds_To_Internal_Time( ( double ) seconds );
		}

		public static long Convert_Seconds_To_Internal_Time( double seconds )
		{
			return ( long )( seconds * 1000.0 );
		}

		// Kinda ugly, but otherwise no way to start a message send from CRShared; override in the appropriate places on client/server
		public virtual void Send_Message( CNetworkMessage message, ESessionID destination_id ) {}
	
		// Protected interface
		protected void Start()
		{
			try
			{
				Initialize();
				Run();
			}
			catch ( CQuitException )
			{
				m_ThreadAborted = true;
			}
			catch ( Exception e )
			{
				CLog.Log_Fatal_Exception( e );
				m_ThreadAborted = true;
			}
			finally
			{
				Shutdown();
			}	
		}

		protected virtual void Initialize() 
		{
			CGenericHandlerManager.Initialize_Thread_Instance();

			PreTaskCallbacks += Service;
			PostTaskCallbacks += Flush_Data_Frames;
			PostTaskCallbacks += Check_For_Shutdown;
		}

		protected virtual void Shutdown() {}

		protected virtual void Service( long current_time ) 
		{
			Build_Thread_Frames();
		}

		protected virtual void Build_Thread_Frames() {}

		protected virtual void Flush_Data_Frames( long current_time )
		{
			CLog.Flush_Frame();
		}

		protected virtual void Check_For_Shutdown( long current_time )
		{
			if ( m_ThreadAborted )
			{
				IsDone = true;
			}
		}

		// private interface
		private void Advance_Current_Time()
		{
			switch ( TimeUnit )
			{
				case TimeKeeperTimeUnit.RealTime:
					CurrentTime = m_ElapsedTime.ElapsedMilliseconds;
					break;
					
				case TimeKeeperTimeUnit.LogicalTime:
					CurrentTime = CurrentTime + m_FrameLength;
					break;
			}
		}
		
		private void Execute_Single_Frame()
		{
			long current_time = CurrentTime;
			if ( current_time <= m_LastExecuteTime )
			{
				return;
			}
			
			if ( PreTaskCallbacks != null )
			{	
				PreTaskCallbacks( current_time );
			}
					
			m_Scheduler.Service_Tasks( current_time );
			
			if ( PostTaskCallbacks != null )
			{
				PostTaskCallbacks( current_time );
			}
			
			m_LastExecuteTime = current_time;
			
			m_FrameCount++;
		}
		
		private void Sleep_Time_Keeper()
		{
			long current_elapsed_time = m_ElapsedTime.ElapsedMilliseconds;

			long next_execution_time;
			if ( m_ShouldCatchup )
			{
				next_execution_time = m_FrameCount * m_FrameLength;
			}
			else
			{
				next_execution_time = current_elapsed_time + m_FrameLength;
			}

			long sleep_time = next_execution_time - current_elapsed_time;
			if ( sleep_time > 0 )
			{
				Thread.Sleep( (int) sleep_time );
			}
			else
			{
				Thread.Sleep( 0 );
			}
		}
			
		// Properties
		public TimeKeeperExecutionMode ExecutionMode { get; private set; }
		public TimeKeeperTimeUnit TimeUnit { get; private set; }
		public TimeKeeperAuthority Authority { get; private set; }
		public long CurrentTime { get; private set; }
		public CTaskScheduler TaskScheduler { get { return m_Scheduler; } }
		public bool IsDone { get; protected set; }
		public event TimeKeeperCallback PreTaskCallbacks;
		public event TimeKeeperCallback PostTaskCallbacks;
		
		public static long CurrentThreadTime { get { return m_CurrentThreadTime; } }
		public static CTimeKeeperThread TimeKeeperInstance { get { return m_TimeKeeperInstance; } }
		
		// Fields		
		private CTaskScheduler m_Scheduler = new CTaskScheduler();
		private Stopwatch m_ElapsedTime = new Stopwatch();
		
		private long m_FrameLength = 100;
		private long m_LastExecuteTime = -1;
		private long m_FrameCount = 0;
		private bool m_IsThreadTimer;
		private bool m_ShouldCatchup;
		
		[ThreadStatic]
		private static long m_CurrentThreadTime;
		
		[ThreadStatic]
		private static CTimeKeeperThread m_TimeKeeperInstance = null;

		protected static bool m_ThreadAborted = false;
	}
}
