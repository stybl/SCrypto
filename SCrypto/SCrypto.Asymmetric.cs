﻿namespace SCrypto.Asymmetric
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Contains method that use RSA to perform asymmetric encryption/decryption.
    /// </summary>
    public static class RSA
    {
        /// <summary>
        /// Create an RSA public-private key pair.
        /// </summary>
        /// <returns>The generated public and private keys.</returns>
        public static Tuple<string, string> CreateKeyPair()
        {
            CspParameters cspParams = new CspParameters { ProviderType = 1 };

            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(1024, cspParams);

            string publicKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(false));
            string privateKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(true));

            return new Tuple<string, string>(publicKey, privateKey);
        }

        /// <summary>
        /// Encrypt the provided string using RSA.
        /// </summary>
        /// <param name="publicKey">The public key to encrypt the data with.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>Encrypted data.</returns>
        public static byte[] Encrypt(string publicKey, string data)
        {
            // User error checks
            if (string.IsNullOrWhiteSpace(publicKey))
            {
                throw new ArgumentException("Public key required!", "publicKey");
            }

            if (data == null || data == string.Empty)
            {
                throw new ArgumentException("Data required!", "data");
            }

            CspParameters cspParams = new CspParameters { ProviderType = 1 };
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);

            rsaProvider.ImportCspBlob(Convert.FromBase64String(publicKey));

            byte[] plainBytes = Encoding.UTF8.GetBytes(data);
            byte[] encryptedBytes = rsaProvider.Encrypt(plainBytes, false);

            return encryptedBytes;
        }

        /// <summary>
        /// Decrypt the provided data using RSA.
        /// </summary>
        /// <param name="privateKey">The key to be used for decryption.</param>
        /// <param name="encryptedBytes">The data to be decrypted.</param>
        /// <returns>Decrypted data.</returns>
        public static string Decrypt(string privateKey, byte[] encryptedBytes)
        {
            // User error checks
            if (string.IsNullOrWhiteSpace(privateKey))
            {
                throw new ArgumentException("Private key required!", "privateKey");
            }

            if (encryptedBytes == null || encryptedBytes.Length == 0)
            {
                throw new ArgumentException("Data required!", "encryptedBytes");
            }

            CspParameters cspParams = new CspParameters { ProviderType = 1 };
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);

            rsaProvider.ImportCspBlob(Convert.FromBase64String(privateKey));

            byte[] plainBytes = rsaProvider.Decrypt(encryptedBytes, false);

            string plainText = Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);

            return plainText;
        }
    }
}
