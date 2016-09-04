// Code From https://github.com/jbubriski/Encryptamajig/blob/master/src/Encryptamajig/Encryptamajig/AesEncryptamajig.cs
// Changes: 
// Support dotnet core
// Use Pbkdf2 instead of Rfc2898DeriveBytes

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MPServer
{
    /// <summary>
    /// A simple wrapper to the AesManaged class and the AES algorithm.
    /// Requires a securely stored key which should be a random string of characters that an attacker could never guess.
    /// Make sure to save the Key if you want to decrypt your data later!
    /// If you're using this with a Web app, put the key in the web.config and encrypt the web.config.
    /// </summary>
    public class AesEncryptamajig
    {
        private const int SaltSize = 128/8;

        /// <summary>
        /// Encrypts the plainText input using the given Key.
        /// A 128 bit random salt will be generated and prepended to the ciphertext before it is base64 encoded.
        /// </summary>
        /// <param name="plainText">The plain text to encrypt.</param>
        /// <param name="key">The plain text encryption key.</param>
        /// <returns>The salt and the ciphertext, Base64 encoded for convenience.</returns>
        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            // generate a 128-bit salt using a secure PRNG
            var saltBytes = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            // Derive a new Salt and IV from the Key
            var keyDerivationBytes =
                KeyDerivation.Pbkdf2(key, saltBytes, KeyDerivationPrf.HMACSHA512, 10000, (256 + 128)/8);
            var keyBytes = keyDerivationBytes.Take(256/8).ToArray();
            var ivBytes = keyDerivationBytes.Skip(256/8).ToArray();

            // Create an encryptor to perform the stream transform.
            // Create the streams used for encryption.
            using (var aesManaged = Aes.Create())
            using (var encryptor = aesManaged.CreateEncryptor(keyBytes, ivBytes))
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    // Send the data through the StreamWriter, through the CryptoStream, to the underlying MemoryStream
                    streamWriter.Write(plainText);
                }

                // Return the encrypted bytes from the memory stream, in Base64 form so we can send it right to a database (if we want).
                var cipherTextBytes = memoryStream.ToArray();
                Array.Resize(ref saltBytes, saltBytes.Length + cipherTextBytes.Length);
                Array.Copy(cipherTextBytes, 0, saltBytes, SaltSize, cipherTextBytes.Length);

                return Convert.ToBase64String(saltBytes);
            }
        }

        /// <summary>
        /// Decrypts the ciphertext using the Key.
        /// </summary>
        /// <param name="cipherText">The ciphertext to decrypt.</param>
        /// <param name="key">The plain text encryption key.</param>
        /// <returns>The decrypted text.</returns>
        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            // Extract the salt from our ciphertext
            var allTheBytes = Convert.FromBase64String(cipherText);
            var saltBytes = allTheBytes.Take(SaltSize).ToArray();
            var ciphertextBytes = allTheBytes.Skip(SaltSize).ToArray();

            var keyDerivationBytes =
                KeyDerivation.Pbkdf2(key, saltBytes, KeyDerivationPrf.HMACSHA512, 10000, (256 + 128)/8);
            var keyBytes = keyDerivationBytes.Take(256/8).ToArray();
            var ivBytes = keyDerivationBytes.Skip(256/8).ToArray();

            // Create a decrytor to perform the stream transform.
            // Create the streams used for decryption.
            // The default Cipher Mode is CBC and the Padding is PKCS7 which are both good
            using (var aesManaged = Aes.Create())
            using (var decryptor = aesManaged.CreateDecryptor(keyBytes, ivBytes))
            using (var memoryStream = new MemoryStream(ciphertextBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            using (var streamReader = new StreamReader(cryptoStream))
            {
                // Return the decrypted bytes from the decrypting stream.
                return streamReader.ReadToEnd();
            }
        }
    }
}