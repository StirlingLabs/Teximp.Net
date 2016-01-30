using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeximpNet
{
    public enum ImageType
    {
        Unknown = 0,
        Bitmap = 1,
        UInt16 = 2,
        Int16 = 3,
        UInt32 = 4,
        Int32 = 5,
        Float = 6,
        Double = 7,
        Complex = 8,
        RGB16 = 9,
        RGBA16 = 10,
        RGBF = 11,
        RFBAF = 12
    }

    public enum ImageFormat
    {
        Unknown = -1,
        BMP = 0,
        ICO = 1,
        JPEG = 2,
        JNG = 3,
        KOALA = 4,
        LBM = 5,
        IFF = LBM,
        MNG = 6,
        PBM = 7,
        PBMRAW = 8,
        PCD = 9,
        PCX = 10,
        PGM = 11,
        PGMRAW = 12,
        PNG = 13,
        PPM = 14,
        PPMRAW = 15,
        RAS = 16,
        TARGA = 17,
        TIFF = 18,
        WBMP = 19,
        PSD = 20,
        CUT = 21,
        XBM = 22,
        XPM = 23,
        DDS = 24,
        GIF = 25,
        HDR = 26,
        FAXG3 = 27,
        SGI = 28,
        EXR = 29,
        J2K = 30,
        JP2 = 31,
        PFM = 32,
        PICT = 33,
        RAW = 34,
        WEBP = 35,
        JXR = 36
    }

    public enum ImageColorType
    {
        MinIsWhite = 0,
        MinIsBlack = 1,
        RGB = 2,
        Palette = 3,
        RGBA = 4,
        CMYK = 5
    }

    public enum ImageFilter
    {
        Box = 0,
        Bicubic = 1,
        Bilinear = 2,
        Bspline = 3,
        CatmullRom = 4,
        Lanczos3 = 5
    }

    public enum ImageConversion
    {
        To4Bits,
        To8Bits,
        To16Bits555,
        To16Bits565,
        To24Bits,
        To32Bits,
        ToGreyscale
    }
}
