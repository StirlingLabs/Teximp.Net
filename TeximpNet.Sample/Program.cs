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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TeximpNet.Compression;

namespace TeximpNet.Sample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            String dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            String bunnyImage = Path.Combine(dir, "bunny.jpg");

            Surface image = Surface.LoadFromFile(bunnyImage);
            image.FlipVertically();

            //Since we're displaying this to a form, we're using the compressor to generate mipmaps but outputting the data into BGRA format.
            Compressor compressor = new Compressor();
            compressor.Input.GenerateMipmaps = true;
            compressor.Input.SetData(image);
            compressor.Compression.Format = CompressionFormat.BGRA;
            compressor.Compression.SetBGRAPixelFormat(); //If want the output images in RGBA ordering, you get set the pixel layout differently

            List<CompressedImageData> mips = new List<CompressedImageData>();
            if (!compressor.Process(mips))
                throw new ArgumentException("Unable to process image.");

            List<Bitmap> bitmaps = new List<Bitmap>(mips.Count);
            foreach (CompressedImageData imgData in mips)
                bitmaps.Add(ToBitmap(imgData));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MipViewerForm form = new MipViewerForm(bitmaps);
            form.Text = "Viewing bunny.jpg";
            Application.Run(form);
        }

        private static Bitmap ToBitmap(CompressedImageData imageData)
        {
            Bitmap bitmap = new Bitmap(imageData.Width, imageData.Height, PixelFormat.Format32bppArgb);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, imageData.Width, imageData.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            MemoryHelper.CopyMemory(data.Scan0, imageData.DataPtr, imageData.Width * imageData.Height * 4);

            bitmap.UnlockBits(data);

            return bitmap;
        }
    }
}
