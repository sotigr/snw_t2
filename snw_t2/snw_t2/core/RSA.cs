using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace snw.core
{
    public class RSA
    {
        private BigInteger p, q, n, phi, e, d;
        private bool use_padding = false;
        private int prime_length = 128;
        private bool keys_generated = false;
        public RSA(int key_bit_lenth, bool oaep_padding = false, bool generate_keys_now = true)
        {
            prime_length = key_bit_lenth / 2;
            use_padding = oaep_padding;
            if (generate_keys_now)
                GenerateKeys();
        }
        public void GenerateKeys()
        {
            p = GenerateRandomPrime(prime_length);
            q = GenerateRandomPrime(prime_length);

            n = p * q;

            //This detects propable errors in big prime selection.
            if (n.ToByteArray().Length * 8 != prime_length * 2)
                GenerateKeys();

            phi = (p - 1) * (q - 1);

            do
            {
                e = GenerateRandomCoprime(phi);
                d = ExtendedEuclidean(e % phi, phi).u1;
            } while (d < 0);
            keys_generated = true;
        }

        public string[] Encrypt(byte[] message)
        {
            if (keys_generated)
            {
                if (use_padding)
                    message = ApplyOAEP(message, "SHA-256 MGF1", 32 + 32 + 1);

                int pt_ln = message.Length;
                List<string> res = new List<string>();

                List<string> res_ts_0 = new List<string>();
                List<string> res_ts_1 = new List<string>();
                List<string> res_ts_2 = new List<string>();
                List<string> res_ts_3 = new List<string>();

                int task_share = pt_ln / 4;

                int task0_cycles = task_share;
                int task1_cycles = task_share * 2;
                int task2_cycles = task_share * 3;
                int task3_cycles = pt_ln;

                Task t0 = Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < task0_cycles; i++)
                    {
                        res_ts_0.Add(BigInteger.ModPow(message[i], e, n).ToString());
                    }
                });
                Task t1 = Task.Factory.StartNew(() =>
                {
                    for (int i = task0_cycles; i < task1_cycles; i++)
                    {
                        res_ts_1.Add(BigInteger.ModPow(message[i], e, n).ToString());
                    }
                });
                Task t2 = Task.Factory.StartNew(() =>
                {
                    for (int i = task1_cycles; i < task2_cycles; i++)
                    {
                        res_ts_2.Add(BigInteger.ModPow(message[i], e, n).ToString());
                    }
                });
                Task t3 = Task.Factory.StartNew(() =>
                {
                    for (int i = task2_cycles; i < task3_cycles; i++)
                    {
                        res_ts_3.Add(BigInteger.ModPow(message[i], e, n).ToString());
                    }
                });

                t0.Wait();
                t1.Wait();
                t2.Wait();
                t3.Wait();

                res.AddRange(res_ts_0);
                res.AddRange(res_ts_1);
                res.AddRange(res_ts_2);
                res.AddRange(res_ts_3);

                return res.ToArray();
            }
            else
                throw new System.InvalidOperationException("RSA - The public and private keys have not been generated yet.");
        }
        public byte[] Decrypt(string[] message)
        {
            if (keys_generated)
            {
                int pt_ln = message.Length;
                List<byte> res = new List<byte>();

                List<byte> res_ts_0 = new List<byte>();
                List<byte> res_ts_1 = new List<byte>();
                List<byte> res_ts_2 = new List<byte>();
                List<byte> res_ts_3 = new List<byte>();

                int task_share = pt_ln / 4;

                int task0_cycles = task_share;
                int task1_cycles = task_share * 2;
                int task2_cycles = task_share * 3;
                int task3_cycles = pt_ln;

                Task t0 = Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < task0_cycles; i++)
                    {
                        res_ts_0.Add((byte)BigInteger.ModPow(BigInteger.Parse(message[i].Trim()), d, n));
                    }
                });
                Task t1 = Task.Factory.StartNew(() =>
                {
                    for (int i = task0_cycles; i < task1_cycles; i++)
                    {
                        res_ts_1.Add((byte)BigInteger.ModPow(BigInteger.Parse(message[i].Trim()), d, n));
                    }
                });
                Task t2 = Task.Factory.StartNew(() =>
                {
                    for (int i = task1_cycles; i < task2_cycles; i++)
                    {
                        res_ts_2.Add((byte)BigInteger.ModPow(BigInteger.Parse(message[i].Trim()), d, n));
                    }
                });
                Task t3 = Task.Factory.StartNew(() =>
                {
                    for (int i = task2_cycles; i < task3_cycles; i++)
                    {
                        res_ts_3.Add((byte)BigInteger.ModPow(BigInteger.Parse(message[i].Trim()), d, n));
                    }
                });

                t0.Wait();
                t1.Wait();
                t2.Wait();
                t3.Wait();

                res.AddRange(res_ts_0);
                res.AddRange(res_ts_1);
                res.AddRange(res_ts_2);
                res.AddRange(res_ts_3);

                if (use_padding)
                    return RemoveOAEP(res.ToArray(), "SHA-256 MGF1");
                else
                    return res.ToArray();
            }
            else
                throw new System.InvalidOperationException("RSA - The public and private keys have not been generated yet.");
        }

        public rsa_component.PublicKey GetPublicKey()
        {
            if (keys_generated)
            return new rsa_component.PublicKey { e = e.ToString(), n = n.ToString() };
            throw new System.InvalidOperationException("RSA - The public key has not been generated yet.");
        }
        public rsa_component.PrivateKey GetPrivateKey()
        {
            if (keys_generated)
            return new rsa_component.PrivateKey { d = d.ToString(), n = n.ToString() };
            throw new System.InvalidOperationException("RSA - The private key has not been generated yet.");
        }
        public rsa_component.PrimeSet GetPrimeSet()
        {
            if (keys_generated)
            return new rsa_component.PrimeSet { p = p.ToString(), q = q.ToString() };
            throw new System.InvalidOperationException("RSA - The prime set has not been generated yet.");
        }
        private BigInteger GetGCDByModulus(BigInteger value1, BigInteger value2)
        {
            while (value1 != 0 && value2 != 0)
            {
                if (value1 > value2)
                    value1 %= value2;
                else
                    value2 %= value1;
            }
            return BigInteger.Max(value1, value2);
        }
        private bool Coprime(BigInteger value1, BigInteger value2)
        {
            return GetGCDByModulus(value1, value2).IsOne;
        }
        private BigInteger GenerateRandomCoprime(BigInteger number)
        {
            bool found = false;
            BigInteger resault = BigInteger.Zero;
            while (!found)
            {
                resault = GenerateRandomPrime(prime_length - 1, 10);
                if (Coprime(number, resault))
                    found = true;
            }
            return resault;
        }

        private BigInteger GenerateRandomPrime(int length, int witnesses = 10, int tasks = 6)
        {
            bool flag = false;
            BigInteger num = BigInteger.Zero;
            while (!flag)
            {
                List<Task> tl = new List<Task>();
                for (int i = 0; i < tasks; i++)
                {
                    tl.Add(Task.Factory.StartNew(() =>
                    {
                        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                        byte[] bytes = new byte[length / 8];
                        rng.GetBytes(bytes);

                        BigInteger p = new BigInteger(bytes);

                        bool isprime = p.IsProbablyPrime(witnesses);
                        if (isprime)
                        {
                            num = p;
                            flag = true;
                        }

                    }));
                }
                for (int i = 0; i < tasks - 1; i++)
                {
                    tl[i].Wait();
                }

                tl.Clear();
            }
            return num;
        }
        static ExtendedEuclideanResult ExtendedEuclidean(BigInteger a, BigInteger b)
        {
            BigInteger x0 = 1, xn = 1;
            BigInteger y0 = 0, yn = 0;
            BigInteger x1 = 0;
            BigInteger y1 = 1;
            BigInteger q;
            BigInteger r = a % b;

            while (r > 0)
            {
                q = a / b;
                xn = x0 - q * x1;
                yn = y0 - q * y1;

                x0 = x1;
                y0 = y1;
                x1 = xn;
                y1 = yn;
                a = b;
                b = r;
                r = a % b;
            }

            return new ExtendedEuclideanResult()
            {
                u1 = xn,
                u2 = yn,
                gcd = b
            };
        }

        struct ExtendedEuclideanResult
        {
            public BigInteger u1;
            public BigInteger u2;
            public BigInteger gcd;
        }

        static byte[] SHA256(byte[] input)
        {
            return  SHA1.Create().ComputeHash(input);
        }
        static byte[] MGF1(byte[] seed, int seedOffset, int seedLength, int desiredLength)
        {
            int hLen = 20;
            int offset = 0;
            int i = 0;
            byte[] mask = new byte[desiredLength];
            byte[] temp = new byte[seedLength + 4];
            Array.Copy(seed, seedOffset, temp, 4, seedLength);
            while (offset < desiredLength)
            {
                temp[0] = (byte)(i >> 24);
                temp[1] = (byte)(i >> 16);
                temp[2] = (byte)(i >> 8);
                temp[3] = (byte)i;
                int remaining = desiredLength - offset;
                Array.Copy(SHA256(temp), 0, mask, offset, remaining < hLen ? remaining : hLen);
                offset = offset + hLen;
                i = i + 1;
            }
            return mask;
        }
        private byte[] ApplyOAEP(byte[] message, String parameters, int length)
        {
            Random random = new Random();
            String[] tokens = parameters.Split(' ');
            if (tokens.Length != 2 || tokens[0] != ("SHA-256") || tokens[1] != ("MGF1"))
            {
                return null;
            }
            int mLen = message.Length;
            int hLen = 20;
            if (mLen > length - (hLen << 1) - 1)
            {
                return null;
            }
            int zeroPad = length - mLen - (hLen << 1) - 1;
            byte[] dataBlock = new byte[length - hLen];
            Array.Copy(SHA256(Encoding.UTF8.GetBytes(parameters)), 0, dataBlock, 0, hLen);
            Array.Copy(message, 0, dataBlock, hLen + zeroPad + 1, mLen);
            dataBlock[hLen + zeroPad] = 1;
            byte[] seed = new byte[hLen];
            random.NextBytes(seed);
            byte[] dataBlockMask = MGF1(seed, 0, hLen, length - hLen);
            for (int i = 0; i < length - hLen; i++)
            {
                dataBlock[i] ^= dataBlockMask[i];
            }
            byte[] seedMask = MGF1(dataBlock, 0, length - hLen, hLen);
            for (int i = 0; i < hLen; i++)
            {
                seed[i] ^= seedMask[i];
            }
            byte[] padded = new byte[length];
            Array.Copy(seed, 0, padded, 0, hLen);
            Array.Copy(dataBlock, 0, padded, hLen, length - hLen);
            return padded;
        }

        private byte[] RemoveOAEP(byte[] message, string parameters)
        {
            string[] tokens = parameters.Split(' ');
            if (tokens.Length != 2 || tokens[0] != ("SHA-256") || tokens[1] != ("MGF1"))
            {
                return null;
            }
            int mLen = message.Length;
            int hLen = 20;
            if (mLen < (hLen << 1) + 1)
            {
                return null;
            }
            byte[] copy = new byte[mLen];
            Array.Copy(message, 0, copy, 0, mLen);
            byte[] seedMask = MGF1(copy, hLen, mLen - hLen, hLen);
            for (int i = 0; i < hLen; i++)
            {
                copy[i] ^= seedMask[i];
            }
            byte[] paramsHash = SHA256(Encoding.UTF8.GetBytes(parameters));
            byte[] dataBlockMask = MGF1(copy, 0, hLen, mLen - hLen);
            int index = -1;
            for (int i = hLen; i < mLen; i++)
            {
                copy[i] ^= dataBlockMask[i - hLen];
                if (i < (hLen << 1))
                {
                    if (copy[i] != paramsHash[i - hLen])
                    {
                        return null;
                    }
                }
                else if (index == -1)
                {
                    if (copy[i] == 1)
                    {
                        index = i + 1;
                    }
                }
            }
            if (index == -1 || index == mLen)
            {
                return null;
            }
            byte[] unpadded = new byte[mLen - index];
            Array.Copy(copy, index, unpadded, 0, mLen - index);
            return unpadded;
        }
    }
    public static class PrimeExtensions
    {
        private static ThreadLocal<Random> s_Gen = new ThreadLocal<Random>(
          () =>
          {
              return new Random();
          }
        );

        private static Random Gen
        {
            get
            {
                return s_Gen.Value;
            }
        }

        public static Boolean IsProbablyPrime(this BigInteger value, int witnesses = 10)
        {
            if (value <= 1)
                return false;

            if (witnesses <= 0)
                witnesses = 10;

            BigInteger d = value - 1;
            int s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }

            Byte[] bytes = new Byte[value.ToByteArray().LongLength];
            BigInteger a;

            for (int i = 0; i < witnesses; i++)
            {
                do
                {
                    Gen.NextBytes(bytes);

                    a = new BigInteger(bytes);
                }
                while (a < 2 || a >= value - 2);

                BigInteger x = BigInteger.ModPow(a, d, value);
                if (x == 1 || x == value - 1)
                    continue;

                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, value);

                    if (x == 1)
                        return false;
                    if (x == value - 1)
                        break;
                }

                if (x != value - 1)
                    return false;
            }

            return true;
        }

    }
}