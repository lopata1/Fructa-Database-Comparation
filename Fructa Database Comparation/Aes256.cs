using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fructa_Database_Comparation
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    public class Aes256
    {
        readonly byte[] _key;

        public Aes256(byte[] key)
        {
            byte[] newKey = Enumerable.Repeat<byte>(0, 32).ToArray();
            key.CopyTo(newKey, 0);
            _key = newKey;
        }

        Aes CreateAes()
        {
            var aes = Aes.Create();

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 256; // AES256
            aes.BlockSize = 128;

            // Consumer defined symmetrical key
            aes.Key = _key;

            return aes;
        }

        public byte[] Encrypt(string plainText)
        {
            byte[] encryptedBytesWithIv = null;

            using (var aes = CreateAes())
            {
                // Random, unigue initialization vector every time
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (var wtr = new StreamWriter(cs))
                            {
                                wtr.Write(plainText);
                            }

                            var encyptedBytes = ms.ToArray();

                            // Since the IV in unique for every encryption attempt,
                            //  we'll store it as the first 16 bytes of the encrypted byte array.
                            encryptedBytesWithIv = aes.IV.Concat(encyptedBytes).ToArray();
                        }
                    }
                }
            }

            return encryptedBytesWithIv;
        }

        public string Decrypt(byte[] encryptedBytes)
        {
            string plainText = null;

            using (var aes = CreateAes())
            {
                // During encryption, we stored the unique IV in the first 16 bytes
                aes.IV = encryptedBytes.Take(16).ToArray();

                // The actual value to decrypt is the rest of the bytes after the stored IV.
                var encryptedValue = encryptedBytes.Skip(16).ToArray();

                using (var decryptor = aes.CreateDecryptor())
                {
                    using (var ms = new MemoryStream(encryptedValue))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var rdr = new StreamReader(cs))
                            {
                                plainText = rdr.ReadToEnd();
                            }
                        }
                    }
                }
            }

            return plainText;
        }
    }

}
