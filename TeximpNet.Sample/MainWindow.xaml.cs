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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using TeximpNet.Compression;
using TeximpNet.DDS;

namespace TeximpNet.Sample
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            LoadBunnyImages();
        }

        private void LoadBunnyImages()
        {
            Canvas canvas = this.FindControl<Canvas>("Mips");
            String bunnyPath = Path.Combine(AppContext.BaseDirectory, "bunny.jpg");
            BunnyMipmaps mipmaps = new BunnyMipmaps(bunnyPath);
            if (mipmaps.Load())
            {
                int offset = 0;

                DDSContainer ddsContainer = mipmaps.DDSContainer;
                foreach(MipData mipData in ddsContainer.MipChains[0])
                {
                    canvas.Children.Add(ToImage(mipData, new Point(offset, 0)));

                    offset += mipData.Width;
                }

                canvas.MinWidth = offset;
                canvas.MinHeight = ddsContainer.MipChains[0][0].Height;
            }
        }

        private Image ToImage(MipData mipData, Point offset)
        {
            Bitmap bitmap = new Bitmap(PixelFormat.Bgra8888, mipData.Data, mipData.Width, mipData.Height, mipData.RowPitch);

            Image image = new Image();
            image.Source = bitmap;
            image.Width = bitmap.PixelWidth;
            image.Height = bitmap.PixelHeight;
            image.RenderTransform = new Avalonia.Media.TranslateTransform(offset.X, offset.Y);

            return image;
        }
    }
}
