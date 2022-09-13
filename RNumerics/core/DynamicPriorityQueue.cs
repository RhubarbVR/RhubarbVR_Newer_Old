using System;
using System.Collections;
using System.Collections.Generic;

namespace RNumerics
{


	/// <summary>
	/// To use DynamicPriorityQueue, your queue node type needs to subclass this one.
	/// However the priority and index members are for internal queue use, not yours!
	/// </summary>
	public abstract class DynamicPriorityQueueNode
	{
		// queue priority value for this node. never modify this value!
		public float Priority { get; protected internal set; }

		// current position in the queue's tree/array. not meaningful externally, do not use this value for anything!!
		internal int Index { get; set; }
	}




	/// <summary>
	/// This is a min-heap priority queue class that does not use any fixed-size internal data structures.
	/// It is maent mainly for use on subsets of larger graphs.
	/// If you need a PQ for a larger portion of a graph, consider IndexPriorityQueue instead.
	/// 
	/// You need to subclass DynamicPriorityQueueNode, and *you* allocate the nodes, not the queue.
	/// If there is a chance you will re-use nodes, consider using a MemoryPool<T>.
	/// See DijkstraGraphDistance for example usage.
	/// 
	/// conceptually based on https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
	/// </summary>
	public class DynamicPriorityQueue<T> : IEnumerable<T>
		where T : DynamicPriorityQueueNode
	{
		// set this to true during development to catch issues
		public bool EnableDebugChecks = false;

		DVector<T> _nodes;       // tree of allocated nodes, stored linearly. active up to num_nodes (allocated may be larger)



		public DynamicPriorityQueue() {
			Count = 0;
			_nodes = new DVector<T>();
		}

		/// <summary>
		/// number of nodes currently in queue
		/// </summary>
		public int Count { get; private set; }


		/// <summary>
		/// reset the queue to empty state. 
		/// if bFreeMemory is false, we don't discard internal data structures, so there will be less allocation next time
		/// (this does not make a huge difference...)
		/// </summary>
		public void Clear(in bool bFreeMemory = true) {
			if (bFreeMemory) {
				_nodes = new DVector<T>();
			}

			Count = 0;
		}

		/// <summary>
		/// node at head of queue
		/// </summary>
		public T First => _nodes[1];

		/// <summary>
		/// Priority of node at head of queue
		/// </summary>
		public float FirstPriority => _nodes[1].Priority;


		/// <summary>
		/// constant-time check to see if node is already in queue
		/// </summary>
		public bool Contains(in T node) {
			return _nodes[node.Index] == node;
		}


		/// <summary>
		/// Add node to list w/ given priority
		/// Behavior is undefined if you call w/ same node twice
		/// </summary>
		public void Enqueue(in T node, in float priority) {
			if (EnableDebugChecks && Contains(node) == true) {
				throw new Exception("DynamicPriorityQueue.Enqueue: tried to add node that is already in queue!");
			}

			node.Priority = priority;
			Count++;
			_nodes.Insert(node, Count);
			node.Index = Count;
			Move_up(_nodes[Count]);
		}

		/// <summary>
		/// remove node at head of queue, update queue, and return that node
		/// </summary>
		public T Dequeue() {
			if (EnableDebugChecks && Count == 0) {
				throw new Exception("DynamicPriorityQueue.Dequeue: queue is empty!");
			}

			var returnMe = _nodes[1];
			Remove(returnMe);
			return returnMe;
		}



		/// <summary>
		/// remove this node from queue. Undefined behavior if called w/ same node twice!
		/// Behavior is undefined if you call w/ node that is not in queue
		/// </summary>
		public void Remove(in T node) {
			if (EnableDebugChecks && Contains(node) == false) {
				throw new Exception("DynamicPriorityQueue.Remove: tried to remove node that does not exist in queue!");
			}

			//If the node is already the last node, we can remove it immediately
			if (node.Index == Count) {
				_nodes[Count] = null;
				Count--;
				return;
			}

			//Swap the node with the last node
			var formerLastNode = _nodes[Count];
			Swap_nodes(node, formerLastNode);
			_nodes[Count] = null;
			Count--;

			//Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
			On_node_updated(formerLastNode);
		}




		/// <summary>
		/// update priority at node, and then move it to correct position in queue
		/// Behavior is undefined if you call w/ node that is not in queue
		/// </summary>
		public void Update(in T node, in float priority) {
			if (EnableDebugChecks && Contains(node) == false) {
				throw new Exception("DynamicPriorityQueue.Update: tried to update node that does not exist in queue!");
			}

			node.Priority = priority;
			On_node_updated(node);
		}



		public IEnumerator<T> GetEnumerator() {
			for (var i = 1; i <= Count; i++) {
				yield return _nodes[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}



		/*
         * Internals
         */


		// swap node locations and indices
		private void Swap_nodes(in T node1, in T node2) {
			_nodes[node1.Index] = node2;
			_nodes[node2.Index] = node1;
			(node2.Index, node1.Index) = (node1.Index, node2.Index);
		}


		// move node up tree to correct position by iteratively swapping w/ parent
		private void Move_up(in T node) {
			var parent = node.Index / 2;
			while (parent >= 1) {
				var parentNode = _nodes[parent];
				if (Has_higher_priority(parentNode, node)) {
					break;
				}

				Swap_nodes(node, parentNode);
				parent = node.Index / 2;
			}
		}


		// move node down tree branches to correct position, by iteratively swapping w/ children
		private void Move_down(in T node) {
			// we will put input node at this position after we are done swaps (ie actually moves, not swaps)
			var cur_node_index = node.Index;

			while (true) {
				var min_node = node;
				var iLeftChild = 2 * cur_node_index;

				// past end of tree, must be in the right spot
				if (iLeftChild > Count) {
					// store input node in final position
					node.Index = cur_node_index;
					_nodes[cur_node_index] = node;
					break;
				}

				// check if priority is larger than either child - if so we want to swap
				var left_child_node = _nodes[iLeftChild];
				if (Has_higher_priority(left_child_node, min_node)) {
					min_node = left_child_node;
				}
				var iRightChild = iLeftChild + 1;
				if (iRightChild <= Count) {
					var right_child_node = _nodes[iRightChild];
					if (Has_higher_priority(right_child_node, min_node)) {
						min_node = right_child_node;
					}
				}

				// if we found node with higher priority, swap with it (ie move it up) and take its place
				// (but we only write start node to final position, not intermediary slots)
				if (min_node != node) {
					_nodes[cur_node_index] = min_node;

					(cur_node_index, min_node.Index) = (min_node.Index, cur_node_index);
				}
				else {
					// store input node in final position
					node.Index = cur_node_index;
					_nodes[cur_node_index] = node;
					break;
				}
			}
		}





		/// call after node is modified, to move it to correct position in queue
		private void On_node_updated(in T node) {
			var parentIndex = node.Index / 2;
			var parentNode = _nodes[parentIndex];
			if (parentIndex > 0 && Has_higher_priority(node, parentNode)) {
				Move_up(node);
			}
			else {
				Move_down(node);
			}
		}


		/// returns true if priority at higher is less than at lower
		private bool Has_higher_priority(in T higher, in T lower) {
			return higher.Priority < lower.Priority;
		}



		/// <summary>
		/// Check if queue has been corrupted
		/// </summary>
		public bool IsValidQueue() {
			for (var i = 1; i < Count; i++) {
				if (_nodes[i] != null) {
					var childLeftIndex = 2 * i;
					if (childLeftIndex < Count && _nodes[childLeftIndex] != null && Has_higher_priority(_nodes[childLeftIndex], _nodes[i])) {
						return false;
					}

					var childRightIndex = childLeftIndex + 1;
					if (childRightIndex < Count && _nodes[childRightIndex] != null && Has_higher_priority(_nodes[childRightIndex], _nodes[i])) {
						return false;
					}
				}
			}
			return true;
		}
	}
}
