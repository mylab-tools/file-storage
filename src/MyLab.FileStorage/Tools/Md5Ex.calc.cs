﻿using System.Diagnostics.CodeAnalysis;

namespace MyLab.FileStorage.Tools;

public partial class Md5Ex
{
    [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "More compact this way")]
    static Md5Ex()
    {
        PADDING = new byte[64];
        PADDING[0] = 0x80;
    }
    private static void Md5Init(Md5Context context)  /* context */
    {
        context.Count[0] = context.Count[1] = 0;

        /* Load magic initialization constants. */
        context.State[0] = 0x67452301;
        context.State[1] = 0xefcdab89;
        context.State[2] = 0x98badcfe;
        context.State[3] = 0x10325476;
    }

    private const int S11 = 7;
    private const int S12 = 12;
    private const int S13 = 17;
    private const int S14 = 22;
    private const int S21 = 5;
    private const int S22 = 9;
    private const int S23 = 14;
    private const int S24 = 20;
    private const int S31 = 4;
    private const int S32 = 11;
    private const int S33 = 16;
    private const int S34 = 23;
    private const int S41 = 6;
    private const int S42 = 10;
    private const int S43 = 15;
    private const int S44 = 21;
    private static byte[] PADDING;

    private static uint F(uint x, uint y, uint z)
    {
        return (((x) & (y)) | ((~x) & (z)));
    }

    private static uint G(uint x, uint y, uint z)
    {
        return (((x) & (z)) | ((y) & (~z)));
    }

    private static uint H(uint x, uint y, uint z)
    {
        return ((x) ^ (y) ^ (z));
    }

    private static uint I(uint x, uint y, uint z)
    {
        return ((y) ^ ((x) | (~z)));
    }

    private static uint ROTATE_LEFT(uint x, int n)
    {
        return (((x) << (n)) | ((x) >> (32 - (n))));
    }

    private static void FF(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        (a) += F((b), (c), (d)) + (x) + (uint)(ac);
        (a) = ROTATE_LEFT((a), (s));
        (a) += (b);
    }

    private static void GG(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        (a) += G((b), (c), (d)) + (x) + (uint)(ac);
        (a) = ROTATE_LEFT((a), (s));
        (a) += (b);
    }

    private static void HH(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        (a) += H((b), (c), (d)) + (x) + (uint)(ac);
        (a) = ROTATE_LEFT((a), (s));
        (a) += (b);
    }

    private static void II(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        (a) += I((b), (c), (d)) + (x) + (uint)(ac);
        (a) = ROTATE_LEFT((a), (s));
        (a) += (b);
    }

    private static void Md5Update(Md5Context context, /* context */
        byte[] input, /* input block */
        uint inputIndex, // Starting index for input block
        uint inputLen) /* length of input block */
    {
        /* Compute number of bytes mod 64 */
        uint index = (uint)((context.Count[0] >> 3) & 0x3F);

        /* Update number of bits */
        if ((context.Count[0] += ((uint)inputLen << 3)) < ((uint)inputLen << 3))
        {
            context.Count[1]++;
        }

        context.Count[1] += ((uint)inputLen >> 29);

        uint partLen = 64 - index;

        /* Transform as many times as possible. */
        uint i = 0;
        if (inputLen >= partLen)
        {
            Buffer.BlockCopy(input, (int)inputIndex, context.Buffer, (int)index, (int)partLen);
            Md5Transform(context.State, context.Buffer, 0);

            for (i = partLen; i + 63 < inputLen; i += 64)
            {
                Md5Transform(context.State, input, inputIndex + i);
            }

            index = 0;
        }

        /* Buffer remaining input */
        Buffer.BlockCopy(input, (int)(inputIndex + i), context.Buffer, (int)index, (int)(inputLen - i));
    }

    private static void Md5Final(byte[] digest, /* message digest */
        Md5Context context) /* context */
    {
        byte[] bits = new byte[8];

        /* Save number of bits */
        Encode(bits, context.Count, 8);

        /* Pad out to 56 mod 64. */
        uint index = (uint)((context.Count[0] >> 3) & 0x3f);
        uint padLen = (index < 56) ? (56 - index) : (120 - index);
        Md5Update(context, PADDING, 0, padLen);

        /* Append length (before padding) */
        Md5Update(context, bits, 0, 8);

        /* Store state in digest */
        Encode(digest, context.State, 16);

        /* Zeroize sensitive information. */
        context.Clear();
    }

    private static void Md5Transform(uint[] state,
        byte[] block,
        uint blockIndex)
    {
        uint a = state[0], b = state[1], c = state[2], d = state[3];
        uint[] x = new uint[16];

        Decode(x, block, blockIndex, 64);

        /* Round 1 */
        FF(ref a, b, c, d, x[0], S11, 0xd76aa478); /* 1 */
        FF(ref d, a, b, c, x[1], S12, 0xe8c7b756); /* 2 */
        FF(ref c, d, a, b, x[2], S13, 0x242070db); /* 3 */
        FF(ref b, c, d, a, x[3], S14, 0xc1bdceee); /* 4 */
        FF(ref a, b, c, d, x[4], S11, 0xf57c0faf); /* 5 */
        FF(ref d, a, b, c, x[5], S12, 0x4787c62a); /* 6 */
        FF(ref c, d, a, b, x[6], S13, 0xa8304613); /* 7 */
        FF(ref b, c, d, a, x[7], S14, 0xfd469501); /* 8 */
        FF(ref a, b, c, d, x[8], S11, 0x698098d8); /* 9 */
        FF(ref d, a, b, c, x[9], S12, 0x8b44f7af); /* 10 */
        FF(ref c, d, a, b, x[10], S13, 0xffff5bb1); /* 11 */
        FF(ref b, c, d, a, x[11], S14, 0x895cd7be); /* 12 */
        FF(ref a, b, c, d, x[12], S11, 0x6b901122); /* 13 */
        FF(ref d, a, b, c, x[13], S12, 0xfd987193); /* 14 */
        FF(ref c, d, a, b, x[14], S13, 0xa679438e); /* 15 */
        FF(ref b, c, d, a, x[15], S14, 0x49b40821); /* 16 */

        /* Round 2 */
        GG(ref a, b, c, d, x[1], S21, 0xf61e2562); /* 17 */
        GG(ref d, a, b, c, x[6], S22, 0xc040b340); /* 18 */
        GG(ref c, d, a, b, x[11], S23, 0x265e5a51); /* 19 */
        GG(ref b, c, d, a, x[0], S24, 0xe9b6c7aa); /* 20 */
        GG(ref a, b, c, d, x[5], S21, 0xd62f105d); /* 21 */
        GG(ref d, a, b, c, x[10], S22, 0x02441453); /* 22 */
        GG(ref c, d, a, b, x[15], S23, 0xd8a1e681); /* 23 */
        GG(ref b, c, d, a, x[4], S24, 0xe7d3fbc8); /* 24 */
        GG(ref a, b, c, d, x[9], S21, 0x21e1cde6); /* 25 */
        GG(ref d, a, b, c, x[14], S22, 0xc33707d6); /* 26 */
        GG(ref c, d, a, b, x[3], S23, 0xf4d50d87); /* 27 */
        GG(ref b, c, d, a, x[8], S24, 0x455a14ed); /* 28 */
        GG(ref a, b, c, d, x[13], S21, 0xa9e3e905); /* 29 */
        GG(ref d, a, b, c, x[2], S22, 0xfcefa3f8); /* 30 */
        GG(ref c, d, a, b, x[7], S23, 0x676f02d9); /* 31 */
        GG(ref b, c, d, a, x[12], S24, 0x8d2a4c8a); /* 32 */

        /* Round 3 */
        HH(ref a, b, c, d, x[5], S31, 0xfffa3942); /* 33 */
        HH(ref d, a, b, c, x[8], S32, 0x8771f681); /* 34 */
        HH(ref c, d, a, b, x[11], S33, 0x6d9d6122); /* 35 */
        HH(ref b, c, d, a, x[14], S34, 0xfde5380c); /* 36 */
        HH(ref a, b, c, d, x[1], S31, 0xa4beea44); /* 37 */
        HH(ref d, a, b, c, x[4], S32, 0x4bdecfa9); /* 38 */
        HH(ref c, d, a, b, x[7], S33, 0xf6bb4b60); /* 39 */
        HH(ref b, c, d, a, x[10], S34, 0xbebfbc70); /* 40 */
        HH(ref a, b, c, d, x[13], S31, 0x289b7ec6); /* 41 */
        HH(ref d, a, b, c, x[0], S32, 0xeaa127fa); /* 42 */
        HH(ref c, d, a, b, x[3], S33, 0xd4ef3085); /* 43 */
        HH(ref b, c, d, a, x[6], S34, 0x04881d05); /* 44 */
        HH(ref a, b, c, d, x[9], S31, 0xd9d4d039); /* 45 */
        HH(ref d, a, b, c, x[12], S32, 0xe6db99e5); /* 46 */
        HH(ref c, d, a, b, x[15], S33, 0x1fa27cf8); /* 47 */
        HH(ref b, c, d, a, x[2], S34, 0xc4ac5665); /* 48 */

        /* Round 4 */
        II(ref a, b, c, d, x[0], S41, 0xf4292244); /* 49 */
        II(ref d, a, b, c, x[7], S42, 0x432aff97); /* 50 */
        II(ref c, d, a, b, x[14], S43, 0xab9423a7); /* 51 */
        II(ref b, c, d, a, x[5], S44, 0xfc93a039); /* 52 */
        II(ref a, b, c, d, x[12], S41, 0x655b59c3); /* 53 */
        II(ref d, a, b, c, x[3], S42, 0x8f0ccc92); /* 54 */
        II(ref c, d, a, b, x[10], S43, 0xffeff47d); /* 55 */
        II(ref b, c, d, a, x[1], S44, 0x85845dd1); /* 56 */
        II(ref a, b, c, d, x[8], S41, 0x6fa87e4f); /* 57 */
        II(ref d, a, b, c, x[15], S42, 0xfe2ce6e0); /* 58 */
        II(ref c, d, a, b, x[6], S43, 0xa3014314); /* 59 */
        II(ref b, c, d, a, x[13], S44, 0x4e0811a1); /* 60 */
        II(ref a, b, c, d, x[4], S41, 0xf7537e82); /* 61 */
        II(ref d, a, b, c, x[11], S42, 0xbd3af235); /* 62 */
        II(ref c, d, a, b, x[2], S43, 0x2ad7d2bb); /* 63 */
        II(ref b, c, d, a, x[9], S44, 0xeb86d391); /* 64 */

        state[0] += a;
        state[1] += b;
        state[2] += c;
        state[3] += d;

        /* Zeroize sensitive information. */
        Array.Clear(x, 0, x.Length);
    }

    private static void Encode(byte[] output,
        uint[] input,
        uint len)
    {
        for (uint i = 0, j = 0; j < len; i++, j += 4)
        {
            output[j] = (byte)(input[i] & 0xff);
            output[j + 1] = (byte)((input[i] >> 8) & 0xff);
            output[j + 2] = (byte)((input[i] >> 16) & 0xff);
            output[j + 3] = (byte)((input[i] >> 24) & 0xff);
        }
    }

    private static void Decode(uint[] output,
        byte[] input,
        uint inputIndex,
        uint len)
    {
        for (uint i = 0, j = 0; j < len; i++, j += 4)
        {
            output[i] = ((uint)input[inputIndex + j]) |
                        (((uint)input[inputIndex + j + 1]) << 8) |
                        (((uint)input[inputIndex + j + 2]) << 16) |
                        (((uint)input[inputIndex + j + 3]) << 24);
        }
    }
}