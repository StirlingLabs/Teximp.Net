using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeximpNet.Compression
{
    public enum CompressionFormat
    {
        BGRA = 0,

        //DX9 formats
        DXT1 = 1,
        DXT1a = 2,
        DXT3 = 3,
        DXT5 = 4,
        DXT5n = 5,

        BC1 = DXT1,
        BC1a = DXT1a,
        BC2 = DXT3,
        BC3 = DXT5,
        BC3n = DXT5n,
        BC4 = 6,
        BC5 = 7
    }

    public enum WrapMode
    {
        Clamp = 0,
        Repeat = 1,
        Mirror = 2
    }

    public enum TextureType
    {
        Texture2D = 0,
        TextureCube = 1
    }

    public enum MipmapFilter
    {
        Box = 0,
        Triangle = 1,
        Kaiser = 2
    }

    public enum CompressionQuality
    {
        Fastest = 0,
        Normal = 1,
        Production = 2,
        Highest = 3
    }

    public enum RoundMode
    {
        None = 0,
        ToNextPowerOfTwo = 1,
        ToNearestPowerOfTwo = 2,
        ToPreviousPowerOfTwo = 3
    }

    public enum AlphaMode
    {
        None = 0,
        Transparency = 1,
        Premultiplied = 2
    }

    public enum CubeMapFace
    {
        None = -1,
        Positive_X = 0,
        Negative_X = 1,
        Positive_Y = 2,
        Negative_Y = 3,
        Positive_Z = 4,
        Negative_Z = 5
    }
}
