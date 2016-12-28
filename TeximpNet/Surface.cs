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
using TeximpNet.Unmanaged;
using System.IO;

namespace TeximpNet
{
    public sealed class Surface : IDisposable
    {
        private IntPtr m_imagePtr;

        private bool m_isDisposed;

        public bool IsDisposed
        {
            get
            {
                return m_isDisposed;
            }
        }

        public ImageType ImageType
        {
            get
            {
                if(m_imagePtr == IntPtr.Zero)
                    return ImageType.Unknown;

                return FreeImageLibrary.Instance.GetImageType(m_imagePtr);
            }
        }

        public int BitsPerPixel
        {
            get
            {
                if(m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetBitsPerPixel(m_imagePtr);
            }
        }

        public int Pitch
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetPitch(m_imagePtr);
            }
        }

        public int Width
        {
            get
            {
                if(m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetWidth(m_imagePtr);
            }
        }

        public int Height
        {
            get
            {
                if(m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetHeight(m_imagePtr);
            }
        }

        public uint RedMask
        {
            get
            {
                if(m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetRedMask(m_imagePtr);
            }
        }

        public uint GreenMask
        {
            get
            {
                if(m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetGreenMask(m_imagePtr);
            }
        }

        public uint BlueMask
        {
            get
            {
                if(m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetBlueMask(m_imagePtr);
            }
        }

        public bool IsTransparent
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return false;

                return FreeImageLibrary.Instance.IsTransparent(m_imagePtr);
            }
        }

        public IntPtr DataPtr
        {
            get
            {
                if(m_imagePtr == IntPtr.Zero)
                    return IntPtr.Zero;

                return FreeImageLibrary.Instance.GetData(m_imagePtr);
            }
        }

        public IntPtr NativePtr
        {
            get
            {
                return m_imagePtr;
            }
        }

        public Surface(int width, int height) : this(32, width, height) { }

        public Surface(int width, int height, bool hasAlpha) : this((hasAlpha) ? 32 : 24, width, height) { }

        public Surface(int bpp, int width, int height) : this(bpp, width, height, 0, 0, 0) { }

        public Surface(int bpp, int width, int height, uint redMask, uint greenMask, uint blueMask)
        {
            m_imagePtr = FreeImageLibrary.Instance.Allocate(width, height, bpp, redMask, greenMask, blueMask);
        }

        public Surface(ImageType imageType, int bpp, int width, int height) : this(imageType, bpp, width, height, 0, 0, 0) { }

        public Surface(ImageType imageType, int bpp, int width, int height, uint redMask, uint greenMask, uint blueMask)
        {
            m_imagePtr = FreeImageLibrary.Instance.AllocateT(imageType, width, height, bpp, redMask, greenMask, blueMask);
        }

        public Surface(IntPtr imagePtr)
        {
            m_imagePtr = imagePtr;
        }

        ~Surface()
        {
            Dispose(false);
        }

        public static Surface LoadFromFile(String filename, int flags = 0)
        {
            IntPtr imagePtr = FreeImageLibrary.Instance.LoadFromFile(filename, flags);
            if (imagePtr == IntPtr.Zero)
                return null;

            return new Surface(imagePtr);
        }

        public static Surface LoadFromStream(Stream stream, int flags = 0)
        {
            IntPtr imagePtr = FreeImageLibrary.Instance.LoadFromStream(stream, flags);
            if (imagePtr == IntPtr.Zero)
                return null;

            return new Surface(imagePtr);
        }

        public Surface Clone()
        {
            if(m_isDisposed)
                return null;

            IntPtr imagePtr = FreeImageLibrary.Instance.Clone(m_imagePtr);
            return new Surface(imagePtr);
        }

        public bool ConvertTo(ImageConversion convertTo)
        {
            IntPtr newImagePtr = IntPtr.Zero;

            switch(convertTo)
            {
                case ImageConversion.To4Bits:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo4Bits(m_imagePtr);
                    break;
                case ImageConversion.To8Bits:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo8Bits(m_imagePtr);
                    break;
                case ImageConversion.To16Bits555:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo16Bits555(m_imagePtr);
                    break;
                case ImageConversion.To16Bits565:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo16Bits565(m_imagePtr);
                    break;
                case ImageConversion.To24Bits:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo24Bits(m_imagePtr);
                    break;
                case ImageConversion.To32Bits:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo32Bits(m_imagePtr);
                    break;
                case ImageConversion.ToGreyscale:
                    newImagePtr = FreeImageLibrary.Instance.ConvertToGreyscale(m_imagePtr);
                    break;
                default:
                    return false;
            }

            if(newImagePtr == IntPtr.Zero)
                return false;

            FreeImageLibrary.Instance.Unload(m_imagePtr);
            m_imagePtr = newImagePtr;

            return true;
        }

        public bool FlipHorizontally()
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.FlipHorizontal(m_imagePtr);
        }

        public bool FlipVertically()
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.FlipVertical(m_imagePtr);
        }

        public bool Rotate(double angle)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            IntPtr newImagePtr = FreeImageLibrary.Instance.Rotate(m_imagePtr, angle);

            if (newImagePtr == IntPtr.Zero)
                return false;

            FreeImageLibrary.Instance.Unload(m_imagePtr);
            m_imagePtr = newImagePtr;

            return true;
        }

        public bool PreMultiplyAlpha()
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.PreMultiplyWithAlpha(m_imagePtr);
        }

        public bool AdjustGamma(double gamma)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.AdjustGamma(m_imagePtr, gamma);
        }

        public bool AdjustBrightness(double percentage)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.AdjustBrightness(m_imagePtr, percentage);
        }

        public bool AdjustContrast(double percentage)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.AdjustContrast(m_imagePtr, percentage);
        }

        public bool Invert()
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.Invert(m_imagePtr);
        }

        public bool Resize(int width, int height, ImageFilter filter)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            IntPtr newImagePtr = FreeImageLibrary.Instance.Rescale(m_imagePtr, width, height, filter);

            if (newImagePtr == IntPtr.Zero)
                return false;

            FreeImageLibrary.Instance.Unload(m_imagePtr);
            m_imagePtr = newImagePtr;

            return true;
        }

        public bool SwapColors(RGBAQuad colorToReplace, RGBAQuad colorToReplaceWith, bool ignoreAlpha)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.SwapColors(m_imagePtr, colorToReplace, colorToReplaceWith, ignoreAlpha) > 0;
        }

        public bool GenerateMipMaps(List<Surface> mipChain, ImageFilter filter,  bool includeFirst)
        {
            if (mipChain == null || m_imagePtr == IntPtr.Zero)
                return false;

            if (includeFirst)
                mipChain.Add(this);

            int width = Width;
            int height = Height;
            int mipCount = MemoryHelper.CountMipmaps(width, height, 1);

            for(int i = 1; i < mipCount; i++)
            {
                int mipWidth = width;
                int mipHeight = height;
                MemoryHelper.CalculateMipmapLevelDimensions(i, ref mipWidth, ref mipHeight);

                IntPtr mipPtr = FreeImageLibrary.Instance.Rescale(m_imagePtr, mipWidth, mipHeight, filter);
                if(mipPtr != IntPtr.Zero)
                {
                    mipChain.Add(new Surface(mipPtr));
                }
            }

            return true;
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
                if(m_imagePtr != IntPtr.Zero)
                {
                    FreeImageLibrary.Instance.Unload(m_imagePtr);
                    m_imagePtr = IntPtr.Zero;
                }

                m_isDisposed = true;
            }
        }
    }
}
