using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace TeximpNet
{
    public class DDSContainer
    {
        //The 4 characters "DDS "
        private const uint DDS_MAGIC = 0x20534444;

        //Temp stream transfer buffer
        private StreamTransferBuffer m_buffer;

        public static unsafe void Test(String filename)
        {
            using(FileStream fs = File.OpenRead(filename))
            {
                DDSContainer container = new DDSContainer();
                using(container.m_buffer = new StreamTransferBuffer())
                {
                    DDSHeader header;
                    DDSHeader10? headerExt;
                    container.ReadHeader(fs, out header, out headerExt);
                    char c = (char) 114;
                }
            }
        }

        public static bool IsDDSFile(String filename)
        {
            if(!File.Exists(filename))
                return false;

            using(FileStream fs = File.OpenRead(filename))
                return IsDDSFile(fs);
        }

        public static bool IsDDSFile(Stream input)
        {
            if(input == null || !input.CanRead)
                return false;

            long minSize = (long) (MemoryHelper.SizeOf<DDSHeader>() + sizeof(uint));
            if(!StreamHelper.CanReadBytes(input, minSize))
                return false;

            //Check magic word
            long pos = input.Position;
            uint magicWord;
            StreamHelper.ReadUInt32(input, out magicWord);
            input.Position = pos;

            return magicWord == DDS_MAGIC;
        }

        private bool ReadHeader(Stream input, out DDSHeader header, out DDSHeader10? headerExt)
        {
            headerExt = null;

            //Validate that this is a DDS file and can at a minimum read (basic) header info from it
            if(!IsDDSFile(input))
            {
                header = new DDSHeader();
                return false;
            }

            //Magic word read, advance by size of uint
            input.Position += sizeof(uint);

            //Read primary header
            m_buffer.Read<DDSHeader>(input, out header);

            //Verify header
            if(header.Size != MemoryHelper.SizeOf<DDSHeader>() || header.PixelFormat.Size != MemoryHelper.SizeOf<DDSPixelFormat>())
                return false;

            //Possibly read extended header
            if(header.PixelFormat.FourCC == MakeFourCC('D', 'X', '1', '0'))
            {
                //Check if we can read the header
                long minSize = MemoryHelper.SizeOf<DDSHeader10>();
                if(!StreamHelper.CanReadBytes(input, minSize))
                    return false;

                DDSHeader10 header10;
                m_buffer.Read<DDSHeader10>(input, out header10);

                headerExt = header10;
            }

            return true;
        }

        private static uint MakeFourCC(char a, char b, char c, char d)
        {
            return ((uint) a) | (((uint) b) << 8) | (((uint) b) << 16) | (((uint) b) << 24);
        }


        #region DDS File data structures 

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private unsafe struct DDSHeader
        {
            public uint Size;
            public DDSHeaderFlags Flags;
            public uint Height;
            public uint Width;
            public uint PitchOrLinearSize;
            public uint Depth;
            public uint MipMapCount;
            public fixed uint Reserved1[11];
            public DDSPixelFormat PixelFormat;
            public DDSHeaderCaps Caps;
            public DDSHeaderCaps2 Caps2;
            public uint Caps3;
            public uint Caps4;
            public uint Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct DDSHeader10
        {
            public DXGIFormat Format;
            public D3D10ResourceDimension ResourceDimension;
            public DDSHeader10Flags MiscFlag;
            public uint ArraySize;
            public DDSHeader10Flags2 MiscFlags2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct DDSPixelFormat
        {
            public uint Size;
            public DDSPixelFormatFlags Flags;
            public uint FourCC;
            public uint RGBBitCount;
            public uint RedBitMask;
            public uint GreenBitMask;
            public uint BlueBitMask;
            public uint AlphaBitMask;
        }

        private enum D3D10ResourceDimension : uint
        {
            Unknown = 0,
            Buffer = 1,
            Texture1D = 2,
            Texture2D = 3,
            Texture3D = 4
        }

        [Flags]
        private enum DDSHeaderFlags : uint
        {
            None = 0,
            Caps = 0x1,
            Height = 0x2,
            Width = 0x4,
            Pitch = 0x8,
            PixelFormat = 0x1000,
            MipMapCount = 0x20000,
            LinearSize = 0x80000,
            Depth = 0x800000
        }

        [Flags]
        private enum DDSHeaderCaps : uint
        {
            None = 0,
            Complex = 0x8,
            Texture = 0x1000,
            MipMap = 0x400000
        }

        [Flags]
        private enum DDSHeaderCaps2 : uint
        {
            None = 0,
            Cubemap = 0x200,
            Cubemap_PositiveX = Cubemap | 0x400,
            Cubemap_NegativeX = Cubemap | 0x800,
            Cubemap_PositiveY = Cubemap | 0x1000,
            Cubemap_NegativeY = Cubemap | 0x2000,
            Cubemap_PositiveZ = Cubemap | 0x4000,
            Cubemap_NegativeZ = Cubemap | 0x8000,
            Cubemap_AllFaces = Cubemap_PositiveX | Cubemap_NegativeX | Cubemap_PositiveY | Cubemap_NegativeY | Cubemap_PositiveZ | Cubemap_NegativeZ,
            Volume = 0x200000
        }

        [Flags]
        private enum DDSHeader10Flags : uint
        {
            None = 0,
            TextureCube = 0x4
        }

        [Flags]
        private enum DDSHeader10Flags2 : uint
        {
            None = 0,
            AlphaModeStraight = 0x1,
            AlphaModePremultiplied = 0x2,
            AlphaModeOpaque = 0x3,
            AlphaModeCustom = 0x4
        }

        [Flags]
        private enum DDSPixelFormatFlags : uint
        {
            None = 0,
            AlphaPixels = 0x1,
            Alpha = 0x2,
            FourCC = 0x4,
            RGB = 0x40,
            RGBA = RGB | Alpha,
            YUV = 0x200,
            Luminance = 0x20000,
            LuminanceAlpha = Luminance | Alpha,
            Pal8 = 0x00000020,
            Pal8Alpha = Pal8 | Alpha,
            BumpDUDV = 0x00080000
        }

        private enum DXGIFormat : uint
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

        #endregion
    }
}
