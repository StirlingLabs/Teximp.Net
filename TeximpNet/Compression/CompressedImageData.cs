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

namespace TeximpNet.Compression
{
    /// <summary>
    /// Represents output image data from a <see cref="Compressor"/>.
    /// </summary>
    public sealed class CompressedImageData : IDisposable
    {
        private int m_width, m_height, m_depth;
        private int m_arrayIndex;
        private TextureType m_type;
        private CompressionFormat m_format;
        private IntPtr m_data;
        private int m_sizeInBytes;
        private bool m_isDisposed;

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width
        {
            get
            {
                return m_width;
            }
        }

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int Height
        {
            get
            {
                return m_height;
            }
        }

        /// <summary>
        /// Gets the depth of the image, if 3D. If 2D or Cube, this will be one.
        /// </summary>
        public int Depth
        {
            get
            {
                return m_depth;
            }
        }

        /// <summary>
        /// Gets the array index of the image, if it is part of an array or cubmap texture.
        /// </summary>
        public int ArrayIndex
        {
            get
            {
                return m_arrayIndex;
            }
        }

        /// <summary>
        /// Gets the cubemap face of the image, if it is part of a cubemap.
        /// </summary>
        public CubeMapFace Face
        {
            get
            {
                return (m_type == TextureType.TextureCube) ? (CubeMapFace)m_arrayIndex : CubeMapFace.None;
            }
        }

        /// <summary>
        /// Gets the format of the image.
        /// </summary>
        public CompressionFormat Format
        {
            get
            {
                return m_format;
            }
        }

        /// <summary>
        /// Gets a pointer to the image data.
        /// </summary>
        public IntPtr DataPtr
        {
            get
            {
                return m_data;
            }
        }

        /// <summary>
        /// Gets the size of the image data in bytes.
        /// </summary>
        public int SizeInBytes
        {
            get
            {
                return m_sizeInBytes;
            }
        }

        /// <summary>
        /// Gets whether or not the image data has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return m_isDisposed;
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="CompressedImageData"/> class. This initializes a 2D texture.
        /// </summary>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="format">Image format.</param>
        /// <param name="arrayIndex">Optional array index, for non-array 2D images, this is just zero.</param>
        public CompressedImageData(int width, int height, CompressionFormat format, int arrayIndex = 0)
        {
            //NVTT supports array 2D textures, but we have no way of of setting it in the current C-API!!
            Initialize(width, height, 1, arrayIndex, TextureType.Texture2D, format);
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="CompressedImageData"/> class. This initializes a cubemap face.
        /// </summary>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="face">Cubemap face the image represents, if it is part of a cubemap.</param>
        /// <param name="format">Image format.</param>
        public CompressedImageData(int width, int height, CubeMapFace face, CompressionFormat format)
        {
            int arrayIndex = (face == CubeMapFace.None) ? 0 : (int)face;
            m_type = (face == CubeMapFace.None) ? TextureType.Texture2D : TextureType.TextureCube;

            Initialize(width, height, 1, arrayIndex, m_type, format);
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="CompressedImageData"/> class. This initializes a 3D texture.
        /// </summary>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="depth">Depth of the image.</param>
        /// <param name="format">Image format.</param>
        public CompressedImageData(int width, int height, int depth, CompressionFormat format)
        {
            Initialize(width, height, depth, 0, TextureType.Texture3D, format);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CompressedImageData" /> class.
        /// </summary>
        ~CompressedImageData()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Initialize(int width, int height, int depth,  int arrayIndex, TextureType type, CompressionFormat format)
        {
            m_width = width;
            m_height = height;
            m_depth = depth;
            m_format = format;
            m_arrayIndex = arrayIndex;
            m_type = type;
            m_isDisposed = false;

            m_sizeInBytes = CalculateSizeInBytes();
            m_data = MemoryHelper.AllocateMemory(m_sizeInBytes);
            GC.AddMemoryPressure(m_sizeInBytes);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool isDisposing)
        {
            if(!m_isDisposed)
            {
                if(m_data != IntPtr.Zero)
                {
                    MemoryHelper.FreeMemory(m_data);
                    m_data = IntPtr.Zero;

                    GC.RemoveMemoryPressure(m_sizeInBytes);
                }

                m_isDisposed = true;
            }
        }

        private int CalculateSizeInBytes()
        {
            if(m_format == CompressionFormat.BGRA)
                return m_width * m_height * m_depth * 4;

            int formatSize = 0;

            switch(m_format)
            {
                case CompressionFormat.BC1:
                case CompressionFormat.BC1a:
                case CompressionFormat.BC4:
                    formatSize = 8;
                    break;
                case CompressionFormat.BC2:
                case CompressionFormat.BC3:
                case CompressionFormat.BC3n:
                case CompressionFormat.BC5:
                    formatSize = 16;
                    break;
            }

            int width = Math.Max(1, (m_width + 3) / 4);
            int height = Math.Max(1, (m_height + 3) / 4);

            return width * height * m_depth * formatSize;
        }
    }
}
