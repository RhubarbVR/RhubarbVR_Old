using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;

namespace RhubarbEngine.Render.Shader.Vals
{
    public struct Val_bool
    {
        bool value;
        public Val_bool(bool _value)
        {
            value = _value;
        }
    }
    public struct Val_int
    {
        int value;
        public Val_int(int _value)
        {
            value = _value;
        }
    }

    public struct Val_uint
    {
        uint value;
        public Val_uint(uint _value)
        {
            value = _value;
        }
    }

    public struct Val_float
    {
        float value;
        public Val_float(float _value)
        {
            value = _value;
        }
    }

    public struct Val_double
    {
        double value;
        public Val_double(double _value)
        {
            value = _value;
        }
    }

    public struct Val_bvec2
    {
        Vector2b value;
        public Val_bvec2(Vector2b _value)
        {
            value = _value;
        }
    }

    public struct Val_bvec3
    {
        Vector3b value;
        public Val_bvec3(Vector3b _value)
        {
            value = _value;
        }
    }

    public struct Val_bvec4
    {
        Vector4b value;
        public Val_bvec4(Vector4b _value)
        {
            value = _value;
        }
    }

    public struct Val_ivec2
    {
        Vector2i value;
        public Val_ivec2(Vector2i _value)
        {
            value = _value;
        }
    }

    public struct Val_ivec3
    {
        Vector3i value;
        public Val_ivec3(Vector3i _value)
        {
            value = _value;
        }
    }

    public struct Val_ivec4
    {
        Vector4i value;
        public Val_ivec4(Vector4i _value)
        {
            value = _value;
        }
    }


}
