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
using TeximpNet.Compression;
using TeximpNet.DDS;
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
            DDSContainer ddsContainer;

            Assert.True(compressor.Process(out ddsContainer));
            Assert.NotNull(ddsContainer);
            Assert.True(ddsContainer.MipChains.Count == 1);

            foreach(DDS.MipChain mips in ddsContainer.MipChains)
            {
                Assert.True(mips.Count == compressor.Input.MipmapCount);

                foreach(DDS.MipData mip in mips)
                    Assert.NotNull(mip);
            }

            ddsContainer.Dispose();
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

                DDSContainer ddsContainer;

                Assert.True(compressor.Process(out ddsContainer));
                Assert.NotNull(ddsContainer);
                Assert.True(ddsContainer.MipChains.Count == 1 && ddsContainer.MipChains[0].Count == compressor.Input.MipmapCount);

                //First image should be the same...
                MipData mip0 = ddsContainer.MipChains[0][0];
                Assert.True(mip0.Width == width);
                Assert.True(mip0.Height == height);

                RGBAQuad[] processedData = new RGBAQuad[25];
                MemoryHelper.Read<RGBAQuad>(mip0.Data, processedData, 0, processedData.Length);

                for(int i = 0; i < data.Length; i++)
                {
                    RGBAQuad a = data[i];
                    RGBAQuad b = data[i];

                    Assert.True(a.R == b.R);
                    Assert.True(a.G == b.G);
                    Assert.True(a.B == b.B);
                    Assert.True(a.A == b.A);
                }

                ddsContainer.Dispose();
            }
            finally
            {
                MemoryHelper.PinObject(data);
            }
        }

        [Fact]
        public void TestSetMipData()
        {
            RGBAQuad[] dataRGBA = new RGBAQuad[25];
            BGRAQuad[] dataBGRA = new BGRAQuad[25];
            int width = 5;
            int height = 5;

            for(int i = 0; i < width * height; i++)
            {
                dataRGBA[i] = new RGBAQuad(200, 5, 100, 255);
                dataBGRA[i] = new BGRAQuad(100, 5, 200, 255);
            }

            IntPtr dataRGBAPtr = MemoryHelper.PinObject(dataRGBA);
            IntPtr dataBGRAPtr = MemoryHelper.PinObject(dataBGRA);

            MipData rgbaMip = new MipData(width, height, width * 4, dataRGBAPtr, false);
            MipData bgraMip = new MipData(width, height, width * 4, dataBGRAPtr, false);

            Compressor compressor = new Compressor();
            compressor.Input.GenerateMipmaps = true;
            compressor.Input.SetData(bgraMip, true);
            compressor.Compression.SetBGRAPixelFormat();
            compressor.Compression.Format = CompressionFormat.BGRA;

            DDSContainer outputImage1;
            compressor.Process(out outputImage1);

            //Check bgra
            for(int i = 0, offset = 0; i < width * height; i++, offset += 4)
            {
                IntPtr outputPtr = outputImage1.MipChains[0][0].Data;
                BGRAQuad v1 = MemoryHelper.Read<BGRAQuad>(MemoryHelper.AddIntPtr(outputPtr, offset));
                BGRAQuad v2 = dataBGRA[i];

                Assert.True(v1.R == v2.R && v1.G == v2.G && v1.B == v2.B && v1.A == v2.A);
            }

            outputImage1.Dispose();
            compressor.Input.SetData(rgbaMip, false);
            compressor.Compression.SetRGBAPixelFormat();

            DDSContainer outputImage2;
            compressor.Process(out outputImage2);

            //Check rgba
            for(int i = 0, offset = 0; i < width * height; i++, offset += 4)
            {
                IntPtr outputPtr = outputImage2.MipChains[0][0].Data;
                RGBAQuad v1 = MemoryHelper.Read<RGBAQuad>(MemoryHelper.AddIntPtr(outputPtr, offset));
                RGBAQuad v2 = dataRGBA[i];

                Assert.True(v1.R == v2.R && v1.G == v2.G && v1.B == v2.B && v1.A == v2.A);
            }

            MemoryHelper.UnpinObject(dataRGBA);
            MemoryHelper.UnpinObject(dataBGRA);
            outputImage2.Dispose();
        }
    }

    //Long running test...separate so can run in parallel
    public class CompressorTestFixture_TextureCubemapTestFixture : TeximpTestFixture
    {
        [Fact]
        public void TestProcessCubemapTexture()
        {
            List<String> fileNames = new List<string>();
            fileNames.Add("right_PosX.png");
            fileNames.Add("left_NegX.png");
            fileNames.Add("top_PosY.png");
            fileNames.Add("bot_NegY.png");
            fileNames.Add("front_PosZ.png");
            fileNames.Add("back_NegZ.png");

            Compressor compressor = new Compressor();
            compressor.Input.GenerateMipmaps = true;
            compressor.Compression.Format = CompressionFormat.DXT1;

            Surface[] surfaces = new Surface[6];

            //Load in parallel
            Parallel.For(0, fileNames.Count, (int i) =>
            {
                String file = GetInputFile(Path.Combine("Cubemap", fileNames[i]));
                surfaces[i] = Surface.LoadFromFile(file, true);
            });

            try
            {
                compressor.Input.SetData(surfaces);
            }
            finally
            {
                foreach(Surface s in surfaces)
                    s.Dispose();
            }

            String outputFile = GetOutputFile("Nebula.dds");
            String outputFile2 = GetOutputFile("Nebula-2.dds");

            Assert.True(compressor.Process(outputFile));

            //Look at compressed image processing too
            DDSContainer ddsContainer;
            Assert.True(compressor.Process(out ddsContainer));
            Assert.NotNull(ddsContainer);
            Assert.True(ddsContainer.MipChains.Count == 6);

            foreach(MipChain mips in ddsContainer.MipChains)
            {
                Assert.True(mips.Count == compressor.Input.MipmapCount);

                foreach(MipData mip in mips)
                    Assert.NotNull(mip);
            }

            //Save out file so we can compare
            ddsContainer.Write(outputFile2);
            ddsContainer.Dispose();
        }
    }

    //Long running test...separate so can run in parallel
    public class CompressorTestFixture_TextureArrayTestFixture : TeximpTestFixture
    {
        [Fact]
        public void TestProcess2DArrayTexture()
        {
            int width = 256;
            int height = 256;
            int arrayCount = 10;

            Compressor compressor = new Compressor();
            compressor.Input.GenerateMipmaps = true;
            compressor.Input.SetTextureLayout(TextureType.Texture2DArray, width, height, 1, arrayCount);
            compressor.Compression.Format = CompressionFormat.DXT1;

            //Load the first 10 noise bitmaps
            String fileNameTemplate = "Noise_Perlin0{0}{1}{2}.bmp";
            List<int> digits = new List<int>();

            for(int i = 0; i < arrayCount; i++)
            {
                TestHelper.GetDigits(digits, i, 3);

                String inputFile = GetInputFile(Path.Combine("3D_Perlin_Noise", String.Format(fileNameTemplate, digits[0].ToString(), digits[1].ToString(), digits[2].ToString())));
                using(Surface imageFromFile = Surface.LoadFromFile(inputFile, true))
                {
                    //Make sure its 32-bit BGRA data
                    imageFromFile.ConvertTo(ImageConversion.To32Bits);

                    compressor.Input.SetMipmapData(imageFromFile, 0, i);
                }
            }

            //Write to file
            String outputFile = GetOutputFile("2DArray_Perlin_Noise.dds");
            Assert.True(compressor.Process(outputFile));

            //Also process to list of mipmaps
            DDSContainer ddsContainer;

            Assert.True(compressor.Process(out ddsContainer));
            Assert.NotNull(ddsContainer);
            Assert.True(ddsContainer.MipChains.Count == arrayCount);

            foreach(MipChain mips in ddsContainer.MipChains)
            {
                Assert.True(mips.Count == compressor.Input.MipmapCount);

                foreach(MipData mip in mips)
                    Assert.NotNull(mip);
            }

            ddsContainer.Dispose();
        }
    }

    //Long running test...separate so can run in parallel
    public class CompressorTestFixture_Texture3DTestFixture : TeximpTestFixture
    {
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
                    TestHelper.GetDigits(digits, slice, 3);

                    String inputFile = GetInputFile(Path.Combine("3D_Perlin_Noise", String.Format(fileNameTemplate, digits[0].ToString(), digits[1].ToString(), digits[2].ToString())));
                    using(Surface imageFromFile = Surface.LoadFromFile(inputFile, true))
                    {
                        //Make sure its 32-bit BGRA data
                        imageFromFile.ConvertTo(ImageConversion.To32Bits);

                        //Copy contents of slice to our 3D image
                        ImageHelper.CopyColorImageData(currSlice, imageFromFile.DataPtr, imageFromFile.Pitch, 0, imageFromFile.Width, imageFromFile.Height, 1);
                    }

                    currSlice = MemoryHelper.AddIntPtr(currSlice, slicePitch);
                }

                //Finally set the 3D image to the compressor
                Compressor compressor = new Compressor();
                compressor.Input.GenerateMipmaps = true;
                compressor.Input.SetTextureLayout(TextureType.Texture3D, width, height, depth);
                compressor.Input.SetMipmapData(data, true, ImageInfo.From3D(width, height, depth));
                compressor.Compression.Format = CompressionFormat.BGRA; //DXT1 doesn't seem to load in the DirectX Texture tool...

                String outputFile = GetOutputFile("3D_Perlin_Noise.dds");
                Assert.True(compressor.Process(outputFile));

                //Also process to list of mipmaps
                DDSContainer ddsContainer;

                Assert.True(compressor.Process(out ddsContainer));
                Assert.NotNull(ddsContainer);
                Assert.True(ddsContainer.MipChains.Count == 1);

                foreach(MipChain mips in ddsContainer.MipChains)
                {
                    Assert.True(mips.Count == compressor.Input.MipmapCount);

                    foreach(MipData mip in mips)
                        Assert.NotNull(mip);
                }

                ddsContainer.Dispose();
            }
            finally
            {
                MemoryHelper.FreeMemory(data);
            }
        }
    }
}
