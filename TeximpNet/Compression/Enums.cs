/*
* Copyright (c) 2016-2017 TeximpNet - Nicholas Woodfield
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
