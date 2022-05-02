using System;
using System.Collections.Generic;


namespace RNumerics
{
	/// <summary>
	/// Summary description for PolyLine.
	/// </summary>
	public class DPolyLine2f
	{
		public struct Edge
		{
			public int v1;
			public int v2;

			public Edge(int vertex1, int vertex2) { v1 = vertex1; v2 = vertex2; }
		}

		public struct Vertex
		{
			public int index;
			public float x;
			public float y;

			public Vertex(float fX, float fY, int nIndex) { x = fX; y = fY; index = nIndex; }
		}

		public DPolyLine2f() {
			Vertices = new List<Vertex>();
			Edges = new List<Edge>();
		}

		public DPolyLine2f(DPolyLine2f copy) {
			Vertices = new List<Vertex>(copy.Vertices);
			Edges = new List<Edge>(copy.Edges);
		}

		public List<Edge> Edges { get; private set; }
		public List<Vertex> Vertices { get; private set; }

		public int VertexCount => Vertices.Count;
		public int EdgeCount => Edges.Count;


		public void Clear() {
			Vertices.Clear();
			Edges.Clear();
		}


		public Vertex GetVertex(int i) {
			return Vertices[i];
		}


		public int AddVertex(float fX, float fY) {
			var nIndex = Vertices.Count;
			Vertices.Add(new Vertex(fX, fY, nIndex));
			return nIndex;
		}

		public int AddEdge(int v1, int v2) {
			var nIndex = Edges.Count;
			Edges.Add(new Edge(v1, v2));
			return nIndex;
		}


		/*
				public PolyLine Simplify(float fThreshold, int nMaxSkip) {

					// get rid of normals
					m_normals = null;

					// square threshold
					fThreshold *= fThreshold;

					// iterate through vertices
					PolyLine simpStroke = new PolyLine();

					int nLastGoodVertex = 0;
					Vertex vLastGood = m_vertices[nLastGoodVertex];
					Vertex vNext;

					simpStroke.AddVertex( vLastGood.x, vLastGood.y );

					while (nLastGoodVertex < m_nNextVertex - 2) {

						int nNextVert = nLastGoodVertex + 1;
						vNext = m_vertices[nNextVert];
						float fDistSqr = (vNext.x - vLastGood.x)*(vNext.x - vLastGood.x) + (vNext.y - vLastGood.y)*(vNext.y - vLastGood.y);
						int si = 0;
						while (fDistSqr < fThreshold && nNextVert < m_nNextVertex-2 && si < nMaxSkip) {
							nNextVert++;
							vNext = m_vertices[nNextVert];
							fDistSqr = (vNext.x - vLastGood.x)*(vNext.x - vLastGood.x) + (vNext.y - vLastGood.y)*(vNext.y - vLastGood.y);
							si++;
						}

						// add new vertex and edge
						if (fDistSqr > fThreshold || si >= nMaxSkip) {
							simpStroke.AddVertex( vNext.x, vNext.y );
							simpStroke.AddEdge( simpStroke.VertexCount-2, simpStroke.VertexCount-1 );
						} 

						nLastGoodVertex = nNextVert;
						vLastGood = vNext;
					}

					// always add last vertex
					simpStroke.AddVertex( m_vertices[m_nNextVertex-1].x, m_vertices[m_nNextVertex-1].y );
					simpStroke.AddEdge( simpStroke.VertexCount-2, simpStroke.VertexCount-1 );

					return simpStroke;
				}
		*/

		// note: this function assumes that the PolyLine is a closed loop with
		// vertex and edges in random order, and tries to order them coherently...
		// (it is not very good...)
		public bool OrderVertices() {

			// need a new array of vertices and edges
			var newVertices = new List<Vertex>(Vertices.Count);
			var newEdges = new List<Edge>(Edges.Count);
			var tmpEdges = new int[2];  // temporary

			// start at a random vertex, add to newVertices
			var nCurVertex = 0;
			var newVi = 0;
			var newEi = 0;
			newVertices[newVi++] = Vertices[nCurVertex];

			// loop until all vertices are done
			var nLastEdge = -1;
			while (newVi != Vertices.Count) {

				// find the two edges connected to this vertex. If there are
				// more than two, we are in trouble..
				var ei = 0;
				for (var i = 0; i < Edges.Count; ++i) {
					if (Edges[i].v1 == nCurVertex || Edges[i].v2 == nCurVertex) {
						if (ei > 1) {
							return false;
						}

						tmpEdges[ei++] = i;
					}
				}

				// we should have two now
				if (ei != 2) {
					return false;
				}

				// one of them has to be the edge we touched last time, unless we
				// have something bad...
				int nWhichEdge;
				if (nLastEdge == -1) {
					nWhichEdge = 0;
				}
				else if (tmpEdges[0] == nLastEdge) {
					nWhichEdge = tmpEdges[1];
				}
				else if (tmpEdges[1] == nLastEdge) {
					nWhichEdge = tmpEdges[0];
				}
				else {
					return false;       // failure!
				}

				// extract the other vertex from this edge
				var nNextVertex = (Edges[nWhichEdge].v1 == nCurVertex) ? Edges[nWhichEdge].v2 : Edges[nWhichEdge].v1;

				// add this vertex
				newVertices[newVi++] = Vertices[nNextVertex];

				// add an edge
				newEdges[newEi++] = new Edge(nCurVertex, nNextVertex);

				// update control variables
				nCurVertex = nNextVertex;
				nLastEdge = nWhichEdge;
			}

			// [RMS TODO] should we close the loop here ??
			newEdges[newEi++] = new Edge(nCurVertex, 0);

			// replace edges and vertices of this stroke, and clear normals
			Edges = newEdges;
			Vertices = newVertices;

			return true;

		}

	}


}
