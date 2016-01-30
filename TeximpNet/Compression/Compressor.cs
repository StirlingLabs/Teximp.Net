using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeximpNet.Unmanaged;
using System.Runtime.InteropServices;

namespace TeximpNet.Compression
{
    public sealed class Compressor : IDisposable
    {
        private IntPtr m_compressorPtr;
        private IntPtr m_inputOptionsPtr;
        private IntPtr m_compressionOptionsPtr;
        private IntPtr m_outputOptionsPtr;

        private InputOptions m_inputOptions;
        private CompressionOptions m_compressionOptions;
        private OutputOptions m_outputOptions;
        private bool m_isDisposed;

        private List<CompressedImageData> m_mipChain;
        private CompressedImageData m_currentMip;
        private CompressionFormat m_format;
        private int m_currentBytePos;

        public IntPtr NativePtr
        {
            get
            {
                return m_compressorPtr;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return m_isDisposed;
            }
        }

        public InputOptions Input
        {
            get
            {
                return m_inputOptions;
            }
        }

        public CompressionOptions Compression
        {
            get
            {
                return m_compressionOptions;
            }
        }

        public OutputOptions Output
        {
            get
            {
                return m_outputOptions;
            }
        }

        public Compressor()
        {
            m_compressorPtr = NvTextureToolsLibrary.Instance.CreateCompressor();
            m_inputOptionsPtr = NvTextureToolsLibrary.Instance.CreateInputOptions();
            m_compressionOptionsPtr = NvTextureToolsLibrary.Instance.CreateCompressionOptions();
            m_outputOptionsPtr = NvTextureToolsLibrary.Instance.CreateOutputOptions();

            m_inputOptions = new InputOptions(m_inputOptionsPtr);
            m_compressionOptions = new CompressionOptions(m_compressionOptionsPtr);
            m_outputOptions = new OutputOptions(m_outputOptionsPtr, BeginImage, OutputImage, EndImage);

            m_isDisposed = false;
        }

        ~Compressor()
        {
            Dispose(false);
        }

        public bool Process(String filename)
        {
            if(String.IsNullOrEmpty(filename))
                return false;

            m_outputOptions.ResetCallbacks();
            m_outputOptions.SetOutputToFile(filename);

            return NvTextureToolsLibrary.Instance.Process(m_compressorPtr, m_inputOptionsPtr, m_compressionOptionsPtr, m_outputOptionsPtr);
        }

        public bool Process(List<CompressedImageData> mipChain)
        {
            if(mipChain == null)
                return false;

            m_outputOptions.SetOutputToMemory();

            m_format = m_compressionOptions.Format;
            m_mipChain = mipChain;

            return NvTextureToolsLibrary.Instance.Process(m_compressorPtr, m_inputOptionsPtr, m_compressionOptionsPtr, m_outputOptionsPtr);
        }

        private void BeginImage(int size, int width, int height, int depth, int face, int mipLevel)
        {
            m_currentMip = new CompressedImageData(width, height, (CubeMapFace) face, m_format);
            m_currentBytePos = 0;
            m_mipChain.Add(m_currentMip);
        }

        private void OutputImage(IntPtr data, int size)
        {
            if(m_currentMip == null)
                return;

            MemoryHelper.CopyMemory(m_currentMip.DataPtr + m_currentBytePos, data, size);
            m_currentBytePos += size;
        }

        private void EndImage()
        {
            m_currentMip = null;
            m_currentBytePos = 0;
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
                NvTextureToolsLibrary lib = NvTextureToolsLibrary.Instance;

                if(isDisposing)
                {
                    m_outputOptions.ResetCallbacks();

                    m_inputOptions = null;
                    m_compressionOptions = null;
                    m_outputOptions = null;
                }

                //I think if we're getting called by the finalizer at the point that the lib is null, we're probably shutting down anyways...
                if(lib != null && lib.IsLibraryLoaded)
                {
                    if(m_compressorPtr != IntPtr.Zero)
                    {
                        lib.DestroyCompressor(m_compressorPtr);
                        m_compressorPtr = IntPtr.Zero;
                    }

                    if(m_inputOptionsPtr != IntPtr.Zero)
                    {
                        lib.DestroyInputOptions(m_inputOptionsPtr);
                        m_inputOptionsPtr = IntPtr.Zero;
                    }

                    if(m_compressionOptionsPtr != IntPtr.Zero)
                    {
                        lib.DestroyCompressionOptions(m_compressionOptionsPtr);
                        m_compressionOptionsPtr = IntPtr.Zero;
                    }

                    if(m_outputOptionsPtr != IntPtr.Zero)
                    {
                        lib.DestroyOutputOptions(m_outputOptionsPtr);
                        m_outputOptionsPtr = IntPtr.Zero;
                    }
                }

                m_isDisposed = true;
            }
        }

        public sealed class InputOptions
        {
            private IntPtr m_inputOptionsPtr;
            private TextureType m_type;
            private int m_width, m_height, m_depth;
            private AlphaMode m_alphaMode;
            private float m_inputGamma, m_outputGamma;
            private bool m_generateMipmaps;
            private int m_mipMaxLevel; //When -1, full chain is generated, other values limit # of mips that get generated
            private int m_mipCount;
            private MipmapFilter m_mipFilter;
            private float m_kaiserWidth, m_kaiserAlpha, m_kaiserStretch;
            private bool m_isNormalMap;
            private bool m_normalizeMipmaps;
            private bool m_convertToNormalMap;
            private float m_redScale, m_greenScale, m_blueScale, m_alphaScale;
            private float m_smallBumpFreqScale, m_mediumBumpFreqScale, m_bigBumpFreqScale, m_largeDumpFreqScale;
            private int m_maxExtent;
            private RoundMode m_roundMode;
            private WrapMode m_wrapMode;

            public IntPtr NativePtr
            {
                get
                {
                    return m_inputOptionsPtr;
                }
            }

            public TextureType TextureType
            {
                get
                {
                    return m_type;
                }
            }

            public int Width
            {
                get
                {
                    return m_width;
                }
            }

            public int Height
            {
                get
                {
                    return m_height;
                }
            }

            public int Depth
            {
                get
                {
                    return m_depth;
                }
            }

            public int FaceCount
            {
                get
                {
                    return (m_type == TextureType.TextureCube) ? 6 : 1;
                }
            }

            public int MipmapCount
            {
                get
                {
                    return (m_mipMaxLevel == -1) ? m_mipCount : m_mipMaxLevel;
                }
            }

            public bool GenerateMipmaps
            {
                get
                {
                    return m_generateMipmaps;
                }
                set
                {
                    SetMipmapGeneration(value, -1);
                }
            }

            public AlphaMode AlphaMode
            {
                get
                {
                    return m_alphaMode;
                }
                set
                {
                    m_alphaMode = value;
                    NvTextureToolsLibrary.Instance.SetInputOptionsAlphaMode(m_inputOptionsPtr, value);
                }
            }

            public RoundMode RoundMode
            {
                get
                {
                    return m_roundMode;
                }
                set
                {
                    m_roundMode = value;
                    NvTextureToolsLibrary.Instance.SetInputOptionsRoundMode(m_inputOptionsPtr, value);
                }
            }

            public int MaxTextureExtent
            {
                get
                {
                    return m_maxExtent;
                }
                set
                {
                    m_maxExtent = value;
                    NvTextureToolsLibrary.Instance.SetInputOptionsMaxExtents(m_inputOptionsPtr, value);
                }
            }

            public MipmapFilter MipmapFilter
            {
                get
                {
                    return m_mipFilter;
                }
                set
                {
                    m_mipFilter = value;
                    NvTextureToolsLibrary.Instance.SetInputOptionsMipmapFilter(m_inputOptionsPtr, value);
                }
            }

            public WrapMode WrapMode
            {
                get
                {
                    return m_wrapMode;
                }
                set
                {
                    m_wrapMode = value;
                    NvTextureToolsLibrary.Instance.SetInputOptionsWrapMode(m_inputOptionsPtr, value);
                }
            }

            public bool IsNormalMap
            {
                get
                {
                    return m_isNormalMap;
                }
                set
                {
                    m_isNormalMap = value;

                    NvTextureToolsLibrary.Instance.SetInputOptionsNormalMap(m_inputOptionsPtr, value);
                }
            }

            public bool NormalizeMipmaps
            {
                get
                {
                    return m_normalizeMipmaps;
                }
                set
                {
                    m_normalizeMipmaps = value;
                    NvTextureToolsLibrary.Instance.SetInputOptionsNormalizeMipmaps(m_inputOptionsPtr, value);
                }
            }

            public bool ConvertToNormalMap
            {
                get
                {
                    return m_convertToNormalMap;
                }
                set
                {
                    m_convertToNormalMap = value;
                    NvTextureToolsLibrary.Instance.SetInputOptionsConvertToNormalMap(m_inputOptionsPtr, value);
                }
            }

            internal InputOptions(IntPtr nativePtr)
            {
                m_inputOptionsPtr = nativePtr;

                m_type = TextureType.Texture2D;
                m_width = 0;
                m_height = 0;
                m_depth = 0;
                m_alphaMode = AlphaMode.None;

                m_inputGamma = 2.2f;
                m_outputGamma = 2.2f;

                m_generateMipmaps = true;
                m_mipMaxLevel = -1;
                m_mipFilter = MipmapFilter.Box;
                m_mipCount = 0;

                m_kaiserWidth = 3;
                m_kaiserAlpha = 4.0f;
                m_kaiserStretch = 1.0f;

                m_isNormalMap = false;
                m_normalizeMipmaps = true;
                m_convertToNormalMap = false;
                m_redScale = 0f;
                m_greenScale = 0f;
                m_blueScale = 0f;
                m_alphaScale = 1.0f;

                float denom = 1.0f + 0.5f + 0.25f + 0.125f;
                m_smallBumpFreqScale = 1.0f / denom;
                m_mediumBumpFreqScale = 0.5f / denom;
                m_bigBumpFreqScale = 0.25f / denom;
                m_largeDumpFreqScale = 0.125f / denom;

                //Input format is always BGRA_8UB so don't expose it here
            }

            public void SetTextureLayout(TextureType type, int width, int height, int depth)
            {
                m_type = type;
                m_width = width;
                m_height = height;
                m_depth = depth;

                if(m_generateMipmaps)
                    m_mipMaxLevel = MemoryHelper.CountMipmaps(width, height, depth);

                NvTextureToolsLibrary.Instance.SetInputOptionsTextureLayout(m_inputOptionsPtr, type, width, height, depth);
            }

            public void ClearTextureLayout()
            {
                m_type = TextureType.Texture2D;
                m_width = 0;
                m_height = 0;
                m_depth = 0;

                NvTextureToolsLibrary.Instance.ResetInputOptionsTextureLayout(m_inputOptionsPtr);
            }

            /// <summary>
            /// Sets mipmap data as input. Format is always considered to be in 32-bit BGRA form. Don't forget to set the texture layout first otherwise this will error.
            /// </summary>
            /// <param name="data">Pointer to data.</param>
            /// <param name="width">Width of the image.</param>
            /// <param name="height">Height of the image.</param>
            /// <param name="depth">Depth of the image.</param>
            /// <param name="face">Cubemap face that the image corresponds to.</param>
            /// <param name="mipmapLevel">Mip level the image corresponds to.</param>
            /// <returns>True if the data was successfully set, false otherwise (e.g. does not match texture layout which needs to be set first).</returns>
            public bool SetMipmapData(IntPtr data, int width, int height, int depth, CubeMapFace face, int mipmapLevel)
            {
                return SetMipmapData(data, true, width, height, depth, face, mipmapLevel);
            }

            /// <summary>
            /// Sets mipmap data as input. Don't forget to set the texture layout first otherwise this will error.
            /// </summary>
            /// <param name="data">Pointer to data.</param>
            /// <param name="isBGRA">True if the data is in BGRA format, if false then RGBA. If false then the data is copied and converted to BGRA format.</param>
            /// <param name="width">Width of the image.</param>
            /// <param name="height">Height of the image.</param>
            /// <param name="depth">Depth of the image.</param>
            /// <param name="face">Cubemap face that the image corresponds to.</param>
            /// <param name="mipmapLevel">Mip level the image corresponds to.</param>
            /// <returns>True if the data was successfully set, false otherwise (e.g. does not match texture layout which needs to be set first).</returns>
            public bool SetMipmapData(IntPtr data, bool isBGRA, int width, int height, int depth, CubeMapFace face, int mipmapLevel)
            {
                if(data == IntPtr.Zero)
                    return false;

                IntPtr bgraPtr = data;
                bool needToDisposeBGRAPtr = false;

                if(!isBGRA)
                {
                    bgraPtr = ConvertToBGRA(data, width, height);
                    needToDisposeBGRAPtr = true;
                }

                try
                {
                    return NvTextureToolsLibrary.Instance.SetInputOptionsMipmapData(m_inputOptionsPtr, bgraPtr, width, height, depth, (int)face, mipmapLevel);
                }
                finally
                {
                    if (needToDisposeBGRAPtr)
                        MemoryHelper.FreeMemory(bgraPtr);
                }
            }

            public bool SetMipmapData(Surface data, CubeMapFace face, int mipmapLevel)
            {
                if(data == null || data.ImageType != ImageType.Bitmap)
                    return false;

                //Ensure we are 32-bit bitmap
                Surface rgbaData = data;
                bool needToDispose = false;
                if(data.BitsPerPixel != 32)
                {
                    rgbaData = data.Clone();
                    if (!rgbaData.ConvertTo(ImageConversion.To32Bits))
                    {
                        rgbaData.Dispose();
                        return false;
                    }

                    needToDispose = true;
                }

                int width = rgbaData.Width;
                int height = rgbaData.Height;

                IntPtr bgraPtr = rgbaData.DataPtr;
                bool needToDisposeBGRAPtr = false;

                //Need to convert to BGRA since big endian has data in RGBA form
                if (!FreeImageLibrary.Instance.IsLittleEndian)
                {
                    bgraPtr = ConvertToBGRA(rgbaData.DataPtr, width, height);
                    needToDisposeBGRAPtr = true;
                }

                try
                {
                    return NvTextureToolsLibrary.Instance.SetInputOptionsMipmapData(m_inputOptionsPtr, bgraPtr, width, height, 1, (int)face, mipmapLevel);
                }
                finally
                {
                    if (needToDispose)
                        rgbaData.Dispose();

                    if (needToDisposeBGRAPtr)
                        MemoryHelper.FreeMemory(bgraPtr);
                }        
            }

            private unsafe IntPtr ConvertToBGRA(IntPtr rgbaPtr, int width, int height)
            {
                IntPtr bgraPtr = MemoryHelper.AllocateMemory(4 * width * height);

                byte* pBGRA = (byte*)bgraPtr.ToPointer();
                byte* pRGBA = (byte*)rgbaPtr.ToPointer();

                int totalTexels = width * height;

                for(int i = 0; i < totalTexels; i++)
                {
                    //RGBA -> BGRA
                    pBGRA[0] = pRGBA[2];
                    pBGRA[1] = pRGBA[1];
                    pBGRA[2] = pRGBA[0];
                    pBGRA[3] = pRGBA[3];

                    pBGRA += 4;
                    pRGBA += 4;
                }

                return bgraPtr;
            }

            public bool SetData(Surface data)
            {
                if (data == null || data.ImageType != ImageType.Bitmap)
                    return false;

                SetTextureLayout(TextureType.Texture2D, data.Width, data.Height, 1);

                bool success = SetMipmapData(data, 0, 0);

                if (!success)
                    ClearTextureLayout();

                return success;
            }

            public bool SetData(Surface[] cubeFaces)
            {
                if (cubeFaces == null)
                    return false;

                Surface first = cubeFaces[0];

                if (first == null)
                    return false;

                for(int i = 1; i < cubeFaces.Length; i++)
                {
                    Surface next = cubeFaces[i];
                    if (next == null)
                        return false;

                    if (first.Width != next.Width || first.Height != next.Height)
                        return false;
                }

                SetTextureLayout(TextureType.TextureCube, first.Width, first.Height, 1);

                for (int i = 0; i < cubeFaces.Length; i++)
                {
                    //Set each cubemap face, if errors then reset and return
                    if(!SetMipmapData(cubeFaces[i], (CubeMapFace)i, 0))
                    {
                        ClearTextureLayout();
                        return false;
                    }
                }

                return true;
            }

            public void SetNormalFilter(float small, float medium, float big, float large)
            {
                float total = small + medium + big + large;

                m_smallBumpFreqScale = small / total;
                m_mediumBumpFreqScale = medium / total;
                m_bigBumpFreqScale = big / total;
                m_largeDumpFreqScale = large / total;

                NvTextureToolsLibrary.Instance.SetInputOptionsNormalFilter(m_inputOptionsPtr, small, medium, big, large);
            }

            public void GetNormalFilter(out float small, out float medium, out float big, out float large)
            {
                small = m_smallBumpFreqScale;
                medium = m_mediumBumpFreqScale;
                big = m_bigBumpFreqScale;
                large = m_largeDumpFreqScale;
            }

            public void SetGamma(float inputGamma, float outputGamma)
            {
                m_inputGamma = inputGamma;
                m_outputGamma = outputGamma;

                NvTextureToolsLibrary.Instance.SetInputOptionsGamma(m_inputOptionsPtr, m_inputGamma, m_outputGamma);
            }

            public void GetGamma(out float inputGamma, out float outputGamma)
            {
                inputGamma = m_inputGamma;
                outputGamma = m_outputGamma;
            }

            public void SetMipmapGeneration(bool generateMips, int maxLevel)
            {
                m_generateMipmaps = generateMips;
                m_mipMaxLevel = maxLevel;

                if(maxLevel != -1)
                    m_mipCount = 1;
                else
                    m_mipCount = MemoryHelper.CountMipmaps(m_width, m_height, m_depth);

                NvTextureToolsLibrary.Instance.SetInputOptionsMipmapGeneration(m_inputOptionsPtr, generateMips, maxLevel);
            }

            public void SetKaiserParameters(float width, float alpha, float stretch)
            {
                m_kaiserWidth = width;
                m_kaiserAlpha = alpha;
                m_kaiserStretch = stretch;

                NvTextureToolsLibrary.Instance.SetInputOptionsKaiserParameters(m_inputOptionsPtr, width, alpha, stretch);
            }

            public void GetKaiserParameters(out float width, out float alpha, out float stretch)
            {
                width = m_kaiserWidth;
                alpha = m_kaiserAlpha;
                stretch = m_kaiserStretch;
            }

            public void SetHeightEvaluation(float redScale, float greenScale, float blueScale, float alphaScale)
            {
                m_redScale = redScale;
                m_greenScale = greenScale;
                m_blueScale = blueScale;
                m_alphaScale = alphaScale;

                NvTextureToolsLibrary.Instance.SetInputOptionsHeightEvaluation(m_inputOptionsPtr, redScale, greenScale, blueScale, alphaScale);
            }

            public void GetHeightEvaluation(out float redScale, out float greenScale, out float blueScale, out float alphaScale)
            {
                redScale = m_redScale;
                greenScale = m_greenScale;
                blueScale = m_blueScale;
                alphaScale = m_alphaScale;
            }
        }

        public sealed class CompressionOptions
        {
            private IntPtr m_compressionOptionsPtr;
            private CompressionFormat m_format;
            private CompressionQuality m_quality;
            private float m_rColorWeight, m_gColorWeight, m_bColorWeight, m_aColorWeight;
            private uint m_bitCount;
            private uint m_rMask, m_gMask, m_bMask, m_aMask;
            private bool m_enableColorDithering, m_enableAlphaDithering, m_binaryAlpha;
            private int m_alphaThreshold;

            public IntPtr NativePtr
            {
                get
                {
                    return m_compressionOptionsPtr;
                }
            }

            public CompressionFormat Format
            {
                get
                {
                    return m_format;
                }
                set
                {
                    m_format = value;
                    NvTextureToolsLibrary.Instance.SetCompressionOptionsFormat(m_compressionOptionsPtr, value);
                }
            }

            public CompressionQuality Quality
            {
                get
                {
                    return m_quality;
                }
                set
                {
                    m_quality = value;
                    NvTextureToolsLibrary.Instance.SetCompressionOptionsQuality(m_compressionOptionsPtr, value);
                }
            }

            internal CompressionOptions(IntPtr nativePtr)
            {
                m_compressionOptionsPtr = nativePtr;

                m_format = CompressionFormat.DXT1;
                m_quality = CompressionQuality.Normal;
                m_bitCount = 32;
                m_bMask = 0x000000FF;
                m_gMask = 0x0000FF00;
                m_rMask = 0x00FF0000;
                m_aMask = 0xFF000000;
            }

            /// <summary>
            /// Sets the color output format if no block compression is set (up to 32 bit RGBA). For example, to convert to RGB 5:6:5 format,
            /// <code>SetPixelFormat(16, 0x001F, 0x07E0, 0xF800, 0)</code>.
            /// </summary>
            /// <param name="bitsPerPixel">Bits per pixel of the color format.</param>
            /// <param name="red_mask">Mask for the bits that correspond to the red channel.</param>
            /// <param name="green_mask">Mask for the bits that correspond to the green channel.</param>
            /// <param name="blue_mask">Mask for the bits that correspond to the blue channel.</param>
            /// <param name="alpha_mask">Mask for the bits that correspond to the alpha channel.</param>
            public void SetPixelFormat(uint bitsPerPixel, uint red_mask, uint green_mask, uint blue_mask, uint alpha_mask)
            {
                m_bitCount = bitsPerPixel;
                m_rMask = red_mask;
                m_gMask = green_mask;
                m_bMask = blue_mask;
                m_aMask = alpha_mask;

                NvTextureToolsLibrary.Instance.SetCompressionOptionsPixelFormat(m_compressionOptionsPtr, bitsPerPixel, red_mask, green_mask, blue_mask, alpha_mask);
            }

            /// <summary>
            /// Sets the color output format if no block compression is set to RGBA format rather than BGRA format. Essentially this sets the
            /// masks so red and blue values are swapped.
            /// </summary>
            public void SetRGBAPixelFormat()
            {
                uint alphaMask = 0xFF000000;
                uint blueMask = 0xFF0000;
                uint greenMask = 0xFF00;
                uint redMask = 0xFF;

                SetPixelFormat(32, redMask, greenMask, blueMask, alphaMask);
            }

            /// <summary>
            /// Sets the color output format if no block compression to the default BGRA format.
            /// </summary>
            public void SetBGRAPixelFormat()
            {
                uint alphaMask = 0xFF000000;
                uint redMask = 0xFF0000;
                uint greenMask = 0xFF00;
                uint blueMask = 0xFF;

                SetPixelFormat(32, redMask, greenMask, blueMask, alphaMask);
            }

            public void GetPixelFormat(out uint bitsPerPixel, out uint red_mask, out uint green_mask, out uint blue_mask, out uint alpha_mask)
            {
                bitsPerPixel = m_bitCount;
                red_mask = m_rMask;
                green_mask = m_gMask;
                blue_mask = m_bMask;
                alpha_mask = m_aMask;
            }

           public void SetQuantization(bool enableColorDithering, bool enableAlphaDithering, bool binaryAlpha, int alphaThreshold)
            {
                m_enableColorDithering = enableColorDithering;
                m_enableAlphaDithering = enableAlphaDithering;
                m_binaryAlpha = binaryAlpha;
                m_alphaThreshold = alphaThreshold;

                NvTextureToolsLibrary.Instance.SetCompressionOptionsQuantization(m_compressionOptionsPtr, enableColorDithering, enableAlphaDithering, binaryAlpha, alphaThreshold);
            }

            public void GetQuantization(out bool enableColorDithering, out bool enableAlphaDithering, out bool binaryAlpha, out int alphaThreshold)
            {
                enableColorDithering = m_enableColorDithering;
                enableAlphaDithering = m_enableAlphaDithering;
                binaryAlpha = m_binaryAlpha;
                alphaThreshold = m_alphaThreshold;
            }

            public void SetColorWeights(float red_weight, float green_weight, float blue_weight, float alpha_weight)
            {
                m_rColorWeight = red_weight;
                m_gColorWeight = green_weight;
                m_bColorWeight = blue_weight;
                m_aColorWeight = alpha_weight;

                NvTextureToolsLibrary.Instance.SetCompressionOptionsColorWeights(m_compressionOptionsPtr, red_weight, green_weight, blue_weight, alpha_weight);
            }

            public void GetColorWeights(out float red_weight, out float green_weight, out float blue_weight, out float alpha_weight)
            {
                red_weight = m_rColorWeight;
                green_weight = m_gColorWeight;
                blue_weight = m_bColorWeight;
                alpha_weight = m_aColorWeight;
            }
        }

        public sealed class OutputOptions
        {
            private IntPtr m_outputOptionsPtr;
            private BeginImageHandler m_beginCallback;
            private OutputHandler m_outputCallback;
            private EndImageHandler m_endCallback;
            private IntPtr m_beginPtr, m_outputPtr, m_endPtr;
            private bool m_outputToMemory;
            private bool m_outputHeader;

            public IntPtr NativePtr
            {
                get
                {
                    return m_outputOptionsPtr;
                }
            }

            public bool OutputHeader
            {
                get
                {
                    return m_outputHeader;
                }
                set
                {
                    m_outputHeader = value;
                    NvTextureToolsLibrary.Instance.SetOutputOptionsOutputHeader(m_outputOptionsPtr, value);
                }
            }

            internal OutputOptions(IntPtr nativePtr, BeginImageHandler beginCallback, OutputHandler outputCallback, EndImageHandler endCallback)
            {
                m_outputOptionsPtr = nativePtr;

                m_beginCallback = beginCallback;
                m_outputCallback = outputCallback;
                m_endCallback = endCallback;

                m_beginPtr = Marshal.GetFunctionPointerForDelegate(beginCallback);
                m_outputPtr = Marshal.GetFunctionPointerForDelegate(outputCallback);
                m_endPtr = Marshal.GetFunctionPointerForDelegate(m_endCallback);

                m_outputToMemory = false;
                m_outputHeader = true; //API says the write handler will output the texture file header by default
            }

            internal void SetOutputToFile(String fileName)
            {
                ResetCallbacks();

                NvTextureToolsLibrary.Instance.SetOutputOptionsFileName(m_outputOptionsPtr, fileName);
            }

            internal void SetOutputToMemory()
            {
                if(m_outputToMemory)
                    return;

                m_outputToMemory = true;
                NvTextureToolsLibrary.Instance.SetOutputOptionsOutputHandler(m_outputOptionsPtr, m_beginPtr, m_outputPtr, m_endPtr);
            }

            internal void ResetCallbacks()
            {
                if(m_outputToMemory)
                {
                    NvTextureToolsLibrary.Instance.SetOutputOptionsOutputHandler(m_outputOptionsPtr, m_beginPtr, m_outputPtr, m_endPtr);
                    m_outputToMemory = false;
                }
            }
        }
    }
}
