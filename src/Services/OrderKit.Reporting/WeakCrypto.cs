using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace OrderKit
{
    /// <summary>
    /// Security Code Scan fixture, compiled into OrderKit so the SCS analyzer sees it.
    /// The sinks in "Tool Triggering (Synthetic Data)/roslyn-sast" sit outside every
    /// project, so a build-driven analyzer never binds them and scs.sarif stays empty.
    /// These rules fire on the API call itself (no taint source needed):
    /// SCS0006 weak hash, SCS0010 weak cipher, SCS0013 ECB mode, SCS0005 weak random,
    /// SCS0004 certificate validation disabled. Test data only, not for production.
    /// </summary>
    public static class WeakCrypto
    {
        /// <summary>MD5 fingerprint. SCS0006 / CA5351.</summary>
        public static string Md5Hex(string payload)
        {
            using (MD5 md5 = MD5.Create())
            {
                return ToHex(md5.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty)));
            }
        }

        /// <summary>SHA-1 fingerprint. SCS0006 / CA5350.</summary>
        public static string Sha1Hex(string payload)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                return ToHex(sha1.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty)));
            }
        }

        /// <summary>DES in ECB mode. SCS0010 / SCS0013 / CA5351.</summary>
        public static byte[] DesEcbEncrypt(byte[] key, byte[] plain)
        {
            using (DES des = DES.Create())
            {
                des.Key = key;
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform enc = des.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                    {
                        cs.Write(plain, 0, plain.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>Session token from System.Random. SCS0005 / CA5394.</summary>
        public static string NewSessionToken(int seed)
        {
            Random rng = new Random(seed);
            byte[] buf = new byte[16];
            rng.NextBytes(buf);
            return ToHex(buf);
        }

        /// <summary>HTTP handler that accepts any server certificate. SCS0004 / CA5359.</summary>
        public static HttpClientHandler TrustAllHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;
            return handler;
        }

        private static string ToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
