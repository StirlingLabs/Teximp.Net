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
using System.Threading.Tasks;
using Xunit;

namespace TeximpNet.Test
{
    public class SurfaceTestFixture : TeximpTestFixture
    {
        [Fact]
        public void TestSurfaceLoadSave()
        {
            String fileName = GetInputFile("bunny.jpg");

            Surface surfaceFromFile = Surface.LoadFromFile(fileName);
           
            Assert.NotNull(surfaceFromFile);

            //Save loaded image to file to make sure we get the same file out
            bool successToFile = surfaceFromFile.SaveToFile(ImageFormat.PNG, GetOutputFile("LoadFromFile.png"));
            Assert.True(successToFile);
            surfaceFromFile.Dispose();
            Assert.True(surfaceFromFile.IsDisposed);
            surfaceFromFile.Dispose(); //Make sure we can double dispose without any problems

            using (FileStream stream = File.OpenRead(fileName))
            {
                Surface surfaceFromStream = Surface.LoadFromStream(stream);

                Assert.NotNull(surfaceFromStream);

                //Save the loaded image to a stream to make sure we get the same file out
                using (FileStream another = File.Create(GetOutputFile("LoadFromStream.png")))
                {
                    bool successToStream = surfaceFromStream.SaveToStream(ImageFormat.PNG, another);
                    Assert.True(successToStream);
                }

                surfaceFromStream.Dispose();
            }
        }

        [Fact]
        public void TestSurfaceFlipInvert()
        {
            String fileName = GetInputFile("bunny.jpg");
            Surface surfaceFromFile = Surface.LoadFromFile(fileName);
            Assert.NotNull(surfaceFromFile);

            Assert.True(surfaceFromFile.FlipHorizontally());
            Assert.True(surfaceFromFile.FlipVertically());
            Assert.True(surfaceFromFile.Invert());

            surfaceFromFile.SaveToFile(ImageFormat.JPEG, GetOutputFile("TestFlipInvert.jpg"));
            surfaceFromFile.Dispose();
        }

        [Fact]
        public void TestSurfaceAdjustBrightness()
        {
            String fileName = GetInputFile("bunny.jpg");
            Surface surfaceFromFile = Surface.LoadFromFile(fileName);
            Assert.NotNull(surfaceFromFile);

            Surface clone = surfaceFromFile.Clone();
            Assert.NotNull(clone);

            Assert.True(clone.AdjustBrightness(50));
            clone.SaveToFile(ImageFormat.JPEG, GetOutputFile("TestBrightnessLighter.jpg"));
            clone.Dispose();
            
            Assert.True(surfaceFromFile.AdjustBrightness(-50));
            surfaceFromFile.SaveToFile(ImageFormat.JPEG, GetOutputFile("TestBrightnessDarker.jpg"));
            surfaceFromFile.Dispose();
        }

        [Fact]
        public void TestSurfaceRotateClone()
        {
            String fileName = GetInputFile("bunny.jpg");
            Surface surfaceFromFile = Surface.LoadFromFile(fileName);
            Assert.NotNull(surfaceFromFile);
            
            //Purposefully try to clone outside the range
            Surface clone = surfaceFromFile.Clone(surfaceFromFile.Width + 5, 0, surfaceFromFile.Width + 20, 10);
            Assert.Null(clone);

            clone = surfaceFromFile.Clone(100, 100, 400, 400);
            Assert.NotNull(clone);

            surfaceFromFile.Dispose();

            clone.SaveToFile(ImageFormat.JPEG, GetOutputFile("SubimageClone.jpg"));

            Assert.True(clone.Rotate(45));
            clone.SaveToFile(ImageFormat.JPEG, GetOutputFile("Rotated.jpg"));

            clone.Dispose();
        }

        [Fact]
        public void TestSurfaceGenerateMipMaps()
        {            
            //Generate mipmaps and combine all of them into a single image
            String fileName = GetInputFile("bunny.jpg");
            Surface surfaceFromFile = Surface.LoadFromFile(fileName);
            Assert.NotNull(surfaceFromFile);

            surfaceFromFile.ConvertTo(ImageConversion.To24Bits);

            List<Surface> mips = new List<Surface>();
            surfaceFromFile.GenerateMipMaps(mips, ImageFilter.Box);

            int maxWidth = 0;
            foreach (Surface m in mips)
                maxWidth += m.Width;

            Surface megaMips = new Surface(maxWidth, surfaceFromFile.Height, false);

            int left = 0;
            foreach(Surface m in mips)
            {
                bool success = megaMips.CopyFrom(m, left, 0);
                left += m.Width;

                Assert.True(success);

                m.Dispose();
            }

            megaMips.SaveToFile(ImageFormat.JPEG, GetOutputFile("MipMapChain.jpg"));
            megaMips.Dispose();
        }

        [Fact]
        public void TestSurfaceSwapColors()
        {
            String fileName = GetInputFile("bunny.jpg");
            Surface surfaceFromFile = Surface.LoadFromFile(fileName);
            Assert.NotNull(surfaceFromFile);

            Assert.True(surfaceFromFile.SwapColors(new RGBAQuad(217, 177, 126, 255), new RGBAQuad(255, 100, 255, 255), true));

            surfaceFromFile.SaveToFile(ImageFormat.JPEG, GetOutputFile("SwappedColors.jpg"));
            surfaceFromFile.Dispose();
        }

        [Fact]
        public void TestSurfaceGammaContrast()
        {
            String fileName = GetInputFile("bunny.jpg");
            Surface surfaceFromFile = Surface.LoadFromFile(fileName);
            Assert.NotNull(surfaceFromFile);

            Surface gammaSurface = surfaceFromFile.Clone();
            Assert.True(gammaSurface.AdjustGamma(5));

            gammaSurface.SaveToFile(ImageFormat.JPEG, GetOutputFile("Gamma.jpg"));
            gammaSurface.Dispose();

            Assert.True(surfaceFromFile.AdjustContrast(50));
            
            surfaceFromFile.SaveToFile(ImageFormat.JPEG, GetOutputFile("Contrast.jpg"));
            surfaceFromFile.Dispose();
        }

        [Fact]
        public void TestThreadedImageLoading()
        {
            //!Note! Originally this was to test loading the library when executing multiple threads
            //XUnit is already running tests in parallel. Freeing the native library at this point caused an unknown error that aborted OTHER
            //tests! Changed this to test load a few images in parallel...

            String fileName = GetInputFile("bunny.jpg");

            List<Task> tasks = new List<Task>();

            for(int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    Surface surfaceFromFile = Surface.LoadFromFile(fileName);
                    Assert.NotNull(surfaceFromFile);
                    Assert.True(surfaceFromFile.Width > 0);
                    Assert.True(surfaceFromFile.Height > 0);
                    surfaceFromFile.Dispose();
                }));
            }

            foreach (Task t in tasks)
                t.Wait();
        }

        [Fact]
        public void TestLoadPalette()
        {
            String fileName = GetInputFile("256Color.bmp");

            Surface surface = Surface.LoadFromFile(fileName);

            Assert.True(surface != null);
            Assert.True(surface.HasPalette);
            Assert.True(surface.PaletteColorCount == 256);
            surface.Dispose();
        }

        [Fact]
        public void TestConversion()
        {
            String fileName = GetInputFile("256Color.bmp");

            Surface surface = Surface.LoadFromFile(fileName);

            List<ImageConversion> formats = new List<ImageConversion>(Enum.GetValues(typeof(ImageConversion)) as ImageConversion[]);

            foreach(ImageConversion format in formats)
            {
                using (Surface clone = surface.Clone())
                {
                    Assert.True(clone.ConvertTo(format));
                }
            }

            surface.Dispose();
        }

        [Fact]
        public void TestLoadRawData_RGBA()
        {
            RGBAQuad[] data = new RGBAQuad[100];
            int width = 10;
            int height = 10;

            for (int i = 0; i < 100; i++)
                data[i] = new RGBAQuad(200, 5, 100, 255); //RGBA data, reddish color. If gets flipped, it'll come out as a purple-ish color.

            IntPtr ptr = MemoryHelper.PinObject(data);

            Surface newSurface = Surface.LoadFromRawData(ptr, width, height, width * 4, false, true);
            Assert.NotNull(newSurface);

            String outputFile = GetOutputFile("rawRedDot-RgbaSrc.bmp");
            newSurface.SaveToFile(ImageFormat.BMP, outputFile);
        }

        [Fact]
        public void TestLoadRawData_BGRA()
        {
            BGRAQuad[] data = new BGRAQuad[100];
            int width = 10;
            int height = 10;

            for (int i = 0; i < 100; i++)
                data[i] = new BGRAQuad(100, 5, 200, 255); //BGRA data, reddish color. If gets flipped, it'll come out as a purple-ish color.

            IntPtr ptr = MemoryHelper.PinObject(data);

            Surface newSurface = Surface.LoadFromRawData(ptr, width, height, width * 4, true, true);
            Assert.NotNull(newSurface);

            String outputFile = GetOutputFile("rawRedDot-BgraSrc.bmp");
            newSurface.SaveToFile(ImageFormat.BMP, outputFile);
        }
    }
}
