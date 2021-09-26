using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbDataTypes;
using RNumerics;
namespace RhubarbEngine.World.DataStructure
{
	public interface IDataNode
	{
		byte[] GetByteArray();

		void SetByteArray(byte[] array);

	}

	public static class DatatNodeTools
	{
        private static readonly Type[] _types = new Type[]{
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
                typeof(DataNode<short>),
                typeof(DataNode<decimal>),
                typeof(DataNode<byte[]>),
                //Generic System Types
                typeof(DataNode<NetPointer>),
                typeof(DataNode<DateTime>),
                typeof(DataNode<Playback>),
                typeof(DataNode<ColorHSV>),
                typeof(DataNode<DBNull>),


                //Numerics Types
                typeof(DataNode<Vector2f>),
                typeof(DataNode<Vector2d>),
                typeof(DataNode<Vector3b>),
                typeof(DataNode<Vector3i>),
                typeof(DataNode<Vector2i>),
                typeof(DataNode<Vector3f>),
                typeof(DataNode<Vector4f>),
                typeof(DataNode<Colorf>),
                typeof(DataNode<Vector2b>),

                typeof(DataNode<Vector3d>),
                typeof(DataNode<Quaternionf>),
                typeof(DataNode<Frame3f>),
                typeof(DataNode<AxisAlignedBox2d>),
                typeof(DataNode<Vector2u>),
                typeof(DataNode<Index2i>),

                typeof(DataNode<Segment2d>),
                typeof(DataNode<IParametricCurve2d>),
                typeof(DataNode<PlanarSolid2d>),
                typeof(DataNode<DMesh3>),
                typeof(DataNode<DCurve3>),
                typeof(DataNode<PolyLine2d>),
                typeof(DataNode<Polygon2d>),
                typeof(DataNode<GeneralPolygon2d>),
                typeof(DataNode<Arc2d>),
                typeof(DataNode<Circle2d>),
                typeof(DataNode<ParametricCurveSequence2>),
                //Numerics Types


        };
        public static Type[] dataNode = _types;
	}
}
