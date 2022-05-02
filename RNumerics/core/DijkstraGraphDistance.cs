using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RNumerics
{
	/// <summary>
	/// Compute Dijkstra shortest-path algorithm on a graph. 
	/// Computation is index-based, but can use sparse data
	/// structures if the index space will be sparse.
	/// 
	/// Construction is somewhat complicated, but see shortcut static
	/// methods at end of file for common construction cases:
	///   - MeshVertices(mesh) - compute on vertices of mesh
	///   - MeshVertices(mesh) - compute on vertices of mesh
	/// 
	/// </summary>
	public class DijkstraGraphDistance
	{
		public const float INVALID_VALUE = float.MaxValue;

		/// <summary>
		/// if you enable this, then you can call GetOrder()
		/// </summary>
		public bool TrackOrder = false;


		class GraphNode : DynamicPriorityQueueNode, IEquatable<GraphNode>
		{
			public int id;
			public GraphNode parent;
			public bool frozen;

			public bool Equals(GraphNode other) {
				return id == other.id;
			}
		}

		readonly DynamicPriorityQueue<GraphNode> _sparseQueue;
		readonly SparseObjectList<GraphNode> _sparseNodes;
		readonly MemoryPool<GraphNode> _sparseNodePool;


		struct GraphNodeStruct : IEquatable<GraphNodeStruct>
		{
			public int id;
			public int parent;
			public bool frozen;
			public float distance;

			public GraphNodeStruct(int id, int parent, float distance) {
				this.id = id;
				this.parent = parent;
				this.distance = distance;
				frozen = false;
			}

			public bool Equals(GraphNodeStruct other) {
				return id == other.id;
			}
			public static readonly GraphNodeStruct Zero = new() { id = -1, parent = -1, distance = INVALID_VALUE, frozen = false };
		}

		readonly IndexPriorityQueue _denseQueue;
		readonly GraphNodeStruct[] _denseNodes;
		readonly Func<int, bool> _nodeFilterF;
		readonly Func<int, int, float> _nodeDistanceF;
		readonly Func<int, IEnumerable<int>> _neighboursF;

		// maybe should be sparse array?
		private List<int> _seeds;
		List<int> _order;

		/// <summary>
		/// Constructor configures the graph as well. Graph is not specified
		/// explicitly, is provided via functions, for maximum flexibility.
		/// 
		/// nMaxID: maximum ID that will be added. 
		/// bSparse: is ID space large but sparse? this will save memory
		/// nodeFilterF: restrict to a subset of nodes (eg if you want to filter neighbours but not change neighboursF
		/// nodeDistanceF: must return (symmetric) distance between two nodes a and b
		/// neighboursF: return enumeration of neighbours of a
		/// seeds: although Vector2d, are actually pairs (id, seedvalue)   (or use AddSeed later)
		/// </summary>
		public DijkstraGraphDistance(int nMaxID, bool bSparse,
			Func<int, bool> nodeFilterF,
			Func<int, int, float> nodeDistanceF,
			Func<int, IEnumerable<int>> neighboursF,
			IEnumerable<Vector2d> seeds = null                // these are pairs (index, seedval)
			) {
			_nodeFilterF = nodeFilterF;
			_nodeDistanceF = nodeDistanceF;
			_neighboursF = neighboursF;


			if (bSparse) {
				_sparseQueue = new DynamicPriorityQueue<GraphNode>();
				_sparseNodes = new SparseObjectList<GraphNode>(nMaxID, 0);
				_sparseNodePool = new MemoryPool<GraphNode>();
			}
			else {
				_denseQueue = new IndexPriorityQueue(nMaxID);
				_denseNodes = new GraphNodeStruct[nMaxID];
			}

			_seeds = new List<int>();
			MaxDistance = float.MinValue;
			if (seeds != null) {
				foreach (var v in seeds) {
					AddSeed((int)v.x, (float)v.y);
				}
			}
		}


		/// <summary>
		/// reset internal data structures/etc
		/// </summary>
		public void Reset() {
			if (_sparseNodes != null) {
				_sparseQueue.Clear(false);
				_sparseNodes.Clear();
				_sparseNodePool.ReturnAll();

			}
			else {
				_denseQueue.Clear(false);
				Array.Clear(_denseNodes, 0, _denseNodes.Length);
			}

			_seeds = new List<int>();
			MaxDistance = float.MinValue;
		}


		/// <summary>
		/// Add seed point as id/distance pair
		/// </summary>
		public void AddSeed(int id, float seed_dist) {
			if (_sparseNodes != null) {
				var g = Get_node(id);
				Debug.Assert(_sparseQueue.Contains(g) == false);
				_sparseQueue.Enqueue(g, seed_dist);
			}
			else {
				Debug.Assert(_denseQueue.Contains(id) == false);
				Enqueue_node_dense(id, seed_dist, -1);
			}
			_seeds.Add(id);
		}
		public bool IsSeed(int id) {
			return _seeds.Contains(id);
		}


		/// <summary>
		/// Compute distances from seeds to all other ids.
		/// </summary>
		public void Compute() {
			if (TrackOrder == true) {
				_order = new List<int>();
			}

			if (_sparseNodes != null) {
				Compute_Sparse();
			}
			else {
				Compute_Dense();
			}
		}
		protected void Compute_Sparse() {
			while (_sparseQueue.Count > 0) {
				var g = _sparseQueue.Dequeue();
				MaxDistance = Math.Max(g.Priority, MaxDistance);
				g.frozen = true;
				if (TrackOrder) {
					_order.Add(g.id);
				}

				Update_neighbours_sparse(g);
			}
		}
		protected void Compute_Dense() {
			while (_denseQueue.Count > 0) {
				var idx_priority = _denseQueue.FirstPriority;
				var idx = _denseQueue.Dequeue();
				var g = _denseNodes[idx];
				g.frozen = true;
				if (TrackOrder) {
					_order.Add(g.id);
				}

				g.distance = MaxDistance;
				_denseNodes[idx] = g;
				MaxDistance = Math.Max(idx_priority, MaxDistance);
				Update_neighbours_dense(g.id);
			}
		}


		/// <summary>
		/// Compute distances that are less/equal to fMaxDistance from the seeds
		/// Terminates early, so Queue may not be empty
		/// </summary>
		public void ComputeToMaxDistance(float fMaxDistance) {
			if (TrackOrder == true) {
				_order = new List<int>();
			}

			if (_sparseNodes != null) {
				ComputeToMaxDistance_Sparse(fMaxDistance);
			}
			else {
				ComputeToMaxDistance_Dense(fMaxDistance);
			}
		}
		protected void ComputeToMaxDistance_Sparse(float fMaxDistance) {
			while (_sparseQueue.Count > 0) {
				var g = _sparseQueue.Dequeue();
				MaxDistance = Math.Max(g.Priority, MaxDistance);
				if (MaxDistance > fMaxDistance) {
					return;
				}

				g.frozen = true;
				if (TrackOrder) {
					_order.Add(g.id);
				}

				Update_neighbours_sparse(g);
			}
		}
		protected void ComputeToMaxDistance_Dense(float fMaxDistance) {
			while (_denseQueue.Count > 0) {
				var idx_priority = _denseQueue.FirstPriority;
				MaxDistance = Math.Max(idx_priority, MaxDistance);
				if (MaxDistance > fMaxDistance) {
					return;
				}

				var idx = _denseQueue.Dequeue();
				var g = _denseNodes[idx];
				g.frozen = true;
				if (TrackOrder) {
					_order.Add(g.id);
				}

				g.distance = MaxDistance;
				_denseNodes[idx] = g;
				Update_neighbours_dense(g.id);
			}
		}




		/// <summary>
		/// Compute distances until node_id is frozen, or (optional) max distance is reached
		/// Terminates early, so Queue may not be empty
		/// [TODO] can reimplement this w/ internal call to ComputeToNode(func) ?
		/// </summary>
		public void ComputeToNode(int node_id, float fMaxDistance = INVALID_VALUE) {
			if (TrackOrder == true) {
				_order = new List<int>();
			}

			if (_sparseNodes != null) {
				ComputeToNode_Sparse(node_id, fMaxDistance);
			}
			else {
				ComputeToNode_Dense(node_id, fMaxDistance);
			}
		}
		protected void ComputeToNode_Sparse(int node_id, float fMaxDistance) {
			while (_sparseQueue.Count > 0) {
				var g = _sparseQueue.Dequeue();
				MaxDistance = Math.Max(g.Priority, MaxDistance);
				if (MaxDistance > fMaxDistance) {
					return;
				}

				g.frozen = true;
				if (TrackOrder) {
					_order.Add(g.id);
				}

				if (g.id == node_id) {
					return;
				}

				Update_neighbours_sparse(g);
			}
		}
		protected void ComputeToNode_Dense(int node_id, float fMaxDistance) {
			while (_denseQueue.Count > 0) {
				var idx_priority = _denseQueue.FirstPriority;
				MaxDistance = Math.Max(idx_priority, MaxDistance);
				if (MaxDistance > fMaxDistance) {
					return;
				}

				var idx = _denseQueue.Dequeue();
				var g = _denseNodes[idx];
				g.frozen = true;
				if (TrackOrder) {
					_order.Add(g.id);
				}

				g.distance = MaxDistance;
				_denseNodes[idx] = g;
				if (g.id == node_id) {
					return;
				}

				Update_neighbours_dense(g.id);
			}
		}







		/// <summary>
		/// Compute distances until node_id is frozen, or (optional) max distance is reached
		/// Terminates early, so Queue may not be empty
		/// </summary>
		public int ComputeToNode(Func<int, bool> terminatingNodeF, float fMaxDistance = INVALID_VALUE) {
			if (TrackOrder == true) {
				_order = new List<int>();
			}

			return _sparseNodes != null
				? ComputeToNode_Sparse(terminatingNodeF, fMaxDistance)
				: ComputeToNode_Dense(terminatingNodeF, fMaxDistance);
		}
		protected int ComputeToNode_Sparse(Func<int, bool> terminatingNodeF, float fMaxDistance) {
			while (_sparseQueue.Count > 0) {
				var g = _sparseQueue.Dequeue();
				MaxDistance = Math.Max(g.Priority, MaxDistance);
				if (MaxDistance > fMaxDistance) {
					return -1;
				}

				g.frozen = true;
				if (TrackOrder) {
					_order.Add(g.id);
				}

				if (terminatingNodeF(g.id)) {
					return g.id;
				}

				Update_neighbours_sparse(g);
			}
			return -1;
		}
		protected int ComputeToNode_Dense(Func<int, bool> terminatingNodeF, float fMaxDistance) {
			while (_denseQueue.Count > 0) {
				var idx_priority = _denseQueue.FirstPriority;
				MaxDistance = Math.Max(idx_priority, MaxDistance);
				if (MaxDistance > fMaxDistance) {
					return -1;
				}

				var idx = _denseQueue.Dequeue();
				var g = _denseNodes[idx];
				g.frozen = true;
				if (TrackOrder) {
					_order.Add(g.id);
				}

				g.distance = MaxDistance;
				_denseNodes[idx] = g;
				if (terminatingNodeF(g.id)) {
					return g.id;
				}

				Update_neighbours_dense(g.id);
			}
			return -1;
		}







		/// <summary>
		/// Get the maximum distance encountered during the Compute()
		/// </summary>
		public float MaxDistance { get; private set; }


		/// <summary>
		/// Get the computed distance at node id. returns InvalidValue if node was not computed.
		/// </summary>
		public float GetDistance(int id) {
			if (_sparseNodes != null) {
				var g = _sparseNodes[id];
				return g == null ? INVALID_VALUE : g.Priority;
			}
			else {
				var g = _denseNodes[id];
				return g.frozen ? g.distance : INVALID_VALUE;
			}
		}



		/// <summary>
		/// Get (internal) list of frozen nodes in increasing distance-order.
		/// Requries that TrackOrder=true before Compute call.
		/// </summary>
		public List<int> GetOrder() {
			return TrackOrder
				? _order
				: throw new InvalidOperationException("DijkstraGraphDistance.GetOrder: Must set TrackOrder = true");
		}



		/// <summary>
		/// Walk from node fromv back to the (graph-)nearest seed point.
		/// </summary>
		public bool GetPathToSeed(int fromv, List<int> path) {
			if (_sparseNodes != null) {
				var g = Get_node(fromv);
				if (g.frozen == false) {
					return false;
				}

				path.Add(fromv);
				while (g.parent != null) {
					path.Add(g.parent.id);
					g = g.parent;
				}
				return true;
			}
			else {
				var g = _denseNodes[fromv];
				if (g.frozen == false) {
					return false;
				}

				path.Add(fromv);
				while (g.parent != -1) {
					path.Add(g.parent);
					g = _denseNodes[g.parent];
				}
				return true;
			}
		}




		/*
         * Internals below here
         */



		GraphNode Get_node(int id) {
			var g = _sparseNodes[id];
			if (g == null) {
				//g = new GraphNode() { id = id, parent = null, frozen = false };
				g = _sparseNodePool.Allocate();
				g.id = id;
				g.parent = null;
				g.frozen = false;
				_sparseNodes[id] = g;
			}
			return g;
		}



		void Update_neighbours_sparse(GraphNode parent) {
			var cur_dist = parent.Priority;
			foreach (var nbr_id in _neighboursF(parent.id)) {
				if (_nodeFilterF(nbr_id) == false) {
					continue;
				}

				var nbr = Get_node(nbr_id);
				if (nbr.frozen) {
					continue;
				}

				var nbr_dist = _nodeDistanceF(parent.id, nbr_id) + cur_dist;
				if (nbr_dist == INVALID_VALUE) {
					continue;
				}

				if (_sparseQueue.Contains(nbr)) {
					if (nbr_dist < nbr.Priority) {
						nbr.parent = parent;
						_sparseQueue.Update(nbr, nbr_dist);
					}
				}
				else {
					nbr.parent = parent;
					_sparseQueue.Enqueue(nbr, nbr_dist);
				}
			}
		}




		void Enqueue_node_dense(int id, float dist, int parent_id) {
			var g = new GraphNodeStruct(id, parent_id, dist);
			_denseNodes[id] = g;
			_denseQueue.Insert(id, dist);
		}

		void Update_neighbours_dense(int parent_id) {
			var g = _denseNodes[parent_id];
			var cur_dist = g.distance;
			foreach (var nbr_id in _neighboursF(parent_id)) {
				if (_nodeFilterF(nbr_id) == false) {
					continue;
				}

				var nbr = _denseNodes[nbr_id];
				if (nbr.frozen) {
					continue;
				}

				var nbr_dist = _nodeDistanceF(parent_id, nbr_id) + cur_dist;
				if (nbr_dist == INVALID_VALUE) {
					continue;
				}

				if (_denseQueue.Contains(nbr_id)) {
					if (nbr_dist < nbr.distance) {
						nbr.parent = parent_id;
						_denseQueue.Update(nbr_id, nbr_dist);
						_denseNodes[nbr_id] = nbr;
					}
				}
				else {
					Enqueue_node_dense(nbr_id, nbr_dist, parent_id);
				}
			}
		}



	}
}
