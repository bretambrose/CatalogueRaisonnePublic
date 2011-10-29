using System;
using System.Threading;
using System.Collections.Generic;

namespace CRShared
{
	public class CScheduledTask : IComparable< CScheduledTask >, IComparable
	{
		// Construction
		public CScheduledTask()
		{
			RunTime = 0;
		}
		
		public CScheduledTask( long time )
		{
			RunTime = time;
		}
		
		// Methods
		// Public interface
		public virtual void Execute( long current_time ) 
		{
		}
		
		public int CompareTo( CScheduledTask task )
		{
			return RunTime.CompareTo( task.RunTime );
		}
		
		int IComparable.CompareTo( object other )
		{
			if ( !( other is CScheduledTask ) )
			{
				throw new InvalidOperationException( "CompareTo: expected second operand to be a scheduled task" );
			}
			
			return CompareTo( other as CScheduledTask );
		}
		
		// Properties
		public virtual long RerunDelta { get { return -1; } protected set {} }
		public long RunTime { get; set; }
	}
	
	public class CRecurrentTask : CScheduledTask
	{
		// Construction
		public CRecurrentTask( long time, long recurrence_time ) :
			base( time )
		{
			RerunDelta = recurrence_time;
		}
		
		// Properties
		public override long RerunDelta { get; protected set; }		
	}
	
	public interface IOrderedTask
	{
		void Execute();
	}

	public class CTaskScheduler 
	{
		// Construction
		public CTaskScheduler()
		{
			m_ScheduledTasks = new THeap< CScheduledTask >();
			m_OrderedTasks = new List< IOrderedTask >();
		}
		
		// methods
		// Public interface
		public void Service_Tasks( long current_time )
		{	
			CurrentScheduler = this;
					
			while ( !m_ScheduledTasks.Empty )
			{
				if ( m_ScheduledTasks.Peek_Top().RunTime > current_time )
				{
					break;
				}
				
				CScheduledTask task = m_ScheduledTasks.Pop_Top();

				CLog.Log( ELoggingChannel.Logic, ELogLevel.Medium, String.Format( "Executing task of type {0}", task.GetType().Name ) );		

				task.Execute( current_time );
				
				long recurrence_delta = task.RerunDelta;
				if ( recurrence_delta > 0 )
				{
					task.RunTime = current_time + recurrence_delta;
					Add_Scheduled_Task( task );
				}
			}
			
			while ( m_OrderedTasks.Count > 0 )
			{
				List< IOrderedTask > ordered_copy = new List< IOrderedTask >( m_OrderedTasks );
				m_OrderedTasks.Clear();

				foreach ( var task in ordered_copy )
				{
					task.Execute();
				}
			}

			CurrentScheduler = null;
		}
		
		public void Add_Scheduled_Task( CScheduledTask task )
		{
			m_ScheduledTasks.Add( task );
		}

		public void Add_Ordered_Task( IOrderedTask task )
		{
			m_OrderedTasks.Add( task );
		}

		// properties
		public static CTaskScheduler CurrentScheduler { get { return m_CurrentScheduler; } private set { m_CurrentScheduler = value; } }
		
		// fields		
		private THeap< CScheduledTask > m_ScheduledTasks;
		private List< IOrderedTask > m_OrderedTasks;
		
		[ThreadStatic]
		private static CTaskScheduler m_CurrentScheduler;
		
	}
}
