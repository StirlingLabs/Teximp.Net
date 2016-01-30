using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TeximpNet
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RGBAQuad
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public RGBAQuad(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public BGRAQuad ToBGRA()
        {
            return new BGRAQuad(B, G, R, A);
        }
    }

    public struct BGRAQuad
    {
        public byte B;
        public byte G;
        public byte R;
        public byte A;

        public BGRAQuad(byte b, byte g, byte r, byte a)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        public RGBAQuad ToRGBA()
        {
            return new RGBAQuad(R, G, B, A);
        }
    }
}
