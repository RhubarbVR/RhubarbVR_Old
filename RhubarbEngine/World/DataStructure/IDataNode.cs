using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbDataTypes;
using g3;
namespace RhubarbEngine.World.DataStructure
{
	public interface IDataNode
	{
		byte[] getByteArray();

		void setByteArray(byte[] array);

	}

	public static class DatatNodeTools
	{
		public static Type[] dataNode = new Type[]{
			typeof(object),
			typeof(DataNodeList),
			typeof(DataNodeGroup),
            
                //Generic System 
                typeof(DataNode<int>),
				typeof(DataNode<uint>),
				typeof(DataNode<bool>),
				typeof(DataNode<char>),
				typeof(DataNode<string>),
				typeof(DataNode<float>),
				typeof(DataNode<double>),
				typeof(DataNode<long>),
				typeof(DataNode<ulong>),
				typeof(DataNode<byte>),
				typeof(DataNode<sbyte>),
                //Generic System Types
                typeof(DataNode<NetPointer>),
				typeof(DataNode<DateTime>),
				typeof(DataNode<byte[]>),

                
                //List Types
                typeof(DataNode<List<int>>),
				typeof(DataNode<List<float>>),
				typeof(DataNode<List<double>>),
                //list Types

                //Numerics Types
                typeof(DataNode<Vector2f>),
				typeof(DataNode<Vector2d>),
				typeof(DataNode<Vector3f>),
				typeof(DataNode<Vector4f>),
				typeof(DataNode<Colorf>),



				typeof(DataNode<Vector3d>),
				typeof(DataNode<Quaternionf>),
				typeof(DataNode<Frame3f>),
				typeof(DataNode<AxisAlignedBox2d>),
				typeof(DataNode<Vector2u>),
				typeof(DataNode<Index2i>),
				typeof(DataNode<DCurve3>),
				typeof(DataNode<PolyLine2d>),
				typeof(DataNode<Polygon2d>),
				typeof(DataNode<GeneralPolygon2d>),
				typeof(DataNode<Segment2d>),
				typeof(DataNode<Arc2d>),
				typeof(DataNode<Circle2d>),
				typeof(DataNode<ParametricCurveSequence2>),
				typeof(DataNode<IParametricCurve2d>),
				typeof(DataNode<PlanarSolid2d>),
				typeof(DataNode<DMesh3>),
                //Numerics Types
                               typeof(DataNode<Playback>),

                //DVector Types
                typeof(DataNode<DVector<double>>),
				typeof(DataNode<DVector<float>>),
				typeof(DataNode<DVector<int>>),
				typeof(DataNode<DVector<short>>),
                //DVector Types
                typeof(DataNode<DVector<DateTime>>),

		};
	}
}
