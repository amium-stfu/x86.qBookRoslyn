using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Amium.Helpers
{
    public class License2
    {


        static int CalcChecksum(string text)
        {
            int cs = 0x4711;
            foreach (char c in text)
            {
                cs += (int)((byte)(c) * (byte)(c) * Math.Atan((byte)c / 3.14) * Math.Sin((byte)c / 3.14) * 100);
                cs &= 0xFFFF;
            }
            return cs;
        }

        public static string AddChecksum(string text)
        {
            string cs = CalcChecksum(text).ToString("X4");
            //string newText = text.Insert(3, cs[0].ToString()).Insert(7, cs[1].ToString()).Insert(9, cs[2].ToString()).Insert(text.Length + 3, cs[3].ToString());
            string newText = text.Insert(3, cs[2].ToString()).Insert(5, cs[0].ToString()).Insert(6, cs[3].ToString()).Insert(text.Length + 3, cs[1].ToString());
            return newText;
        }

        public static string VerifyAndStripLicenseChecksum(string text)
        {
            //this code MUST by copy'n'pasted to Xero/
            char c0 = text[5];
            char c1 = text[text.Length - 1];
            char c2 = text[3];
            char c3 = text[6];

            string newText = text.Remove(text.Length - 1, 1).Remove(6, 1).Remove(5, 1).Remove(3, 1);
            string cs = CalcChecksum(newText).ToString("X4");

            if ("" + c0 + c1 + c2 + c3 != cs)
            {
                //bad checksum!
                return null;
            }

            return newText;
        }

        public static string GetHardwareId()
        {
            string usbSerial = HardwareInfo.GetUsbDiskSerial();
            if (usbSerial != null)
            {
                //Xero started from USB drive?! use the Stick's serial as the hardware id
                return usbSerial;
            }
            else
            {
                return ""
                    + HardwareInfo.GetBoardSerial().Replace(" ", "")
                    + HardwareInfo.GetProcessorId();
            }
        }

        public static string GetSalt()
        {
            //return GetHardwareId() + "Am!um2020#";
            return HardwareInfo.GetDefaultHardwareId().Split(',')[0] + "Am!um2020#";
        }

        static Rijndael rij = new Rijndael();
        public static string Encrypt(string text)
        {
            return rij.Encrypt(text, GetSalt());
        }

        public static string Decrypt(string text)
        {
            return rij.Decrypt(text, GetSalt());
        }


        string AlphaNumTable = "XHMWZNQ781DVOTKE0C3Y4R6GJSPU9L52FIBA";
        private string CipherAndReorder(string text)
        {
            text = text.ToUpper();
            text = text.Length.ToString("X2") + text;
            text = text.PadRight(7, '0');
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                int index;
                if (char.IsDigit(c))
                    sb.Append(AlphaNumTable[(byte)c - 48 + 26]);
                else if (char.IsLetter(c))
                    sb.Append(AlphaNumTable[(byte)c - 65]);
                else
                    sb.Append(c);
            }

            //reorder

            StringBuilder sb2 = new StringBuilder();
            string s1 = sb.ToString();
            s1 = s1.PadRight((int)(((s1.Length / 7) + 1) * 7), '0');
            int len = s1.Length;
            for (int i = 0; i < len; i += 7)
            {
                if (i + 4 < len) sb2.Append(s1[i + 4]);
                if (i + 1 < len) sb2.Append(s1[i + 1]);
                if (i + 0 < len) sb2.Append(s1[i + 0]);
                if (i + 2 < len) sb2.Append(s1[i + 2]);
                if (i + 6 < len) sb2.Append(s1[i + 6]);
                if (i + 3 < len) sb2.Append(s1[i + 3]);
                if (i + 5 < len) sb2.Append(s1[i + 5]);
            }

            return sb2.ToString();
        }

        private string DecipherAndReorder(string text)
        {
            text = text.ToUpper();
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                int index = AlphaNumTable.IndexOf(c);
                if (index >= 0 && index < 26)
                    sb.Append((char)(index + 65));
                else if (index >= 26 && index < 36)
                    sb.Append((char)(index - 26 + 48));
                else
                    sb.Append(c);
            }

            //reorder
            StringBuilder sb2 = new StringBuilder();
            string s1 = sb.ToString();
            int len = s1.Length;
            int origLen = -1;
            for (int i = 0; i < len; i += 7)
            {
                if (i + 2 < len) sb2.Append(s1[i + 2]);
                if (i + 1 < len) sb2.Append(s1[i + 1]);
                if (i + 3 < len) sb2.Append(s1[i + 3]);
                if (i + 5 < len) sb2.Append(s1[i + 5]);
                if (i + 0 < len) sb2.Append(s1[i + 0]);
                if (i + 6 < len) sb2.Append(s1[i + 6]);
                if (i + 4 < len) sb2.Append(s1[i + 4]);

                if (i == 0) //first
                {
                    origLen = Convert.ToInt16(sb2.ToString().Substring(0, 2), 16);
                }
            }

            string s2 = sb2.ToString();
            if (origLen >= 0)
                return s2.Substring(2, origLen);
            else
                return s2.Substring(2);
        }


        static string HardwareId = null;
        static string XeroLicense2Type = null; //PC or USB:X
        public static string GetLicense(string filename, out string source)
        {
            string licenseStr = null;
            source = null;
            try
            {
                string enc = System.IO.File.ReadAllText(filename);
                string hardwareIdEx = Amium.Helpers.HardwareInfo.GetDefaultHardwareId();

                //System.Windows.Forms.MessageBox.Show("enc:\r\n" + enc + "\r\n\r\nhw:\r\n" + hardwareIdEx, "debug");
                HardwareId = hardwareIdEx.Split(',')[0];
                if (hardwareIdEx.Split(',').Length > 1)
                    source = hardwareIdEx.Split(',')[1];

                string text = Amium.Helpers.License2.Decrypt(enc);
                //textBoxText.Text = text;
                //System.Windows.Forms.MessageBox.Show("text:\r\n" + text, "debug");
                string license = Amium.Helpers.License2.VerifyAndStripLicenseChecksum(text);
                //System.Windows.Forms.MessageBox.Show("lic:\r\n" + license, "debug");
                if (license == null)
                {
                    //#ERR: bad checksum
                    //textBoxLicense.Text = "#ERR: bad checksum";
                }
                else
                {
                    //OK
                    licenseStr = license;
                    //textBoxLicense.Text = license;
                }
            }
            catch (Exception ex)
            {
                licenseStr = null;
            }

            return licenseStr;
        }


        //public static void GetLicenseAsync(string filename, out string license, out string source)
        //{

        //}

    }


    public class Rijndael
    {
        #region Consts
        /// <summary>
        /// Change this Inputkey GUID with a new GUID when you use this code in your own program !!!
        /// Keep this inputkey very safe and prevent someone from decoding it some way !!!
        /// </summary>
        internal const string Inputkey = "560A18CD-6346-4CF0-A2E8-671F9B6B9EA9";
        #endregion

        #region Encryption
        /// <summary>
        /// Encrypt the given text and give the byte array back as a BASE64 string
        /// </summary>
        /// <param name="text">The text to encrypt</param>
        /// <param name="salt">The pasword salt</param>
        /// <returns>The encrypted text</returns>
        public string Encrypt(string text, string salt)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            var aesAlg = NewRijndaelManaged(salt);

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            var msEncrypt = new MemoryStream();

            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
                swEncrypt.Write(text);

            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        #endregion

        #region Decrypt
        /// <summary>
        /// Checks if a string is base64 encoded
        /// </summary>
        /// <param name="base64String">The base64 encoded string</param>
        /// <returns></returns>
        private static bool IsBase64String(string base64String)
        {
            base64String = base64String.Trim();

            return (base64String.Length % 4 == 0) &&
                    Regex.IsMatch(base64String, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        /// <summary>
        /// Decrypts the given text
        /// </summary>
        /// <param name="cipherText">The encrypted BASE64 text</param>
        /// <param name="salt">The pasword salt</param>
        /// <returns>De gedecrypte text</returns>
        public string Decrypt(string cipherText, string salt)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");

            if (!IsBase64String(cipherText))
                throw new Exception("The cipherText input parameter is not base64 encoded");

            var aesAlg = NewRijndaelManaged(salt);
            aesAlg.Padding = PaddingMode.PKCS7;
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            var cipher = Convert.FromBase64String(cipherText);

            using (var msDecrypt = new MemoryStream(cipher))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
        #endregion

        #region NewRijndaelManaged
        /// <summary>
        /// Create a new RijndaelManaged class and initialize it
        /// </summary>
        /// <param name="salt">The pasword salt</param>
        /// <returns></returns>
        private static RijndaelManaged NewRijndaelManaged(string salt)
        {
            if (salt == null) throw new ArgumentNullException("salt");
            var saltBytes = Encoding.ASCII.GetBytes(salt);
            var key = new Rfc2898DeriveBytes(Inputkey, saltBytes);

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

            return aesAlg;
        }
        #endregion
      
    }
}
