using CefSharp.OffScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace RhubarbEngine.Render
{
    public class UpdateDatingTexture2D: IDisposable
    {
        public Texture target;
        private GraphicsDevice _gb;
        Texture staging;
        public unsafe void UpdateBitmap(BitmapBuffer update)
        {
            if (!(update.Width == target.Width && update.Height == target.Height)) throw new Exception("Not Same");

            CommandList cl = _gb.ResourceFactory.CreateCommandList();
            cl.Begin();

            fixed (byte* bmpData = update.Buffer)
            {
                // Get the address of the first line.
                IntPtr ptr = (IntPtr)bmpData;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = update.Buffer.Length;
                byte[] argbValues = new byte[bytes + 1];
                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, bytes);
                byte e;
                fixed (byte* apin = argbValues)
                {
                    for (int i = 0; i < bytes; i += 4)
                    {
                        e = apin[i];
                        apin[i] = apin[i + 2];
                        apin[i + 2] = e;
                    }
                    var pin = apin;
                    MappedResource map = _gb.Map(staging, MapMode.Write, 0);
                    uint rowWidth = (uint)((update.Width) * 4);
                    if (rowWidth == map.RowPitch)
                    {
                        Unsafe.CopyBlock(map.Data.ToPointer(), pin, (uint)((update.Width) * (update.Height) * 4));
                    }
                    else
                    {
                        for (uint y = 0; y < update.Height; y++)
                        {
                            byte* dstStart = (byte*)map.Data.ToPointer() + y * map.RowPitch;
                            byte* srcStart = (byte*)pin + y * rowWidth;
                            Unsafe.CopyBlock(dstStart, srcStart, rowWidth);
                        }
                    }
                    _gb.Unmap(staging, 0);

                    cl.CopyTexture(
                        staging, 0, 0, 0, 0, 0,
                        target, 0, 0, 0, 0, 0,
                        (uint)update.Width, (uint)update.Height, 1, 1);

                }
                cl.End();
                _gb.SubmitCommands(cl);
                cl.Dispose();

            }
        }

        public TextureView InitializeView(Texture texture,GraphicsDevice gb)
        {
            _gb = gb;
            target = texture;
            BuildCopyTexture();
            return gb.ResourceFactory.CreateTextureView(target);
        }

        private void BuildCopyTexture()
        {
            staging = _gb.ResourceFactory.CreateTexture(
             TextureDescription.Texture2D((uint)target.Width, (uint)target.Height, 1, 1, target.Format, TextureUsage.Staging));

        }

        public void Initialize(Texture texture, GraphicsDevice gb)
        {
            _gb = gb;
            target = texture;
            BuildCopyTexture();
        }

        public void Dispose()
        {
            staging?.Dispose();
        }
    }
}
