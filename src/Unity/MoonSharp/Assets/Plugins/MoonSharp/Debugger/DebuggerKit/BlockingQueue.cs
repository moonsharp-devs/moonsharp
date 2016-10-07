using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MoonSharp.DebuggerKit
{
	// Taken from http://element533.blogspot.it/2010/01/stoppable-blocking-queue-for-net.html
	internal class BlockingQueue<T>
	{
		private readonly Queue<T> _queue = new Queue<T>();
		private bool _stopped;

		public bool Enqueue(T item)
		{
			if (_stopped)
				return false;
			lock (_queue)
			{
				if (_stopped)
					return false;
				_queue.Enqueue(item);
				Monitor.Pulse(_queue);
			}
			return true;
		}

		public T Dequeue()
		{
			if (_stopped)
				return default(T);
			lock (_queue)
			{
				if (_stopped)
					return default(T);
				while (_queue.Count == 0)
				{
					Monitor.Wait(_queue);
					if (_stopped)
						return default(T);
				}
				return _queue.Dequeue();
			}
		}

		public void Stop()
		{
			if (_stopped)
				return;
			lock (_queue)
			{
				if (_stopped)
					return;
				_stopped = true;
				Monitor.PulseAll(_queue);
			}
		}
	}
}
