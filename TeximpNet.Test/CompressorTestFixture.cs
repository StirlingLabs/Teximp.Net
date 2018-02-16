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
using TeximpNet.Compression;
using Xunit;

namespace TeximpNet.Test
{
    public class CompressorTestFixture : TeximpTestFixture
    {
        [Fact]
        public void TestSetSurfaceData()
        {
            String fileName = GetInputFile("726x337.png");

            Surface surfaceFromFile = Surface.LoadFromFile(fileName);

            //FreeImage is lower left origin (like OpenGL), NVTT is upper-left (like D3D)
            surfaceFromFile.FlipVertically();

            Assert.NotNull(surfaceFromFile);

            Compressor compressor = new Compressor();
            compressor.Compression.Format = CompressionFormat.DXT1;
            compressor.Input.GenerateMipmaps = true;
            compressor.Input.SetData(surfaceFromFile);

            String outputFile = GetOutputFile("726x337.dds");

            Assert.True(compressor.Process(outputFile));

            //Also process to list of mipmaps
            List<CompressedImageData> images = new List<CompressedImageData>();

            Assert.True(compressor.Process(images));
            Assert.True(images.Count == compressor.Input.MipmapCount);

            foreach (CompressedImageData image in images)
                Assert.NotNull(image);
        }

        [Fact]
        public void TestSetSurfaceData_PitchNoPadding()
        {
            String fileName = GetInputFile("bunny.jpg");

            Surface surfaceFromFile = Surface.LoadFromFile(fileName);

            //FreeImage is lower left origin (like OpenGL), NVTT is upper-left (like D3D)
            surfaceFromFile.FlipVertically();

            Assert.NotNull(surfaceFromFile);

            Compressor compressor = new Compressor();
            compressor.Compression.Format = CompressionFormat.DXT1;
            compressor.Input.GenerateMipmaps = true;
            compressor.Input.SetData(surfaceFromFile);

            String outputFile = GetOutputFile("bunny.dds");

            Assert.True(compressor.Process(outputFile));
        }

        private void GetDigits(List<int> list, int num, int minDigitsCount)
        {
            list.Clear();

            while(num > 0)
            {
                list.Add(num % 10);
                num = num / 10;
            }

            //Pad any zeros
            while(list.Count < minDigitsCount)
                list.Add(0);

            list.Reverse();
        }

        [Fact]
        public void TestProcess3DTexture()
        {
            int width = 256;
            int height = 256;
            int depth = 256;
            int slicePitch = width * height * 4;

            IntPtr data = MemoryHelper.AllocateMemory(width * height * depth * 4);
            List<int> digits = new List<int>();
            try
            {
                //Load the 256 BMPs
                String fileNameTemplate = "Noise_Perlin0{0}{1}{2}.bmp";

                IntPtr currSlice = data;

                for(int slice = 0; slice < depth; slice++)
                {
                    GetDigits(digits, slice, 3);

                    String inputFile = GetInputFile(Path.Combine("3D_Perlin_Noise", String.Format(fileNameTemplate, digits[0].ToString(), digits[1].ToString(), digits[2].ToString())));
                    using (Surface imageFromFile = Surface.LoadFromFile(inputFile))
                    {
                        imageFromFile.FlipVertically();

                        //Copy contents of slice to our 3D image
                        MemoryHelper.CopyBGRAImageData(currSlice, imageFromFile.DataPtr, imageFromFile.Width, imageFromFile.Height, 1, imageFromFile.Pitch, 0);
                    }

                    currSlice = MemoryHelper.AddIntPtr(currSlice, slicePitch);
                }

                //Finally set the 3D image to the compressor
                Compressor compressor = new Compressor();
                compressor.Input.GenerateMipmaps = true;
                compressor.Input.SetTextureLayout(TextureType.Texture3D, width, height, depth);
                compressor.Input.SetMipmapData(data, true, ImageInfo.From3D(width, height, depth));
                compressor.Compression.Format = CompressionFormat.DXT1;

                String outputFile = GetOutputFile("3D_Perlin_Noise.dds");
                Assert.True(compressor.Process(outputFile));

                //Also process to list of mipmaps
                List<CompressedImageData> images = new List<CompressedImageData>();

                Assert.True(compressor.Process(images));
                Assert.True(images.Count == compressor.Input.MipmapCount);

                foreach (CompressedImageData image in images)
                    Assert.NotNull(image);
            }
            finally
            {
                MemoryHelper.FreeMemory(data);
            }
        }

        [Fact]
        public void TestRGBAData()
        {
            RGBAQuad[] data = new RGBAQuad[25];
            int width = 5;
            int height = 5;

            for (int i = 0; i < 25; i++)
                data[i] = new RGBAQuad(200, 5, 100, 255);

            IntPtr dataPtr = MemoryHelper.PinObject(data);

            try
            {
                Compressor compressor = new Compressor();
                compressor.Input.GenerateMipmaps = true;
                compressor.Input.SetTextureLayout(TextureType.Texture2D, width, height);
                compressor.Input.SetMipmapData(dataPtr, false, ImageInfo.From2D(width, height));
                compressor.Compression.SetRGBAPixelFormat();
                compressor.Compression.Format = CompressionFormat.BGRA;

                List<CompressedImageData> images = new List<CompressedImageData>();
                Assert.True(compressor.Process(images));
                Assert.True(images.Count == compressor.Input.MipmapCount);

                //First image should be the same...
                CompressedImageData mip0 = images[0];
                Assert.True(mip0.Width == width);
                Assert.True(mip0.Height == height);

                RGBAQuad[] processedData = new RGBAQuad[25];
                MemoryHelper.Read<RGBAQuad>(mip0.DataPtr, processedData, 0, processedData.Length);

                for(int i = 0; i < data.Length; i++)
                {
                    RGBAQuad a = data[i];
                    RGBAQuad b = data[i];

                    Assert.True(a.R == b.R);
                    Assert.True(a.G == b.G);
                    Assert.True(a.B == b.B);
                    Assert.True(a.A == b.A);
                }
            }
            finally
            {
                MemoryHelper.PinObject(data);
            }
        }
    }
}
