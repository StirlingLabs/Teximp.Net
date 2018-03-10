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

namespace TeximpNet
{
    /// <summary>
    /// Collection of helper methods for images.
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Gets the number of mipmaps that should be in the chain where the first image has the specified width/height/depth.
        /// </summary>
        /// <param name="width">Width of the first image in the mipmap chain.</param>
        /// <param name="height">Height of the first image in the mipmap chain.</param>
        /// <param name="depth">Depth of the first image in the mipmap chain.</param>
        /// <returns>Number of mipmaps that can be generated for the image.</returns>
        public static int CountMipmaps(int width, int height, int depth)
        {
            int mipmap = 0;

            while(width != 1 || height != 1 || depth != 1)
            {
                width = Math.Max(1, width / 2);
                height = Math.Max(1, height / 2);
                depth = Math.Max(1, depth / 2);
                mipmap++;
            }

            return mipmap + 1;
        }

        /// <summary>
        /// Calculates the mipmap level dimension given the level and the first level's dimensions.
        /// </summary>
        /// <param name="mipLevel">Mip map level to calculate for.</param>
        /// <param name="width">Initially the first level's width, holds the width of the mip level after function returns.</param>
        /// <param name="height">Initially the first level's height, holds the height of the mip level after function returns.</param>
        public static void CalculateMipmapLevelDimensions(int mipLevel, ref int width, ref int height)
        {
            width = Math.Max(1, width >> mipLevel);
            height = Math.Max(1, height >> mipLevel);
        }

        /// <summary>
        /// Calculates the mipmap level dimension given the level and the first level's dimensions.
        /// </summary>
        /// <param name="mipLevel">Mip map level to calculate for.</param>
        /// <param name="width">Initially the first level's width, holds the width of the mip level after function returns.</param>
        /// <param name="height">Initially the first level's height, holds the height of the mip level after function returns.</param>
        /// <param name="depth">Initially the first level's depth, holds the depth of the mip level after function returns.</param>
        public static void CalculateMipmapLevelDimensions(int mipLevel, ref int width, ref int height, ref int depth)
        {
            width = Math.Max(1, width >> mipLevel);
            height = Math.Max(1, height >> mipLevel);
            depth = Math.Max(1, depth >> mipLevel);
        }

        /// <summary>
        /// Gets the previous power of two value.
        /// </summary>
        /// <param name="v">Previous value.</param>
        /// <returns>Previous power of two.</returns>
        public static int PreviousPowerOfTwo(int v)
        {
            return NextPowerOfTwo(v + 1) / 2;
        }

        /// <summary>
        /// Gets the nearest power of two value.
        /// </summary>
        /// <param name="v">Starting value.</param>
        /// <returns>Nearest power of two.</returns>
        public static int NearestPowerOfTwo(int v)
        {
            int np2 = NextPowerOfTwo(v);
            int pp2 = PreviousPowerOfTwo(v);

            if(np2 - v <= v - pp2)
                return np2;
            else
                return pp2;
        }

        /// <summary>
        /// Get the next power of two value.
        /// </summary>
        /// <param name="v">Starting value.</param>
        /// <returns>Next power of two.</returns>
        public static int NextPowerOfTwo(int v)
        {
            int p = 1;
            while(v > p)
            {
                p += p;
            }
            return p;
        }

        /// <summary>
        /// Computes pitch information about an image given its DXGI format and uncompressed dimensions.
        /// </summary>
        /// <param name="format">Format of the image data.</param>
        /// <param name="width">Uncompressed width, in texels.</param>
        /// <param name="height">Uncompressed height, in texels.</param>
        /// <param name="rowPitch">Total # of bytes per scanline.</param>
        /// <param name="slicePitch">Total # of bytes per slice (if 3D texture).</param>
        /// <param name="widthCount">Compressed width, if the format is a compressed image format, otherwise the given width.</param>
        /// <param name="heightCount">Compressed height, if the format is a compressed image format, otherwise the given height.</param>
        /// <param name="legacyDword">True if need to use workaround computation for some incorrectly created DDS files based on legacy DirectDraw assumptions about pitch alignment.</param>
        public static void ComputePitch(DDS.DXGIFormat format, int width, int height, out int rowPitch, out int slicePitch, out int widthCount, out int heightCount, bool legacyDword = false)
        {
            ComputePitch(format, width, height, out rowPitch, out slicePitch, out widthCount, out heightCount, legacyDword);
        }

        /// <summary>
        /// Computes pitch information about an image given its DXGI format and uncompressed dimensions.
        /// </summary>
        /// <param name="format">Format of the image data.</param>
        /// <param name="width">Uncompressed width, in texels.</param>
        /// <param name="height">Uncompressed height, in texels.</param>
        /// <param name="rowPitch">Total # of bytes per scanline.</param>
        /// <param name="slicePitch">Total # of bytes per slice (if 3D texture).</param>
        /// <param name="widthCount">Compressed width, if the format is a compressed image format, otherwise the given width.</param>
        /// <param name="heightCount">Compressed height, if the format is a compressed image format, otherwise the given height.</param>
        /// <param name="bytesPerPixel">Gets the size of the format.</param>
        /// <param name="legacyDword">True if need to use workaround computation for some incorrectly created DDS files based on legacy DirectDraw assumptions about pitch alignment.</param>
        public static void ComputePitch(DDS.DXGIFormat format, int width, int height, out int rowPitch, out int slicePitch, out int widthCount, out int heightCount, out int bytesPerPixel, bool legacyDword = false)
        {
            widthCount = width;
            heightCount = height;

            if(DDS.FormatConverter.IsCompressed(format))
            {
                int blockSize = DDS.FormatConverter.GetCompressedBlockSize(format);

                widthCount = Math.Max(1, (width + 3) / 4);
                heightCount = Math.Max(1, (height + 3) / 4);

                rowPitch = widthCount * blockSize;
                slicePitch = rowPitch * heightCount;

                bytesPerPixel = blockSize / 8;
            }
            else if(DDS.FormatConverter.IsPacked(format))
            {
                rowPitch = ((width + 1) >> 1) * 4;
                slicePitch = rowPitch * height;

                int bitsPerPixel = DDS.FormatConverter.GetBitsPerPixel(format);
                bytesPerPixel = Math.Max(1, bitsPerPixel / 8);
            }
            else
            {
                int bitsPerPixel = DDS.FormatConverter.GetBitsPerPixel(format);
                bytesPerPixel = Math.Max(1, bitsPerPixel / 8);

                if(legacyDword)
                {
                    //Allow for old DDS files that based pitch on certain assumptions
                    rowPitch = ((width * bitsPerPixel + 31) / 32) * sizeof(int);
                    slicePitch = rowPitch * height;
                }
                else
                {
                    rowPitch = (width * bitsPerPixel + 7) / 8;
                    slicePitch = rowPitch * height;
                }
            }
        }

        /// <summary>
        /// Copies 32-bit BGRA color data from the src point (with specified row/slice pitch -- it may be padded!) into a NON-PADDED 32-bit BGRA color image. This
        /// doesn't validate any data, so use at your own risk.
        /// </summary>
        /// <param name="dstBgraPtr">Destination BGRA pointer</param>
        /// <param name="srcBgraPtr">Source BGRA pointer</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="depth">Depth of the image</param>
        /// <param name="rowPitch">Pitch of each scanline of source image.</param>
        /// <param name="slicePitch">Slice of each depth slice of source image.</param>
        public static unsafe void CopyBGRAImageData(IntPtr dstBgraPtr, IntPtr srcBgraPtr, int width, int height, int depth, int rowPitch, int slicePitch)
        {
            int formatSize = 4; //4-byte BGRA texel
            int rowStride = width * formatSize;
            int depthStride = width * height * formatSize;

            IntPtr dstPtr = dstBgraPtr;
            IntPtr srcPtr = srcBgraPtr;

            //Iterate for each depth
            for(int slice = 0; slice < depth; slice++)
            {
                //Start with a pointer that points to the start of the slice
                IntPtr sPtr = srcPtr;

                //And iterate + copy each line per the height of the image
                for(int row = 0; row < height; row++)
                {
                    MemoryHelper.CopyMemory(dstPtr, sPtr, rowStride);

                    //Advance the temporary slice pointer and the source pointer
                    sPtr = MemoryHelper.AddIntPtr(sPtr, rowPitch);
                    dstPtr = MemoryHelper.AddIntPtr(dstPtr, rowStride);
                }

                //Advance the src pointer by the slice pitch to get to the next image
                srcPtr = MemoryHelper.AddIntPtr(srcPtr, slicePitch);
            }
        }

        /// <summary>
        /// Copies 32-bit RGBA color data from the src point (with specified row/slice pitch -- it may be padded!) into a NON-PADDED 32-bit BGRA color image. This
        /// doesn't validate any data, so use at your own risk.
        /// </summary>
        /// <param name="dstBgraPtr">Destination BGRA pointer.</param>
        /// <param name="srcRgbaPtr">Source RGBA pointer.</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="depth">Depth of the image</param>
        /// <param name="rowPitch">Pitch of each scanline of source image.</param>
        /// <param name="slicePitch">Slice of each depth slice of source image.</param>
        public static unsafe void CopyRGBAImageData(IntPtr dstBgraPtr, IntPtr srcRgbaPtr, int width, int height, int depth, int rowPitch, int slicePitch)
        {
            int formatSize = 4; //4-byte BGRA texel
            int rowStride = width * formatSize;
            int depthStride = width * height * formatSize;

            IntPtr dstPtr = dstBgraPtr;
            IntPtr srcPtr = srcRgbaPtr;

            //Iterate for each depth
            for(int slice = 0; slice < depth; slice++)
            {
                //Start with a pointer that points to the start of the slice
                IntPtr sPtr = srcPtr;

                //And iterate + copy each line per the height of the image
                for(int row = 0; row < height; row++)
                {
                    CopyLineToBGRA(sPtr, dstPtr, width);

                    //Advance the temporary slice pointer and the source pointer
                    sPtr = MemoryHelper.AddIntPtr(sPtr, rowPitch);
                    dstPtr = MemoryHelper.AddIntPtr(dstPtr, rowStride);
                }

                //Advance the src pointer by the slice pitch to get to the next image
                srcPtr = MemoryHelper.AddIntPtr(srcPtr, slicePitch);
            }
        }

        /// <summary>
        /// Copies texel by texel in the scanline, swapping R and B components along the way.
        /// </summary>
        /// <param name="rgbaLine">Scanline of RGBA texels, the source data.</param>
        /// <param name="bgraLine">Scanline of BGRA texels, the destination data.</param>
        /// <param name="count">Number of texels to copy.</param>
        public static unsafe void CopyLineToBGRA(IntPtr rgbaLine, IntPtr bgraLine, int count)
        {
            byte* rgbaPtr = (byte*) rgbaLine.ToPointer();
            byte* bgraPtr = (byte*) bgraLine.ToPointer();

            for(int i = 0, byteIndex = 0; i < count; i++, byteIndex += 4)
            {
                int index0 = byteIndex;
                int index1 = byteIndex + 1;
                int index2 = byteIndex + 2;
                int index3 = byteIndex + 3;

                //RGBA -> BGRA
                byte r = rgbaPtr[index0];
                byte g = rgbaPtr[index1];
                byte b = rgbaPtr[index2];
                byte a = rgbaPtr[index3];

                bgraPtr[index0] = b;
                bgraPtr[index1] = g;
                bgraPtr[index2] = r;
                bgraPtr[index3] = a;
            }
        }

        /// <summary>
        /// Copies texel by texel in the scanline, swapping B and R components along the way.
        /// </summary>
        /// <param name="bgraLine">Scanline of BGRA texels, the source data.</param>
        /// <param name="rgbaLine">Scanline of RGBA texels, the destination data.</param>
        /// <param name="count">Number of texels to copy.</param>
        public static unsafe void CopyLineToRGBA(IntPtr bgraLine, IntPtr rgbaLine, int count)
        {
            byte* rgbaPtr = (byte*) rgbaLine.ToPointer();
            byte* bgraPtr = (byte*) bgraLine.ToPointer();

            for(int i = 0, byteIndex = 0; i < count; i++, byteIndex += 4)
            {
                int index0 = byteIndex;
                int index1 = byteIndex + 1;
                int index2 = byteIndex + 2;
                int index3 = byteIndex + 3;

                //BGRA -> RGBA
                byte b = bgraPtr[index0];
                byte g = bgraPtr[index1];
                byte r = bgraPtr[index2];
                byte a = bgraPtr[index3];

                rgbaPtr[index0] = r;
                rgbaPtr[index1] = g;
                rgbaPtr[index2] = b;
                rgbaPtr[index3] = a;
            }
        }
    }
}
