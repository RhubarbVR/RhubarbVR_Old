using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.IO;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Advanced;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class ImageSharpExtensions
    {
        public static System.Drawing.Bitmap ToBitmap<TPixel>(this Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var memoryStream = new MemoryStream())
            {
                var imageEncoder = image.GetConfiguration().ImageFormatsManager.FindEncoder(PngFormat.Instance);
                image.Save(memoryStream, imageEncoder);

                memoryStream.Seek(0, SeekOrigin.Begin);

                return new System.Drawing.Bitmap(memoryStream);
            }
        }

        public static Image<TPixel> ToImageSharpImage<TPixel>(this System.Drawing.Bitmap bitmap) where TPixel : unmanaged, IPixel<TPixel>
        {
        if (bitmap == null) throw new Exception("BitMap is null");
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

                memoryStream.Seek(0, SeekOrigin.Begin);

                return Image.Load<TPixel>(memoryStream);
            }
        }

        public unsafe static void UpdateTexture(this Texture tex, ImageSharpTexture update, GraphicsDevice gd, ResourceFactory factory)
    {

        if(!(update.Width == tex.Width&& update.Height == tex.Height && update.MipLevels == tex.MipLevels)) throw new Exception("Not Same");

        Texture staging = factory.CreateTexture(
            TextureDescription.Texture2D(update.Width, update.Height, update.MipLevels, 1, update.Format, TextureUsage.Staging));

        Texture ret = tex;

        CommandList cl = gd.ResourceFactory.CreateCommandList();
        cl.Begin();
        for (uint level = 0; level < update.MipLevels; level++)
        {
            Image<Rgba32> image = update.Images[level];
            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixelSpan))
            {
                throw new VeldridException("Unable to get image pixelspan.");
            }
            fixed (void* pin = &MemoryMarshal.GetReference(pixelSpan))
            {
                MappedResource map = gd.Map(staging, MapMode.Write, level);
                uint rowWidth = (uint)(image.Width * 4);
                if (rowWidth == map.RowPitch)
                {
                    Unsafe.CopyBlock(map.Data.ToPointer(), pin, (uint)(image.Width * image.Height * 4));
                }
                else
                {
                    for (uint y = 0; y < image.Height; y++)
                    {
                        byte* dstStart = (byte*)map.Data.ToPointer() + y * map.RowPitch;
                        byte* srcStart = (byte*)pin + y * rowWidth;
                        Unsafe.CopyBlock(dstStart, srcStart, rowWidth);
                    }
                }
                gd.Unmap(staging, level);

                cl.CopyTexture(
                    staging, 0, 0, 0, level, 0,
                    ret, 0, 0, 0, level, 0,
                    (uint)image.Width, (uint)image.Height, 1, 1);

            }
        }
        cl.End();

        gd.SubmitCommands(cl);
        staging.Dispose();
        cl.Dispose();

    }
    public unsafe static void UpdateTextureBmp(this Texture tex, System.Drawing.Bitmap update, GraphicsDevice gd, ResourceFactory factory)
    {

        if (!(update.Width == tex.Width && update.Height == tex.Height)) throw new Exception("Not Same");

        Texture staging = factory.CreateTexture(
            TextureDescription.Texture2D((uint)update.Width, (uint)update.Height, 1, 1, tex.Format, TextureUsage.Staging));

        Texture ret = tex;

        CommandList cl = gd.ResourceFactory.CreateCommandList();
        cl.Begin();

        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, update.Width, update.Height);
        System.Drawing.Imaging.BitmapData bmpData =
            update.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
            update.PixelFormat);

        // Get the address of the first line.
        IntPtr ptr = bmpData.Scan0;

        // Declare an array to hold the bytes of the bitmap.
        int bytes = Math.Abs(bmpData.Stride) * update.Height;
        byte[] argbValues = new byte[bytes + 1];
        // Copy the RGB values into the array.
        System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, bytes);
        byte e;
        fixed (byte* apin = argbValues)
        {
            for (int i = 0; i < bytes; i += 4)
            {
                    e = apin[i];
                    apin[i] = apin[i+2];
                    apin[i + 2] = e;
            }
        }

            fixed (byte* pine = argbValues)
            {
                var pin = pine;
                MappedResource map = gd.Map(staging, MapMode.Write, 0);
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
                gd.Unmap(staging, 0);

                cl.CopyTexture(
                    staging, 0, 0, 0, 0, 0,
                    ret, 0, 0, 0, 0, 0,
                    (uint)update.Width, (uint)update.Height, 1, 1);

            }
        
        cl.End();

        gd.SubmitCommands(cl);
        staging.Dispose();
        cl.Dispose();

    }
}