using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
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
    public struct Val_uvec2
    {
        Vector2u value;
        public Val_uvec2(Vector2u _value)
        {
            value = _value;
        }
    }

    public struct Val_uvec3
    {
        Vector3u value;
        public Val_uvec3(Vector3u _value)
        {
            value = _value;
        }
    }

    public struct Val_uvec4
    {
        Vector4u value;
        public Val_uvec4(Vector4u _value)
        {
            value = _value;
        }
    }

    public struct Val_vec2
    {
        Vector2f value;
        public Val_vec2(Vector2f _value)
        {
            value = _value;
        }
    }

    public struct Val_vec3
    {
        Vector3f value;
        public Val_vec3(Vector3f _value)
        {
            value = _value;
        }
    }

    public struct Val_vec4
    {
        Vector4f value;
        public Val_vec4(Vector4f _value)
        {
            value = _value;
        }
    }

    public struct Val_dvec2
    {
        Vector2d value;
        public Val_dvec2(Vector2d _value)
        {
            value = _value;
        }
    }

    public struct Val_dvec3
    {
        Vector3d value;
        public Val_dvec3(Vector3d _value)
        {
            value = _value;
        }
    }
    public struct Val_dvec4
    {
        Vector4d value;
        public Val_dvec4(Vector4d _value)
        {
            value = _value;
        }
    }

    public struct Val_mat2x2
    {
        Matrix2f value;
        public Val_mat2x2(Matrix2f _value)
        {
            value = _value;
        }
    }


    public struct Val_mat3x2
    {
        Matrix3x2 value;
        public Val_mat3x2(Matrix3x2 _value)
        {
            value = _value;
        }
    }
}
