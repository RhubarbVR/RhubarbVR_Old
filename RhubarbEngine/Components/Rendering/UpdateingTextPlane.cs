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
using RhubarbEngine.Render;
using RhubarbEngine.World.Asset;
using RNumerics;
using Veldrid;
using System.Numerics;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.IO;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Components.Assets.Procedural_Meshes;

namespace RhubarbEngine.Components.Rendering
{
	[Category(new string[] { "Rendering" })]
	public class UpdateingTextPlane : Component
	{
        public Sync<string> Text;

        public Sync<Colorf> TextColor;

        public Sync<Vector2u> CharacterSizePix;

        public Driver<Vector2u> TextSizeDrive;

        public Driver<string> TextDrive;

        public Driver<Colorf> TextColorDrive;

        public Driver<float> Hight;

        public Driver<float> Width;

        public override void BuildSyncObjs(bool newRefIds)
		{
            Text = new Sync<string>(this, newRefIds,"Hello");
            Text.Changed += TextReload;
            TextColor = new Sync<Colorf>(this, newRefIds,Colorf.Black);
            TextColor.Changed += TextColor_Changed;
            CharacterSizePix = new Sync<Vector2u>(this, newRefIds,new Vector2u(50,70));
            CharacterSizePix.Changed += TextReload;
            TextSizeDrive = new Driver<Vector2u>(this, newRefIds);
            TextSizeDrive.Changed += TextReload;
            TextDrive = new Driver<string>(this, newRefIds);
            TextSizeDrive.Changed += TextReload;
            TextColorDrive = new Driver<Colorf>(this, newRefIds);
            TextColorDrive.Changed += TextColor_Changed;
            Hight = new Driver<float>(this, newRefIds);
            Hight.Changed += TextReload;
            Width = new Driver<float>(this, newRefIds);
            Width.Changed += TextReload;
        }

        public override void OnAttach()
        {
            base.OnAttach();
            var mesh = Entity.AttachComponent<PlaneMesh>();
            Hight.Target = mesh.Height;
            Width.Target = mesh.Width;
            var mit = Entity.AttachComponent<RMaterial>();
            var text = Entity.AttachComponent<Rendering.TextRender>();
            text.Font.Target = World.staticAssets.MainFont;
            TextSizeDrive.Target = text.Scale;
            mit.Shader.Target = World.staticAssets.BasicUnlitShader;
            TextDrive.Target = text.Text;
            var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.Target = text;
            var colorfield = mit.GetField<Render.Material.Fields.ColorField>("TintColor", Render.Shader.ShaderType.MainFrag);
            TextColorDrive.Target = colorfield.field;
            var meshrender = Entity.AttachComponent<MeshRender>();
            meshrender.Materials.Add().Target = mit;
            meshrender.Mesh.Target = mesh;
            meshrender.RenderOrderOffset.Value--;
        }

        private void TextColor_Changed(IChangeable obj)
        {
            TextColorDrive.Drivevalue = TextColor.Value;
        }

        private void TextReload(IChangeable obj)
        {
            TextDrive.Drivevalue = Text.Value;
            var outsize = new Vector2u((uint)(CharacterSizePix.Value.x * Text.Value.Length), CharacterSizePix.Value.y);
            TextSizeDrive.Drivevalue = outsize;
            var planeSize = new Vector2f(outsize.x/ outsize.y,1f);
            Hight.Drivevalue = planeSize.y;
            Width.Drivevalue = planeSize.x;
        }

        public UpdateingTextPlane(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public UpdateingTextPlane()
		{
		}
	}
}
