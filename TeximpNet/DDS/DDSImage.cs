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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TeximpNet.DDS
{
    /// <summary>
    /// Represents a set of texture images that was loaded from a DDS file format. A number of texture types are supported, such as 1D, 2D, and 3D image data. Each <see cref="MipChain"/>
    /// collection represents a complete mipmap chain of a single face (e.g. 6 of these chains make up a cubemap). Most textures will just have a single mipmap chain.
    /// </summary>
    [DebuggerDisplay("Dimension = {Dimension}, Format = {Format}, ArrayCount = {MipChains.Count}, MipCount = {MipChains.Count == 0 ? 0 : MipChains[0].Count}")]
    public sealed class DDSImage : IDisposable
    {
        //The 4 characters "DDS "
        private static readonly FourCC DDS_MAGIC = new FourCC('D', 'D', 'S', ' ');

        private bool m_isDisposed;
        private List<MipChain> m_mipChains;
        private DXGIFormat m_format;
        private TextureDimension m_dimension;

        /// <summary>
        /// Gets or sets the texture dimension. Cubemaps must have six entries in <see cref="MipChains"/>.
        /// </summary>
        public TextureDimension Dimension
        {
            get
            {
                return m_dimension;
            }
            set
            {
                m_dimension = value;
            }
        }

        /// <summary>
        /// Gets or sets the texture format. All surfaces must have the same format.
        /// </summary>
        public DXGIFormat Format
        {
            get
            {
                return m_format;
            }
            set
            {
                m_format = value;
            }
        }

        /// <summary>
        /// Gets the collection of mipmap chains. Typically there will be a single mipmap chain (sometimes just containing one image, if no mipmaps). Cubemaps must have six mipmap chains,
        /// and array textures may have any number.
        /// </summary>
        public List<MipChain> MipChains
        {
            get
            {
                return m_mipChains;
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="DDSImage"/> class.
        /// </summary>
        public DDSImage()
        {
            m_mipChains = new List<MipChain>();
            m_format = DXGIFormat.Unknown;
            m_dimension = TextureDimension.Two;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="DDSImage"/> class.
        /// </summary>
        /// <param name="mipChains">Collection of mipmap chains.</param>
        /// <param name="format">Format of the image data.</param>
        /// <param name="texDim">Identifies the dimensions of the image data.</param>
        public DDSImage(List<MipChain> mipChains, DXGIFormat format, TextureDimension texDim)
        {
            m_mipChains = mipChains;
            m_format = format;
            m_dimension = texDim;
            m_isDisposed = false;
        }

        /// <summary>
        /// Validates the contained mipmap surfaces, e.g. all array main images must be the same dimensions and have the same number of mipmaps, cubemaps must have 6 faces, data/sizes must at least be in valid ranges, etc.
        /// </summary>
        /// <returns>True if the image data is not correctly initialized, false if it passes some basic checks.</returns>
        public bool Validate()
        {
            if(m_format == DXGIFormat.Unknown)
                return false;

            if(m_mipChains.Count == 0 || m_mipChains[0].Count == 0)
                return false;

            //Validate cubemap...must have multiples of 6 faces (can be an array of cubes).
            if(m_dimension == TextureDimension.Cube && (m_mipChains.Count % 6) != 0)
                return false;

            //Validate 3d texture..can't have arrays
            if(m_dimension == TextureDimension.Three && m_mipChains.Count > 1)
                return false;

            int width, height, depth, rowPitch, slicePitch;

            //Save the first image dimensions
            MipSurface firstSurface = m_mipChains[0][0];
            width = firstSurface.Width;
            height = firstSurface.Height;
            depth = firstSurface.Depth;
            rowPitch = firstSurface.RowPitch;
            slicePitch = firstSurface.SlicePitch;

            //Validate first surface
            if(width < 1 || height < 1 || depth < 1 || rowPitch < 1 || slicePitch < 1)
                return false;

            //Go through each chain and validate against the first texture and ensure mipmaps are progressively smaller
            int mipCount = -1;

            for(int i = 0; i < m_mipChains.Count; i++)
            {
                MipChain mipmaps = m_mipChains[i];

                //Mips must exist...
                if(mipmaps == null || mipmaps.Count == 0)
                    return false;

                //Grab a mip count from first chain
                if(mipCount == -1)
                    mipCount = mipmaps.Count;

                //Each chain must have the same number of mip surfaces
                if(mipmaps.Count != mipCount)
                    return false;

                //Each mip surface must have data and check sizes
                MipSurface prevMip = mipmaps[0];

                //Check against the first main image we looked at earlier
                if(prevMip.Width != width || prevMip.Height != height || prevMip.Depth != depth || prevMip.Data == IntPtr.Zero || prevMip.RowPitch != rowPitch || prevMip.SlicePitch != slicePitch)
                    return false;

                for(int mipLevel = 1; mipLevel < mipmaps.Count; mipLevel++)
                {
                    MipSurface nextMip = mipmaps[mipLevel];

                    //Ensure each mipmap is progressively smaller at the least
                    if(nextMip.Width > prevMip.Width || nextMip.Height > prevMip.Height || nextMip.Depth > prevMip.Depth || nextMip.Data == IntPtr.Zero 
                        || nextMip.RowPitch > prevMip.RowPitch || nextMip.SlicePitch > prevMip.SlicePitch || nextMip.RowPitch == 0 || nextMip.SlicePitch == 0)
                        return false;

                    prevMip = nextMip;
                }
            }

            return true;
        }

        /// <summary>
        /// Writes images to a DDS file to disk.
        /// </summary>
        /// <param name="fileName">File to write to. If it doesn't exist, it will be created.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public bool Write(String fileName, DDSFlags flags = DDSFlags.None)
        {
            return Write(fileName, m_mipChains, m_format, m_dimension, flags);
        }

        /// <summary>
        /// Writes images contained as DDS formatted data to a stream.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public bool Write(Stream output, DDSFlags flags = DDSFlags.None)
        {
            return Write(output, m_mipChains, m_format, m_dimension, flags);
        }

        /// <summary>
        /// Determines whether a file contains the DDS format. This does not validate the header.
        /// </summary>
        /// <param name="fileName">Name of the file to check.</param>
        /// <returns>True if the file is DDS format, false if not. </returns>
        public static bool IsDDSFile(String fileName)
        {
            if(!File.Exists(fileName))
                return false;

            using(FileStream fs = File.OpenRead(fileName))
                return IsDDSFile(fs);
        }

        /// <summary>
        /// Determines whether a stream contains the DDS format. This does not validate the header. It does not
        /// advance the stream.
        /// </summary>
        /// <param name="input">Stream containing the file data.</param>
        /// <returns>True if the file is DDS format, false if not. </returns>
        public static bool IsDDSFile(Stream input)
        {
            if(input == null || !input.CanRead)
                return false;

            long minSize = (long) (MemoryHelper.SizeOf<Header>() + FourCC.SizeInBytes);
            if(!StreamHelper.CanReadBytes(input, minSize))
                return false;

            //Check magic word
            long pos = input.Position;
            uint magicWord;
            StreamHelper.ReadUInt32(input, out magicWord);
            input.Position = pos;

            return magicWord == DDS_MAGIC;
        }

        /// <summary>
        /// Reads a DDS file from disk. Image data is always returned as DXGI-Compliant
        /// format, therefore some old legacy formats will automatically be converted.
        /// </summary>
        /// <param name="fileName">File to load.</param>
        /// <param name="flags">Flags to control how the DDS data is loaded.</param>
        /// <returns>Loaded image data, or null if the data failed to load.</returns>
        public static DDSImage Read(String fileName, DDSFlags flags = DDSFlags.None)
        {
            if(!File.Exists(fileName))
                return null;

            using(FileStream fs = File.OpenRead(fileName))
                return Read(fs, flags);
        }

        /// <summary>
        /// Reads DDS formatted data from a stream. Image data is always returned as DXGI-Compliant
        /// format, therefore some old legacy formats will automatically be converted.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <param name="flags">Flags to control how the DDS data is loaded.</param>
        /// <returns>Loaded image data, or null if the data failed to load.</returns>
        public static DDSImage Read(Stream input, DDSFlags flags = DDSFlags.None)
        {
            StreamTransferBuffer buffer = new StreamTransferBuffer();

            Header header;
            Header10? headerExt;

            //Reads + validates header(s)
            if(!ReadHeader(input, buffer, out header, out headerExt))
                return null;

            //Gather up metadata 
            List<MipChain> mipChains = null;
            DXGIFormat format = DXGIFormat.Unknown;
            TextureDimension texDim = TextureDimension.Two;
            ConversionFlags convFlags = ConversionFlags.None;
            bool legacyDword = (flags & DDSFlags.LegacyDword) == DDSFlags.LegacyDword ? true : false;

            int width = Math.Max((int) header.Width, 1);
            int height = Math.Max((int) header.Height, 1);
            int depth = Math.Max((int) header.Depth, 1);
            int mipCount = (int) header.MipMapCount;
            int arrayCount = 1;

            //Has extended header, a modern DDS
            if(headerExt.HasValue)
            {
                Header10 extendedHeader = headerExt.Value;
                arrayCount = (int) extendedHeader.ArraySize;
                format = extendedHeader.Format;

                switch(extendedHeader.ResourceDimension)
                {
                    case D3D10ResourceDimension.Texture1D:
                        {
                            texDim = TextureDimension.One;

                            if(height > 1 || depth > 1)
                                return null;
                        }
                        break;
                    case D3D10ResourceDimension.Texture2D:
                        {
                            if((extendedHeader.MiscFlags & Header10Flags.TextureCube) == Header10Flags.TextureCube)
                            {
                                //Can have arrays of tex cubes, so must be multiples of 6
                                if(arrayCount % 6 != 0)
                                    return null;

                                arrayCount *= 6;

                                texDim = TextureDimension.Cube;
                            }
                            else
                            {
                                texDim = TextureDimension.Two;
                            }

                            if(depth > 1)
                                return null;
                        }
                        break;
                    case D3D10ResourceDimension.Texture3D:
                        {
                            texDim = TextureDimension.Three;

                            if(arrayCount > 1 || (header.Caps2 & HeaderCaps2.Volume) != HeaderCaps2.Volume)
                                return null;
                        }
                        break;

                }
            }
            else
            {
                //Otherwise, read legacy DDS and possibly convert data

                //Check volume flag
                if((header.Caps2 & HeaderCaps2.Volume) == HeaderCaps2.Volume)
                {
                    texDim = TextureDimension.Three;
                }
                else
                {
                    //legacy DDS could not express 1D textures, so either a cubemap or a 2D non-array texture

                    if((header.Caps2 & HeaderCaps2.Cubemap) == HeaderCaps2.Cubemap)
                    {
                        //Must have all six faces. DirectX 8 and above always would write out all 6 faces
                        if((header.Caps2 & HeaderCaps2.Cubemap_AllFaces) != HeaderCaps2.Cubemap_AllFaces)
                            return null;

                        arrayCount = 6;
                        texDim = TextureDimension.Cube;
                    }
                    else
                    {
                        texDim = TextureDimension.Two;
                    }
                }

                format = FormatConverter.DetermineDXGIFormat(header.PixelFormat, flags, out convFlags);
            }

            //Modify conversion flags, if necessary
            FormatConverter.ModifyConversionFormat(ref format, ref convFlags, flags);

            //If palette image, the palette will be the first thing
            int[] palette = null;
            if(FormatConverter.HasConversionFlag(convFlags, ConversionFlags.Pal8))
            {
                palette = new int[256];
                int palSize = palette.Length * sizeof(int);
                buffer.ReadBytes(input, palSize);

                if(buffer.LastReadByteCount != palSize)
                    return null;

                MemoryHelper.CopyBytes<int>(buffer.ByteArray, 0, palette, 0, palette.Length);
            }

            //Now read data based on available mip/arrays
            mipChains = new List<MipChain>(arrayCount);

            byte[] scanline = buffer.ByteArray;
            IntPtr scanlinePtr = buffer.Pointer;
            bool noPadding = (flags & DDSFlags.NoPadding) == DDSFlags.NoPadding ? true : false;
            bool isCompressed = FormatConverter.IsCompressed(format);
            bool errored = false;

            try
            {
                //Iterate over each array face...
                for(int i = 0; i < arrayCount; i++)
                {
                    MipChain mipChain = new MipChain(mipCount);
                    mipChains.Add(mipChain);

                    //Iterate over each mip face...
                    for(int mipLevel = 0; mipLevel < mipCount; mipLevel++)
                    {
                        //Calculate mip dimensions
                        int mipWidth = width;
                        int mipHeight = height;
                        int mipDepth = depth;
                        ImageHelper.CalculateMipmapLevelDimensions(mipLevel, ref mipWidth, ref mipHeight, ref mipDepth);

                        //Compute pitch, based on MSDN programming guide which says PitchOrLinearSize is unreliable and to calculate based on format.
                        //"real" mip width/height is the given mip width/height for all non-compressed, compressed images it will be smaller since each block
                        //is a 4x4 region of pixels.
                        int realMipWidth, realMipHeight, dstRowPitch, dstSlicePitch, bytesPerPixel;
                        ImageHelper.ComputePitch(format, mipWidth, mipHeight, out dstRowPitch, out dstSlicePitch, out realMipWidth, out realMipHeight, out bytesPerPixel, legacyDword);
     
                        int srcRowPitch = dstRowPitch;
                        int srcSlicePitch = dstSlicePitch;

                        //Are we converting from a legacy format, possibly?
                        if(!headerExt.HasValue)
                        {
                            int legacySize = FormatConverter.LegacyFormatBitsPerPixelFromConversionFlag(convFlags);
                            if(legacySize != 0)
                            {
                                srcRowPitch = (realMipWidth * legacySize + 7) / 8;
                                srcSlicePitch = srcRowPitch * realMipHeight;
                            }
                        }

                        //If output data is requested not to have padding, recompute destination pitches
                        if(noPadding)
                        {
                            dstRowPitch = bytesPerPixel * realMipWidth;
                            dstSlicePitch = dstRowPitch * realMipHeight;
                        }

                        //Setup memory to hold the loaded image
                        IntPtr data = MemoryHelper.AllocateMemory(mipDepth * dstSlicePitch);
                        MipSurface mipSurface = new MipSurface(mipWidth, mipHeight, mipDepth, dstRowPitch, dstSlicePitch, data);
                        mipChain.Add(mipSurface);

                        //Ensure read buffer is sufficiently sized for a single scanline
                        if(buffer.Length < srcRowPitch)
                            buffer.Resize(srcRowPitch, false);

                        IntPtr dstPtr = data;

                        //Advance stream one slice at a time...
                        for(int slice = 0; slice < mipDepth; slice++)
                        {
                            long slicePos = input.Position;
                            IntPtr dPtr = dstPtr;

                            //Copy scanline into temp buffer, do any conversions, copy to output
                            for(int row = 0; row < realMipHeight; row++)
                            {
                                int numBytesRead = input.Read(scanline, 0, srcRowPitch);
                                if(numBytesRead != srcRowPitch)
                                {
                                    errored = true;
                                    System.Diagnostics.Debug.Assert(false);
                                    return null;
                                }

                                //Copy scanline, optionally convert data
                                FormatConverter.CopyScanline(dPtr, dstRowPitch, scanlinePtr, srcRowPitch, format, convFlags, palette);

                                //Increment dest pointer to next row
                                dPtr = MemoryHelper.AddIntPtr(dPtr, dstRowPitch);
                            }

                            //Advance stream and destination pointer to the next slice
                            input.Position = slicePos + srcSlicePitch;
                            dstPtr = MemoryHelper.AddIntPtr(dstPtr, dstSlicePitch);
                        }
                    }
                }
            }
            finally
            {
                //If errored, clean up any mip surfaces we allocated...no null entries should have been made either
                if(errored)
                {
                    foreach(MipChain chain in mipChains)
                    {
                        foreach(MipSurface surface in chain)
                            surface.Dispose();
                    }
                }
            }

            if (mipChains.Count == 0 || mipChains[0].Count == 0)
            {
                System.Diagnostics.Debug.Assert(false);
                return null;
            }

            return new DDSImage(mipChains, format, texDim);
        }

        /// <summary>
        /// Writes a DDS file to disk. Image data is expected to be DXGI-compliant data, but an effort is made to write out D3D9-compatible headers when possible.
        /// </summary>
        /// <param name="fileName">File to write to. If it doesn't exist, it will be created.</param>
        /// <param name="mipChains">Mipmap chains to write. Each mipmap chain represents a single face (so > 1 represents an array texture or a Cubemap). All faces must have
        /// equivalent dimensions and each chain must have the same number of mipmaps.</param>
        /// <param name="format">DXGI format the image data is stored as.</param>
        /// <param name="texDim">Dimension of the texture to write.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public static bool Write(String fileName, List<MipChain> mipChains, DXGIFormat format, TextureDimension texDim, DDSFlags flags = DDSFlags.None)
        {
            if(!File.Exists(fileName))
                return false;

            using(FileStream fs = File.Create(fileName))
                return Write(fs, mipChains, format, texDim, flags);
        }

        /// <summary>
        /// Writes DDS formatted data to a stream. Image data is expected to be DXGI-compliant data, but an effort is made to write out D3D9-compatible headers when possible.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="mipChains">Mipmap chains to write. Each mipmap chain represents a single face (so > 1 represents an array texture or a Cubemap). All faces must have
        /// equivalent dimensions and each chain must have the same number of mipmaps.</param>
        /// <param name="format">DXGI format the image data is stored as.</param>
        /// <param name="texDim">Dimension of the texture to write.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public static bool Write(Stream output, List<MipChain> mipChains, DXGIFormat format, TextureDimension texDim, DDSFlags flags = DDSFlags.None)
        {
            if(output == null || !output.CanWrite || mipChains == null || mipChains.Count == 0 || mipChains[0].Count == 0 || format == DXGIFormat.Unknown)
                return false;

            //Extract details
            int width, height, depth, arrayCount, mipCount;
            MipSurface firstMip = mipChains[0][0];
            width = firstMip.Width;
            height = firstMip.Height;
            depth = firstMip.Depth;
            arrayCount = mipChains.Count;
            mipCount = mipChains[0].Count;

            int maxPitch = 0;

            //Validate all the surfaces are valid
            foreach(MipChain mipChain in mipChains)
            {
                if(mipChain == null || mipChain.Count != mipCount)
                    return false;

                //Ensure first matches extracted details
                MipSurface mip0 = mipChain[0];
                if(mip0.Width != width || mip0.Height != height || mip0.Depth != depth)
                    return false;

                foreach(MipSurface mip in mipChain)
                {
                    if(mip == null || mip.Data == IntPtr.Zero)
                        return false;

                    maxPitch = Math.Max(mip.RowPitch, maxPitch);
                }
            }

            //Setup a transfer buffer
            StreamTransferBuffer buffer = new StreamTransferBuffer(maxPitch, false);

            //Write out header
            if(!WriteHeader(output, buffer, texDim, format, width, height, depth, arrayCount, mipCount, flags))
                return false;

            //Iterate over each array face...
            for(int i = 0; i < arrayCount; i++)
            {
                MipChain mipChain = mipChains[i];

                //Iterate over each mip face...
                for(int mipLevel = 0; mipLevel < mipCount; mipLevel++)
                {
                    MipSurface mip = mipChain[mipLevel];

                    //Compute pitch, based on MSDN programming guide. We will write out these pitches rather than the supplied in order to conform to the recomendation
                    //that we compute pitch based on format
                    int realMipWidth, realMipHeight, dstRowPitch, dstSlicePitch, bytesPerPixel;
                    ImageHelper.ComputePitch(format, mip.Width, mip.Height, out dstRowPitch, out dstSlicePitch, out realMipWidth, out realMipHeight, out bytesPerPixel);

                    //Ensure write buffer is sufficiently sized for a single scanline
                    if(buffer.Length < dstRowPitch)
                        buffer.Resize(dstRowPitch, false);

                    //Sanity check
                    if(dstRowPitch < mip.RowPitch)
                        return false;

                    //Advance stream one slice at a time...
                    for(int slice = 0; slice < mip.Depth; slice++)
                    {
                        int bytesToWrite = dstSlicePitch;

                        //Copy scanline into temp buffer, write to output
                        for(int row = 0; row < realMipHeight; row++)
                        {
                            MemoryHelper.CopyMemory(buffer.Pointer, mip.Data, dstRowPitch);
                            buffer.WriteBytes(output, dstRowPitch);
                            bytesToWrite -= dstRowPitch;
                        }

                        //Pad slice if necessary
                        if(bytesToWrite > 0)
                        {
                            MemoryHelper.ClearMemory(buffer.Pointer, 0, bytesToWrite);
                            buffer.WriteBytes(output, bytesToWrite);
                        }
                    }
                }

                return true;
            }

            return true;
        }

        private static bool ReadHeader(Stream input, StreamTransferBuffer buffer, out Header header, out Header10? headerExt)
        {
            headerExt = null;

            //Validate that this is a DDS file and can at a minimum read (basic) header info from it
            if(!IsDDSFile(input))
            {
                header = new Header();
                return false;
            }

            //Magic word read, advance by size of the magic word
            input.Position += FourCC.SizeInBytes;

            //Read primary header
            buffer.Read<Header>(input, out header);

            //Verify header
            if(header.Size != MemoryHelper.SizeOf<Header>() || header.PixelFormat.Size != MemoryHelper.SizeOf<PixelFormat>())
                return false;

            //Possibly read extended header
            if(header.PixelFormat.IsDX10Extended)
            {
                //Check if we can read the header
                long minSize = MemoryHelper.SizeOf<Header10>();
                if(!StreamHelper.CanReadBytes(input, minSize))
                    return false;

                Header10 header10;
                buffer.Read<Header10>(input, out header10);

                //Check array size
                if(header10.ArraySize == 0)
                    return false;

                headerExt = header10;
            }

            //Ensure have at least one miplevel, seems like sometimes this will be zero even though there is one mip surface (the main image)
            if(header.MipMapCount == 0)
                header.MipMapCount = 1;

            return true;
        }

        private static bool WriteHeader(Stream output, StreamTransferBuffer buffer, TextureDimension texDim, DXGIFormat format, int width, int height, int depth, int arrayCount, int mipCount, DDSFlags flags)
        {
            //Force the DX10 header...
            bool writeDX10Header = (flags & DDSFlags.ForceExtendedHeader) == DDSFlags.ForceExtendedHeader;

            //Or do DX10 if the following is true...1D textures or 2D texture arrays that aren't cubemaps...
            if(!writeDX10Header)
            {
                switch(texDim)
                {
                    case TextureDimension.One:
                        writeDX10Header = true;
                        break;
                    case TextureDimension.Two:
                        writeDX10Header = arrayCount > 1;
                        break;
                }
            }

            //Figure out pixel format, if not writing DX10 header...
            PixelFormat pixelFormat;
            if(!writeDX10Header)
            {
                switch(format)
                {
                    case DXGIFormat.R8G8B8A8_UNorm:
                        pixelFormat = PixelFormat.A8B8G8R8;
                        break;
                    case DXGIFormat.R16G16_UNorm:
                        pixelFormat = PixelFormat.G16R16;
                        break;
                    case DXGIFormat.R8G8_UNorm:
                        pixelFormat = PixelFormat.A8L8;
                        break;
                    case DXGIFormat.R16_UNorm:
                        pixelFormat = PixelFormat.L16;
                        break;
                    case DXGIFormat.R8_UNorm:
                        pixelFormat = PixelFormat.L8;
                        break;
                    case DXGIFormat.A8_UNorm:
                        pixelFormat = PixelFormat.A8;
                        break;
                    case DXGIFormat.R8G8_B8G8_UNorm:
                        pixelFormat = PixelFormat.R8G8_B8G8;
                        break;
                    case DXGIFormat.G8R8_G8B8_UNorm:
                        pixelFormat = PixelFormat.G8R8_G8B8;
                        break;
                    case DXGIFormat.BC1_UNorm:
                        pixelFormat = PixelFormat.DXT1;
                        break;
                    case DXGIFormat.BC2_UNorm:
                        pixelFormat = PixelFormat.DXT3;
                        break;
                    case DXGIFormat.BC3_UNorm:
                        pixelFormat = PixelFormat.DXT5;
                        break;
                    case DXGIFormat.BC4_UNorm:
                        pixelFormat = PixelFormat.BC4_UNorm;
                        break;
                    case DXGIFormat.BC4_SNorm:
                        pixelFormat = PixelFormat.BC4_SNorm;
                        break;
                    case DXGIFormat.BC5_UNorm:
                        pixelFormat = PixelFormat.BC5_UNorm;
                        break;
                    case DXGIFormat.BC5_SNorm:
                        pixelFormat = PixelFormat.BC5_SNorm;
                        break;
                    case DXGIFormat.B5G6R5_UNorm:
                        pixelFormat = PixelFormat.R5G6B5;
                        break;
                    case DXGIFormat.B5G5R5A1_UNorm:
                        pixelFormat = PixelFormat.A1R5G5B5;
                        break;
                    case DXGIFormat.B8G8R8A8_UNorm:
                        pixelFormat = PixelFormat.A8R8G8B8;
                        break;
                    case DXGIFormat.B8G8R8X8_UNorm:
                        pixelFormat = PixelFormat.X8R8G8B8;
                        break;
                    case DXGIFormat.B4G4R4A4_UNorm:
                        pixelFormat = PixelFormat.A4R4G4B4;
                        break;
                    case DXGIFormat.R32G32B32A32_Float:
                        pixelFormat = PixelFormat.R32G32B32A32_Float;
                        break;
                    case DXGIFormat.R16G16B16A16_Float:
                        pixelFormat = PixelFormat.R16G16B16A16_Float;
                        break;
                    case DXGIFormat.R16G16B16A16_UNorm:
                        pixelFormat = PixelFormat.R16G16B16A16_UNorm;
                        break;
                    case DXGIFormat.R16G16B16A16_SNorm:
                        pixelFormat = PixelFormat.R16G16B16A16_SNorm;
                        break;
                    case DXGIFormat.R32G32_Float:
                        pixelFormat = PixelFormat.R32G32_Float;
                        break;
                    case DXGIFormat.R16G16_Float:
                        pixelFormat = PixelFormat.R16G16_Float;
                        break;
                    case DXGIFormat.R32_Float:
                        pixelFormat = PixelFormat.R32_Float;
                        break;
                    case DXGIFormat.R16_Float:
                        pixelFormat = PixelFormat.R16_Float;
                        break;
                    default:
                        pixelFormat = PixelFormat.DX10Extended;
                        writeDX10Header = true;
                        break;
                }
            }
            else
            {
                pixelFormat = PixelFormat.DX10Extended;
            }

            Header header = new Header();
            header.Size = (uint) MemoryHelper.SizeOf<Header>();
            header.PixelFormat = pixelFormat;
            header.Flags = HeaderFlags.Caps | HeaderFlags.Width | HeaderFlags.Height | HeaderFlags.PixelFormat;
            header.Caps = HeaderCaps.Texture;

            Header10? header10 = null;

            if(mipCount > 0)
            {
                header.Flags |= HeaderFlags.MipMapCount;
                header.MipMapCount = (uint) mipCount;
                header.Caps |= HeaderCaps.MipMap;
            }

            switch(texDim)
            {
                case TextureDimension.One:
                    header.Width = (uint) width;
                    header.Height = 1;
                    header.Depth = 1;

                    //Should always be writing out extended header for 1D textures
                    System.Diagnostics.Debug.Assert(writeDX10Header);

                    header10 = new Header10(format, D3D10ResourceDimension.Texture1D, Header10Flags.None, (uint) arrayCount, Header10Flags2.None);

                    break;
                case TextureDimension.Two:
                    header.Width = (uint) width;
                    header.Height = (uint) height;
                    header.Depth = 1;

                    if(writeDX10Header)
                        header10 = new Header10(format, D3D10ResourceDimension.Texture2D, Header10Flags.None, (uint) arrayCount, Header10Flags2.None);

                    break;
                case TextureDimension.Cube:
                    header.Width = (uint) width;
                    header.Height = (uint) height;
                    header.Depth = 1;
                    header.Caps |= HeaderCaps.Complex;
                    header.Caps2 |= HeaderCaps2.Cubemap_AllFaces;

                    //can support array tex cubes, so must be multiples of 6
                    if(arrayCount % 6 != 0)
                        return false;

                    if(writeDX10Header)
                        header10 = new Header10(format, D3D10ResourceDimension.Texture2D, Header10Flags.TextureCube, (uint) arrayCount / 6, Header10Flags2.None);

                    break;
                case TextureDimension.Three:
                    header.Width = (uint) width;
                    header.Height = (uint) height;
                    header.Depth = (uint) depth;
                    header.Flags |= HeaderFlags.Depth;
                    header.Caps2 |= HeaderCaps2.Volume;

                    if(arrayCount != 1)
                        return false;

                    if(writeDX10Header)
                        header10 = new Header10(format, D3D10ResourceDimension.Texture3D, Header10Flags.None, 1, Header10Flags2.None);

                    break;
            }

            int realWidth, realHeight, rowPitch, slicePitch;
            ImageHelper.ComputePitch(format, width, height, out rowPitch, out slicePitch, out realWidth, out realHeight);

            if(FormatConverter.IsCompressed(format))
            {
                header.Flags |= HeaderFlags.LinearSize;
                header.PitchOrLinearSize = (uint) slicePitch;
            }
            else
            {
                header.Flags |= HeaderFlags.Pitch;
                header.PitchOrLinearSize = (uint) rowPitch;
            }

            //Write out magic word, DDS header, and optionally extended header
            buffer.Write<FourCC>(output, DDS_MAGIC);
            buffer.Write<Header>(output, header);

            if(header10.HasValue)
            {
                System.Diagnostics.Debug.Assert(header.PixelFormat.IsDX10Extended);
                buffer.Write<Header10>(output, header10.Value);
            }

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if(!m_isDisposed)
            {
                if(isDisposing)
                {
                    for(int i = 0; i < m_mipChains.Count; i++)
                    {
                        MipChain mipChain = m_mipChains[i];
                        if(mipChain == null)
                            continue;

                        for(int j = 0; j < mipChain.Count; j++)
                        {
                            MipSurface mipSurface = mipChain[j];
                            if(mipSurface != null)
                                mipSurface.Dispose();
                        }

                        mipChain.Clear();
                    }

                    m_mipChains.Clear();
                }

                m_isDisposed = true;
            }
        }
    }
}
