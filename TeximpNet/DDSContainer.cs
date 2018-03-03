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

        //Buffer to transfer to/from a stream
        private byte[] m_transferBuffer = new byte[81920];

        //Pin the transfer buffer once for duration of read/write
        private GCHandle m_pinnedBuffer;



        public static unsafe void Test(String filename)
        {
            using(FileStream fs = File.OpenRead(filename))
            {
                DDSContainer container = new DDSContainer();
                container.PinTransferBuffer();

                DDSHeader header;
                DDSHeader10? headerExt;
                container.ReadHeader(fs, out header, out headerExt);

                container.UnpinTransferBuffer();
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

        private void PinTransferBuffer()
        {
            m_pinnedBuffer = GCHandle.Alloc(m_transferBuffer, GCHandleType.Pinned);
        }

        private void UnpinTransferBuffer()
        {
            m_pinnedBuffer.Free();
        }
        
        private void ReadFromStream<T>(Stream input, out T value) where T : struct
        {
            input.Read(m_transferBuffer, 0, MemoryHelper.SizeOf<T>());
            MemoryHelper.Read<T>(m_pinnedBuffer.AddrOfPinnedObject(), out value);
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
            ReadFromStream<DDSHeader>(input, out header);

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
                ReadFromStream<DDSHeader10>(input, out header10);
               
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
            public uint Flags;
            public uint Height;
            public uint Width;
            public uint PitchOrLinearSize;
            public uint Depth;
            public uint MipMapCount;
            public fixed uint Reserved1[11];
            public DDSPixelFormat PixelFormat;
            public uint Caps;
            public uint Caps2;
            public uint Caps3;
            public uint Caps4;
            public uint Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct DDSHeader10
        {
            public DXGIFormat Format;
            public D3D10ResourceDimension ResourceDimension;
            public uint MiscFlag;
            public uint ArraySize;
            public uint MiscFlags2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct DDSPixelFormat
        {
            public uint Size;
            public uint Flags;
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
