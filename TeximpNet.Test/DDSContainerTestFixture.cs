/*
* Copyright (c) 2016-2019 TeximpNet - Nicholas Woodfield
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
using TeximpNet.DDS;
using Xunit;

namespace TeximpNet.Test
{
    public class DDSContainerTestFixture : TeximpTestFixture
    {
        [Fact]
        public void TestLoadCompressed()
        {
            String file = GetInputFile("DDS/bunny_DXT1.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Two, false, true, true, false);

            file = GetInputFile("DDS/bunny_DXT5_NoMips.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.BC3_UNorm, TextureDimension.Two, false, false, true, false);
        }

        [Fact]
        public void TestRoundTripCompressed()
        {
            String file = GetInputFile("DDS/bunny_DXT1.dds");
            String fileOut = GetOutputFile("bunny_DXT1.dds");

            using(DDSContainer image = DDSFile.Read(file))
            {
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Two, false, true, true, false);

                Assert.True(image.Write(fileOut));
            }

            using(DDSContainer image = DDSFile.Read(fileOut))
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Two, false, true, true, false);
        }

        [Fact]
        public void TestRoundTripCompressed_ForceDX10()
        {
            String file = GetInputFile("DDS/bunny_DXT1.dds");
            String fileOut = GetOutputFile("bunny_DXT1_DX10.dds");

            using (DDSContainer image = DDSFile.Read(file))
            {
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Two, false, true, true, false);

                Assert.True(image.Write(fileOut, DDSFlags.ForceExtendedHeader));
            }

            using (DDSContainer image = DDSFile.Read(fileOut))
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Two, false, true, true, false);
        }

        [Fact]
        public void TestLoadUncompressed()
        {
            //no 24bpp DXGI, so convert to RGBA, also swizzles it automatically
            String file = GetInputFile("DDS/bunny_BGR.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.R8G8B8A8_UNorm, TextureDimension.Two, false, true, true, false);

            file = GetInputFile("DDS/bunny_BGRA.dds");
            using (DDSContainer image = DDSFile.Read(file, DDSFlags.ForceRgb))
                AssertImage(image, DXGIFormat.R8G8B8A8_UNorm, TextureDimension.Two, false, true, true, false);

            file = GetInputFile("DDS/bunny_BGRA.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Two, false, true, true, false);

            file = GetInputFile("DDS/bunny_RgbaFloat.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.R32G32B32A32_Float, TextureDimension.Two, false, true, true, false);
        }

        [Fact]
        public void TestLoadThenMakeFreeImage()
        {
            String file = GetInputFile("DDS/bunny_BGRA.dds");
            String fileOut = GetOutputFile("bunny_BGRA.bmp");

            using (DDSContainer image = DDSFile.Read(file))
            {
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Two, false, true, true, false);

                MipData mip0 = image.MipChains[0][0];
                Surface s = Surface.LoadFromRawData(mip0.Data, mip0.Width, mip0.Height, mip0.RowPitch, true, true);
                Assert.True(s.SaveToFile(ImageFormat.BMP, fileOut));
            }
        }

        [Fact]
        public void TestRoundTripUnCompressed()
        {
            String file = GetInputFile("DDS/bunny_BGRA.dds");
            String fileOut = GetOutputFile("bunny_BGRA.dds");

            using (DDSContainer image = DDSFile.Read(file))
            {
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Two, false, true, true, false);

                Assert.True(image.Write(fileOut));
            }

            using (DDSContainer image = DDSFile.Read(fileOut))
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Two, false, true, true, false);
        }

        [Fact]
        public void TestRoundTripUnCompressed_ForceDX10()
        {
            String file = GetInputFile("DDS/bunny_BGRA.dds");
            String fileOut = GetOutputFile("bunny_BGRA_DX10.dds");

            using (DDSContainer image = DDSFile.Read(file))
            {
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Two, false, true, true, false);

                Assert.True(image.Write(fileOut, DDSFlags.ForceExtendedHeader));
            }

            using (DDSContainer image = DDSFile.Read(fileOut))
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Two, false, true, true, false);
        }

        [Fact]
        public void TestLoadCubemap()
        {
            String file = GetInputFile("DDS/Cubemap.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Cube, false, true, true, false);
        }

        [Fact]
        public void TestRoundTripCubemap()
        {
            String file = GetInputFile("DDS/Cubemap.dds");
            String fileOut = GetOutputFile("Cubemap.dds");

            using (DDSContainer image = DDSFile.Read(file))
            {
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Cube, false, true, true, false);

                Assert.True(image.Write(fileOut));
            }

            using (DDSContainer image = DDSFile.Read(fileOut))
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Cube, false, true, true, false);
        }

        [Fact]
        public void TestRoundTripCubemap_ForceDX10()
        {
            String file = GetInputFile("DDS/Cubemap.dds");
            String fileOut = GetOutputFile("Cubemap_DX10.dds");

            using (DDSContainer image = DDSFile.Read(file))
            {
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Cube, false, true, true, false);

                Assert.True(image.Write(fileOut, DDSFlags.ForceExtendedHeader));
            }

            using (DDSContainer image = DDSFile.Read(fileOut))
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Cube, false, true, true, false);
        }

        [Fact]
        public void TestLoadDX10ThenMakeFreeImage()
        {
            String file = GetInputFile("DDS/bunny_BGRA_DX10.dds");
            String fileOut = GetOutputFile("bunny_BGRA_DX10.bmp");

            using (DDSContainer image = DDSFile.Read(file))
            {
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Two, false, true, true, false);

                MipData mip0 = image.MipChains[0][0];
                Surface s = Surface.LoadFromRawData(mip0.Data, mip0.Width, mip0.Height, mip0.RowPitch, true, true);
                Assert.True(s.SaveToFile(ImageFormat.BMP, fileOut));
            }
        }

        [Fact]
        public void TestLoadVolume()
        {
           String file = GetInputFile("DDS/Volume_BGRA.dds");
           using (DDSContainer image = DDSFile.Read(file, DDSFlags.ForceRgb))
                AssertImage(image, DXGIFormat.R8G8B8A8_UNorm, TextureDimension.Three, false, true, true, true);

            file = GetInputFile("DDS/Volume_BGRA.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Three, false, true, true, true);
            
            file = GetInputFile("DDS/Volume_DXT1.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.BC1_UNorm, TextureDimension.Three, false, true, true, true);
        }

        [Fact]
        public void TestRoundTripVolume()
        {
            String file = GetInputFile("DDS/Volume_BGRA.dds");
            String fileOut = GetOutputFile("Volume_BGRA.dds");
            using (DDSContainer image = DDSFile.Read(file))
            {
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Three, false, true, true, true);

                Assert.True(image.Write(fileOut));
            }

            using (DDSContainer image = DDSFile.Read(fileOut))
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Three, false, true, true, true);
        }

        [Fact]
        public void TestRoundTripVolume_ForceDX10()
        {
            String file = GetInputFile("DDS/Volume_BGRA.dds");
            String fileOut = GetOutputFile("Volume_BGRA_DX10.dds");
            using (DDSContainer image = DDSFile.Read(file))
            {
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Three, false, true, true, true);

                Assert.True(image.Write(fileOut));
            }

            using (DDSContainer image = DDSFile.Read(fileOut))
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Three, false, true, true, true);
        }

        [Fact]
        public void TestLoadLegacyFormats()
        {
            String file = GetInputFile("DDS/bunny_Luminance.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.R8_UNorm, TextureDimension.Two, false, true, true, false);

            file = GetInputFile("DDS/bunny_Palette.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.R8G8B8A8_UNorm, TextureDimension.Two, false, true, true, false);

            file = GetInputFile("DDS/bunny_PaletteAlpha.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.R8G8B8A8_UNorm, TextureDimension.Two, false, true, true, false);
        }

        [Fact]
        public void TestLoad16BitRGBA()
        {
            String file = GetInputFile("DDS/bunny_Rgba4444.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.B4G4R4A4_UNorm, TextureDimension.Two, false, true, true, false);

            file = GetInputFile("DDS/bunny_Rgb565.dds");
            using (DDSContainer image = DDSFile.Read(file))
                AssertImage(image, DXGIFormat.B5G6R5_UNorm, TextureDimension.Two, false, true, true, false);
        }

        [Fact]
        public void TestWriteFreeImage()
        {
            String file = GetInputFile("bunny.jpg");
            Surface mip0 = Surface.LoadFromFile(file);

            List<Surface> mipChain = new List<Surface>();
            mip0.GenerateMipMaps(mipChain, ImageFilter.Lanczos3, true);

            //Flip images, maybe have this as an option to the write method?
            foreach (Surface s in mipChain)
                s.FlipVertically();

            String fileOut = GetOutputFile("bunny_freeImage_RGBA.dds");
            Assert.True(DDSFile.Write(fileOut, mipChain, TextureDimension.Two, DDSFlags.ForceRgb));

            fileOut = GetOutputFile("bunny_freeImage_BGRA.dds");
            Assert.True(DDSFile.Write(fileOut, mipChain, TextureDimension.Two));

            using (DDSContainer image = DDSFile.Read(fileOut))
                AssertImage(image, DXGIFormat.B8G8R8A8_UNorm, TextureDimension.Two, false, true, false, false);
        }

        private static void AssertImage(DDSContainer image, DXGIFormat format, TextureDimension texDim, bool hasArray, bool hasMips, bool hasHeight, bool hasDepth)
        {
            Assert.NotNull(image);
            Assert.True(image.Format == format);
            Assert.True(image.Dimension == texDim);
            Assert.NotNull(image.MipChains);

            if (hasArray)
                Assert.True(image.MipChains.Count > 1);
            else if (texDim == TextureDimension.Cube)
                Assert.True(image.MipChains.Count == 6);
            else
                Assert.True(image.MipChains.Count == 1);

            foreach(MipChain mips in image.MipChains)
            {
                Assert.NotNull(mips);

                if (hasMips)
                    Assert.True(mips.Count > 1);
                else
                    Assert.True(mips.Count == 1);

                //Only check height/depth on the first mip level...Just want to make sure we don't get all 1's,
                //which should be the default
                bool checkHeight = hasHeight;
                bool checkDepth = hasDepth;
                foreach(MipData surface in mips)
                {
                    Assert.NotNull(surface);
                    Assert.True(surface.Data != IntPtr.Zero);
                    Assert.True(surface.RowPitch > 0);
                    Assert.True(surface.SlicePitch > 0);
                    Assert.True(surface.Width >= 1);

                    if (checkHeight)
                        Assert.True(surface.Height > 1);
                    else
                        Assert.True(surface.Height >= 1);

                    if (checkDepth)
                        Assert.True(surface.Depth > 1);
                    else
                        Assert.True(surface.Depth >= 1);

                    checkHeight = false;
                    checkDepth = false;
                }
            }

            Assert.True(image.Validate());
        }
    }
}