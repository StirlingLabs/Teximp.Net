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
    public sealed class CompressedImageData : IDisposable
    {
        private int m_width, m_height;
        private CubeMapFace m_face;
        private CompressionFormat m_format;
        private IntPtr m_data;
        private int m_sizeInBytes;
        private bool m_isDisposed;

        public int Width
        {
            get
            {
                return m_width;
            }
        }

        public int Height
        {
            get
            {
                return m_height;
            }
        }

        public CubeMapFace Face
        {
            get
            {
                return m_face;
            }
        }

        public CompressionFormat Format
        {
            get
            {
                return m_format;
            }
        }

        public IntPtr DataPtr
        {
            get
            {
                return m_data;
            }
        }

        public int SizeInBytes
        {
            get
            {
                return m_sizeInBytes;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return m_isDisposed;
            }
        }

        public CompressedImageData(int width, int height, CompressionFormat format) : this(width, height, CubeMapFace.None, format) { }

        public CompressedImageData(int width, int height, CubeMapFace face, CompressionFormat format)
        {
            m_width = width;
            m_height = height;
            m_format = format;
            m_isDisposed = false;

            m_sizeInBytes = CalculateSizeInBytes();
            m_data = MemoryHelper.AllocateMemory(m_sizeInBytes);
        }

        ~CompressedImageData()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if(!m_isDisposed)
            {
                if(m_data != IntPtr.Zero)
                {
                    MemoryHelper.FreeMemory(m_data);
                    m_data = IntPtr.Zero;
                }

                m_isDisposed = true;
            }
        }

        private int CalculateSizeInBytes()
        {
            if(m_format == CompressionFormat.BGRA)
                return m_width * m_height * 4;

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

            return width * height * formatSize;
        }
    }
}
