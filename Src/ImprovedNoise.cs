using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DifferentMethods.NoiseMaps
{

    /* 
    Taken from http://flafla2.github.io/2014/08/09/perlinnoise.html
    and modified for speed and seedability.
    */
    public class ImprovedNoise
    {

        static ImprovedNoise()
        {
            p = new int[512];
            for (int x = 0; x < 512; x++)
            {
                p[x] = permutation[x % 256];
            }
        }

        public static float Sample(float x, float y, float z, int repeat = 0, int seed = 0)
        {

            int xi = (int)(x + seed) & 255;
            int yi = (int)(y + seed) & 255;
            int zi = (int)(z + seed) & 255;

            if (repeat > 0)
            {
                xi %= repeat;
                yi %= repeat;
                zi %= repeat;
            }

            float xf = x - (int)x;
            float yf = y - (int)y;


            float zf = z - (int)z;
            float u = Fade(xf);
            float v = Fade(yf);
            float w = Fade(zf);

            int aaa, aba, aab, abb, baa, bba, bab, bbb;
            aaa = p[p[p[xi] + yi] + zi];
            aba = p[p[p[xi] + Inc(yi, repeat)] + zi];
            aab = p[p[p[xi] + yi] + Inc(zi, repeat)];
            abb = p[p[p[xi] + Inc(yi, repeat)] + Inc(zi, repeat)];
            baa = p[p[p[Inc(xi, repeat)] + yi] + zi];
            bba = p[p[p[Inc(xi, repeat)] + Inc(yi, repeat)] + zi];
            bab = p[p[p[Inc(xi, repeat)] + yi] + Inc(zi, repeat)];
            bbb = p[p[p[Inc(xi, repeat)] + Inc(yi, repeat)] + Inc(zi, repeat)];

            float x1, x2, y1, y2;
            x1 = Lerp(Grad(aaa, xf, yf, zf), Grad(baa, xf - 1, yf, zf), u);
            x2 = Lerp(Grad(aba, xf, yf - 1, zf), Grad(bba, xf - 1, yf - 1, zf), u);
            y1 = Lerp(x1, x2, v);

            x1 = Lerp(Grad(aab, xf, yf, zf - 1), Grad(bab, xf - 1, yf, zf - 1), u);
            x2 = Lerp(Grad(abb, xf, yf - 1, zf - 1), Grad(bbb, xf - 1, yf - 1, zf - 1), u);
            y2 = Lerp(x1, x2, v);

            return Lerp(y1, y2, w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Grad(int hash, float x, float y, float z)
        {
            int h = hash & 15;
            float u = h < 8 /* 0b1000 */ ? x : y;
            float v;

            if (h < 4 /* 0b0100 */)
                v = y;
            else if (h == 12 /* 0b1100 */ || h == 14 /* 0b1110*/)
                v = x;
            else
                v = z;

            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Lerp(float a, float b, float x) => a + x * (b - a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Inc(int num, int repeat) => repeat > 0 ? (num + 1) % repeat : num + 1;

        static readonly int[] permutation = { 151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        static readonly int[] p;


    }
}