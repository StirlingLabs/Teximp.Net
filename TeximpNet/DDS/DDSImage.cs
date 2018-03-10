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
using System.IO;
using System.Runtime.InteropServices;

namespace TeximpNet.DDS
{
    /// <summary>
    /// Represents a set of texture images that was loaded from a DDS file format. A number of texture types are supported, such as 1D, 2D, and 3D image data. Each <see cref="MipChain"/>
    /// collection represents a complete mipmap chain of a single face (e.g. 6 of these chains make up a cubemap). Most textures will just have a single mipmap chain.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
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

            //Validate cubemap...must have exactly 6 faces
            if(m_dimension == TextureDimension.Cube && m_mipChains.Count != 6)
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

        public static DDSImage Read(String fileName, DDSFlags flags = DDSFlags.None)
        {
            if(!File.Exists(fileName))
                return null;

            using(FileStream fs = File.OpenRead(fileName))
                return Read(fs, flags);
        }

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
                            if((extendedHeader.MiscFlag & Header10Flags.TextureCube) == Header10Flags.TextureCube)
                            {
                                if(arrayCount != 6)
                                    return null;

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

                if(buffer.LastReadByteCount != palette.Length)
                    return null;

                MemoryHelper.CopyBytes<int>(buffer.ByteArray, 0, palette, 0, palette.Length);
            }

            //Now read data based on available mip/arrays
            mipChains = new List<MipChain>(arrayCount);

            byte[] scanline = buffer.ByteArray;
            IntPtr scanlinePtr = buffer.Pointer;
            bool noPadding = (flags & DDSFlags.NoPadding) == DDSFlags.NoPadding ? true : false;
            bool isCompressed = FormatConverter.IsCompressed(format);
            bool resizedScanline = false;
            bool errored = false;
            GCHandle handle = new GCHandle();

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
                        ImageHelper.CalculateMipmapLevelDimensions(0, ref mipWidth, ref mipHeight, ref mipDepth);

                        //Compute pitch, MSDN says PitchOrLinearSize is unreliable and to calculate based on format.
                        int compressedWidth, compressedHeight, rowPitch, slicePitch, bytesPerPixel;
                        ImageHelper.ComputePitch(format, mipWidth, mipHeight, out rowPitch, out slicePitch, out compressedWidth, out compressedHeight, out bytesPerPixel, legacyDword);

                        int srcRowPitch = rowPitch;
                        int srcSlicePitch = slicePitch;

                        //If output data is requested not to have padding, recompute pitches
                        if(noPadding)
                        {
                            if(isCompressed)
                            {
                                rowPitch = bytesPerPixel * compressedWidth;
                                slicePitch = rowPitch * compressedHeight;
                            }
                            else
                            {
                                rowPitch = bytesPerPixel * width;
                                slicePitch = rowPitch * height;
                            }
                        }

                        //Setup memory to hold the loaded image
                        IntPtr data = MemoryHelper.AllocateMemory(mipDepth * slicePitch);
                        MipSurface mipSurface = new MipSurface(mipWidth, mipHeight, mipDepth, rowPitch, slicePitch, data);
                        mipChain.Add(mipSurface);

                        //Ensure read buffer is sufficiently sized for a single scanline
                        if(!resizedScanline && scanline.Length < rowPitch)
                        {
                            resizedScanline = true;
                            scanline = new byte[rowPitch];
                            handle = GCHandle.Alloc(scanline, GCHandleType.Pinned);
                        }

                        IntPtr dstPtr = data;

                        //Advance stream one slice at a time...
                        for(int slice = 0; slice < depth; slice++)
                        {
                            long slicePos = input.Position;
                            IntPtr dPtr = dstPtr;

                            //Copy scanline into temp buffer, do any conversions, copy to output
                            for(int row = 0; row < height; row++)
                            {
                                int numBytesRead = input.Read(scanline, 0, rowPitch);
                                if(numBytesRead != rowPitch)
                                {
                                    errored = true;
                                    return null;
                                }

                                FormatConverter.CopyScanline(dPtr, rowPitch,)

                                dPtr = MemoryHelper.AddIntPtr(dPtr, rowPitch);
                            }

                            //Advance stream to the next slice
                            input.Position = slicePos + slicePitch;
                            dstPtr = MemoryHelper.AddIntPtr(dstPtr, slicePitch);
                        }
                    }
                }
            }
            finally
            {
                //Free temp scanline buffer, if we allocated it
                if(resizedScanline && handle.IsAllocated)
                    handle.Free();

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

            return null;
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
