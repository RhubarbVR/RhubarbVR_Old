using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using System.IO;

namespace BaseR
{
    public static class RhubarbIO
    {

            public static void Serialize<T>(BinaryWriter writer, object obj)
        {
            Type ty = typeof(T);
            switch (ty)
            {
                //Generic System 
                case Type _ when ty == typeof(int):
                    writer.Write((int)obj);
                    return;
                case Type _ when ty == typeof(uint):
                    writer.Write((uint)obj);
                    return;
                case Type _ when ty == typeof(bool):
                    writer.Write((bool)obj);
                    return;
                case Type _ when ty == typeof(char):
                    writer.Write((char)obj);
                    return;
                case Type _ when ty == typeof(string):
                    writer.Write((string)obj);
                    return;
                case Type _ when ty == typeof(float):
                    writer.Write((float)obj);
                    return;
                case Type _ when ty == typeof(double):
                    writer.Write((double)obj);
                    return;
                case Type _ when ty == typeof(long):
                    writer.Write((long)obj);
                    return;
                case Type _ when ty == typeof(ulong):
                    writer.Write((ulong)obj);
                    return;
                case Type _ when ty == typeof(byte):
                    writer.Write((byte)obj);
                    return;
                case Type _ when ty == typeof(sbyte):
                    writer.Write((sbyte)obj);
                    return;
                //Generic System Types

                //List Types
                case Type _ when ty == typeof(List<int>):
                    gSerialization.Store((List<int>)obj, writer);
                    return;
                case Type _ when ty == typeof(List<float>):
                    gSerialization.Store((List<float>)obj, writer);
                    return;
                case Type _ when ty == typeof(List<double>):
                    gSerialization.Store((List<double>)obj, writer);
                    return;
                //list Types

                //Numerics Types
                case Type _ when ty == typeof(Vector2f):
                    gSerialization.Store((Vector2f)obj, writer);
                    return;
                case Type _ when ty == typeof(Vector2d):
                    gSerialization.Store((Vector2d)obj, writer);
                    return;
                case Type _ when ty == typeof(Vector3f):
                    gSerialization.Store((Vector3f)obj, writer);
                    return;
                case Type _ when ty == typeof(Vector3d):
                    gSerialization.Store((Vector3d)obj, writer);
                    return;
                case Type _ when ty == typeof(Quaternionf):
                    gSerialization.Store((Quaternionf)obj, writer);
                    return;
                case Type _ when ty == typeof(Frame3f):
                    gSerialization.Store((Frame3f)obj, writer);
                    return;
                case Type _ when ty == typeof(AxisAlignedBox2d):
                    gSerialization.Store((AxisAlignedBox2d)obj, writer);
                    return;
                case Type _ when ty == typeof(DCurve3):
                    gSerialization.Store((DCurve3)obj, writer);
                    return;
                case Type _ when ty == typeof(PolyLine2d):
                    gSerialization.Store((PolyLine2d)obj, writer);
                    return;
                case Type _ when ty == typeof(Polygon2d):
                    gSerialization.Store((Polygon2d)obj, writer);
                    return;
                case Type _ when ty == typeof(GeneralPolygon2d):
                    gSerialization.Store((GeneralPolygon2d)obj, writer);
                    return;
                case Type _ when ty == typeof(Segment2d):
                    gSerialization.Store((Segment2d)obj, writer);
                    return;
                case Type _ when ty == typeof(Arc2d):
                    gSerialization.Store((Arc2d)obj, writer);
                    return;
                case Type _ when ty == typeof(Circle2d):
                    gSerialization.Store((Circle2d)obj, writer);
                    return;
                case Type _ when ty == typeof(ParametricCurveSequence2):
                    gSerialization.Store((ParametricCurveSequence2)obj, writer);
                    return;
                case Type _ when ty == typeof(IParametricCurve2d):
                    gSerialization.Store((IParametricCurve2d)obj, writer);
                    return;
                case Type _ when ty == typeof(PlanarSolid2d):
                    gSerialization.Store((PlanarSolid2d)obj, writer);
                    return;
                case Type _ when ty == typeof(DMesh3):
                    gSerialization.Store((DMesh3)obj, writer);
                    return;
                case Type _ when ty == typeof(DMesh3):
                    gSerialization.Store((DMesh3)obj, writer);
                    return;
                //Numerics Types

                //DVector Types
                case Type _ when ty == typeof(DVector<double>):
                    gSerialization.Store((DVector<double>)obj, writer);
                    return;
                case Type _ when ty == typeof(DVector<float>):
                    gSerialization.Store((DVector<float>)obj, writer);
                    return;
                case Type _ when ty == typeof(DVector<int>):
                    gSerialization.Store((DVector<int>)obj, writer);
                    return;
                case Type _ when ty == typeof(DVector<short>):
                    gSerialization.Store((DVector<short>)obj, writer);
                    return;
                //DVector Types

                //BaseRTypes
                case Type _ when ty == typeof(NetPointer):
                    writer.Write(((NetPointer)obj).getID());
                    return;
                //BaseRTypes

                default:
                    throw new Exception("Unknown type to Serialize " + ty.FullName);
                    return;
            }
        }
        public static object DeSerialize<T>(BinaryReader reader)
        {
            object val;
            Type ty = typeof(T);
            switch (ty)
            {
                //Generic System Types
                case Type _ when ty == typeof(int):
                    return reader.ReadInt32();
                case Type _ when ty == typeof(uint):
                    return reader.ReadUInt32();
                case Type _ when ty == typeof(bool):
                    return reader.ReadBoolean();
                case Type _ when ty == typeof(char):
                    return reader.ReadChar();
                case Type _ when ty == typeof(string):
                    return reader.ReadString();
                case Type _ when ty == typeof(float):
                    return reader.ReadSingle();
                case Type _ when ty == typeof(double):
                    return reader.ReadDouble();
                case Type _ when ty == typeof(long):
                    return reader.ReadInt64();
                case Type _ when ty == typeof(ulong):
                    return reader.ReadUInt64();
                case Type _ when ty == typeof(byte):
                    return reader.ReadUInt16();
                case Type _ when ty == typeof(sbyte):
                    return reader.ReadInt16();
                //Generic System Types

                //list Types
                case Type _ when ty == typeof(List<int>):
                    val = (object)new List<int>();
                    gSerialization.Restore((List<int>)val, reader);
                    return val;
                case Type _ when ty == typeof(List<float>):
                    val = (object)new List<float>();
                    gSerialization.Restore((List<float>)val, reader);
                    return val;
                case Type _ when ty == typeof(List<double>):
                    val = (object)new List<double>();
                    gSerialization.Restore((List<double>)val, reader);
                    return val;
                //list Types


                //Numerics Types
                case Type _ when ty == typeof(Vector2f):
                    Vector2f Vector2f_val = new Vector2f();
                    gSerialization.Restore(ref Vector2f_val, reader);
                    return Vector2f_val;
                case Type _ when ty == typeof(Vector2d):
                    Vector2d Vector2d_val = new Vector2d();
                    gSerialization.Restore(ref Vector2d_val, reader);
                    return Vector2d_val;
                case Type _ when ty == typeof(Vector3f):
                    Vector3f Vector3f_val = new Vector3f();
                    gSerialization.Restore(ref Vector3f_val, reader);
                    return Vector3f_val;
                case Type _ when ty == typeof(Vector3d):
                    Vector3d Vector3d_val = new Vector3d();
                    gSerialization.Restore(ref Vector3d_val, reader);
                    return Vector3d_val;
                case Type _ when ty == typeof(Quaternionf):
                    Quaternionf Quaternionf_val = new Quaternionf();
                    gSerialization.Restore(ref Quaternionf_val, reader);
                    return Quaternionf_val;
                case Type _ when ty == typeof(Frame3f):
                    Frame3f Frame3f_val = new Frame3f();
                    gSerialization.Restore(ref Frame3f_val, reader);
                    return Frame3f_val;
                case Type _ when ty == typeof(AxisAlignedBox2d):
                    AxisAlignedBox2d AxisAlignedBox2d_val = new AxisAlignedBox2d();
                    gSerialization.Restore(ref AxisAlignedBox2d_val, reader);
                    return AxisAlignedBox2d_val;
                case Type _ when ty == typeof(DCurve3):
                    DCurve3 DCurve3_val = new DCurve3();
                    gSerialization.Restore(DCurve3_val, reader);
                    return DCurve3_val;
                case Type _ when ty == typeof(PolyLine2d):
                    PolyLine2d PolyLine2d_val = new PolyLine2d();
                    gSerialization.Restore(PolyLine2d_val, reader);
                    return PolyLine2d_val;
                case Type _ when ty == typeof(Polygon2d):
                    Polygon2d Polygon2d_val = new Polygon2d();
                    gSerialization.Restore(Polygon2d_val, reader);
                    return Polygon2d_val;
                case Type _ when ty == typeof(GeneralPolygon2d):
                    GeneralPolygon2d GeneralPolygon2d_val = new GeneralPolygon2d();
                    gSerialization.Restore(GeneralPolygon2d_val, reader);
                    return GeneralPolygon2d_val;
                case Type _ when ty == typeof(Segment2d):
                    Segment2d Segment2d_val = new Segment2d();
                    gSerialization.Restore(ref Segment2d_val, reader);
                    return Segment2d_val;
                case Type _ when ty == typeof(Arc2d):
                    Arc2d Arc2d_val = default(Arc2d);
                    gSerialization.Restore(ref Arc2d_val, reader);
                    return Arc2d_val;
                case Type _ when ty == typeof(Circle2d):
                    Circle2d Circle2d_val = default(Circle2d);
                    gSerialization.Restore(ref Circle2d_val, reader);
                    return Circle2d_val;
                case Type _ when ty == typeof(ParametricCurveSequence2):
                    ParametricCurveSequence2 ParametricCurveSequence2_val = default(ParametricCurveSequence2);
                    gSerialization.Restore(ref ParametricCurveSequence2_val, reader);
                    return ParametricCurveSequence2_val;
                case Type _ when ty == typeof(IParametricCurve2d):
                    gSerialization.Restore(out IParametricCurve2d IParametricCurve2d_val, reader);
                    return IParametricCurve2d_val;
                case Type _ when ty == typeof(PlanarSolid2d):
                    PlanarSolid2d PlanarSolid2d_val = default(PlanarSolid2d);
                    gSerialization.Restore(PlanarSolid2d_val, reader);
                    return PlanarSolid2d_val;
                case Type _ when ty == typeof(DMesh3):
                    DMesh3 DMesh3_val = default(DMesh3);
                    gSerialization.Restore(DMesh3_val, reader);
                    return DMesh3_val;
                //Numerics Types


                //DVector Types
                case Type _ when ty == typeof(DVector<double>):
                    DVector<double> DVector_double_val = default(DVector<double>);
                    gSerialization.Restore(DVector_double_val, reader);
                    return DVector_double_val;
                case Type _ when ty == typeof(DVector<float>):
                    DVector<float> DVector_float_val = default(DVector<float>);
                    gSerialization.Restore(DVector_float_val, reader);
                    return DVector_float_val;
                case Type _ when ty == typeof(DVector<int>):
                    DVector<int> DVector_int_val = default(DVector<int>);
                    gSerialization.Restore(DVector_int_val, reader);
                    return DVector_int_val;
                case Type _ when ty == typeof(DVector<short>):
                    DVector<short> DVector_short_val = default(DVector<short>);
                    gSerialization.Restore(DVector_short_val, reader);
                    return DVector_short_val;
                //DVector Types

                //BaseRTypes
                case Type _ when ty == typeof(NetPointer):
                    return  new NetPointer(reader.ReadUInt64());
                //BaseRTypes

                default:
                    throw new Exception("Unknown type to DeSerialize " + ty.FullName);
                    return null;
            }
        }
    }
}
