using System;
using System.Collections.Generic;
using System.Text;

using Vulkan;

namespace Veldrid.Vk
{
	internal static partial class VkFormats
	{
		internal static VkFormat VdToVkPixelFormat(PixelFormat format, bool toDepthFormat = false)
		{
            return format switch
            {
                PixelFormat.R8_UNorm => VkFormat.R8Unorm,
                PixelFormat.R8_SNorm => VkFormat.R8Snorm,
                PixelFormat.R8_UInt => VkFormat.R8Uint,
                PixelFormat.R8_SInt => VkFormat.R8Sint,
                PixelFormat.R16_UNorm => toDepthFormat ? VkFormat.D16Unorm : VkFormat.R16Unorm,
                PixelFormat.R16_SNorm => VkFormat.R16Snorm,
                PixelFormat.R16_UInt => VkFormat.R16Uint,
                PixelFormat.R16_SInt => VkFormat.R16Sint,
                PixelFormat.R16_Float => VkFormat.R16Sfloat,
                PixelFormat.R32_UInt => VkFormat.R32Uint,
                PixelFormat.R32_SInt => VkFormat.R32Sint,
                PixelFormat.R32_Float => toDepthFormat ? VkFormat.D32Sfloat : VkFormat.R32Sfloat,
                PixelFormat.R8_G8_UNorm => VkFormat.R8g8Unorm,
                PixelFormat.R8_G8_SNorm => VkFormat.R8g8Snorm,
                PixelFormat.R8_G8_UInt => VkFormat.R8g8Uint,
                PixelFormat.R8_G8_SInt => VkFormat.R8g8Sint,
                PixelFormat.R16_G16_UNorm => VkFormat.R16g16Unorm,
                PixelFormat.R16_G16_SNorm => VkFormat.R16g16Snorm,
                PixelFormat.R16_G16_UInt => VkFormat.R16g16Uint,
                PixelFormat.R16_G16_SInt => VkFormat.R16g16Sint,
                PixelFormat.R16_G16_Float => VkFormat.R16g16b16a16Sfloat,
                PixelFormat.R32_G32_UInt => VkFormat.R32g32Uint,
                PixelFormat.R32_G32_SInt => VkFormat.R32g32Sint,
                PixelFormat.R32_G32_Float => VkFormat.R32g32b32a32Sfloat,
                PixelFormat.R8_G8_B8_A8_UNorm => VkFormat.R8g8b8a8Unorm,
                PixelFormat.R8_G8_B8_A8_UNorm_SRgb => VkFormat.R8g8b8a8Srgb,
                PixelFormat.B8_G8_R8_A8_UNorm => VkFormat.B8g8r8a8Unorm,
                PixelFormat.B8_G8_R8_A8_UNorm_SRgb => VkFormat.B8g8r8a8Srgb,
                PixelFormat.R8_G8_B8_A8_SNorm => VkFormat.R8g8b8a8Snorm,
                PixelFormat.R8_G8_B8_A8_UInt => VkFormat.R8g8b8a8Uint,
                PixelFormat.R8_G8_B8_A8_SInt => VkFormat.R8g8b8a8Sint,
                PixelFormat.R16_G16_B16_A16_UNorm => VkFormat.R16g16b16a16Unorm,
                PixelFormat.R16_G16_B16_A16_SNorm => VkFormat.R16g16b16a16Snorm,
                PixelFormat.R16_G16_B16_A16_UInt => VkFormat.R16g16b16a16Uint,
                PixelFormat.R16_G16_B16_A16_SInt => VkFormat.R16g16b16a16Sint,
                PixelFormat.R16_G16_B16_A16_Float => VkFormat.R16g16b16a16Sfloat,
                PixelFormat.R32_G32_B32_A32_UInt => VkFormat.R32g32b32a32Uint,
                PixelFormat.R32_G32_B32_A32_SInt => VkFormat.R32g32b32a32Sint,
                PixelFormat.R32_G32_B32_A32_Float => VkFormat.R32g32b32a32Sfloat,
                PixelFormat.BC1_Rgb_UNorm => VkFormat.Bc1RgbUnormBlock,
                PixelFormat.BC1_Rgb_UNorm_SRgb => VkFormat.Bc1RgbSrgbBlock,
                PixelFormat.BC1_Rgba_UNorm => VkFormat.Bc1RgbaUnormBlock,
                PixelFormat.BC1_Rgba_UNorm_SRgb => VkFormat.Bc1RgbaSrgbBlock,
                PixelFormat.BC2_UNorm => VkFormat.Bc2UnormBlock,
                PixelFormat.BC2_UNorm_SRgb => VkFormat.Bc2SrgbBlock,
                PixelFormat.BC3_UNorm => VkFormat.Bc3UnormBlock,
                PixelFormat.BC3_UNorm_SRgb => VkFormat.Bc3SrgbBlock,
                PixelFormat.BC4_UNorm => VkFormat.Bc4UnormBlock,
                PixelFormat.BC4_SNorm => VkFormat.Bc4SnormBlock,
                PixelFormat.BC5_UNorm => VkFormat.Bc5UnormBlock,
                PixelFormat.BC5_SNorm => VkFormat.Bc5SnormBlock,
                PixelFormat.BC7_UNorm => VkFormat.Bc7UnormBlock,
                PixelFormat.BC7_UNorm_SRgb => VkFormat.Bc7SrgbBlock,
                PixelFormat.ETC2_R8_G8_B8_UNorm => VkFormat.Etc2R8g8b8UnormBlock,
                PixelFormat.ETC2_R8_G8_B8_A1_UNorm => VkFormat.Etc2R8g8b8a1UnormBlock,
                PixelFormat.ETC2_R8_G8_B8_A8_UNorm => VkFormat.Etc2R8g8b8a8UnormBlock,
                PixelFormat.D32_Float_S8_UInt => VkFormat.D32SfloatS8Uint,
                PixelFormat.D24_UNorm_S8_UInt => VkFormat.D24UnormS8Uint,
                PixelFormat.R10_G10_B10_A2_UNorm => VkFormat.A2b10g10r10UnormPack32,
                PixelFormat.R10_G10_B10_A2_UInt => VkFormat.A2b10g10r10UintPack32,
                PixelFormat.R11_G11_B10_Float => VkFormat.B10g11r11UfloatPack32,
                _ => throw new VeldridException($"Invalid {nameof(PixelFormat)}: {format}"),
            };
        }
	}
}
