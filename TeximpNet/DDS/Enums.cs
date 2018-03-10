/*
* Copyright (c) 2016-2018 TeximpNet - Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;

namespace TeximpNet.DDS
{
    public enum DXGIFormat : uint
    {
        Unknown = 0,
        R32G32B32A32_Typeless = 1,
        R32G32B32A32_Float = 2,
        R32G32B32A32_UInt = 3,
        R32G32B32A32_SInt = 4,
        R32G32B32_Typeless = 5,
        R32G32B32_Float = 6,
        R32G32B32_UInt = 7,
        R32G32B32_SInt = 8,
        R16G16B16A16_Typeless = 9,
        R16G16B16A16_Float = 10,
        R16G16B16A16_UNorm = 11,
        R16G16B16A16_UInt = 12,
        R16G16B16A16_SNorm = 13,
        R16G16B16A16_SInt = 14,
        R32G32_Typeless = 15,
        R32G32_Float = 16,
        R32G32_UInt = 17,
        R32G32_SInt = 18,
        R32G8X24_Typeless = 19,
        D32_Float_S8X24_UInt = 20,
        R32_Float_X8X24_Typeless = 21,
        X32_Typeless_G8X24_UInt = 22,
        R10G10B10A2_Typeless = 23,
        R10G10B10A2_UNorm = 24,
        R10G10B10A2_UInt = 25,
        R11G11B10_Float = 26,
        R8G8B8A8_Typeless = 27,
        R8G8B8A8_UNorm = 28,
        R8G8B8A8_UNorm_SRGB = 29,
        R8G8B8A8_UInt = 30,
        R8G8B8A8_SNorm = 31,
        R8G8B8A8_SInt = 32,
        R16G16_Typeless = 33,
        R16G16_Float = 34,
        R16G16_UNorm = 35,
        R16G16_UInt = 36,
        R16G16_SNorm = 37,
        R16G16_SInt = 38,
        R32_Typeless = 39,
        D32_Float = 40,
        R32_Float = 41,
        R32_UInt = 42,
        R32_SInt = 43,
        R24G8_Typeless = 44,
        D24_UNorm_S8_UInt = 45,
        R24_UNorm_X8_Typeless = 46,
        X24_Typeless_G8_UInt = 47,
        R8G8_Typeless = 48,
        R8G8_UNorm = 49,
        R8G8_UInt = 50,
        R8G8_SNorm = 51,
        R8G8_SInt = 52,
        R16_Typeless = 53,
        R16_Float = 54,
        D16_UNorm = 55,
        R16_UNorm = 56,
        R16_UInt = 57,
        R16_SNorm = 58,
        R16_SInt = 59,
        R8_Typeless = 60,
        R8_UNorm = 61,
        R8_UInt = 62,
        R8_SNorm = 63,
        R8_SInt = 64,
        A8_UNorm = 65,
        R1_UNorm = 66,
        R9G9B9E5_SharedExp = 67,
        R8G8_B8G8_UNorm = 68,
        G8R8_G8B8_UNorm = 69,
        BC1_Typeless = 70,
        BC1_UNorm = 71,
        BC1_UNorm_SRGB = 72,
        BC2_Typeless = 73,
        BC2_UNorm = 74,
        BC2_UNorm_SRGB = 75,
        BC3_Typeless = 76,
        BC3_UNorm = 77,
        BC3_UNorm_SRGB = 78,
        BC4_Typeless = 79,
        BC4_UNorm = 80,
        BC4_SNorm = 81,
        BC5_Typeless = 82,
        BC5_UNorm = 83,
        BC5_SNorm = 84,
        B5G6R5_UNorm = 85,
        B5G5R5A1_UNorm = 86,
        B8G8R8A8_UNorm = 87,
        B8G8R8X8_UNorm = 88,
        R10G10B10_XR_Bias_A2_UNorm = 89,
        B8G8R8A8_Typeless = 90,
        B8G8R8A8_UNorm_SRGB = 91,
        B8G8R8X8_Typeless = 92,
        B8G8R8X8_UNorm_SRGB = 93,
        BC6H_Typeless = 94,
        BC6H_UF16 = 95,
        BC6H_SF16 = 96,
        BC7_Typeless = 97,
        BC7_UNorm = 98,
        BC7_UNorm_SRGB = 99,
        AYUV = 100,
        Y410 = 101,
        Y416 = 102,
        NV12 = 103,
        P010 = 104,
        P016 = 105,
        Opaque_420 = 106,
        YUY2 = 107,
        Y210 = 108,
        Y216 = 109,
        NV11 = 110,
        AI44 = 111,
        IA44 = 112,
        P8 = 113,
        A8P8 = 114,
        B4G4R4A4_UNorm = 115,
        P208 = 130,
        V208 = 131,
        V408 = 132
    }

    /// <summary>
    /// Flags for handling reading and writing DDS files.
    /// </summary>
    [Flags]
    public enum DDSFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Assume pitch is DWORD aligned instead of BYTE aligned (used by some legacy DDS files due to incorrect assumptions).
        /// </summary>
        LegacyDword = 0x1,

        /// <summary>
        /// When loading, do not use work around for long standing D3DX DDS file format issue which reversed the 10:10:10:2 color order masks.
        /// </summary>
        NoR10B10G10A2Fixup = 0x2,

        /// <summary>
        /// When loading, convert DXGI 1.1 BGR formats to RGB order to avoid use of optional WDDM 1.1 features.
        /// </summary>
        ForceRgb = 0x4,

        /// <summary>
        /// When loading, avoid usage of 5:6:5, 5:5:5:1, and 4:4:4:4 formats, instead expand them to 8:8:8:8 to avoid use of optional WDDM 1.2 features.
        /// </summary>
        No16Bpp = 0x8,

        /// <summary>
        /// When loading, return image data without row/slice padding (e.g. strict pitch = width * bytes-per-pixel). When saving image data, it will always be padded based on
        /// the DDS Programming guide on MSDN (<see cref="DDSImage.ComputePitch(DXGIFormat, int, int, out int, out int, out int, out int, bool)"/>).
        /// </summary>
        NoPadding = 0x10,

        /// <summary>
        /// Always write the 'DX10' header extension when writing DDS files. Otherwise DX9 compatible DDS files will be attempted to be written, if the format permits it.
        /// </summary>
        ForceExtendedHeader = 0x20
    };

    public enum TextureDimension
    {
        /// <summary>
        /// 1D texture that has width.
        /// </summary>
        One = 0,

        /// <summary>
        /// 2D texture that has width/height.
        /// </summary>
        Two = 1,

        /// <summary>
        /// 3D texture that has width/height/depth.
        /// </summary>
        Three = 2,

        /// <summary>
        /// Cubemap texture that has 6 square faces, essentially a specialized 2D texture array.
        /// </summary>
        Cube = 3
    }
}
