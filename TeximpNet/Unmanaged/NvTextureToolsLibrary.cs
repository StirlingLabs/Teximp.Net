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
using System.Runtime.InteropServices;
using TeximpNet.Compression;

namespace TeximpNet.Unmanaged
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void BeginImageHandler(int size, int width, int height, int depth, int face, int mipLevel);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool OutputHandler(IntPtr data, int size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void EndImageHandler();

    public sealed class NvTextureToolsLibrary : UnmanagedLibrary
    {
        private static readonly Object s_sync = new Object();

        /// <summary>
        /// Default name of the 32-bit unmanaged library. Based on runtime implementation the prefix ("lib" on non-windows) and extension (.dll, .so, .dylib) will be appended automatically.
        /// </summary>
        private const String Default32BitName = "nvtt32";

        /// <summary>
        /// Default name of the 64-bit unmanaged library. Based on runtime implementation the prefix ("lib" on non-windows) and extension (.dll, .so, .dylib) will be appended automatically.
        /// </summary>
        private const String Default64BitName = "nvtt64";

        private static NvTextureToolsLibrary s_instance;

        public static NvTextureToolsLibrary Instance
        {
            get
            {
                lock(s_sync)
                {
                    if (s_instance == null)
                        s_instance = CreateInstance();

                    return s_instance;
                }
            }
        }

        private NvTextureToolsLibrary(String default32BitName, String default64BitName, Type[] unmanagedFunctionDelegateTypes) 
            : base(default32BitName, default64BitName, unmanagedFunctionDelegateTypes) { }

        private static NvTextureToolsLibrary CreateInstance()
        {
            return new NvTextureToolsLibrary(Default32BitName, Default64BitName, Helper.GetNestedTypes(typeof(Functions)));
        }

        #region Input options

        public IntPtr CreateInputOptions()
        {
            LoadIfNotLoaded();

            Functions.nvttCreateInputOptions func = GetFunction<Functions.nvttCreateInputOptions>(FunctionNames.nvttCreateInputOptions);

            return func();
        }

        public void DestroyInputOptions(IntPtr inputOptions)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttDestroyInputOptions func = GetFunction<Functions.nvttDestroyInputOptions>(FunctionNames.nvttDestroyInputOptions);

            func(inputOptions);
        }

        public void SetInputOptionsTextureLayout(IntPtr inputOptions, TextureType type, int width, int height, int depth)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsTextureLayout func = GetFunction<Functions.nvttSetInputOptionsTextureLayout>(FunctionNames.nvttSetInputOptionsTextureLayout);

            func(inputOptions, type, width, height, depth);
        }

        public void ResetInputOptionsTextureLayout(IntPtr inputOptions)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttResetInputOptionsTextureLayout func = GetFunction<Functions.nvttResetInputOptionsTextureLayout>(FunctionNames.nvttResetInputOptionsTextureLayout);

            func(inputOptions);
        }

        public bool SetInputOptionsMipmapData(IntPtr inputOptions, IntPtr data, int width, int height, int depth, int face, int mipmap)
        {
            if(inputOptions == IntPtr.Zero || data == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsMipmapData func = GetFunction<Functions.nvttSetInputOptionsMipmapData>(FunctionNames.nvttSetInputOptionsMipmapData);

            return TranslateBool(func(inputOptions, data, width, height, depth, face, mipmap));
        }

        public void SetInputOptionsFormat(IntPtr inputOptions, int pixelFormat)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsFormat func = GetFunction<Functions.nvttSetInputOptionsFormat>(FunctionNames.nvttSetInputOptionsFormat);

            func(inputOptions, (NvttInputFormat) pixelFormat);
        }

        public void SetInputOptionsAlphaMode(IntPtr inputOptions, AlphaMode alphaMode)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsAlphaMode func = GetFunction<Functions.nvttSetInputOptionsAlphaMode>(FunctionNames.nvttSetInputOptionsAlphaMode);

            func(inputOptions, alphaMode);
        }

        public void SetInputOptionsGamma(IntPtr inputOptions, float inputGamma, float outputGamma)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsGamma func = GetFunction<Functions.nvttSetInputOptionsGamma>(FunctionNames.nvttSetInputOptionsGamma);

            func(inputOptions, inputGamma, outputGamma);
        }

        public void SetInputOptionsWrapMode(IntPtr inputOptions, WrapMode wrapMode)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsWrapMode func = GetFunction<Functions.nvttSetInputOptionsWrapMode>(FunctionNames.nvttSetInputOptionsWrapMode);

            func(inputOptions, wrapMode);
        }

        public void SetInputOptionsMipmapFilter(IntPtr inputOptions, MipmapFilter filter)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsMipmapFilter func = GetFunction<Functions.nvttSetInputOptionsMipmapFilter>(FunctionNames.nvttSetInputOptionsMipmapFilter);

            func(inputOptions, filter);
        }

        public void SetInputOptionsMipmapGeneration(IntPtr inputOptions, bool isEnabled, int maxLevel)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsMipmapGeneration func = GetFunction<Functions.nvttSetInputOptionsMipmapGeneration>(FunctionNames.nvttSetInputOptionsMipmapGeneration);

            func(inputOptions, (isEnabled) ? NvttBool.True : NvttBool.False, maxLevel);
        }

        public void SetInputOptionsKaiserParameters(IntPtr inputOptions, float width, float alpha, float stretch)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsKaiserParameters func = GetFunction<Functions.nvttSetInputOptionsKaiserParameters>(FunctionNames.nvttSetInputOptionsKaiserParameters);

            func(inputOptions, width, alpha, stretch);
        }

        public void SetInputOptionsNormalMap(IntPtr inputOptions, bool isNormalMap)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsNormalMap func = GetFunction<Functions.nvttSetInputOptionsNormalMap>(FunctionNames.nvttSetInputOptionsNormalMap);

            func(inputOptions, (isNormalMap) ? NvttBool.True : NvttBool.False);
        }

        public void SetInputOptionsConvertToNormalMap(IntPtr inputOptions, bool convertToNormalMap)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsConvertToNormalMap func = GetFunction<Functions.nvttSetInputOptionsConvertToNormalMap>(FunctionNames.nvttSetInputOptionsConvertToNormalMap);

            func(inputOptions, (convertToNormalMap) ? NvttBool.True : NvttBool.False);
        }

        public void SetInputOptionsHeightEvaluation(IntPtr inputOptions, float redScale, float greenScale, float blueScale, float alphaScale)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsHeightEvaluation func = GetFunction<Functions.nvttSetInputOptionsHeightEvaluation>(FunctionNames.nvttSetInputOptionsHeightEvaluation);

            func(inputOptions, redScale, greenScale, blueScale, alphaScale);
        }

        public void SetInputOptionsNormalFilter(IntPtr inputOptions, float small, float medium, float big, float large)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsNormalFilter func = GetFunction<Functions.nvttSetInputOptionsNormalFilter>(FunctionNames.nvttSetInputOptionsNormalFilter);

            func(inputOptions, small, medium, big, large);
        }

        public void SetInputOptionsNormalizeMipmaps(IntPtr inputOptions, bool normalize)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsNormalizeMipmaps func = GetFunction<Functions.nvttSetInputOptionsNormalizeMipmaps>(FunctionNames.nvttSetInputOptionsNormalizeMipmaps);

            func(inputOptions, (normalize) ? NvttBool.True : NvttBool.False);
        }

        public void SetInputOptionsMaxExtents(IntPtr inputOptions, int dimensions)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsMaxExtents func = GetFunction<Functions.nvttSetInputOptionsMaxExtents>(FunctionNames.nvttSetInputOptionsMaxExtents);

            func(inputOptions, dimensions);
        }

        public void SetInputOptionsRoundMode(IntPtr inputOptions, RoundMode roundMode)
        {
            if(inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsRoundMode func = GetFunction<Functions.nvttSetInputOptionsRoundMode>(FunctionNames.nvttSetInputOptionsRoundMode);

            func(inputOptions, roundMode);
        }

        #endregion

        #region Compression options

        public IntPtr CreateCompressionOptions()
        {
            LoadIfNotLoaded();

            Functions.nvttCreateCompressionOptions func = GetFunction<Functions.nvttCreateCompressionOptions>(FunctionNames.nvttCreateCompressionOptions);

            return func();
        }

        public void DestroyCompressionOptions(IntPtr compressOptions)
        {
            if(compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttDestroyCompressionOptions func = GetFunction<Functions.nvttDestroyCompressionOptions>(FunctionNames.nvttDestroyCompressionOptions);

            func(compressOptions);
        }

        public void SetCompressionOptionsFormat(IntPtr compressOptions, CompressionFormat format)
        {
            if(compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetCompressionOptionsFormat func = GetFunction<Functions.nvttSetCompressionOptionsFormat>(FunctionNames.nvttSetCompressionOptionsFormat);

            func(compressOptions, format);
        }

        public void SetCompressionOptionsQuality(IntPtr compressOptions, CompressionQuality quality)
        {
            if(compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetCompressionOptionsQuality func = GetFunction<Functions.nvttSetCompressionOptionsQuality>(FunctionNames.nvttSetCompressionOptionsQuality);

            func(compressOptions, quality);
        }

        public void SetCompressionOptionsColorWeights(IntPtr compressOptions, float red, float green, float blue, float alpha)
        {
            if(compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetCompressionOptionsColorWeights func = GetFunction<Functions.nvttSetCompressionOptionsColorWeights>(FunctionNames.nvttSetCompressionOptionsColorWeights);

            func(compressOptions, red, green, blue, alpha);
        }

        public void SetCompressionOptionsPixelFormat(IntPtr compressOptions, uint bitsPerPixel, uint red_mask, uint green_mask, uint blue_mask, uint alpha_mask)
        {
            if(compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetCompressionOptionsPixelFormat func = GetFunction<Functions.nvttSetCompressionOptionsPixelFormat>(FunctionNames.nvttSetCompressionOptionsPixelFormat);

            func(compressOptions, bitsPerPixel, red_mask, green_mask, blue_mask, alpha_mask);
        }

        public void SetCompressionOptionsQuantization(IntPtr compressOptions, bool colorDithering, bool alphaDithering, bool binaryAlpha, int alphaThreshold)
        {
            if(compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetCompressionOptionsQuantization func = GetFunction<Functions.nvttSetCompressionOptionsQuantization>(FunctionNames.nvttSetCompressionOptionsQuantization);

            func(compressOptions, (colorDithering) ? NvttBool.True : NvttBool.False, (alphaDithering) ? NvttBool.True : NvttBool.False, (binaryAlpha) ? NvttBool.True : NvttBool.False, alphaThreshold);
        }

        #endregion

        #region Output options

        public IntPtr CreateOutputOptions()
        {
            LoadIfNotLoaded();

            Functions.nvttCreateOutputOptions func = GetFunction<Functions.nvttCreateOutputOptions>(FunctionNames.nvttCreateOutputOptions);

            return func();
        }

        public void DestroyOutputOptions(IntPtr outputOptions)
        {
            if(outputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttDestroyOutputOptions func = GetFunction<Functions.nvttDestroyOutputOptions>(FunctionNames.nvttDestroyOutputOptions);

            func(outputOptions);
        }

        public void SetOutputOptionsFileName(IntPtr outputOptions, String filename)
        {
            if(outputOptions == IntPtr.Zero || String.IsNullOrEmpty(filename))
                return;

            LoadIfNotLoaded();

            Functions.nvttSetOutputOptionsFileName func = GetFunction<Functions.nvttSetOutputOptionsFileName>(FunctionNames.nvttSetOutputOptionsFileName);

            func(outputOptions, filename);
        }

        public void SetOutputOptionsOutputHeader(IntPtr outputOptions, bool value)
        {
            if(outputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetOutputOptionsOutputHeader func = GetFunction<Functions.nvttSetOutputOptionsOutputHeader>(FunctionNames.nvttSetOutputOptionsOutputHeader);

            func(outputOptions, (value) ? NvttBool.True : NvttBool.False);
        }

        public void SetOutputOptionsOutputHandler(IntPtr outputOptions, IntPtr beginImageHandlerCallback, IntPtr outputHandlerCallback, IntPtr endImageHandlerCallback)
        {
            if(outputOptions == IntPtr.Zero || beginImageHandlerCallback == IntPtr.Zero || outputHandlerCallback == IntPtr.Zero || endImageHandlerCallback == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetOutputOptionsOutputHandler func = GetFunction<Functions.nvttSetOutputOptionsOutputHandler>(FunctionNames.nvttSetOutputOptionsOutputHandler);

            func(outputOptions, beginImageHandlerCallback, outputHandlerCallback, endImageHandlerCallback);
        }

        #endregion

        #region Compressor

        public IntPtr CreateCompressor()
        {
            LoadIfNotLoaded();

            Functions.nvttCreateCompressor func = GetFunction<Functions.nvttCreateCompressor>(FunctionNames.nvttCreateCompressor);

            return func();
        }

        public void DestroyCompressor(IntPtr compressor)
        {
            if(compressor == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttDestroyCompressor func = GetFunction<Functions.nvttDestroyCompressor>(FunctionNames.nvttDestroyCompressor);

            func(compressor);
        }

        public bool Process(IntPtr compressor, IntPtr inputOptions, IntPtr compressionOptions, IntPtr outputOptions)
        {
            if(compressor == IntPtr.Zero || inputOptions == IntPtr.Zero || compressionOptions == IntPtr.Zero || outputOptions == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.nvttCompress func = GetFunction<Functions.nvttCompress>(FunctionNames.nvttCompress);

            return TranslateBool(func(compressor, inputOptions, compressionOptions, outputOptions));
        }

        public int EstimateSize(IntPtr compressor, IntPtr inputOptions, IntPtr compressionOptions)
        {
            if(compressor == IntPtr.Zero || inputOptions == IntPtr.Zero || compressionOptions == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.nvttEstimateSize func = GetFunction<Functions.nvttEstimateSize>(FunctionNames.nvttEstimateSize);

            return func(compressor, inputOptions, compressionOptions);
        }

        #endregion

            #region Global functions

        public uint GetVersion()
        {
            LoadIfNotLoaded();

            Functions.nvttVersion func = GetFunction<Functions.nvttVersion>(FunctionNames.nvttVersion);

            return func();
        }

        private static bool TranslateBool(NvttBool value)
        {
            return (value == NvttBool.False) ? false : true;
        }

        #endregion


        #region Function names

        internal static class FunctionNames
        {
            #region Input options

            public const String nvttCreateInputOptions = "nvttCreateInputOptions";
            public const String nvttDestroyInputOptions = "nvttDestroyInputOptions";
            public const String nvttSetInputOptionsTextureLayout = "nvttSetInputOptionsTextureLayout";
            public const String nvttResetInputOptionsTextureLayout = "nvttResetInputOptionsTextureLayout";
            public const String nvttSetInputOptionsMipmapData = "nvttSetInputOptionsMipmapData";
            public const String nvttSetInputOptionsFormat = "nvttSetInputOptionsFormat";
            public const String nvttSetInputOptionsAlphaMode = "nvttSetInputOptionsAlphaMode";
            public const String nvttSetInputOptionsGamma = "nvttSetInputOptionsGamma";
            public const String nvttSetInputOptionsWrapMode = "nvttSetInputOptionsWrapMode";
            public const String nvttSetInputOptionsMipmapFilter = "nvttSetInputOptionsMipmapFilter";
            public const String nvttSetInputOptionsMipmapGeneration = "nvttSetInputOptionsMipmapGeneration";
            public const String nvttSetInputOptionsKaiserParameters = "nvttSetInputOptionsKaiserParameters";
            public const String nvttSetInputOptionsNormalMap = "nvttSetInputOptionsNormalMap";
            public const String nvttSetInputOptionsConvertToNormalMap = "nvttSetInputOptionsConvertToNormalMap";
            public const String nvttSetInputOptionsHeightEvaluation = "nvttSetInputOptionsHeightEvaluation";
            public const String nvttSetInputOptionsNormalFilter = "nvttSetInputOptionsNormalFilter";
            public const String nvttSetInputOptionsNormalizeMipmaps = "nvttSetInputOptionsNormalizeMipmaps";
            public const String nvttSetInputOptionsMaxExtents = "nvttSetInputOptionsMaxExtents";
            public const String nvttSetInputOptionsRoundMode = "nvttSetInputOptionsRoundMode";

            #endregion

            #region Compression options

            public const String nvttCreateCompressionOptions = "nvttCreateCompressionOptions";
            public const String nvttDestroyCompressionOptions = "nvttDestroyCompressionOptions";
            public const String nvttSetCompressionOptionsFormat = "nvttSetCompressionOptionsFormat";
            public const String nvttSetCompressionOptionsQuality = "nvttSetCompressionOptionsQuality";
            public const String nvttSetCompressionOptionsColorWeights = "nvttSetCompressionOptionsColorWeights";
            public const String nvttSetCompressionOptionsPixelFormat = "nvttSetCompressionOptionsPixelFormat";
            public const String nvttSetCompressionOptionsQuantization = "nvttSetCompressionOptionsQuantization";

            #endregion

            #region Output options

            public const String nvttCreateOutputOptions = "nvttCreateOutputOptions";
            public const String nvttDestroyOutputOptions = "nvttDestroyOutputOptions";
            public const String nvttSetOutputOptionsFileName = "nvttSetOutputOptionsFileName";
            public const String nvttSetOutputOptionsOutputHeader = "nvttSetOutputOptionsOutputHeader";
            public const String nvttSetOutputOptionsOutputHandler = "nvttSetOutputOptionsOutputHandler";

            #endregion

            #region Compressor

            public const String nvttCreateCompressor = "nvttCreateCompressor";
            public const String nvttDestroyCompressor = "nvttDestroyCompressor";
            public const String nvttCompress = "nvttCompress";
            public const String nvttEstimateSize = "nvttEstimateSize";

            #endregion

            #region Global functions

            public const String nvttVersion = "nvttVersion";
            public const String nvttErrorString = "nvttErrorString";

            #endregion
        }

        #endregion

        #region Enums

        //Just for easier interop
        internal enum NvttBool
        {
            False = 0,
            True = 1
        }

        //Only format support apparently, kind of no point but keep it around...
        internal enum NvttInputFormat
        {
            BGRA_8UB = 0
        }

        //C-API has the error handler commented out...
        internal enum NvttError
        {
            InvalidInput = 0,
            UserInterruption = 1,
            UnsupportedFeature = 2,
            CudaError = 3,
            Unknown = 4,
            FileOpen = 5,
            FileWrite = 6,
            UnsupportedOutputFormat = 7
        }

        #endregion

        #region Function delegates

        internal static class Functions
        {
            #region Input options

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttCreateInputOptions)]
            public delegate IntPtr nvttCreateInputOptions();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttDestroyInputOptions)]
            public delegate void nvttDestroyInputOptions(IntPtr inputOptions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsTextureLayout)]
            public delegate void nvttSetInputOptionsTextureLayout(IntPtr inputOptions, TextureType type, int width, int height, int depth);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttResetInputOptionsTextureLayout)]
            public delegate void nvttResetInputOptionsTextureLayout(IntPtr inputOptions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsMipmapData)]
            public delegate NvttBool nvttSetInputOptionsMipmapData(IntPtr inputOptions, IntPtr data, int width, int height, int depth, int face, int mipmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsFormat)]
            public delegate void nvttSetInputOptionsFormat(IntPtr inputOptions, NvttInputFormat format);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsAlphaMode)]
            public delegate void nvttSetInputOptionsAlphaMode(IntPtr inputOptions, AlphaMode alphaMode);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsGamma)]
            public delegate void nvttSetInputOptionsGamma(IntPtr inputOptions, float inputGamma, float outputGamma);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsWrapMode)]
            public delegate void nvttSetInputOptionsWrapMode(IntPtr inputOptions, WrapMode wrapMode);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsMipmapFilter)]
            public delegate void nvttSetInputOptionsMipmapFilter(IntPtr inputOptions, MipmapFilter filter);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsMipmapGeneration)]
            public delegate void nvttSetInputOptionsMipmapGeneration(IntPtr inputOptions, NvttBool isEnabled, int maxLevel);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsKaiserParameters)]
            public delegate void nvttSetInputOptionsKaiserParameters(IntPtr inputOptions, float width, float alpha, float stretch);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsNormalMap)]
            public delegate void nvttSetInputOptionsNormalMap(IntPtr inputOptions, NvttBool isNormalMap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsConvertToNormalMap)]
            public delegate void nvttSetInputOptionsConvertToNormalMap(IntPtr inputOptions, NvttBool convertToNormalMap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsHeightEvaluation)]
            public delegate void nvttSetInputOptionsHeightEvaluation(IntPtr inputOptions, float redScale, float greenScale, float blueScale, float alphaScale);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsNormalFilter)]
            public delegate void nvttSetInputOptionsNormalFilter(IntPtr inputOptions, float small, float medium, float big, float large);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsNormalizeMipmaps)]
            public delegate void nvttSetInputOptionsNormalizeMipmaps(IntPtr inputOptions, NvttBool normalize);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsMaxExtents)]
            public delegate void nvttSetInputOptionsMaxExtents(IntPtr inputOptions, int dimensions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsRoundMode)]
            public delegate void nvttSetInputOptionsRoundMode(IntPtr inputOptions, RoundMode roundMode);

            #endregion

            #region Compression options

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttCreateCompressionOptions)]
            public delegate IntPtr nvttCreateCompressionOptions();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttDestroyCompressionOptions)]
            public delegate void nvttDestroyCompressionOptions(IntPtr compressOptions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetCompressionOptionsFormat)]
            public delegate void nvttSetCompressionOptionsFormat(IntPtr compressOptions, CompressionFormat format);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetCompressionOptionsQuality)]
            public delegate void nvttSetCompressionOptionsQuality(IntPtr compressOptions, CompressionQuality quality);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetCompressionOptionsColorWeights)]
            public delegate void nvttSetCompressionOptionsColorWeights(IntPtr compressOptions, float red, float green, float blue, float alpha);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetCompressionOptionsPixelFormat)]
            public delegate void nvttSetCompressionOptionsPixelFormat(IntPtr compressOptions, uint bitsPerPixel, uint red_mask, uint green_mask, uint blue_mask, uint alpha_mask);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetCompressionOptionsQuantization)]
            public delegate void nvttSetCompressionOptionsQuantization(IntPtr compressOptions, NvttBool colorDithering, NvttBool alphaDithering, NvttBool binaryAlpha, int alphaThreshold);

            #endregion

            #region Output options

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttCreateOutputOptions)]
            public delegate IntPtr nvttCreateOutputOptions();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttDestroyOutputOptions)]
            public delegate void nvttDestroyOutputOptions(IntPtr outputOptions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetOutputOptionsFileName)]
            public delegate void nvttSetOutputOptionsFileName(IntPtr outputOptions, [In, MarshalAs(UnmanagedType.LPStr)] String filename);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetOutputOptionsOutputHeader)]
            public delegate void nvttSetOutputOptionsOutputHeader(IntPtr outputOptions, NvttBool value);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetOutputOptionsOutputHandler)]
            public delegate void nvttSetOutputOptionsOutputHandler(IntPtr outputOptions, IntPtr beginImageHandlerCallback, IntPtr outputHandlerCallback, IntPtr endImageHandlerCallback);

            #endregion

            #region Compressor

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttCreateCompressor)]
            public delegate IntPtr nvttCreateCompressor();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttDestroyCompressor)]
            public delegate void nvttDestroyCompressor(IntPtr compressor);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttCompress)]
            public delegate NvttBool nvttCompress(IntPtr compressor, IntPtr inputOptions, IntPtr compressionOptions, IntPtr outputOptions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttEstimateSize)]
            public delegate int nvttEstimateSize(IntPtr compressor, IntPtr inputOptions, IntPtr compressionOptions);

            #endregion

            #region Global functions

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttVersion)]
            public delegate uint nvttVersion();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttErrorString)]
            [return: MarshalAs(UnmanagedType.LPStr)]
            public delegate String nvttErrorString(NvttError err);

            #endregion
        }

        #endregion
    }
}
