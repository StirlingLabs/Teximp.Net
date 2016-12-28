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
