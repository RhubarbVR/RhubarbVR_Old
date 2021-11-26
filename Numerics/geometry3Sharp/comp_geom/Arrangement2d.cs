using System;
using System.Collections.Generic;
using System.Linq;

namespace RNumerics
{
	/// <summary>
	/// Arrangement2d constructs a planar arrangement of a set of 2D line segments.
	/// When a segment is inserted, existing edges are split, and the inserted
	/// segment becomes multiple graph edges. So, the resulting DGraph2 should
	/// not have any edges that intersect.
	/// 
	/// Calculations are performed in double-precision, so there is no guarantee
	/// of correctness. 
	/// 
	/// 
	/// [TODO] multi-level segment has to accelerate find_intersecting_edges()
	/// [TODO] maybe smarter handling
	/// 
	/// </summary>
	public class Arrangement2d
	{
		// graph of arrangement
		public DGraph2 Graph;

		// pointhash for vertices of graph
		public PointHashGrid2d<int> PointHash;

		// points within this tolerance are merged
		public double VertexSnapTol = 0.00001;


		public Arrangement2d(AxisAlignedBox2d boundsHint)
		{
			Graph = new DGraph2();

            var cellSize = boundsHint.MaxDim / 64;
			PointHash = new PointHashGrid2d<int>(cellSize, -1);
		}



		/// <summary>
		/// insert segment [a,b] into the arrangement
		/// </summary>
		public void Insert(Vector2d a, Vector2d b, int gid = -1)
		{
			Insert_segment(a, b, gid);
		}

		/// <summary>
		/// insert segment into the arrangement
		/// </summary>
		public void Insert(Segment2d segment, int gid = -1)
		{
			Insert_segment(segment.P0, segment.P1, gid);
		}

		/// <summary>
		/// sequentially insert segments of polyline
		/// </summary>
		public void Insert(PolyLine2d pline, int gid = -1)
		{
            var N = pline.VertexCount - 1;
			for (var i = 0; i < N; ++i)
			{
                var a = pline[i];
                var b = pline[i + 1];
				Insert_segment(a, b, gid);
			}
		}

		/// <summary>
		/// sequentially insert segments of polygon
		/// </summary>
		public void Insert(Polygon2d poly, int gid = -1)
		{
            var N = poly.VertexCount;
			for (var i = 0; i < N; ++i)
			{
                var a = poly[i];
                var b = poly[(i + 1) % N];
				Insert_segment(a, b, gid);
			}
		}



		/*
         *  Graph improvement
         */



		/// <summary>
		/// connect open boundary vertices within distThresh, by inserting new segments
		/// </summary>
		public void ConnectOpenBoundaries(double distThresh)
		{
            var max_vid = Graph.MaxVertexID;
			for (var vid = 0; vid < max_vid; ++vid)
			{
				if (Graph.IsBoundaryVertex(vid) == false)
                {
                    continue;
                }

                var v = Graph.GetVertex(vid);
                var snap_with = Find_nearest_boundary_vertex(v, distThresh, vid);
				if (snap_with != -1)
				{
                    var v2 = Graph.GetVertex(snap_with);
					Insert(v, v2);
				}
			}
		}







		protected struct SegmentPoint
		{
			public double t;
			public int vid;
		}

		/// <summary>
		/// insert edge [a,b] into the arrangement, splitting existing edges as necessary
		/// </summary>
		protected bool Insert_segment(ref Vector2d a, ref Vector2d b, int gid = -1, double tol = 0)
		{
            // handle degenerate edges
            var a_idx = Find_existing_vertex(a);
            var b_idx = Find_existing_vertex(b);
			if (a_idx == b_idx && a_idx >= 0)
            {
                return false;
            }

            // ok find all intersections
            var hits = new List<Intersection>();
			Find_intersecting_edges(ref a, ref b, hits, tol);
            var N = hits.Count;

            // we are going to construct a list of <t,vertex_id> values along segment AB
            var points = new List<SegmentPoint>();
            var segAB = new Segment2d(a, b);

			// insert intersections into existing segments
			for (var i = 0; i < N; ++i)
			{
                var intr = hits[i];
                var eid = intr.eid;
				double t0 = intr.intr.Parameter0, t1 = intr.intr.Parameter1;

                // insert first point at t0
                var new_eid = -1;
                if (intr.intr.Type is IntersectionType.Point or IntersectionType.Segment)
				{
                    var new_info = Split_segment_at_t(eid, t0, VertexSnapTol);
					new_eid = new_info.b;
                    var v = Graph.GetVertex(new_info.a);
					points.Add(new SegmentPoint() { t = segAB.Project(v), vid = new_info.a });
				}

				// if intersection was on-segment, then we have a second point at t1
				if (intr.intr.Type == IntersectionType.Segment)
				{
					if (new_eid == -1)
					{
                        // did not actually split edge for t0, so we can still use eid
                        var new_info = Split_segment_at_t(eid, t1, VertexSnapTol);
                        var v = Graph.GetVertex(new_info.a);
						points.Add(new SegmentPoint() { t = segAB.Project(v), vid = new_info.a });

					}
					else
					{
                        // find t1 was in eid, rebuild in new_eid
                        var new_seg = Graph.GetEdgeSegment(new_eid);
                        var p1 = intr.intr.Segment1.PointAt(t1);
                        var new_t1 = new_seg.Project(p1);
						Util.gDevAssert(new_t1 <= Math.Abs(new_seg.Extent));

                        var new_info = Split_segment_at_t(new_eid, new_t1, VertexSnapTol);
                        var v = Graph.GetVertex(new_info.a);
						points.Add(new SegmentPoint() { t = segAB.Project(v), vid = new_info.a });
					}

				}

			}

			// find or create start and end points
			if (a_idx == -1)
            {
                a_idx = Find_existing_vertex(a);
            }

            if (a_idx == -1)
			{
				a_idx = Graph.AppendVertex(a);
				PointHash.InsertPointUnsafe(a_idx, a);
			}
			if (b_idx == -1)
            {
                b_idx = Find_existing_vertex(b);
            }

            if (b_idx == -1)
			{
				b_idx = Graph.AppendVertex(b);
				PointHash.InsertPointUnsafe(b_idx, b);
			}

			// add start/end to points list. These may be duplicates but we will sort that out after
			points.Add(new SegmentPoint() { t = segAB.Project(a), vid = a_idx });
			points.Add(new SegmentPoint() { t = segAB.Project(b), vid = b_idx });
			// sort by t
			points.Sort((pa, pb) => (pa.t < pb.t) ? -1 : ((pa.t > pb.t) ? 1 : 0));

			// connect sequential points, as long as they aren't the same point,
			// and the segment doesn't already exist
			for (var k = 0; k < points.Count - 1; ++k)
			{
                var v0 = points[k].vid;
                var v1 = points[k + 1].vid;
				if (v0 == v1)
                {
                    continue;
                }

                if (Math.Abs(points[k].t - points[k + 1].t) < MathUtil.Epsilonf)
                {
                    System.Console.WriteLine("insert_segment: different points with same t??");
                }

                if (Graph.FindEdge(v0, v1) == DGraph2.InvalidID)
                {
                    Graph.AppendEdge(v0, v1, gid);
                }
            }

			return true;
		}
		protected bool Insert_segment(Vector2d a, Vector2d b, int gid = -1, double tol = 0)
		{
			return Insert_segment(ref a, ref b, gid, tol);
		}



		/// <summary>
		/// insert new point into segment eid at parameter value t
		/// If t is within tol of endpoint of segment, we use that instead.
		/// </summary>
		protected Index2i Split_segment_at_t(int eid, double t, double tol)
		{
            var ev = Graph.GetEdgeV(eid);
            var seg = new Segment2d(Graph.GetVertex(ev.a), Graph.GetVertex(ev.b));
            
            int use_vid;
            var new_eid = -1;
            if (t < -(seg.Extent - tol))
            {
                use_vid = ev.a;
            }
            else if (t > (seg.Extent - tol))
            {
                use_vid = ev.b;
            }
            else
            {
                var result = Graph.SplitEdge(eid, out var splitInfo);
                if (result != MeshResult.Ok)
                {
                    throw new Exception("insert_into_segment: edge split failed?");
                }

                use_vid = splitInfo.vNew;
                new_eid = splitInfo.eNewBN;
                var pt = seg.PointAt(t);
                Graph.SetVertex(use_vid, pt);
                PointHash.InsertPointUnsafe(splitInfo.vNew, pt);
            }
            return new Index2i(use_vid, new_eid);
		}


		/// <summary>
		/// find existing vertex at point, if it exists
		/// </summary>
		protected int Find_existing_vertex(Vector2d pt)
		{
			return Find_nearest_vertex(pt, VertexSnapTol);
		}
		/// <summary>
		/// find closest vertex, within searchRadius
		/// </summary>
		protected int Find_nearest_vertex(Vector2d pt, double searchRadius, int ignore_vid = -1)
		{
			var found = (ignore_vid == -1) ?
				PointHash.FindNearestInRadius(pt, searchRadius,
                            (b) => pt.DistanceSquared(Graph.GetVertex(b)))
							:
				PointHash.FindNearestInRadius(pt, searchRadius,
                            (b) => pt.DistanceSquared(Graph.GetVertex(b)),
                            (vid) => vid == ignore_vid);
            return found.Key == PointHash.InvalidValue ? -1 : found.Key;
        }


        /// <summary>
        /// find nearest boundary vertex, within searchRadius
        /// </summary>
        protected int Find_nearest_boundary_vertex(Vector2d pt, double searchRadius, int ignore_vid = -1)
		{
			var found =
				PointHash.FindNearestInRadius(pt, searchRadius,
                            (b) => pt.Distance(Graph.GetVertex(b)),
                            (vid) => Graph.IsBoundaryVertex(vid) == false || vid == ignore_vid);
            return found.Key == PointHash.InvalidValue ? -1 : found.Key;
        }



        protected struct Intersection
		{
			public int eid;
			public int sidex;
			public int sidey;
			public IntrSegment2Segment2 intr;
		}

		/// <summary>
		/// find set of edges in graph that intersect with edge [a,b]
		/// </summary>
		protected bool Find_intersecting_edges(ref Vector2d a, ref Vector2d b, List<Intersection> hits, double tol = 0)
		{
			var num_hits = 0;
			Vector2d x = Vector2d.Zero, y = Vector2d.Zero;
			foreach (var eid in Graph.EdgeIndices())
			{
				Graph.GetEdgeV(eid, ref x, ref y);
                var sidex = Segment2d.WhichSide(ref a, ref b, ref x, tol);
                var sidey = Segment2d.WhichSide(ref a, ref b, ref y, tol);
				if (sidex == sidey && sidex != 0)
                {
                    continue; // both pts on same side
                }

                var intr = new IntrSegment2Segment2(new Segment2d(x, y), new Segment2d(a, b));
				if (intr.Find())
				{
					hits.Add(new Intersection()
					{
						eid = eid,
						sidex = sidex,
						sidey = sidey,
						intr = intr
					});
					num_hits++;
				}
			}
			return num_hits > 0;
		}




	}
}
