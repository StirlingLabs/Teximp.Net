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
using System.Runtime.InteropServices;
using System.IO;

namespace TeximpNet.Unmanaged
{
    public sealed class FreeImageLibrary : UnmanagedLibrary
    {
        private static readonly Object s_sync = new Object();
        private bool? m_isLittleEndian;

        /// <summary>
        /// Default name of the 32-bit unmanaged library. Based on runtime implementation the extension (.dll, .so, .dylib) will be appended automatically.
        /// </summary>
        public const String Default32BitPath = "FreeImage32";

        /// <summary>
        /// Default name of the 64-bit unmanaged library. Based on runtime implementation the extension (.dll, .so, .dylib) will be appended automatically.
        /// </summary>
        public const String Default64BitPath = "FreeImage64";

        private static FreeImageLibrary s_instance;

        private FreeImageIOHandler m_ioHandler;

        public static FreeImageLibrary Instance
        {
            get
            {
                lock (s_sync)
                {
                    if(s_instance == null)
                        s_instance = CreateInstance();

                    return s_instance;
                }
            }
        }

        /// <summary>
        /// Gets if the OS is little endian. If Big Endian, then surface data is RGBA. If little, then surface data is BGRA.
        /// </summary>
        public bool IsLittleEndian
        {
            get
            {
                if(m_isLittleEndian.HasValue)
                    return m_isLittleEndian.Value;

                LoadIfNotLoaded();
                
                Functions.FreeImage_IsLittleEndian func = GetFunction<Functions.FreeImage_IsLittleEndian>(FunctionNames.FreeImage_IsLittleEndian);

                m_isLittleEndian = func();
                return m_isLittleEndian.Value;
            }
        }

        private FreeImageLibrary(String default32BitPath, String default64BitPath, Type[] unmanagedFunctionDelegateTypes) 
            : base(default32BitPath, default64BitPath, unmanagedFunctionDelegateTypes)
        {
            m_ioHandler = new FreeImageIOHandler(Is64Bit && (GetPlatform() != Platform.Windows));
        }

        private static FreeImageLibrary CreateInstance()
        {
            return new FreeImageLibrary(Default32BitPath, Default64BitPath, typeof(Functions).GetNestedTypes());
        }

        #region Allocate / Clone / Unload

        public IntPtr Allocate(int width, int height, int bpp)
        {
            return Allocate(width, height, bpp, 0, 0, 0);
        }

        public IntPtr Allocate(int width, int height, int bpp, uint red_mask, uint green_mask, uint blue_mask)
        {
            LoadIfNotLoaded();

            Functions.FreeImage_Allocate func = GetFunction<Functions.FreeImage_Allocate>(FunctionNames.FreeImage_Allocate);

            return func(width, height, bpp, red_mask, green_mask, blue_mask);
        }

        public IntPtr AllocateT(ImageType imageType, int width, int height, int bpp, uint red_mask, uint green_mask, uint blue_mask)
        {
            LoadIfNotLoaded();

            Functions.FreeImage_AllocateT func = GetFunction<Functions.FreeImage_AllocateT>(FunctionNames.FreeImage_AllocateT);

            return func(imageType, width, height, bpp, red_mask, green_mask, blue_mask);
        }

        public IntPtr Clone(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_Clone func = GetFunction<Functions.FreeImage_Clone>(FunctionNames.FreeImage_Clone);

            return func(bitmap);
        }

        public void Unload(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.FreeImage_Unload func = GetFunction<Functions.FreeImage_Unload>(FunctionNames.FreeImage_Unload);

            func(bitmap);
        }

        public IntPtr Copy(IntPtr bitmap, int left, int top, int right, int bottom)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_Copy func = GetFunction<Functions.FreeImage_Copy>(FunctionNames.FreeImage_Copy);

            return func(bitmap, left, top, right, bottom);
        }

        public bool Paste(IntPtr dstBitmap, IntPtr srcBitmap, int left, int top, int alpha)
        {
            if(dstBitmap == IntPtr.Zero || srcBitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_Paste func = GetFunction<Functions.FreeImage_Paste>(FunctionNames.FreeImage_Paste);

            return func(dstBitmap, srcBitmap, left, top, alpha);
        }

        #endregion

        #region Load / Save

        public IntPtr LoadFromFile(String filename, ImageLoadFlags flags = ImageLoadFlags.Default)
        {
            if(String.IsNullOrEmpty(filename))
                return IntPtr.Zero;

            LoadIfNotLoaded();

            IntPtr name = Marshal.StringToHGlobalAnsi(filename);
            Functions.FreeImage_GetFileType getFileTypeFunc = GetFunction<Functions.FreeImage_GetFileType>(FunctionNames.FreeImage_GetFileType);
            Functions.FreeImage_Load loadFunc = GetFunction<Functions.FreeImage_Load>(FunctionNames.FreeImage_Load);

            try
            {
                ImageFormat format = getFileTypeFunc(name, 0);

                if(format == ImageFormat.Unknown)
                    return IntPtr.Zero;

                return loadFunc(format, name, (int) flags);
            }
            finally
            {
                Marshal.FreeHGlobal(name);
            }
        }

        public unsafe IntPtr LoadFromStream(Stream stream, ImageLoadFlags flags = ImageLoadFlags.Default)
        {
            if (stream == null || !stream.CanRead)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            using (StreamWrapper wrapper = new StreamWrapper(stream))
            {
                Functions.FreeImage_LoadFromHandle loadFunc = GetFunction<Functions.FreeImage_LoadFromHandle>(FunctionNames.FreeImage_LoadFromHandle);
                Functions.FreeImage_GetFileTypeFromHandle getFileTypeFunc = GetFunction<Functions.FreeImage_GetFileTypeFromHandle>(FunctionNames.FreeImage_GetFileTypeFromHandle);

                FreeImageIO io = m_ioHandler.ImageIO;
                IntPtr ioPtr = new IntPtr(&io);

                ImageFormat format = getFileTypeFunc(ioPtr, wrapper.GetHandle(), 0);

                if (format == ImageFormat.Unknown)
                    return IntPtr.Zero;

                return loadFunc(format, ioPtr, wrapper.GetHandle(), (int) flags);
            }
        }

        public bool SaveToFile(ImageFormat format, IntPtr bitmap, String filename, ImageSaveFlags flags = ImageSaveFlags.Default)
        {
            if(String.IsNullOrEmpty(filename) || format == ImageFormat.Unknown || bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_Save func = GetFunction<Functions.FreeImage_Save>(FunctionNames.FreeImage_Save);
            IntPtr name = Marshal.StringToHGlobalAnsi(filename);

            try
            {
                return func(format, bitmap, name, (int) flags);
            }
            finally
            {
                Marshal.FreeHGlobal(name);
            }
        }

        public unsafe bool SaveToStream(ImageFormat format, IntPtr bitmap, Stream stream, ImageSaveFlags flags = ImageSaveFlags.Default)
        {
            if (stream == null || !stream.CanWrite || format == ImageFormat.Unknown || bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            using (StreamWrapper wrapper = new StreamWrapper(stream))
            {
                Functions.FreeImage_SaveToHandle func = GetFunction<Functions.FreeImage_SaveToHandle>(FunctionNames.FreeImage_SaveToHandle);

                FreeImageIO io = m_ioHandler.ImageIO;
                return func(format, bitmap, new IntPtr(&io), wrapper.GetHandle(), (int) flags);
            }
        }

        #endregion

        #region Query routines

        public bool HasPixels(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_HasPixels func = GetFunction<Functions.FreeImage_HasPixels>(FunctionNames.FreeImage_HasPixels);

            return func(bitmap);
        }

        public ImageFormat GetFileTypeFromFile(String filename)
        {
            if(String.IsNullOrEmpty(filename))
                return ImageFormat.Unknown;

            LoadIfNotLoaded();

            Functions.FreeImage_GetFileType func = GetFunction<Functions.FreeImage_GetFileType>(FunctionNames.FreeImage_GetFileType);

            IntPtr name = Marshal.StringToHGlobalAnsi(filename);

            try
            {
                return func(name, 0);
            }
            finally
            {
                Marshal.FreeHGlobal(name);
            }
        }

        public unsafe ImageFormat GetFileTypeFromStream(Stream stream)
        {
            if (stream == null || !stream.CanRead)
                return ImageFormat.Unknown;

            LoadIfNotLoaded();

            using (StreamWrapper wrapper = new StreamWrapper(stream, false))
            {
                Functions.FreeImage_GetFileTypeFromHandle func = GetFunction<Functions.FreeImage_GetFileTypeFromHandle>(FunctionNames.FreeImage_GetFileTypeFromHandle);

                FreeImageIO io = m_ioHandler.ImageIO;
                return func(new IntPtr(&io), wrapper.GetHandle(), 0);
            }
        }

        public ImageType GetImageType(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return ImageType.Unknown;

            LoadIfNotLoaded();

            Functions.FreeImage_GetImageType func = GetFunction<Functions.FreeImage_GetImageType>(FunctionNames.FreeImage_GetImageType);

            return func(bitmap);
        }

        public ImageColorType GetImageColorType(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return ImageColorType.RGBA;

            LoadIfNotLoaded();

            Functions.FreeImage_GetColorType func = GetFunction<Functions.FreeImage_GetColorType>(FunctionNames.FreeImage_GetColorType);

            return func(bitmap);
        }

        public IntPtr GetData(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_GetBits func = GetFunction<Functions.FreeImage_GetBits>(FunctionNames.FreeImage_GetBits);

            return func(bitmap);
        }

        public IntPtr GetScanLine(IntPtr bitmap, int scanline)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_GetScanLine func = GetFunction<Functions.FreeImage_GetScanLine>(FunctionNames.FreeImage_GetScanLine);

            return func(bitmap, scanline);
        }

        public int GetBitsPerPixel(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetBPP func = GetFunction<Functions.FreeImage_GetBPP>(FunctionNames.FreeImage_GetBPP);

            return (int) func(bitmap);
        }

        public int GetWidth(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetWidth func = GetFunction<Functions.FreeImage_GetWidth>(FunctionNames.FreeImage_GetWidth);

            return (int) func(bitmap);
        }

        public int GetHeight(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetHeight func = GetFunction<Functions.FreeImage_GetHeight>(FunctionNames.FreeImage_GetHeight);

            return (int) func(bitmap);
        }

        public int GetPitch(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetPitch func = GetFunction<Functions.FreeImage_GetPitch>(FunctionNames.FreeImage_GetPitch);

            return (int) func(bitmap);
        }

        public uint GetRedMask(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetRedMask func = GetFunction<Functions.FreeImage_GetRedMask>(FunctionNames.FreeImage_GetRedMask);

            return func(bitmap);
        }

        public uint GetGreenMask(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetGreenMask func = GetFunction<Functions.FreeImage_GetGreenMask>(FunctionNames.FreeImage_GetGreenMask);

            return func(bitmap);
        }

        public uint GetBlueMask(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetBlueMask func = GetFunction<Functions.FreeImage_GetBlueMask>(FunctionNames.FreeImage_GetBlueMask);

            return func(bitmap);
        }

        public bool IsTransparent(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_IsTransparent func = GetFunction<Functions.FreeImage_IsTransparent>(FunctionNames.FreeImage_IsTransparent);

            return func(bitmap);
        }

        #endregion

        #region Conversion routines

        public IntPtr ConvertTo4Bits(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo4Bits func = GetFunction<Functions.FreeImage_ConvertTo4Bits>(FunctionNames.FreeImage_ConvertTo4Bits);

            return func(bitmap);
        }

        public IntPtr ConvertTo8Bits(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo8Bits func = GetFunction<Functions.FreeImage_ConvertTo8Bits>(FunctionNames.FreeImage_ConvertTo8Bits);

            return func(bitmap);
        }

        public IntPtr ConvertTo16Bits555(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo16Bits555 func = GetFunction<Functions.FreeImage_ConvertTo16Bits555>(FunctionNames.FreeImage_ConvertTo16Bits555);

            return func(bitmap);
        }

        public IntPtr ConvertTo16Bits565(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo16Bits565 func = GetFunction<Functions.FreeImage_ConvertTo16Bits565>(FunctionNames.FreeImage_ConvertTo16Bits565);

            return func(bitmap);
        }

        public IntPtr ConvertTo24Bits(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo24Bits func = GetFunction<Functions.FreeImage_ConvertTo24Bits>(FunctionNames.FreeImage_ConvertTo24Bits);

            return func(bitmap);
        }

        public IntPtr ConvertTo32Bits(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo32Bits func = GetFunction<Functions.FreeImage_ConvertTo32Bits>(FunctionNames.FreeImage_ConvertTo32Bits);

            return func(bitmap);
        }

        public IntPtr ConvertToGreyscale(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertToGreyscale func = GetFunction<Functions.FreeImage_ConvertToGreyscale>(FunctionNames.FreeImage_ConvertToGreyscale);

            return func(bitmap);
        }

        public bool ConvertToRawBits(IntPtr data, IntPtr bitmap)
        {
            return ConvertToRawBits(data, bitmap, false);
        }

        public bool ConvertToRawBits(IntPtr data, IntPtr bitmap, bool topDown)
        {
            if(data == IntPtr.Zero || bitmap == IntPtr.Zero)
                return false;

            Functions.FreeImage_ConvertToRawBits func = GetFunction<Functions.FreeImage_ConvertToRawBits>(FunctionNames.FreeImage_ConvertToRawBits);

            func(data, bitmap, GetPitch(bitmap), (uint) GetBitsPerPixel(bitmap), (uint) GetRedMask(bitmap), (uint) GetGreenMask(bitmap), (uint) GetBlueMask(bitmap), topDown);
            return true;
        }

        public IntPtr ConvertToStandardType(IntPtr src, bool scaleLinearly)
        {
            if(src == IntPtr.Zero)
                return IntPtr.Zero;

            Functions.FreeImage_ConvertToStandardType func = GetFunction<Functions.FreeImage_ConvertToStandardType>(FunctionNames.FreeImage_ConvertToStandardType);

            return func(src, scaleLinearly);
        }

        public IntPtr ConvertToType(IntPtr src, ImageType dstType, bool scaleLinearly)
        {
            if(src == IntPtr.Zero)
                return IntPtr.Zero;

            Functions.FreeImage_ConvertToType func = GetFunction<Functions.FreeImage_ConvertToType>(FunctionNames.FreeImage_ConvertToType);

            return func(src, dstType, scaleLinearly);
        }

        #endregion

        #region Image manipulation

        public bool FlipHorizontal(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_FlipHorizontal func = GetFunction<Functions.FreeImage_FlipHorizontal>(FunctionNames.FreeImage_FlipHorizontal);

            return func(bitmap);
        }

        public bool FlipVertical(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_FlipVertical func = GetFunction<Functions.FreeImage_FlipVertical>(FunctionNames.FreeImage_FlipVertical);

            return func(bitmap);
        }

        public IntPtr Rescale(IntPtr bitmap, int dst_width, int dst_height, ImageFilter filter)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_Rescale func = GetFunction<Functions.FreeImage_Rescale>(FunctionNames.FreeImage_Rescale);

            return func(bitmap, dst_width, dst_height, filter);
        }

        public bool PreMultiplyWithAlpha(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_PreMultiplyWithAlpha func = GetFunction<Functions.FreeImage_PreMultiplyWithAlpha>(FunctionNames.FreeImage_PreMultiplyWithAlpha);

            return func(bitmap);
        }

        public bool AdjustGamma(IntPtr bitmap, double gamma)
        {
            if(bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_AdjustGamma func = GetFunction<Functions.FreeImage_AdjustGamma>(FunctionNames.FreeImage_AdjustGamma);

            return func(bitmap, gamma);
        }

        public bool AdjustBrightness(IntPtr bitmap, double percentage)
        {
            if(bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_AdjustBrightness func = GetFunction<Functions.FreeImage_AdjustBrightness>(FunctionNames.FreeImage_AdjustBrightness);

            return func(bitmap, percentage);
        }

        public bool AdjustContrast(IntPtr bitmap, double percentage)
        {
            if(bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_AdjustContrast func = GetFunction<Functions.FreeImage_AdjustContrast>(FunctionNames.FreeImage_AdjustContrast);

            return func(bitmap, percentage);
        }

        public bool Invert(IntPtr bitmap)
        {
            if(bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_Invert func = GetFunction<Functions.FreeImage_Invert>(FunctionNames.FreeImage_Invert);

            return func(bitmap);
        }

        public unsafe int SwapColors(IntPtr bitmap, RGBAQuad colorToReplace, RGBAQuad colorToReplaceWith, bool ignoreAlpha)
        {
            if(bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_SwapColors func = GetFunction<Functions.FreeImage_SwapColors>(FunctionNames.FreeImage_SwapColors);

            //BGRA in little endian
            if(IsLittleEndian)
            {
                //Swap RGBA to BGRA
                byte swap = colorToReplace.B;
                colorToReplace.B = colorToReplace.R;
                colorToReplace.R = swap;

                swap = colorToReplaceWith.B;
                colorToReplaceWith.B = colorToReplaceWith.R;
                colorToReplaceWith.R = swap;
            }

            return (int) func(bitmap, new IntPtr(&colorToReplace), new IntPtr(&colorToReplaceWith), ignoreAlpha);
        }

        public IntPtr Rotate(IntPtr bitmap, double angle)
        {
            if(bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            Functions.FreeImage_Rotate func = GetFunction<Functions.FreeImage_Rotate>(FunctionNames.FreeImage_Rotate);

            return func(bitmap, angle, IntPtr.Zero);
        }

        #endregion

        #region Versioning

        public String GetVersion()
        {
            LoadIfNotLoaded();

            Functions.FreeImage_GetVersion func = GetFunction<Functions.FreeImage_GetVersion>(FunctionNames.FreeImage_GetVersion);

            IntPtr ptr = func();

            if(ptr == IntPtr.Zero)
                return String.Empty;

            return Marshal.PtrToStringAnsi(ptr);
        }

        public String GetCopyrightMessage()
        {
            LoadIfNotLoaded();

            Functions.FreeImage_GetCopyrightMessage func = GetFunction<Functions.FreeImage_GetCopyrightMessage>(FunctionNames.FreeImage_GetCopyrightMessage);

            IntPtr ptr = func();

            if(ptr == IntPtr.Zero)
                return String.Empty;

            return Marshal.PtrToStringAnsi(ptr);
        }

        #endregion

        #region Function names

        internal static class FunctionNames
        {

            #region Allocate / Clone / Unload routines

            public const String FreeImage_Allocate = "FreeImage_Allocate";
            public const String FreeImage_AllocateT = "FreeImage_AllocateT";
            public const String FreeImage_Clone = "FreeImage_Clone";
            public const String FreeImage_Unload = "FreeImage_Unload";

            public const String FreeImage_Copy = "FreeImage_Copy";
            public const String FreeImage_Paste = "FreeImage_Paste";

            #endregion

            #region Load / Save routines

            public const String FreeImage_Load = "FreeImage_Load";
            public const String FreeImage_LoadFromHandle = "FreeImage_LoadFromHandle";

            public const String FreeImage_Save = "FreeImage_Save";
            public const String FreeImage_SaveToHandle = "FreeImage_SaveToHandle";

            #endregion

            #region Query routines

            public const String FreeImage_IsLittleEndian = "FreeImage_IsLittleEndian";
            public const String FreeImage_HasPixels = "FreeImage_HasPixels";
            public const String FreeImage_GetFileType = "FreeImage_GetFileType";
            public const String FreeImage_GetFileTypeFromHandle = "FreeImage_GetFileTypeFromHandle";
            public const String FreeImage_GetImageType = "FreeImage_GetImageType";
            public const String FreeImage_GetBits = "FreeImage_GetBits";
            public const String FreeImage_GetScanLine = "FreeImage_GetScanLine";
            public const String FreeImage_GetBPP = "FreeImage_GetBPP";
            public const String FreeImage_GetWidth = "FreeImage_GetWidth";
            public const String FreeImage_GetHeight = "FreeImage_GetHeight";
            public const String FreeImage_GetPitch = "FreeImage_GetPitch";

            public const String FreeImage_GetRedMask = "FreeImage_GetRedMask";
            public const String FreeImage_GetGreenMask = "FreeImage_GetGreenMask";
            public const String FreeImage_GetBlueMask = "FreeImage_GetBlueMask";
            public const String FreeImage_IsTransparent = "FreeImage_IsTransparent";
            public const String FreeImage_GetColorType = "FreeImage_GetColorType";

            #endregion

            #region Conversion routines

            public const String FreeImage_ConvertToRawBits = "FreeImage_ConvertToRawBits";
            public const String FreeImage_ConvertToStandardType = "FreeImage_ConvertToStandardType";
            public const String FreeImage_ConvertToType = "FreeImage_ConvertToType";
            public const String FreeImage_ConvertTo4Bits = "FreeImage_ConvertTo4Bits";
            public const String FreeImage_ConvertTo8Bits = "FreeImage_ConvertTo8Bits";
            public const String FreeImage_ConvertToGreyscale = "FreeImage_ConvertToGreyscale";
            public const String FreeImage_ConvertTo16Bits555 = "FreeImage_ConvertTo16Bits555";
            public const String FreeImage_ConvertTo16Bits565 = "FreeImage_ConvertTo16Bits565";
            public const String FreeImage_ConvertTo24Bits = "FreeImage_ConvertTo24Bits";
            public const String FreeImage_ConvertTo32Bits = "FreeImage_ConvertTo32Bits";

            #endregion

            #region Image manipulation

            public const String FreeImage_FlipHorizontal = "FreeImage_FlipHorizontal";
            public const String FreeImage_FlipVertical = "FreeImage_FlipVertical";
            public const String FreeImage_Rescale = "FreeImage_Rescale";
            public const String FreeImage_PreMultiplyWithAlpha = "FreeImage_PreMultiplyWithAlpha";
            public const String FreeImage_AdjustGamma = "FreeImage_AdjustGamma";
            public const String FreeImage_AdjustBrightness = "FreeImage_AdjustBrightness";
            public const String FreeImage_AdjustContrast = "FreeImage_AdjustContrast";
            public const String FreeImage_Invert = "FreeImage_Invert";
            public const String FreeImage_SwapColors = "FreeImage_SwapColors";
            public const String FreeImage_Rotate = "FreeImage_Rotate";

            #endregion

            #region Versioning

            public const String FreeImage_GetVersion = "FreeImage_GetVersion";
            public const String FreeImage_GetCopyrightMessage = "FreeImage_GetCopyrightMessage";

            #endregion
        }

        #endregion

        #region Function delegates

        internal static class Functions
        {
            #region Allocate / Clone / Unload routines

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Allocate)]
            public delegate IntPtr FreeImage_Allocate(int width, int height, int bpp, uint red_mask, uint green_mask, uint blue_mask);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_AllocateT)]
            public delegate IntPtr FreeImage_AllocateT(ImageType imageType, int width, int height, int bpp, uint red_mask, uint green_mask, uint blue_mask);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Clone)]
            public delegate IntPtr FreeImage_Clone(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Unload)]
            public delegate void FreeImage_Unload(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Copy)]
            public delegate IntPtr FreeImage_Copy(IntPtr bitmap, int left, int top, int right, int bottom);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Paste)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_Paste(IntPtr dstBitmap, IntPtr srcBitmap, int left, int top, int alpha);

            #endregion

            #region Load / Save routines

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Load)]
            public delegate IntPtr FreeImage_Load(ImageFormat format, IntPtr filename, int flags);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_LoadFromHandle)]
            public delegate IntPtr FreeImage_LoadFromHandle(ImageFormat format, IntPtr io, IntPtr ioHandle, int flags);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Save)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_Save(ImageFormat format, IntPtr bitmap, IntPtr filename, int flags);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_SaveToHandle)]
            public delegate bool FreeImage_SaveToHandle(ImageFormat format, IntPtr bitmap, IntPtr io, IntPtr ioHandle, int flags);

            #endregion

            #region Query routines

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_IsLittleEndian)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_IsLittleEndian();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_HasPixels)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_HasPixels(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetFileType)]
            public delegate ImageFormat FreeImage_GetFileType(IntPtr fileName, int size);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetFileTypeFromHandle)]
            public delegate ImageFormat FreeImage_GetFileTypeFromHandle(IntPtr io, IntPtr ioHandle, int size);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetImageType)]
            public delegate ImageType FreeImage_GetImageType(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetBits)]
            public delegate IntPtr FreeImage_GetBits(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetScanLine)]
            public delegate IntPtr FreeImage_GetScanLine(IntPtr bitmp, int scanline);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetBPP)]
            public delegate uint FreeImage_GetBPP(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetWidth)]
            public delegate uint FreeImage_GetWidth(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetHeight)]
            public delegate uint FreeImage_GetHeight(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetPitch)]
            public delegate uint FreeImage_GetPitch(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetRedMask)]
            public delegate uint FreeImage_GetRedMask(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetGreenMask)]
            public delegate uint FreeImage_GetGreenMask(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetBlueMask)]
            public delegate uint FreeImage_GetBlueMask(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_IsTransparent)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_IsTransparent(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetColorType)]
            public delegate ImageColorType FreeImage_GetColorType(IntPtr bitmap);

            #endregion

            #region Conversion routines

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo4Bits)]
            public delegate IntPtr FreeImage_ConvertTo4Bits(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo8Bits)]
            public delegate IntPtr FreeImage_ConvertTo8Bits(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo16Bits555)]
            public delegate IntPtr FreeImage_ConvertTo16Bits555(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo16Bits565)]
            public delegate IntPtr FreeImage_ConvertTo16Bits565(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo24Bits)]
            public delegate IntPtr FreeImage_ConvertTo24Bits(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo32Bits)]
            public delegate IntPtr FreeImage_ConvertTo32Bits(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToGreyscale)]
            public delegate IntPtr FreeImage_ConvertToGreyscale(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToRawBits)]
            public delegate void FreeImage_ConvertToRawBits(IntPtr data, IntPtr bitmap, int pitch, uint bpp, uint red_mask, uint green_mask, uint blue_mask, [In, MarshalAs(UnmanagedType.Bool)] bool topdown);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToStandardType)]
            public delegate IntPtr FreeImage_ConvertToStandardType(IntPtr src, [MarshalAs(UnmanagedType.Bool)] bool scaleLinearly);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToType)]
            public delegate IntPtr FreeImage_ConvertToType(IntPtr src, ImageType dstType, [MarshalAs(UnmanagedType.Bool)] bool scaleLinearly);

            #endregion

            #region Image manipulation

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_FlipHorizontal)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_FlipHorizontal(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_FlipVertical)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_FlipVertical(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Rescale)]
            public delegate IntPtr FreeImage_Rescale(IntPtr bitmap, int ds_width, int dst_height, ImageFilter filter);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_PreMultiplyWithAlpha)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_PreMultiplyWithAlpha(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_AdjustGamma)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_AdjustGamma(IntPtr bitmap, double gamma);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_AdjustBrightness)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_AdjustBrightness(IntPtr bitmap, double percentage);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_AdjustContrast)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_AdjustContrast(IntPtr bitmap, double percentage);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Invert)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_Invert(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_SwapColors)]
            public delegate uint FreeImage_SwapColors(IntPtr bitmap, IntPtr rgbaToReplace, IntPtr rgbaToReplaceWith, [MarshalAs(UnmanagedType.Bool)] bool ignoreAlpha);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Rotate)]
            public delegate IntPtr FreeImage_Rotate(IntPtr bitmap, double angle, IntPtr fillColor);

            #endregion

            #region Versioning

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetVersion)]
            public delegate IntPtr FreeImage_GetVersion();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetCopyrightMessage)]
            public delegate IntPtr FreeImage_GetCopyrightMessage();

            #endregion
        }

        #endregion

        #region FreeImageIOWrapper

        [StructLayout(LayoutKind.Sequential)]
        internal struct FreeImageIO
        {
            public IntPtr ReadProc;
            public IntPtr WriteProc;
            public IntPtr SeekProc;
            public IntPtr TellProc;
        }

        internal sealed class StreamWrapper : IDisposable
        {
            private Stream m_stream;
            private byte[] m_tempBuffer;
            private IntPtr m_tempBufferPtr;
            private GCHandle m_gcHandle;
            private bool m_isDisposed;

            public bool IsDisposed
            {
                get
                {
                    return m_isDisposed;
                }
            }

            public StreamWrapper(Stream str) : this(str, true) { }

            public StreamWrapper(Stream str, bool allocateTempBuffer)
            {
                m_stream = str;

                if (allocateTempBuffer)
                {
                    m_tempBuffer = new byte[8192];
                    m_tempBufferPtr = MemoryHelper.PinObject(m_tempBuffer);
                }

                m_gcHandle = new GCHandle();
                m_isDisposed = false;
            }

            public IntPtr GetHandle()
            {
                if (m_gcHandle.IsAllocated)
                    return GCHandle.ToIntPtr(m_gcHandle);

                m_gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
                return GCHandle.ToIntPtr(m_gcHandle);
            }

            ~StreamWrapper()
            {
                Dispose(false);
            }

            public uint Read(IntPtr buffer, uint size, uint count)
            {
                EnsureCapacity(size);

                uint readCount = 0;

                int read;

                while (readCount < count)
                {
                    read = m_stream.Read(m_tempBuffer, 0, (int)size);
                    if (read != (int)size)
                    {
                        m_stream.Seek(-read, SeekOrigin.Current);
                        break;
                    }

                    MemoryHelper.CopyMemory(buffer, m_tempBufferPtr, read);
                    buffer += read;

                    readCount++;
                }

                return readCount;
            }

            public uint Write(IntPtr buffer, uint size, uint count)
            {
                EnsureCapacity(size);

                uint writeCount = 0;

                while(writeCount < count)
                {
                    MemoryHelper.CopyMemory(m_tempBufferPtr, buffer, (int) size);
                    buffer += (int)size;

                    try
                    {
                        m_stream.Write(m_tempBuffer, 0, (int) size);
                    }
                    catch
                    {
                        return writeCount;
                    }

                    writeCount++;
                }

                return writeCount;
            }

            public void Seek(long offset, int origin)
            {
                m_stream.Seek(offset, (SeekOrigin)origin);
            }

            public long Tell()
            {
                return m_stream.Position;
            }

            public void Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }

            private void Dispose(bool isDisposing)
            {
                if (!m_isDisposed)
                {
                    if (isDisposing)
                    {
                        if (m_tempBufferPtr != IntPtr.Zero)
                        {
                            if (m_tempBuffer != null)
                                MemoryHelper.UnpinObject(m_tempBuffer);

                            m_tempBuffer = null;
                            m_tempBufferPtr = IntPtr.Zero;
                        }
                    }

                    m_gcHandle.Free();

                    m_isDisposed = true;
                }
            }

            private void EnsureCapacity(uint size)
            {
                if(m_tempBuffer == null)
                {
                    m_tempBuffer = new byte[size];
                    m_tempBufferPtr = MemoryHelper.PinObject(m_tempBuffer);
                }
                else if(m_tempBuffer.Length < size)
                {
                    MemoryHelper.UnpinObject(m_tempBuffer);

                    m_tempBuffer = new byte[size];
                    m_tempBufferPtr = MemoryHelper.PinObject(m_tempBuffer);
                }
            }
        }

        internal sealed class FreeImageIOHandler
        {
            #region ImageIO Functions

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate uint FreeImageIO_ReadProc(IntPtr buffer, uint size, uint count, IntPtr ioHandle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate uint FreeImageIO_WriteProc(IntPtr buffer, uint size, uint count, IntPtr ioHandle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int FreeImageIO_SeekProc32(IntPtr ioHandle, int offset, int origin);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int FreeImageIO_SeekProc64(IntPtr ioHandle, long offset, int origin);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int FreeImageIO_TellProc32(IntPtr ioHandle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate long FreeImageIO_TellProc64(IntPtr ioHandle);

            #endregion

            private FreeImageIO_ReadProc m_readProc;
            private FreeImageIO_WriteProc m_writeProc;
            private Delegate m_seekProc;
            private Delegate m_tellProc;

            private FreeImageIO m_imageIO;

            public FreeImageIO ImageIO
            {
                get
                {
                    return m_imageIO;
                }
            }
            
            public FreeImageIOHandler(bool isLong64Bits)
            {
                m_readProc = ReadProc;
                m_writeProc = WriteProc;
                m_seekProc = (isLong64Bits) ? (Delegate) new FreeImageIO_SeekProc64(SeekProc64) : (Delegate) new FreeImageIO_SeekProc32(SeekProc32);
                m_tellProc = (isLong64Bits) ? (Delegate) new FreeImageIO_TellProc64(TellProc64) : (Delegate) new FreeImageIO_TellProc32(TellProc32);

                m_imageIO.ReadProc = Marshal.GetFunctionPointerForDelegate(m_readProc);
                m_imageIO.WriteProc = Marshal.GetFunctionPointerForDelegate(m_writeProc);
                m_imageIO.SeekProc = Marshal.GetFunctionPointerForDelegate(m_seekProc);
                m_imageIO.TellProc = Marshal.GetFunctionPointerForDelegate(m_tellProc);
            }

            private unsafe uint ReadProc(IntPtr buffer, uint size, uint count, IntPtr ioHandle)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                return wrapper.Read(buffer, size, count);
            }

            private uint WriteProc(IntPtr buffer, uint size, uint count, IntPtr ioHandle)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                return wrapper.Write(buffer, size, count);
            }

            private int SeekProc32(IntPtr ioHandle, int offset, int origin)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                wrapper.Seek((long)offset, origin);

                return 0;
            }

            private int SeekProc64(IntPtr ioHandle, long offset, int origin)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                wrapper.Seek(offset, origin);

                return 0;
            }

            private int TellProc32(IntPtr ioHandle)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                return (int)wrapper.Tell();
            }

            private long TellProc64(IntPtr ioHandle)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                return wrapper.Tell();
            }
        }

        #endregion
    }
}
