using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// This class is used to bisect an existing DGraph2 with infinite lines.
	/// This is easier than inserting new segments, which can be done using Arrangement2d.
	/// 
	/// Computations are done in double precision. Use at your own risk.
	/// 
	/// [TODO]
	///   - computation of signs for a split-line is currently O(N). If inserting many
	///     parallel lines, can improve this using standard sorting.
	/// </summary>
	public class GraphSplitter2d
	{
		public DGraph2 Graph;

		/// <summary>
		/// tolerance for WhichSide(vtx) tests
		/// </summary>
		public double OnVertexTol = MathUtil.Epsilonf;

		/// <summary>
		/// default ID for new edges, can override in specific functions
		/// </summary>
		public int InsertedEdgesID = 1;

		/// <summary>
		/// when inserting new segments, we check if their midpoint passes this test
		/// </summary>
		public Func<Vector2d, bool> InsideTestF = null;


		public GraphSplitter2d(DGraph2 graph)
		{
			Graph = graph;
		}

		/// <summary>
		/// split all graph edges that intersect line, and insert segments
		/// connecting these points
		/// </summary>
		public void InsertLine(Line2d line, int insert_edges_id = -1)
		{
			if (insert_edges_id == -1)
            {
                insert_edges_id = InsertedEdgesID;
            }

            Do_split(line, true, insert_edges_id);
		}

        readonly DVector<int> _edgeSigns = new DVector<int>();

		struct Edge_hit
		{
			public int hit_eid;
			public Index2i vtx_signs;
			public int hit_vid;
			public Vector2d hit_pos;
			public double line_t;
		}

        readonly List<Edge_hit> _hits = new List<Edge_hit>();

		protected virtual void Do_split(Line2d line, bool insert_edges, int insert_gid)
		{
			if (_edgeSigns.Length < Graph.MaxVertexID)
            {
                _edgeSigns.resize(Graph.MaxVertexID);
            }

            foreach (var vid in Graph.VertexIndices())
			{
				_edgeSigns[vid] = line.WhichSide(Graph.GetVertex(vid), OnVertexTol);
			}


			_hits.Clear();
			foreach (var eid in Graph.EdgeIndices())
			{
				var ev = Graph.GetEdgeV(eid);
				var signs = new Index2i(_edgeSigns[ev.a], _edgeSigns[ev.b]);
				if (signs.a * signs.b > 0)
                {
                    continue;   // both positive or negative, ignore
                }

                var hit = new Edge_hit() { hit_eid = eid, vtx_signs = signs, hit_vid = -1 };
				var a = Graph.GetVertex(ev.a);
				var b = Graph.GetVertex(ev.b);

				// parallel-edge case (both are zero)
				if (signs.a == signs.b)
				{
					if (a.DistanceSquared(b) > MathUtil.Epsilon)
					{
						// we need to somehow not insert a new segment for this span below. 
						// so, insert two hit points for the ray-interval, with same eid.
						// This will result in this span being skipped by the same-eid test below
						// *however*, if other edges self-intersect w/ this segment, this will *not work*
						// and duplicate edges will be inserted
						hit.hit_vid = ev.a;
						hit.line_t = line.Project(a);
						_hits.Add(hit);
						hit.hit_vid = ev.b;
						hit.line_t = line.Project(b);
						_hits.Add(hit);
					}
					else
					{
						// degenerate edge - fall through to a == 0 case below
						signs.b = 1;
					}
				}

				if (signs.a == 0)
				{
					hit.hit_pos = a;
					hit.hit_vid = ev.a;
					hit.line_t = line.Project(a);
				}
				else if (signs.b == 0)
				{
					hit.hit_pos = b;
					hit.hit_vid = ev.b;
					hit.line_t = line.Project(b);
				}
				else
				{
					var intr = new IntrLine2Segment2(line, new Segment2d(a, b));
					if (intr.Find() == false)
                    {
                        throw new Exception("GraphSplitter2d.Split: signs are different but ray did not it?");
                    }

                    if (intr.IsSimpleIntersection)
					{
						hit.hit_pos = intr.Point;
						hit.line_t = intr.Parameter;
					}
					else
					{
						throw new Exception("GraphSplitter2d.Split: got parallel edge case!");
					}
				}
				_hits.Add(hit);
			}

			// sort by increasing ray-t
			_hits.Sort((hit0, hit1) => hit0.line_t.CompareTo(hit1.line_t));

			// insert segments between successive intersection points
			var N = _hits.Count;
			for (var i = 0; i < N - 1; ++i)
			{
				var j = i + 1;
				// note: skipping parallel segments depends on this eid == eid test (see above)
				if (_hits[i].line_t == _hits[j].line_t || _hits[i].hit_eid == _hits[j].hit_eid)
                {
                    continue;
                }

                var vi = _hits[i].hit_vid;
				var vj = _hits[j].hit_vid;
				if (vi == vj && vi >= 0)
                {
                    continue;
                }

                if (vi >= 0 && vj >= 0)
				{
					var existing = Graph.FindEdge(vi, vj);
					if (existing >= 0)
                    {
                        continue;
                    }
                }

				if (vi == -1)
				{
                    var result = Graph.SplitEdge(_hits[i].hit_eid, out var split);
                    if (result != MeshResult.Ok)
                    {
                        throw new Exception("GraphSplitter2d.Split: first edge split failed!");
                    }

                    vi = split.vNew;
					Graph.SetVertex(vi, _hits[i].hit_pos);
					var tmp = _hits[i];
					tmp.hit_vid = vi;
					_hits[i] = tmp;
				}

				if (vj == -1)
				{
                    var result = Graph.SplitEdge(_hits[j].hit_eid, out var split);
                    if (result != MeshResult.Ok)
                    {
                        throw new Exception("GraphSplitter2d.Split: second edge split failed!");
                    }

                    vj = split.vNew;
					Graph.SetVertex(vj, _hits[j].hit_pos);
					var tmp = _hits[j];
					tmp.hit_vid = vj;
					_hits[j] = tmp;
				}

				// check if we actually want to add this segment
				if (InsideTestF != null)
				{
					var midpoint = 0.5 * (Graph.GetVertex(vi) + Graph.GetVertex(vj));
					if (InsideTestF(midpoint) == false)
                    {
                        continue;
                    }
                }

				if (insert_edges)
                {
                    Graph.AppendEdge(vi, vj, insert_gid);
                }
            }


		}



	}
}
