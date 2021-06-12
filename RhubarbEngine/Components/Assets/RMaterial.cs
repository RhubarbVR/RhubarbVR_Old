using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using g3;
using System.Numerics;
using Veldrid;
using RhubarbEngine.Render;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.IO;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Render.Material.Fields;
using RhubarbEngine.Render.Shader;

namespace RhubarbEngine.Components.Assets
{
    [Category(new string[] { "Assets" })]
    public class RMaterial : AssetProvider<RMaterial>,IAsset
    {
        public AssetRef<RShader> Shader;

        public List<ResourceLayoutElementDescription> resorses;

        public SyncAbstractObjList<MaterialField> Fields;

        public override void onLoaded()
        {
            Logger.Log("Loaded Material");
            load(this);
        }

        public BindableResource[] getBindableResources()
        {
            List<BindableResource> BindableResources = new List<BindableResource>();

            foreach(MaterialField field in Fields)
            {
                if(field.resource == null)
                {
                    throw new Exception("resource is null");
                }
                else
                {
                    BindableResources.Add(field.resource);
                }
            }

            return BindableResources.ToArray();
        }

        public void setValueAtField<T>(string fieldName,ShaderType shaderType,T value)
        {
            foreach (MaterialField item in Fields)
            {
                if (item.fieldName.value == fieldName && item.shaderType.value == shaderType)
                {
                    if (typeof(IWorldObject).IsAssignableFrom(typeof(T)))
                    {
                        item.setValue(((IWorldObject)value).ReferenceID);
                    }
                    else {
                        item.setValue(value);
                    }
                    return;
                }
            }
        }

        public void createField(string fieldName,ShaderType shader,ShaderValueType type)
        {
            Type vatype = typeof(MaterialField);
            switch (type)
            {
                case ShaderValueType.Val_bool:
                    break;
                case ShaderValueType.Val_int:
                    break;
                case ShaderValueType.Val_uint:
                    break;
                case ShaderValueType.Val_float:
                    vatype = typeof(FloatField);
                    break;
                case ShaderValueType.Val_double:
                    break;
                case ShaderValueType.Val_bvec1:
                    break;
                case ShaderValueType.Val_bvec2:
                    break;
                case ShaderValueType.Val_bvec3:
                    break;
                case ShaderValueType.Val_bvec4:
                    break;
                case ShaderValueType.Val_ivec1:
                    break;
                case ShaderValueType.Val_ivec2:
                    break;
                case ShaderValueType.Val_ivec3:
                    break;
                case ShaderValueType.Val_ivec4:
                    break;
                case ShaderValueType.Val_uvec1:
                    break;
                case ShaderValueType.Val_uvec2:
                    break;
                case ShaderValueType.Val_uvec3:
                    break;
                case ShaderValueType.Val_uvec4:
                    break;
                case ShaderValueType.Val_vec1:
                    break;
                case ShaderValueType.Val_vec2:
                    break;
                case ShaderValueType.Val_vec3:
                    break;
                case ShaderValueType.Val_vec4:
                    break;
                case ShaderValueType.Val_dvec1:
                    break;
                case ShaderValueType.Val_dvec2:
                    break;
                case ShaderValueType.Val_dvec3:
                    break;
                case ShaderValueType.Val_dvec4:
                    break;
                case ShaderValueType.Val_mat2x2:
                    break;
                case ShaderValueType.Val_mat3x2:
                    break;
                case ShaderValueType.Val_mat4x2:
                    break;
                case ShaderValueType.Val_mat2x3:
                    break;
                case ShaderValueType.Val_mat3x3:
                    break;
                case ShaderValueType.Val_mat4x3:
                    break;
                case ShaderValueType.Val_mat2x4:
                    break;
                case ShaderValueType.Val_mat3x4:
                    break;
                case ShaderValueType.Val_mat4x4:
                    break;
                case ShaderValueType.Val_color:
                    break;
                case ShaderValueType.Val_texture1D:
                    break;
                case ShaderValueType.Val_texture2D:
                    vatype = typeof(Texture2DField);
                    break;
                case ShaderValueType.Val_texture3D:
                    break;
                default:
                    break;
            }
            MaterialField newField = Fields.Add(vatype, true);
            newField.fieldName.value = fieldName;
            newField.shaderType.value = shader;
            newField.valueType.value = type;

        }

        public void LoadChange(RShader shader)
        {
            logger.Log("Starting Shader Uniform list");
            foreach (ShaderUniform item in shader.Fields)
            {
                bool val = false;
                foreach (MaterialField fildvalue in Fields)
                {
                    if (fildvalue.fieldName.value == item.fieldName)
                    {
                        if (fildvalue.shaderType.value != item.shaderType)
                        {
                            fildvalue.shaderType.value = item.shaderType;
                        }
                        if (fildvalue.valueType.value != item.valueType)
                        {
                            fildvalue.valueType.value = item.valueType;
                        }
                        val = true;
                    }
                }
                if (!val)
                {
                    createField(item.fieldName, item.shaderType, item.valueType);
                }
            }
            load(this);
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            Shader = new AssetRef<RShader>(this, newRefIds);
            Fields = new SyncAbstractObjList<MaterialField>(this, newRefIds);
            Shader.loadChange += LoadChange;
        }
        public RMaterial(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public RMaterial()
        {
        }
    }
}
