using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RNumerics
{
	public class GParallel
	{

		public static void ForEach_Sequential<T>(IEnumerable<T> source, Action<T> body) {
			foreach (var v in source) {
				body(v);
			}
		}
		public static void ForEach<T>(IEnumerable<T> source, Action<T> body) {
			Parallel.ForEach<T>(source, body);
		}


		/// <summary>
		/// Evaluate input actions in parallel
		/// </summary>
		public static void Evaluate(params Action[] funcs) {
			var N = funcs.Length;
			GParallel.ForEach(Interval1i.Range(N), (i) => funcs[i]());
		}



		/// <summary>
		/// Process indices [iStart,iEnd] *inclusive* by passing sub-intervals [start,end] to blockF.
		/// Blocksize is automatically determind unless you specify one.
		/// Iterate over [start,end] *inclusive* in each block
		/// </summary>
		public static void BlockStartEnd(int iStart, int iEnd, Action<int, int> blockF, int iBlockSize = -1, bool bDisableParallel = false) {
			if (iBlockSize == -1) {
				iBlockSize = 100;  // seems to work
			}

			var N = iEnd - iStart + 1;
			var num_blocks = N / iBlockSize;
			// process main blocks in parallel
			if (bDisableParallel) {
				ForEach_Sequential(Interval1i.Range(num_blocks), (bi) => {
					var k = iStart + (iBlockSize * bi);
					blockF(k, k + iBlockSize - 1);
				});
			}
			else {
				ForEach(Interval1i.Range(num_blocks), (bi) => {
					var k = iStart + (iBlockSize * bi);
					blockF(k, k + iBlockSize - 1);
				});
			}
			// process leftover elements
			var remaining = N - (num_blocks * iBlockSize);
			if (remaining > 0) {
				var k = iStart + (num_blocks * iBlockSize);
				blockF(k, k + remaining - 1);
			}
		}
	}








	// the idea for this class is that it provides a clean way to
	// process data through a stream of operations, where the ordering
	// of data members must be maintained. You provide a Producer and Consumer
	// as lambdas, and then provide an enumerator to define the order.
	// 
	// The idea is that between Producer and Consumer can be N intermediate
	// stages (Operators). However this is not implemented yet.
	//
	// If the compute steps are too short, it seems like this approach is not effective.
	// I think this is because the memory overhead of the Queue gets too high, as it
	// can get quite large, eg if the consumer is slower than the producer, or even
	// just can block (eg like writing to disk).
	// Also locking overhead will have some effect.
	//
	// Perhaps an alternative would be to use a fixed buffer of T?
	// However, if T is a class (eg by-reference), then this doesn't help as they still have to be allocated....
	public class ParallelStream<V, T>
	{
		public Func<V, T> ProducerF = null;
		//public List<Action<T>> Operators = new List<Action<T>>();
		public Action<T> ConsumerF = null;
		readonly LockingQueue<T> _store0 = new();
		IEnumerable<V> _source = null;


		// this is the non-threaded variant. useful for comparing/etc.
		public void Run_NoThreads(IEnumerable<V> sourceIn) {
			foreach (var v in sourceIn) {
				var product = ProducerF(v);
				//foreach (var op in Operators)
				//    op(product);
				ConsumerF(product);
			}
		}


		bool _producer_done = false;
		AutoResetEvent _consumer_done_event;

		//int max_queue_size = 0;

		public void Run(IEnumerable<V> sourceIn) {
			_source = sourceIn;
			_producer_done = false;
			_consumer_done_event = new AutoResetEvent(false);

			var producer = new Thread(ProducerThreadFunc) {
				Name = "ParallelStream_producer"
			};
			producer.Start();
			var consumer = new Thread(ConsumerThreadFunc) {
				Name = "ParallelStream_consumer"
			};
			consumer.Start();

			// wait for threads to finish
			_consumer_done_event.WaitOne();

			//System.Console.WriteLine("MAX QUEUE SIZE " + max_queue_size);       
		}



		void ProducerThreadFunc() {
			foreach (var v in _source) {
				var product = ProducerF(v);
				_store0.Add(product);
			}
			_producer_done = true;
		}


		void ConsumerThreadFunc() {
			// this just spins...is that a good idea??

			T next = default;
			while (_producer_done == false || _store0.Count > 0) {
				//max_queue_size = Math.Max(max_queue_size, store0.Count);
				var ok = _store0.Remove(ref next);
				if (ok) {
					ConsumerF(next);
				}
			}

			_consumer_done_event.Set();
		}

	}










	// locking queue - provides thread-safe sequential add/remove/count to Queue<T>
	public class LockingQueue<T>
	{
		readonly Queue<T> _queue;
		readonly object _queue_lock;

		public LockingQueue() {
			_queue = new Queue<T>();
			_queue_lock = new object();
		}

		public bool Remove(ref T val) {
			lock (_queue_lock) {
				if (_queue.Count > 0) {
					val = _queue.Dequeue();
					return true;
				}
				else {
					return false;
				}
			}
		}

		public void Add(T obj) {
			lock (_queue_lock) {
				_queue.Enqueue(obj);
			}
		}

		public int Count
		{
			get {
				lock (_queue_lock) {
					return _queue.Count;
				}
			}
		}
	}
}
