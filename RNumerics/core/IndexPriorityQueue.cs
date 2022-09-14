using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{

	/// <summary>
	/// This is a min-heap priority queue class that does not use an object for each queue node.
	/// Integer IDs must be provided by the user to identify unique nodes.
	/// Internally an array is used to keep track of the mapping from ids to internal indices,
	/// so the max ID must also be provided.
	/// 
	/// See DijkstraGraphDistance for example usage.
	/// 
	/// conceptually based on https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
	/// </summary>
	public sealed class IndexPriorityQueue : IEnumerable<int>
	{
		// set this to true during development to catch issues
		public bool EnableDebugChecks = false;

		struct QueueNode
		{
			public int id;              // external id

			public float priority;      // the priority of this id
			public int index;           // index in tree data structure (tree is stored as flat array)
		}

		DVector<QueueNode> _nodes;       // tree of allocated nodes, stored linearly. active up to num_nodes (allocated may be larger)
		readonly int[] _id_to_index;              // mapping from external ids to internal node indices
												  // [TODO] could make this sparse using SparseList...


		/// <summary>
		/// maxIndex parameter is required because internally a fixed-size array is used to track mapping
		/// from IDs to internal node indices. If this seems problematic because you won't be inserting the
		/// full index space, consider a DynamicPriorityQueue instead.
		/// </summary>
		public IndexPriorityQueue(in int maxID) {
			_nodes = new DVector<QueueNode>();
			_id_to_index = new int[maxID];
			for (var i = 0; i < maxID; ++i) {
				_id_to_index[i] = -1;
			}

			Count = 0;
		}


		public int Count { get; private set; }


		/// <summary>
		/// reset the queue to empty state. 
		/// if bFreeMemory is false, we don't discard internal data structures, so there will be less allocation next time
		/// (this does not make a huge difference...)
		/// </summary>
		public void Clear(in bool bFreeMemory = true) {
			if (bFreeMemory) {
				_nodes = new DVector<QueueNode>();
			}

			Array.Clear(_id_to_index, 0, _id_to_index.Length);
			Count = 0;
		}


		/// <summary>
		/// id of node at head of queue
		/// </summary>
		public int First => _nodes[1].id;

		/// <summary>
		/// Priority of node at head of queue
		/// </summary>
		public float FirstPriority => _nodes[1].priority;

		/// <summary>
		/// constant-time check to see if id is already in queue
		/// </summary>
		public bool Contains(in int id) {
			var iNode = _id_to_index[id];
			return iNode > 0 && iNode <= Count && _nodes[iNode].index > 0;
		}


		/// <summary>
		/// Add id to list w/ given priority
		/// Behavior is undefined if you call w/ same id twice
		/// </summary>
		public void Insert(in int id, in float priority) {
			if (EnableDebugChecks && Contains(id) == true) {
				throw new Exception("IndexPriorityQueue.Insert: tried to add node that is already in queue!");
			}

			var node = new QueueNode {
				id = id,
				priority = priority
			};
			Count++;
			node.index = Count;
			_id_to_index[id] = node.index;
			_nodes.Insert(node, Count);
			Move_up(_nodes[Count].index);
		}
		public void Enqueue(in int id, in float priority) {
			Insert(id, priority);
		}

		/// <summary>
		/// remove node at head of queue, update queue, and return id for that node
		/// </summary>
		public int Dequeue() {
			if (EnableDebugChecks && Count == 0) {
				throw new Exception("IndexPriorityQueue.Dequeue: queue is empty!");
			}

			var id = _nodes[1].id;
			Remove_at_index(1);
			return id;
		}

		/// <summary>
		/// remove this node from queue. Undefined behavior if called w/ same id twice!
		/// Behavior is undefined if you call w/ id that is not in queue
		/// </summary>
		public void Remove(in int id) {
			if (EnableDebugChecks && Contains(id) == false) {
				throw new Exception("IndexPriorityQueue.Remove: tried to remove node that does not exist in queue!");
			}

			var iNode = _id_to_index[id];
			Remove_at_index(iNode);
		}


		/// <summary>
		/// update priority at node id, and then move it to correct position in queue
		/// Behavior is undefined if you call w/ id that is not in queue
		/// </summary>
		public void Update(in int id, in float priority) {
			if (EnableDebugChecks && Contains(id) == false) {
				throw new Exception("IndexPriorityQueue.Update: tried to update node that does not exist in queue!");
			}

			var iNode = _id_to_index[id];

			var n = _nodes[iNode];
			n.priority = priority;
			_nodes[iNode] = n;

			On_node_updated(iNode);
		}


		/// <summary>
		/// Query the priority at node id, assuming it exists in queue
		/// </summary>
		public float GetPriority(in int id) {
			if (EnableDebugChecks && Contains(id) == false) {
				throw new Exception("IndexPriorityQueue.Update: tried to get priorty of node that does not exist in queue!");
			}

			var iNode = _id_to_index[id];
			return _nodes[iNode].priority;
		}



		public IEnumerator<int> GetEnumerator() {
			for (var i = 1; i <= Count; i++) {
				yield return _nodes[i].id;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}



		/*
         * Internals
         */


		private void Remove_at_index(in int iNode) {
			// node-is-last-node case
			if (iNode == Count) {
				_nodes[iNode] = new QueueNode();
				Count--;
				return;
			}

			// [RMS] is there a better way to do this? seems random to move the last node to
			// top of tree? But I guess otherwise we might have to shift entire branches??

			//Swap the node with the last node
			Swap_nodes_at_indices(iNode, Count);
			// after swap, inode is the one we want to keep, and numNodes is the one we discard
			_nodes[Count] = new QueueNode();
			Count--;

			//Now shift iNode (ie the former last node) up or down as appropriate
			On_node_updated(iNode);
		}



		private void Swap_nodes_at_indices(in int i1, in int i2) {
			var n1 = _nodes[i1];
			n1.index = i2;
			var n2 = _nodes[i2];
			n2.index = i1;
			_nodes[i1] = n2;
			_nodes[i2] = n1;

			_id_to_index[n2.id] = i1;
			_id_to_index[n1.id] = i2;
		}

		/// move node at iFrom to iTo
		private void Move(in int iFrom, in int iTo) {
			var n = _nodes[iFrom];
			n.index = iTo;
			_nodes[iTo] = n;
			_id_to_index[n.id] = iTo;
		}

		/// set node at iTo
		private void Set(in int iTo, ref QueueNode n) {
			n.index = iTo;
			_nodes[iTo] = n;
			_id_to_index[n.id] = iTo;
		}


		// move iNode up tree to correct position by iteratively swapping w/ parent
		private void Move_up(int iNode) {
			// save start node, we will move this one to correct position in tree
			var iStart = iNode;
			var iStartNode = _nodes[iStart];

			// while our priority is lower than parent, we swap upwards, ie move parent down
			var iParent = iNode / 2;
			while (iParent >= 1) {
				if (_nodes[iParent].priority < iStartNode.priority) {
					break;
				}

				Move(iParent, iNode);
				iNode = iParent;
				iParent = _nodes[iNode].index / 2;
			}

			// write input node into final position, if we moved it
			if (iNode != iStart) {
				Set(iNode, ref iStartNode);
			}
		}


		// move iNode down tree branches to correct position, by iteratively swapping w/ children
		private void Move_down(int iNode) {
			// save start node, we will move this one to correct position in tree
			var iStart = iNode;
			var iStartNode = _nodes[iStart];

			// keep moving down until lower nodes have higher priority
			while (true) {
				var iMoveTo = iNode;
				var iLeftChild = 2 * iNode;

				// past end of tree, must be in the right spot
				if (iLeftChild > Count) {
					break;
				}

				// check if priority is larger than either child - if so we want to swap
				var min_priority = iStartNode.priority;
				var left_child_priority = _nodes[iLeftChild].priority;
				if (left_child_priority < min_priority) {
					iMoveTo = iLeftChild;
					min_priority = left_child_priority;
				}
				var iRightChild = iLeftChild + 1;
				if (iRightChild <= Count) {
					if (_nodes[iRightChild].priority < min_priority) {
						iMoveTo = iRightChild;
					}
				}

				// if we found node with higher priority, swap with it (ie move it up) and take its place
				// (but we only write start node to final position, not intermediary slots)
				if (iMoveTo != iNode) {
					Move(iMoveTo, iNode);
					iNode = iMoveTo;
				}
				else {
					break;
				}
			}

			// if we moved node, write it to its new position
			if (iNode != iStart) {
				Set(iNode, ref iStartNode);
			}
		}




		/// call after node is modified, to move it to correct position in queue
		private void On_node_updated(in int iNode) {
			var iParent = iNode / 2;
			if (iParent > 0 && Has_higher_priority(iNode, iParent)) {
				Move_up(iNode);
			}
			else {
				Move_down(iNode);
			}
		}






		/// returns true if priority at iHigher is less than at iLower
		private bool Has_higher_priority(in int iHigher, in int iLower) {
			return _nodes[iHigher].priority < _nodes[iLower].priority;
		}



		/// <summary>
		/// Check if queue has been corrupted
		/// </summary>
		public bool IsValidQueue() {
			for (var i = 1; i < Count; i++) {
				var childLeftIndex = 2 * i;
				if (childLeftIndex < Count && Has_higher_priority(childLeftIndex, i)) {
					return false;
				}

				var childRightIndex = childLeftIndex + 1;
				if (childRightIndex < Count && Has_higher_priority(childRightIndex, i)) {
					return false;
				}
			}
			return true;
		}
	}
}
