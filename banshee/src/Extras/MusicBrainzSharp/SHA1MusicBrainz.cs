// This is adapted from $Id: sha1.c 431 2001-03-06 00:12:16Z robert $

using System;

namespace MusicBrainzSharp
{
    internal class SHA1MusicBrainz
    {
        class TransformInfo
        {
            public uint A, B, C, D, E, T;
            public uint[] W = new uint[80];
            public int index;
        }

        const int SHA_BLOCKSIZE = 64;

        const uint CONST1 = 0x5a827999;
        const uint CONST2 = 0x6ed9eba1;
        const uint CONST3 = 0x8f1bbcdc;
        const uint CONST4 = 0xca62c1d6;

        delegate uint FFunction(uint x, uint y, uint z);
        static uint f1(uint x, uint y, uint z)
        {
            return (x & y) | (~x & z);
        }
        static uint f2(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }
        static uint f3(uint x, uint y, uint z)
        {
            return (x & y) | (x & z) | (y & z);
        }
        static uint f4(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }

        static uint R32(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }

        static void FA(uint constant, FFunction f, TransformInfo info)
        {
            info.T = R32(info.A, 5) + f(info.B, info.C, info.D) + info.E + info.W[info.index++] + constant;
            info.B = R32(info.B, 30);
        }
        static void FB(uint constant, FFunction f, TransformInfo info)
        {
            info.E = R32(info.T, 5) + f(info.A, info.B, info.C) + info.D + info.W[info.index++] + constant;
            info.A = R32(info.A, 30);
        }
        static void FC(uint constant, FFunction f, TransformInfo info)
        {
            info.D = R32(info.E, 5) + f(info.T, info.A, info.B) + info.C + info.W[info.index++] + constant;
            info.T = R32(info.T, 30);
        }
        static void FD(uint constant, FFunction f, TransformInfo info)
        {
            info.C = R32(info.D, 5) + f(info.E, info.T, info.A) + info.B + info.W[info.index++] + constant;
            info.E = R32(info.E, 30);
        }
        static void FE(uint constant, FFunction f, TransformInfo info)
        {
            info.B = R32(info.C, 5) + f(info.D, info.E, info.T) + info.A + info.W[info.index++] + constant;
            info.D = R32(info.D, 30);
        }
        static void FT(uint constant, FFunction f, TransformInfo info)
        {
            info.A = R32(info.B, 5) + f(info.C, info.D, info.E) + info.T + info.W[info.index++] + constant;
            info.C = R32(info.C, 30);
        }

        int local;
        uint count_lo, count_hi;
        uint[] digest;
        byte[] data;

        public SHA1MusicBrainz()
        {
            data = new byte[SHA_BLOCKSIZE];
            digest = new uint[5];
            digest[0] = 0x67452301;
            digest[1] = 0xefcdab89;
            digest[2] = 0x98badcfe;
            digest[3] = 0x10325476;
            digest[4] = 0xc3d2e1f0;
        }

        public void Update(string value)
        {
            Update(System.Text.Encoding.ASCII.GetBytes(value));
        }

        public void Update(byte[] buffer)
        {
            int i = 0;
            int buffer_index = 0;
            uint clo = count_lo + (uint)(buffer.Length << 3);
            if(clo < count_lo)
                count_hi++;
            count_lo = clo;
            count_hi += (uint)buffer.Length >> 29;
            if(local > 0) {
                i = SHA_BLOCKSIZE - local;
                if(i > buffer.Length)
                    i = buffer.Length;
                for(int j = 0; j < i; j++)
                    data[j + local] = buffer[j];
                local += i;
                buffer_index += i;
                if(local == SHA_BLOCKSIZE)
                    Transform();
                else
                    return;
            }
            int count = buffer.Length - i;
            while(count >= SHA_BLOCKSIZE) {
                for(int j = 0; j < SHA_BLOCKSIZE; j++)
                    data[j] = buffer[buffer_index + j];
                buffer_index += SHA_BLOCKSIZE;
                count -= SHA_BLOCKSIZE;
                Transform();
            }
            for(int j = 0; j < count; j++)
                data[j] = buffer[buffer_index + j];
            local = count;
        }

        public byte[] Final()
        {
            uint lo_bit_count = count_lo;
            uint hi_bit_count = count_hi;
            int count = (int)((lo_bit_count >> 3) & 0x3f);
            data[count++] = 0x80;
            if(count > SHA_BLOCKSIZE - 8) {
                for(int i = 0; i < SHA_BLOCKSIZE - count; i++)
                    data[i + count] = 0;
                Transform();
                for(int i = 0; i < SHA_BLOCKSIZE - 8; i++)
                    data[i] = 0;
            } else
                for(int i = 0; i < SHA_BLOCKSIZE - 8 - count; i++)
                    data[i + count] = 0;

            data[56] = (byte)((hi_bit_count >> 24) & 0xff);
            data[57] = (byte)((hi_bit_count >> 16) & 0xff);
            data[58] = (byte)((hi_bit_count >> 8) & 0xff);
            data[59] = (byte)((hi_bit_count >> 0) & 0xff);
            data[60] = (byte)((lo_bit_count >> 24) & 0xff);
            data[61] = (byte)((lo_bit_count >> 16) & 0xff);
            data[62] = (byte)((lo_bit_count >> 8) & 0xff);
            data[63] = (byte)((lo_bit_count >> 0) & 0xff);
            Transform();
            byte[] result = new byte[20];
            result[0] = (byte)((digest[0] >> 24) & 0xff);
            result[1] = (byte)((digest[0] >> 16) & 0xff);
            result[2] = (byte)((digest[0] >> 8) & 0xff);
            result[3] = (byte)((digest[0]) & 0xff);
            result[4] = (byte)((digest[1] >> 24) & 0xff);
            result[5] = (byte)((digest[1] >> 16) & 0xff);
            result[6] = (byte)((digest[1] >> 8) & 0xff);
            result[7] = (byte)((digest[1]) & 0xff);
            result[8] = (byte)((digest[2] >> 24) & 0xff);
            result[9] = (byte)((digest[2] >> 16) & 0xff);
            result[10] = (byte)((digest[2] >> 8) & 0xff);
            result[11] = (byte)((digest[2]) & 0xff);
            result[12] = (byte)((digest[3] >> 24) & 0xff);
            result[13] = (byte)((digest[3] >> 16) & 0xff);
            result[14] = (byte)((digest[3] >> 8) & 0xff);
            result[15] = (byte)((digest[3]) & 0xff);
            result[16] = (byte)((digest[4] >> 24) & 0xff);
            result[17] = (byte)((digest[4] >> 16) & 0xff);
            result[18] = (byte)((digest[4] >> 8) & 0xff);
            result[19] = (byte)((digest[4]) & 0xff);
            return result;
        }

        void Transform()
        {
            TransformInfo info = new TransformInfo();
            int data_index = 0;
            for(int i = 0; i < 16; ++i) {
                info.T = BitConverter.ToUInt32(data, data_index);
                data_index += 4;
                info.W[i] = ((info.T << 24) & 0xff000000) | ((info.T << 8) & 0x00ff0000) |
                    ((info.T >> 8) & 0x0000ff00) | ((info.T >> 24) & 0x000000ff);
            }
            for(int i = 16; i < 80; ++i) {
                info.W[i] = info.W[i - 3] ^ info.W[i - 8] ^ info.W[i - 14] ^ info.W[i - 16];
                info.W[i] = R32(info.W[i], 1);
            }

            info.A = digest[0];
            info.B = digest[1];
            info.C = digest[2];
            info.D = digest[3];
            info.E = digest[4];

            FA(CONST1, f1, info); FB(CONST1, f1, info); FC(CONST1, f1, info); FD(CONST1, f1, info);
            FE(CONST1, f1, info); FT(CONST1, f1, info); FA(CONST1, f1, info); FB(CONST1, f1, info);
            FC(CONST1, f1, info); FD(CONST1, f1, info); FE(CONST1, f1, info); FT(CONST1, f1, info);
            FA(CONST1, f1, info); FB(CONST1, f1, info); FC(CONST1, f1, info); FD(CONST1, f1, info);
            FE(CONST1, f1, info); FT(CONST1, f1, info); FA(CONST1, f1, info); FB(CONST1, f1, info);

            FC(CONST2, f2, info); FD(CONST2, f2, info); FE(CONST2, f2, info); FT(CONST2, f2, info);
            FA(CONST2, f2, info); FB(CONST2, f2, info); FC(CONST2, f2, info); FD(CONST2, f2, info);
            FE(CONST2, f2, info); FT(CONST2, f2, info); FA(CONST2, f2, info); FB(CONST2, f2, info);
            FC(CONST2, f2, info); FD(CONST2, f2, info); FE(CONST2, f2, info); FT(CONST2, f2, info);
            FA(CONST2, f2, info); FB(CONST2, f2, info); FC(CONST2, f2, info); FD(CONST2, f2, info);

            FE(CONST3, f3, info); FT(CONST3, f3, info); FA(CONST3, f3, info); FB(CONST3, f3, info);
            FC(CONST3, f3, info); FD(CONST3, f3, info); FE(CONST3, f3, info); FT(CONST3, f3, info);
            FA(CONST3, f3, info); FB(CONST3, f3, info); FC(CONST3, f3, info); FD(CONST3, f3, info);
            FE(CONST3, f3, info); FT(CONST3, f3, info); FA(CONST3, f3, info); FB(CONST3, f3, info);
            FC(CONST3, f3, info); FD(CONST3, f3, info); FE(CONST3, f3, info); FT(CONST3, f3, info);

            FA(CONST4, f4, info); FB(CONST4, f4, info); FC(CONST4, f4, info); FD(CONST4, f4, info);
            FE(CONST4, f4, info); FT(CONST4, f4, info); FA(CONST4, f4, info); FB(CONST4, f4, info);
            FC(CONST4, f4, info); FD(CONST4, f4, info); FE(CONST4, f4, info); FT(CONST4, f4, info);
            FA(CONST4, f4, info); FB(CONST4, f4, info); FC(CONST4, f4, info); FD(CONST4, f4, info);
            FE(CONST4, f4, info); FT(CONST4, f4, info); FA(CONST4, f4, info); FB(CONST4, f4, info);

            digest[0] = digest[0] + info.E;
            digest[1] = digest[1] + info.T;
            digest[2] = digest[2] + info.A;
            digest[3] = digest[3] + info.B;
            digest[4] = digest[4] + info.C;
        }
    }
}
