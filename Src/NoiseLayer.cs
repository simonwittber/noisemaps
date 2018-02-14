using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using UnityEngine;

namespace DifferentMethods.NoiseMaps
{

    [CreateAssetMenu]
    public class NoiseLayer : ScriptableObject
    {

        public int seed;
        public bool forceOne = false;
        public bool forceZero = false;
        public FractalType type;
        public int repeat = 0;
        public Vector3 offset;
        [Range(0, 32)]
        public float frequency = 1;
        [Range(1, 8)]
        public int octaves = 4;
        [Range(0, 32)]
        public float lacunarity = 2;
        [Range(-2, 2)]
        public float persistence = 0.5f;
        [Space]
        public int terraceSteps = 0;
        public bool absolute = false;
        public bool enableBandpass = true;
        [Range(0, 1)]
        public float bandCenter = 0;
        [Range(0, 1)]
        public float bandWidth = 1;
        [Range(0, 7)]
        public float contrast = 1;
        [Range(-1, 1)]
        public float bias = 0;
        public bool invert = false;


        public int size = 1024;

        public int Size
        {
            get
            {
                if (Application.isEditor && !Application.isPlaying)
                    return 128;
                return size;
            }
        }

        public static implicit operator Texture2D(NoiseLayer nl)
        {
            return nl.Texture;
        }

        public static implicit operator Texture(NoiseLayer nl)
        {
            return nl.Texture;
        }

        public Texture2D Texture { get; private set; }

        Color[] pixels;
        int lastSize = -1;

        public void OnEnable()
        {
            Refresh();
        }

        public void OnDisable()
        {
            DestroyImmediate(Texture);
            Texture = null;
            pixels = null;
        }

        public void Refresh()
        {
            if (lastSize != Size || Texture == null || pixels == null)
            {
                lastSize = Size;
                Texture = new Texture2D(Size, Size, TextureFormat.ARGB32, mipmap: false);
                pixels = new Color[Size * Size];
            }
            GenerateTexture(pixels, Size, Size);
            Texture.SetPixels(pixels);
            Texture.Apply();
        }

        public float Sample(float x, float y)
        {
            return Sample(x, y, 0);
        }

        public float Sample(float x, float y, float z)
        {
            if (forceOne) return 1;
            if (forceZero) return 0;
            x += offset.x;
            y += offset.y;
            z += offset.z;
            var pos = offset;
            pos.x += x;
            pos.y += y;
            pos.z += z;

            var N = Noise(pos.x, pos.y, pos.z);
            //make between 0 and 1
            N = (N + 1) * 0.5f;
            //quantize to discrete steps
            if (terraceSteps > 0)
                N = ((int)(N * (float)terraceSteps)) / (float)terraceSteps;
            //make between -1 and 1
            N = N * 2 - 1;
            //make all negative values positive
            if (absolute)
            {
                N = Mathf.Abs(N);
            }
            if (enableBandpass)
            {
                //make between 0 and 1
                N = (N + 1) * 0.5f;
                N = CubicPulse(bandCenter, bandWidth, N);
                //make between -1 and 1
                N = N * 2 - 1;
            }
            N *= contrast;
            //make between 0 and 1
            N = (N + 1) * 0.5f;
            N = bias + N;
            if (invert) N = 1 - N;
            return N;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float CubicPulse(float c, float w, float x)
        {
            x = Mathf.Abs(x - c);
            if (x > w) return 0.0f;
            x /= w;
            return 1.0f - x * x * (3.0f - 2.0f * x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Noise(float x, float y, float z)
        {
            switch (type)
            {
                case FractalType.Billow: return BillowNoise(x, y, z);
                case FractalType.BrownianMotion: return BrownianMotionNoise(x, y, z);
                case FractalType.Ridge: return RidgeNoise(x, y, z);
                case FractalType.MultiFractal: return MultiFractalNoise(x, y, z);
            }
            throw new System.NotImplementedException();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float RidgeNoise(float x, float y, float z)
        {
            var F = frequency;
            var A = 1f;
            var N = 1f - Mathf.Abs(ImprovedNoise.Sample(x * F, y * F, z * F, seed: seed, repeat: repeat));
            N *= N;
            var s = N;
            for (var i = 1; i < octaves; i++)
            {
                var weight = Mathf.Clamp01(s * 2);
                F *= lacunarity;
                A *= persistence;
                s = 1f - Mathf.Abs(ImprovedNoise.Sample(x * F, y * F, z * F, seed: seed + i, repeat: repeat));
                s *= s;
                s *= weight;
                N += s * A;
            }
            //at this point N is between 0 and 1, so we make it -1, 1
            N = N * 2 - 1;
            return N;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float BrownianMotionNoise(float x, float y, float z)
        {
            var N = 0f;
            var F = frequency;
            var A = 1f;
            for (var i = 0; i < octaves; i++)
            {
                var n = ImprovedNoise.Sample(x * F, y * F, z * F, seed: seed + i, repeat: repeat);
                N += (n * A);
                A *= persistence;
                F *= lacunarity;
            }
            return N;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float MultiFractalNoise(float x, float y, float z)
        {
            var N = 1f;
            var F = frequency;
            var A = 1f;
            for (var i = 0; i < octaves; i++)
            {
                var n = -1 + ImprovedNoise.Sample(x * F, y * F, z * F, seed: seed + i, repeat: repeat);
                N *= (n * A);
                A *= persistence;
                F *= lacunarity;
            }
            return N;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float BillowNoise(float x, float y, float z)
        {
            var N = 0f;
            var F = frequency;
            var A = 1f;
            for (var i = 0; i < octaves; i++)
            {
                var s = ImprovedNoise.Sample(x * F, y * F, z * F, seed: seed + i, repeat: repeat);
                s *= A;
                s = Mathf.Abs(s);
                N += (s * lacunarity) + bias;
                A *= persistence;
                F *= lacunarity;
            }
            //at this point N is between 0 and 1, so we make it -1, 1
            N = N * 2 - 1;
            return N;
        }

        public void GenerateTexture(Color[] pixels, int width, int height)
        {
            if (pixels == null) return;

            Parallel.For(0, height, y =>
            {
                for (var x = 0; x < width; x++)
                {
                    pixels[y * width + x] = Color.white * Sample(1f * x / width, 1f * y / height);
                    pixels[y * width + x].a = 1;
                }
            });
        }

        public void GenerateTexture(Color[] pixels, int width, int height, int depth)
        {

            if (pixels == null) return;
            Parallel.For(0, depth, z =>
            {
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var index = z * depth * height + y * width + x;
                        pixels[index] = Color.white * Sample(1f * x / width, 1f * y / height, 1f * z / depth);
                        pixels[index].a = 1;
                    }
                }
            });
        }




    }
}