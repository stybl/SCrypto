﻿namespace SCrypto.PGP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of the PGP algorithm that utilizes RSA and AES-256.
    /// </summary>
    public class PGP
    {
        /// <summary>
        /// The length of all generated session keys.
        /// </summary>
        public const int SessionKeyLength = 64;

        /// <summary>
        /// Initializes a new instance of the <see cref="PGP"/> class.
        /// </summary>
        public PGP()
        {
            this.CreateKeyPair();
            this.SessionKey = null;
            this.EncryptedSessionKey = null;
            this.ClearText = null;
            this.CipherText = null;
            this.RecipientPublicKey = null;
        }

        /// <summary>
        /// Gets the public key (generated).
        /// </summary>
        public string PublicKey { get; private set; }

        /// <summary>
        /// Gets the private key (generated).
        /// </summary>
        public string PrivateKey { get; private set; }

        /// <summary>
        /// Gets the session key (generated).
        /// </summary>
        public string SessionKey { get; private set; }

        /// <summary>
        /// Gets the session key, encrypted with the recipient's public key.
        /// </summary>
        public byte[] EncryptedSessionKey { get; private set; }

        /// <summary>
        /// Gets the unencrypted text (can be provided by the user or through decryption of cipher text).
        /// </summary>
        public string ClearText { get; private set; }

        /// <summary>
        /// Gets the encrypted text (can be provided by the user or through encryption of clear text).
        /// </summary>
        public string CipherText { get; private set; }

        /// <summary>
        /// Gets or sets the recipient's public key (used to encrypt the session key).
        /// </summary>
        public string RecipientPublicKey { get; set; }

        /// <summary>
        /// Encrypt the given text using a generated session key, which is in turn encrypted using the provided public key.
        /// </summary>
        /// <param name="text">The text to be encrypted.</param>
        /// <param name="recipientPublicKey">The key that will be used to encrypt the session key.</param>
        /// <returns>The encrypted text.</returns>
        public string Encrypt(string text, string recipientPublicKey)
        {
            this.RecipientPublicKey = recipientPublicKey;
            this.ClearText = text;
            this.EncryptClearText();
            return this.CipherText;
        }

        /// <summary>
        /// Decrypt the given session key using own private key, then use it to decrypt given cipher text.
        /// </summary>
        /// <param name="text">The cipher text to be decrypted.</param>
        /// <param name="encryptedSessionkey">The (encrypted) session key.</param>
        /// <returns>The decrypted text.</returns>
        public string Decrypt(string text, byte[] encryptedSessionkey)
        {
            this.EncryptedSessionKey = encryptedSessionkey;
            this.CipherText = text;
            this.DecryptCipherText();
            return this.ClearText;
        }

        /// <summary>
        /// Generate a session key, use it to encrypt the ClearText and then encrypt it using the RecipientPublicKey.
        /// </summary>
        private void EncryptClearText()
        {
            this.CreateSessionKey();
            this.EncryptedSessionKey = SCrypto.Asymmetric.RSA.Encrypt(this.RecipientPublicKey, this.SessionKey);
            this.CipherText = SCrypto.Symmetric.AESThenHMAC.SimpleEncryptWithPassword(this.ClearText, this.SessionKey, this.EncryptedSessionKey);
        }

        /// <summary>
        /// Decrypt the EncryptedSessionKey and use it to decrypt the CipherText.
        /// </summary>
        private void DecryptCipherText()
        {
            this.SessionKey = SCrypto.Asymmetric.RSA.Decrypt(this.PrivateKey, this.EncryptedSessionKey);
            this.ClearText = SCrypto.Symmetric.AESThenHMAC.SimpleDecryptWithPassword(this.CipherText, this.SessionKey);
        }

        /// <summary>
        /// Generate an RSA public-private key pair.
        /// </summary>
        private void CreateKeyPair()
        {
            var keys = SCrypto.Asymmetric.RSA.CreateKeyPair();
            this.PublicKey = keys.Item1;
            this.PrivateKey = keys.Item2;
        }

        /// <summary>
        /// Securely generate a random session key.
        /// </summary>
        private void CreateSessionKey()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[256];
                rng.GetBytes(tokenData);

                this.SessionKey = SCrypto.Hash.SHA_256.GetDigest(Convert.ToBase64String(tokenData));
            }
        }
    }
}
