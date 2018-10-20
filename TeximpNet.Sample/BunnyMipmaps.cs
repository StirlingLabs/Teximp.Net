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
using TeximpNet.Compression;
using TeximpNet.DDS;

namespace TeximpNet.Sample
{
    public class BunnyMipmaps : IDisposable
    {
        private bool m_isDisposed;
        private String m_filename;
        private DDSContainer m_ddsContainer;

        public DDSContainer DDSContainer
        {
            get
            {
                return m_ddsContainer;
            }
        }

        public BunnyMipmaps(String filename)
        {
            m_filename = filename;
        }

        public bool Load()
        {
            ClearMips();

            if (!System.IO.File.Exists(m_filename))
                return false;

            Surface image = Surface.LoadFromFile(m_filename);

            if (image == null)
                return false;

            image.FlipVertically();

            //Since we're displaying this to a form, we're using the compressor to generate mipmaps but outputting the data into BGRA format.
            using (Compressor compressor = new Compressor())
            {
                compressor.Input.GenerateMipmaps = true;
                compressor.Input.SetData(image);
                compressor.Compression.Format = CompressionFormat.BGRA;
                compressor.Compression.SetBGRAPixelFormat(); //If want the output images in RGBA ordering, you get set the pixel layout differently

                compressor.Process(out m_ddsContainer);
                return m_ddsContainer != null;
            }
        }

        private void ClearMips()
        {
            if(m_ddsContainer != null)
                m_ddsContainer.Dispose();

            m_ddsContainer = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_isDisposed)
            {
                if (disposing)
                {
                    ClearMips();
                }

                m_isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
