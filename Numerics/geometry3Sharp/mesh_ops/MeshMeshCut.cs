// Copyright (c) Ryan Schmidt (rms@gradientspace.com) - All Rights Reserved
// Distributed under the Boost Software License, Version 1.0. http://www.boost.org/LICENSE_1_0.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNumerics
{
	/// <summary>
	/// 
	/// 
	/// TODO:
	///    - track descendant triangles of each input face
	///    - for missing segments, can resolve in 2D in plane of face
	/// 
	/// 
	/// </summary>
	public class MeshMeshCut
	{
		public DMesh3 Target;
		public DMesh3 CutMesh;

		PointHashGrid3d<int> _pointHash;

		// points within this tolerance are merged
		public double VertexSnapTol = 0.00001;

		// List of vertices in output Target that are on the
		// cut path, after calling RemoveContained. 
		// TODO: still missing some vertices??
		public List<int> CutVertices;


		public void Compute()
		{
			var cellSize = Target.CachedBounds.MaxDim / 64;
			_pointHash = new PointHashGrid3d<int>(cellSize, -1);

			// insert target vertices into hash
			foreach (var vid in Target.VertexIndices())
			{
				var v = Target.GetVertex(vid);
				var existing = Find_existing_vertex(v);
				if (existing != -1)
                {
                    System.Console.WriteLine("VERTEX {0} IS DUPLICATE OF {1}!", vid, existing);
                }

                _pointHash.InsertPointUnsafe(vid, v);
			}

			Initialize();
			Find_segments();
			Insert_face_vertices();
			Insert_edge_vertices();
			Connect_edges();

			// SegmentInsertVertices was constructed by planar polygon
			// insertions in MeshInsertUVPolyCurve calls, but we also
			// need to the segment vertices
			foreach (var sv in _segVertices)
            {
                _segmentInsertVertices.Add(sv.vtx_id);
            }
        }


		public void RemoveContained()
		{
			var spatial = new DMeshAABBTree3(CutMesh, true);
			spatial.WindingNumber(Vector3d.Zero);
			var removeT = new SafeListBuilder<int>();
			gParallel.ForEach(Target.TriangleIndices(), (tid) =>
			{
				var v = Target.GetTriCentroid(tid);
				if (spatial.WindingNumber(v) > 0.9)
                {
                    removeT.SafeAdd(tid);
                }
            });
			MeshEditor.RemoveTriangles(Target, removeT.Result);

			// [RMS] construct set of on-cut vertices? This is not
			// necessarily all boundary vertices...
			CutVertices = new List<int>();
			foreach (var vid in _segmentInsertVertices)
			{
				if (Target.IsVertex(vid))
                {
                    CutVertices.Add(vid);
                }
            }
		}

		public void AppendSegments(double r)
		{
			foreach (var seg in _segments)
			{
				var s = new Segment3d(seg.v0.v, seg.v1.v);
				if (Target.FindEdge(seg.v0.vtx_id, seg.v1.vtx_id) == DMesh3.InvalidID)
                {
                    MeshEditor.AppendLine(Target, s, (float)r);
                }
            }
		}

		public void ColorFaces()
		{
			var counter = 1;
			var gidmap = new Dictionary<int, int>();
			foreach (var key in _subFaces.Keys)
            {
                gidmap[key] = counter++;
            }

            Target.EnableTriangleGroups(0);
			foreach (var tid in Target.TriangleIndices())
			{
				if (_parentFaces.ContainsKey(tid))
                {
                    Target.SetTriangleGroup(tid, gidmap[_parentFaces[tid]]);
                }
                else if (_subFaces.ContainsKey(tid))
                {
                    Target.SetTriangleGroup(tid, gidmap[tid]);
                }
            }
		}


		class SegmentVtx
		{
			public Vector3d v;
			public int type = -1;
			public int initial_type = -1;
			public int vtx_id = DMesh3.InvalidID;
			public int elem_id = DMesh3.InvalidID;
		}
		List<SegmentVtx> _segVertices;
		Dictionary<int, SegmentVtx> _vIDToSegVtxMap;


		// segment vertices in each triangle that we still have to insert
		Dictionary<int, List<SegmentVtx>> _faceVertices;

		// segment vertices in each edge that we still have to insert
		Dictionary<int, List<SegmentVtx>> _edgeVertices;


		class IntersectSegment
		{
			public int base_tid;
			public SegmentVtx v0;
			public SegmentVtx v1;
			public SegmentVtx this[int key]
			{
				get { return (key == 0) ? v0 : v1; }
				set { if (key == 0) { v0 = value; } else
                    {
                        v1 = value;
                    }
                }
			}
		}
		IntersectSegment[] _segments;

		Vector3d[] _baseFaceCentroids;
		Vector3d[] _baseFaceNormals;
		Dictionary<int, HashSet<int>> _subFaces;
		Dictionary<int, int> _parentFaces;

		HashSet<int> _segmentInsertVertices;

		void Initialize()
		{
			_baseFaceCentroids = new Vector3d[Target.MaxTriangleID];
			_baseFaceNormals = new Vector3d[Target.MaxTriangleID];
			foreach (var tid in Target.TriangleIndices())
            {
                Target.GetTriInfo(tid, out _baseFaceNormals[tid], out _, out _baseFaceCentroids[tid]);
            }

            // allocate internals
            _segVertices = new List<SegmentVtx>();
			_edgeVertices = new Dictionary<int, List<SegmentVtx>>();
			_faceVertices = new Dictionary<int, List<SegmentVtx>>();
			_subFaces = new Dictionary<int, HashSet<int>>();
			_parentFaces = new Dictionary<int, int>();
			_segmentInsertVertices = new HashSet<int>();
			_vIDToSegVtxMap = new Dictionary<int, SegmentVtx>();
		}



		/// <summary>
		/// 1) Find intersection segments
		/// 2) sort onto existing input mesh vtx/edge/face
		/// </summary>
		void Find_segments()
		{
			var SegVtxMap = new Dictionary<Vector3d, SegmentVtx>();

			// find intersection segments
			// TODO: intersection polygons
			// TODO: do we need to care about intersection vertices?
			var targetSpatial = new DMeshAABBTree3(Target, true);
			var cutSpatial = new DMeshAABBTree3(CutMesh, true);
			var intersections = targetSpatial.FindAllIntersections(cutSpatial);

			// for each segment, for each vtx, determine if it is 
			// at an existing vertex, on-edge, or in-face
			_segments = new IntersectSegment[intersections.Segments.Count];
			for (var i = 0; i < _segments.Length; ++i)
			{
				var isect = intersections.Segments[i];
				var points = new Vector3dTuple2(isect.point0, isect.point1);
				var iseg = new IntersectSegment()
				{
					base_tid = isect.t0
				};
				_segments[i] = iseg;
				for (var j = 0; j < 2; ++j)
				{
					var v = points[j];

                    // if this exact vtx coord has been seen, use same vtx
                    if (SegVtxMap.TryGetValue(v, out var sv))
                    {
                        iseg[j] = sv;
                        continue;
                    }
                    sv = new SegmentVtx() { v = v };
					_segVertices.Add(sv);
					SegVtxMap[v] = sv;
					iseg[j] = sv;

					// this vtx is tol-equal to input mesh vtx
					var existing_v = Find_existing_vertex(isect.point0);
					if (existing_v >= 0)
					{
						sv.initial_type = sv.type = 0;
						sv.elem_id = existing_v;
						sv.vtx_id = existing_v;
						_vIDToSegVtxMap[sv.vtx_id] = sv;
						continue;
					}

					var tri = new Triangle3d();
					Target.GetTriVertices(isect.t0, ref tri.V0, ref tri.V1, ref tri.V2);
					var tv = Target.GetTriangle(isect.t0);

					// this vtx is tol-on input mesh edge
					var on_edge_i = On_edge(ref tri, ref v);
					if (on_edge_i >= 0)
					{
						sv.initial_type = sv.type = 1;
						sv.elem_id = Target.FindEdge(tv[on_edge_i], tv[(on_edge_i + 1) % 3]);
						Util.gDevAssert(sv.elem_id != DMesh3.InvalidID);
						Add_edge_vtx(sv.elem_id, sv);
						continue;
					}

					// otherwise contained in input mesh face
					sv.initial_type = sv.type = 2;
					sv.elem_id = isect.t0;
					Add_face_vtx(sv.elem_id, sv);
				}

			}

		}




		/// <summary>
		/// For each on-face vtx, we poke the face, and re-sort 
		/// the remaining vertices on that face onto new faces/edges
		/// </summary>
		void Insert_face_vertices()
		{
			while (_faceVertices.Count > 0)
            {
                var pair = _faceVertices.First();
                var tid = pair.Key;
                var triVerts = pair.Value;
                var v = triVerts[triVerts.Count - 1];
                triVerts.RemoveAt(triVerts.Count - 1);

                var result = Target.PokeTriangle(tid, out var pokeInfo);
                if (result == MeshResult.Ok)
                {
                    var new_v = pokeInfo.new_vid;

                    Target.SetVertex(new_v, v.v);
                    v.vtx_id = new_v;
                    _vIDToSegVtxMap[v.vtx_id] = v;
                    _pointHash.InsertPoint(v.vtx_id, v.v);

                    // remove this triangles vtx list because it is no longer valid
                    _faceVertices.Remove(tid);

                    // update remaining verts
                    var pokeEdges = pokeInfo.new_edges;
                    var pokeTris = new Index3i(tid, pokeInfo.new_t1, pokeInfo.new_t2);
                    foreach (var sv in triVerts)
                    {
                        Update_from_poke(sv, pokeEdges, pokeTris);
                        if (sv.type == 1)
                        {
                            Add_edge_vtx(sv.elem_id, sv);
                        }
                        else if (sv.type == 2)
                        {
                            Add_face_vtx(sv.elem_id, sv);
                        }
                    }

                    // track poke subfaces
                    Add_poke_subfaces(tid, ref pokeInfo);
                }
                else
                {
                    throw new Exception("shit");
                }
            }
        }



		/// <summary>
		/// figure out which vtx/edge/face the input vtx is on
		/// </summary>
		void Update_from_poke(SegmentVtx sv, Index3i pokeEdges, Index3i pokeTris)
		{
			// check if within tolerance of existing vtx, because we did not 
			// sort that out before...
			var existing_v = Find_existing_vertex(sv.v);
			if (existing_v >= 0)
			{
				sv.type = 0;
				sv.elem_id = existing_v;
				sv.vtx_id = existing_v;
				_vIDToSegVtxMap[sv.vtx_id] = sv;
				return;
			}

			for (var j = 0; j < 3; ++j)
			{
				if (Is_on_edge(pokeEdges[j], sv.v))
				{
					sv.type = 1;
					sv.elem_id = pokeEdges[j];
					return;
				}
			}

			// [TODO] should use PrimalQuery2d for this!
			for (var j = 0; j < 3; ++j)
			{
				if (Is_in_triangle(pokeTris[j], sv.v))
				{
					sv.type = 2;
					sv.elem_id = pokeTris[j];
					return;
				}
			}

			System.Console.WriteLine("unsorted vertex!");
			sv.elem_id = pokeTris.a;
		}




		/// <summary>
		/// for each on-edge vtx, we split the edge and then
		/// re-sort any of the vertices on that edge onto new edges
		/// </summary>
		void Insert_edge_vertices()
		{
			while (_edgeVertices.Count > 0)
			{
				var pair = _edgeVertices.First();
				var eid = pair.Key;
				var edgeVerts = pair.Value;
				var v = edgeVerts[edgeVerts.Count - 1];
				edgeVerts.RemoveAt(edgeVerts.Count - 1);

				var splitTris = Target.GetEdgeT(eid);

                var result = Target.SplitEdge(eid, out var splitInfo);
                if (result != MeshResult.Ok)
                {
                    throw new Exception("insert_edge_vertices: split failed!");
                }

                var new_v = splitInfo.vNew;
				var splitEdges = new Index2i(eid, splitInfo.eNewBN);

				Target.SetVertex(new_v, v.v);
				v.vtx_id = new_v;
				_vIDToSegVtxMap[v.vtx_id] = v;
				_pointHash.InsertPoint(v.vtx_id, v.v);

				// remove this triangles vtx list because it is no longer valid
				_edgeVertices.Remove(eid);

                // update remaining verts
                foreach (var sv in edgeVerts)
				{
					Update_from_split(sv, splitEdges);
					if (sv.type == 1)
                    {
                        Add_edge_vtx(sv.elem_id, sv);
                    }
                }

				// track subfaces
				Add_split_subfaces(splitTris, ref splitInfo);

			}
		}



		/// <summary>
		/// figure out which vtx/edge the input vtx is on
		/// </summary>
		void Update_from_split(SegmentVtx sv, Index2i splitEdges)
		{
			// check if within tolerance of existing vtx, because we did not 
			// sort that out before...
			var existing_v = Find_existing_vertex(sv.v);
			if (existing_v >= 0)
			{
				sv.type = 0;
				sv.elem_id = existing_v;
				sv.vtx_id = existing_v;
				_vIDToSegVtxMap[sv.vtx_id] = sv;
				return;
			}

			for (var j = 0; j < 2; ++j)
			{
				if (Is_on_edge(splitEdges[j], sv.v))
				{
					sv.type = 1;
					sv.elem_id = splitEdges[j];
					return;
				}
			}

			throw new Exception("update_from_split: unsortable vertex?");
		}







		/// <summary>
		/// Make sure that all intersection segments are represented by
		/// a connected chain of edges.
		/// </summary>
		void Connect_edges()
		{
			var NS = _segments.Length;
			for (var si = 0; si < NS; ++si)
			{
				var seg = _segments[si];
				if (seg.v0 == seg.v1)
                {
                    continue;       // degenerate!
                }

                if (seg.v0.vtx_id == seg.v1.vtx_id)
                {
                    continue;       // also degenerate and how does this happen?
                }

                int a = seg.v0.vtx_id, b = seg.v1.vtx_id;

				if (a == DMesh3.InvalidID || b == DMesh3.InvalidID)
                {
                    throw new Exception("segment vertex is not defined?");
                }

                var eid = Target.FindEdge(a, b);
				if (eid != DMesh3.InvalidID)
                {
                    continue;       // already connected
                }

                // TODO: in many cases there is an edge we added during a
                // poke or split that we could flip to get edge AB. 
                // this is much faster and we should do it where possible!
                // HOWEVER we need to know which edges we can and cannot flip
                // is_inserted_free_edge() should do this but not implemented yet
                // possibly also requires that we do all these flips before any
                // calls to insert_segment() !

                try
				{
					Insert_segment(seg);
				}
				catch (Exception)
				{
					// ignore?
				}
			}
		}


		void Insert_segment(IntersectSegment seg)
		{
			var subfaces = Get_all_baseface_tris(seg.base_tid);

			var op = new RegionOperator(Target, subfaces);

			var n = _baseFaceNormals[seg.base_tid];
			var c = _baseFaceCentroids[seg.base_tid];
            Vector3d.MakePerpVectors(ref n, out var e0, out var e1);

            var mesh = op.Region.SubMesh;
			MeshTransforms.PerVertexTransform(mesh, (v) =>
			{
				v -= c;
				return new Vector3d(v.Dot(e0), v.Dot(e1), 0);
			});

			Vector3d end0 = seg.v0.v, end1 = seg.v1.v;
			end0 -= c;
			end1 -= c;
			var p0 = new Vector2d(end0.Dot(e0), end0.Dot(e1));
			var p1 = new Vector2d(end1.Dot(e0), end1.Dot(e1));
			var path = new PolyLine2d();
			path.AppendVertex(p0);
			path.AppendVertex(p1);

			var insert = new MeshInsertUVPolyCurve(mesh, path);
			insert.Apply();

			var cutVerts = new MeshVertexSelection(mesh);
			cutVerts.SelectEdgeVertices(insert.OnCutEdges);

            MeshTransforms.PerVertexTransform(mesh, (v) => c + (v.x * e0) + (v.y * e1));

			op.BackPropropagate();

            // add new cut vertices to cut list
            foreach (var vid in cutVerts)
            {
                _segmentInsertVertices.Add(op.ReinsertSubToBaseMapV[vid]);
            }

            Add_regionop_subfaces(seg.base_tid, op);
		}





		void Add_edge_vtx(int eid, SegmentVtx vtx)
		{
            if (_edgeVertices.TryGetValue(eid, out var l))
            {
                l.Add(vtx);
            }
            else
            {
                l = new List<SegmentVtx>() { vtx };
                _edgeVertices[eid] = l;
            }
        }

		void Add_face_vtx(int tid, SegmentVtx vtx)
		{
            if (_faceVertices.TryGetValue(tid, out var l))
            {
                l.Add(vtx);
            }
            else
            {
                l = new List<SegmentVtx>() { vtx };
                _faceVertices[tid] = l;
            }
        }



		void Add_poke_subfaces(int tid, ref DMesh3.PokeTriangleInfo pokeInfo)
		{
			var parent = Get_parent(tid);
			var subfaces = Get_subfaces(parent);
			if (tid != parent)
            {
                Add_subface(subfaces, parent, tid);
            }

            Add_subface(subfaces, parent, pokeInfo.new_t1);
			Add_subface(subfaces, parent, pokeInfo.new_t2);
		}
		void Add_split_subfaces(Index2i origTris, ref DMesh3.EdgeSplitInfo splitInfo)
		{
			var parent_1 = Get_parent(origTris.a);
			var subfaces_1 = Get_subfaces(parent_1);
			if (origTris.a != parent_1)
            {
                Add_subface(subfaces_1, parent_1, origTris.a);
            }

            Add_subface(subfaces_1, parent_1, splitInfo.eNewT2);

			if (origTris.b != DMesh3.InvalidID)
			{
				var parent_2 = Get_parent(origTris.b);
                var subfaces_2 = Get_subfaces(parent_2);
				if (origTris.b != parent_2)
                {
                    Add_subface(subfaces_2, parent_2, origTris.b);
                }

                Add_subface(subfaces_2, parent_2, splitInfo.eNewT3);
			}
		}
		void Add_regionop_subfaces(int parent, RegionOperator op)
		{
			var subfaces = Get_subfaces(parent);
            foreach (var tid in op.CurrentBaseTriangles)
			{
				if (tid != parent)
                {
                    Add_subface(subfaces, parent, tid);
                }
            }
		}


		int Get_parent(int tid)
		{
            if (_parentFaces.TryGetValue(tid, out var parent) == false)
            {
                parent = tid;
            }

            return parent;
		}
        HashSet<int> Get_subfaces(int parent)
		{
            if (_subFaces.TryGetValue(parent, out var subfaces) == false)
            {
                subfaces = new HashSet<int>();
                _subFaces[parent] = subfaces;
            }
            return subfaces;
		}
		void Add_subface(HashSet<int> subfaces, int parent, int tid)
		{
			subfaces.Add(tid);
			_parentFaces[tid] = parent;
		}
        List<int> Get_all_baseface_tris(int base_tid)
		{
            var faces = new List<int>(Get_subfaces(base_tid))
            {
                base_tid
            };
            return faces;
		}

		bool Is_inserted_free_edge(int eid)
		{
			var et = Target.GetEdgeT(eid);
			if (Get_parent(et.a) != Get_parent(et.b))
            {
                return false;
            }
            // TODO need to check if we need to save edge AB to connect vertices!
            throw new Exception("not done yet!");
		}




        protected int On_edge(ref Triangle3d tri, ref Vector3d v)
		{
			var s01 = new Segment3d(tri.V0, tri.V1);
			if (s01.DistanceSquared(v) < VertexSnapTol * VertexSnapTol)
            {
                return 0;
            }

            var s12 = new Segment3d(tri.V1, tri.V2);
			if (s12.DistanceSquared(v) < VertexSnapTol * VertexSnapTol)
            {
                return 1;
            }

            var s20 = new Segment3d(tri.V2, tri.V0);
            return s20.DistanceSquared(v) < VertexSnapTol * VertexSnapTol ? 2 : -1;
        }
        protected int On_edge_eid(int tid, Vector3d v)
		{
			var tv = Target.GetTriangle(tid);
			var tri = new Triangle3d();
			Target.GetTriVertices(tid, ref tri.V0, ref tri.V1, ref tri.V2);
			var eidx = On_edge(ref tri, ref v);
			if (eidx < 0)
            {
                return DMesh3.InvalidID;
            }

            var eid = Target.FindEdge(tv[eidx], tv[(eidx + 1) % 3]);
			Util.gDevAssert(eid != DMesh3.InvalidID);
			return eid;
		}
        protected bool Is_on_edge(int eid, Vector3d v)
		{
			var ev = Target.GetEdgeV(eid);
			var seg = new Segment3d(Target.GetVertex(ev.a), Target.GetVertex(ev.b));
			return seg.DistanceSquared(v) < VertexSnapTol * VertexSnapTol;
		}

		protected bool Is_in_triangle(int tid, Vector3d v)
		{
			var tri = new Triangle3d();
			Target.GetTriVertices(tid, ref tri.V0, ref tri.V1, ref tri.V2);
			var bary = tri.BarycentricCoords(v);
			return (bary.x >= 0 && bary.y >= 0 && bary.z >= 0
				  && bary.x < 1 && bary.y <= 1 && bary.z <= 1);

		}



		/// <summary>
		/// find existing vertex at point, if it exists
		/// </summary>
		protected int Find_existing_vertex(Vector3d pt)
		{
			return Find_nearest_vertex(pt, VertexSnapTol);
		}
		/// <summary>
		/// find closest vertex, within searchRadius
		/// </summary>
		protected int Find_nearest_vertex(Vector3d pt, double searchRadius, int ignore_vid = -1)
		{
			var found = (ignore_vid == -1) ?
				_pointHash.FindNearestInRadius(pt, searchRadius,
                            (b) => pt.DistanceSquared(Target.GetVertex(b)))
							:
				_pointHash.FindNearestInRadius(pt, searchRadius,
                            (b) => pt.DistanceSquared(Target.GetVertex(b)),
                            (vid) => vid == ignore_vid);
            return found.Key == _pointHash.InvalidValue ? -1 : found.Key;
        }



    }
}
