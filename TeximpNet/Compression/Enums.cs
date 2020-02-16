/*
* Copyright (c) 2016-2020 TeximpNet - Nicholas Woodfield
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

namespace TeximpNet.Compression
{
    /// <summary>
    /// Enumerates output formats for the <see cref="Compressor"/>.
    /// </summary>
    public enum CompressionFormat
    {
        /// <summary>
        /// Output in a non-compressed color format. By setting the pixel layout both RGBA and BGRA order or any arbitrary pixel format is possible.
        /// </summary>
        BGRA = 0,

        /// <summary>
        /// Default block compression format (same as <see cref="BC1"/>). By default the implicit 1 bit alpha channel is not used
        /// and is always opaque.
        /// </summary>
        DXT1 = 1,

        /// <summary>
        /// Variation of <see cref="DXT1"/> that supports a 1 bit alpha channel.
        /// </summary>
        DXT1a = 2,

        /// <summary>
        /// Block compression format similar to <see cref="DXT1"/> but with an explicit 3 bit alpha channel.
        /// </summary>
        DXT3 = 3,

        /// <summary>
        /// Block compression format similar to <see cref="DXT1"/> but with a compressed alpha block.
        /// </summary>
        DXT5 = 4,

        /// <summary>
        /// Variation of <see cref="DXT5"/> that is used to represent normal maps by encoding the X and Y components as follows:
        /// R=1, G=Y, B=0, A=X. This swizzle is used to faciliate decompression using "Capcon's DXT decompression trick" (https://code.google.com/archive/p/nvidia-texture-tools/wikis/NormalMapCompression.wiki#Capcon%27s_DXT_trick).
        /// </summary>
        DXT5n = 5,

        /// <summary>
        /// Default block compression format. By default the implicit 1 bit alpha channel is not used
        /// and is always opaque.
        /// </summary>
        BC1 = DXT1,

        /// <summary>
        /// Variation of <see cref="BC1"/> that supports a 1 bit alpha channel.
        /// </summary>
        BC1a = DXT1a,

        /// <summary>
        /// Block compression format similar to <see cref="BC1"/> but with an explicit 3 bit alpha channel.
        /// </summary>
        BC2 = DXT3,

        /// <summary>
        /// Block compression format similar to <see cref="BC1"/> but with a compressed alpha block.
        /// </summary>
        BC3 = DXT5,

        /// <summary>
        /// Variation of <see cref="BC3"/> that is used to represent normal maps by encoding the X and Y components as follows:
        /// R=1, G=Y, B=0, A=X. This swizzle is used to faciliate decompression using "Capcon's DXT decompression trick" (https://code.google.com/archive/p/nvidia-texture-tools/wikis/NormalMapCompression.wiki#Capcon%27s_DXT_trick).
        /// </summary>
        BC3n = DXT5n,

        /// <summary>
        /// Block compression format that only contains a single alpha block.
        /// </summary>
        BC4 = 6,

        /// <summary>
        /// Block compression format that contains two alpha blocks. It's typically used to compress normal maps.
        /// </summary>
        BC5 = 7,

        //Not supported, but keep the entry so if NVTT supports it, we know what to add
        //DXT1n = 8,

        //Not supported, but keep the entry so if NVTT supports it, we know what to add
        //CTX1 = 9,

        /// <summary>
        /// Block compression format for HDR textures, that supports 3 color channels each "half floating point" (16 bits).
        /// </summary>
        BC6 = 10,

        /// <summary>
        /// Block compression format that has three color channels (4-7 bits per channel) and transparency (0-8 bits for alpha).
        /// </summary>
        BC7 = 11,

        /// <summary>
        /// Variation of <see cref="BC3"/> that is used to compress HDR textures.
        /// </summary>
        BC3_RGBM = 12,

        /// <summary>
        /// Ericsson Texture Compression format with 3 color channels and no transparency. Commonly used for mobile devices.
        /// </summary>
        ETC1 = 13,

        /// <summary>
        /// Ericsson Texture Compression with only red channel. Commonly used for mobile devices.
        /// </summary>
        ETC2_R = 14,

        /// <summary>
        /// Ericsson Texture Compression with only red/green channel. Commonly used for mobile devices.
        /// </summary>
        ETC2_RG = 15,

        /// <summary>
        /// Ericsson Texture Compression with 3 color channels and no transparency Commonly used for mobile devices.
        /// </summary>
        ETC2_RGB = 16,

        /// <summary>
        /// Ericsson Texture Compression with 3 color channels and transparency. Commonly used for mobile devices.
        /// </summary>
        ETC2_RGBA = 17,

        /// <summary>
        /// Ericsson Texture Compression with 3 color channels and 1-bit alpha channel. Commonly used for mobile devices.
        /// </summary>
        ETC2_RGB_A1 = 18,

        /// <summary>
        /// Ericsson Texture Compression for HDR textures with 3 color channels. Commonly used for mobile devices.
        /// </summary>
        ETC2_RGBM = 19,

        /// <summary>
        /// PVRTC compression format, with 2 bits per pixel and no transparency. Commonly used for mobile devices.
        /// </summary>
        PVR_2BPP_RGB = 20,

        /// <summary>
        /// PVRTC compression format, with 4 bits per pixel and no transparency. Commonly used for mobile devices.
        /// </summary>
        PVR_4BPP_RGB = 21,

        /// <summary>
        /// PVRTC compression format, with 2 bits per pixel and transparency. Commonly used for mobile devices.
        /// </summary>
        PVR_2BPP_RGBA = 22,

        /// <summary>
        /// PVRTC compression format, with 4 bits per pixel and transparency. Commonly used for mobile devices.
        /// </summary>
        PVR_4BPP_RGBA = 23
    }

    /// <summary>
    /// Enumerates what happens when evaluating the color of texels near an image's border
    /// during mipmap generation, as most filters will sample outside the texture.
    /// </summary>
    public enum WrapMode
    {
        /// <summary>
        /// Sampling outside texture range will use the color at the border.
        /// </summary>
        Clamp = 0,

        /// <summary>
        /// Repeats the texture when sampling outside the texture. E.g. sample between 1 and 2 UV is the same as sampling between 0 and 1 UV.
        /// </summary>
        Repeat = 1,

        /// <summary>
        /// Repeats the texture, but flips the image. E.g. sample between 0 and 1 UV is normal
        /// but sampling between 1 and 2 UV the texture is flipped (mirrored). This is the default option.
        /// </summary>
        Mirror = 2
    }

    /// <summary>
    /// Enumerates texture layouts for input data.
    /// </summary>
    public enum TextureType
    {
        /// <summary>
        /// Input data is a 2D texture.
        /// </summary>
        Texture2D = 0,

        /// <summary>
        /// Input data is a Cubemap, which has 6 faces, where each face is a 2D texture.
        /// </summary>
        TextureCube = 1,

        /// <summary>
        /// Input data is a 3D texture.
        /// </summary>
        Texture3D = 2,

        /// <summary>
        /// Input data is an array of 2D textures that share the same width/height.
        /// </summary>
        Texture2DArray
    }

    /// <summary>
    /// Enumerates filter algorithms used by the <see cref="Compressor"/> for mipmap generation.
    /// </summary>
    public enum MipmapFilter
    {
        /// <summary>
        /// A polyphase box filter. It's the default option and good choice for most cases. It's also much faster than the other filters.
        /// </summary>
        Box = 0,

        /// <summary>
        /// A filter that has a larger width and thus produces blurrier results than the box filter.
        /// </summary>
        Triangle = 1,

        /// <summary>
        /// A kaiser-windowed sinc filter that is generally considered the best choice for downsampling filters.
        /// </summary>
        Kaiser = 2
    }

    /// <summary>
    /// Enumerates quality of compressing image data.
    /// </summary>
    public enum CompressionQuality
    {
        /// <summary>
        /// Least quality, but fastest processing time. Results may be reasonable,
        /// but is not considered to be real-time either.
        /// </summary>
        Fastest = 0,

        /// <summary>
        /// Default quality, balanced in terms of quality / speed.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Production quality, and generally produces similar results to normal, but it
        /// may double or triple the time to obtain minor quality improvements.
        /// </summary>
        Production = 2,

        /// <summary>
        /// Best quality, slowest processing time. May be extremely slow as it brute force
        /// compressor and should generally only be used for testing purposes.
        /// </summary>
        Highest = 3
    }

    /// <summary>
    /// Enumerates options for if an image is resized so it's dimensions are a power of two.
    /// </summary>
    public enum RoundMode
    {
        /// <summary>
        /// No resizing.
        /// </summary>
        None = 0,

        /// <summary>
        /// Dimensions are scaled up to the next power of two.
        /// </summary>
        ToNextPowerOfTwo = 1,

        /// <summary>
        /// Dimensions are scaled up or down to nearest power of two depending on which is closer to the current dimension.
        /// </summary>
        ToNearestPowerOfTwo = 2,

        /// <summary>
        /// Dimensions are scaled down to the previous power of two.
        /// </summary>
        ToPreviousPowerOfTwo = 3,

        /// <summary>
        /// Dimensions are scaled up to the next multiple of four.
        /// </summary>
        ToNextMultipleOfFour,

        /// <summary>
        /// Dimensions are scaled up or down to nearest multiple of four depending on which is closer the current dimension.
        /// </summary>
        ToNearestMultipleOfFour,

        /// <summary>
        /// Dimensions are scaled down to the previous multiple of four.
        /// </summary>
        ToPreviousMultipleOfFour
    }

    /// <summary>
    /// Enumerates how alpha data is handled during processing. Transparency can influence
    /// how mipmaps and compression are handled.
    /// </summary>
    public enum AlphaMode
    {
        /// <summary>
        /// Alpha and color channels are processed independently.
        /// </summary>
        None = 0,

        /// <summary>
        /// Image's alpha data is used for transparency.
        /// </summary>
        Transparency = 1,

        /// <summary>
        /// Image's alpha data is used for transparency and premultiplied with the color channels.
        /// </summary>
        Premultiplied = 2
    }

    /// <summary>
    /// Enumerates the faces of a cubemap texture. Faces are always stored in a list in the order of the enum values
    /// (e.g. Positive_X is index 0, and so on).
    /// </summary>
    public enum CubeMapFace
    {
        /// <summary>
        /// Surface is not a cubemap face.
        /// </summary>
        None = -1,

        /// <summary>
        /// Surface represents the +X face.
        /// </summary>
        Positive_X = 0,

        /// <summary>
        /// Surface represents the -X face.
        /// </summary>
        Negative_X = 1,

        /// <summary>
        /// Surface represents the +Y face.
        /// </summary>
        Positive_Y = 2,

        /// <summary>
        /// Surface represents the -Y face.
        /// </summary>
        Negative_Y = 3,

        /// <summary>
        /// Surface represents the +Z face.
        /// </summary>
        Positive_Z = 4,

        /// <summary>
        /// Surface represents the -Z face.
        /// </summary>
        Negative_Z = 5
    }

    /// <summary>
    /// Format of input data for the compressor. (NOTE: High level C# API currently
    /// only operates on 8-bit RGBA/BGRA data, this is only exposed for the unmanaged API).
    /// </summary>
    public enum InputFormat
    {
        /// <summary>
        /// BGRA order 4 channels 8-bit integers.
        /// </summary>
        BGRA_8UB = 0,

        /// <summary>
        /// 4-channel 16-bit floats.
        /// </summary>
        RGBA_16F = 1,

        /// <summary>
        /// 4-channel 32-bit floats.
        /// </summary>
        RGBA_32F = 2,

        /// <summary>
        /// Single channel of 32-bit floats.
        /// </summary>
        R_32F = 3
    }

    /// <summary>
    /// Output file format if a compressor is writing results to the disk.
    /// </summary>
    public enum OutputFileFormat
    {
        /// <summary>
        /// DirectDrawSurface format (widely supported).
        /// </summary>
        DDS = 0,

        /// <summary>
        /// DirectDrawSurface DX10 format (DDS file will have extended header, but not all DXGI formats may be widely supported).
        /// </summary>
        DDS10 = 1,

        /// <summary>
        /// Khronos texture format.
        /// </summary>
        KTX = 2
    }

    /// <summary>
    /// Error codes for errors that can happen when a compressor is executed.
    /// </summary>
    public enum CompressorError
    {
        /// <summary>
        /// No error or unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Input is invalid.
        /// </summary>
        InvalidInput = 1,

        /// <summary>
        /// The feature is not supported.
        /// </summary>
        UnsupportedFeature = 2,

        /// <summary>
        /// Cuda error.
        /// </summary>
        CudaError = 3,

        /// <summary>
        /// File could not be opened.
        /// </summary>
        FileOpen = 4,

        /// <summary>
        /// File is not writable.
        /// </summary>
        FileWrite = 5,

        /// <summary>
        /// Output format is not supported.
        /// </summary>
        UnsupportedOutputFormat = 6
    }
}