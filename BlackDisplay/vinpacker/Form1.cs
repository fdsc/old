using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO.Compression;
using System.IO;

namespace vinpacker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)  // :безопасность
        {
            if (OpenFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var fi = new FileInfo(OpenFileDialog1.FileName);

            MainPack(createChechBox.Checked, keyNameBox.Text, fi, packetNameBox.Text, true);
        }

        public static void MainPack(bool create, string file, FileInfo fi, string packetName, bool isErrorDialog)
        {
            if (!create)
            {
                if (!File.Exists(file + ".pub") || !File.Exists(file + ".private"))
                {
                    if (isErrorDialog)
                        MessageBox.Show("Извините, но данная пара ключей ещё не создана");
                    else
                        Console.WriteLine("error: Извините, но данная пара ключей ещё не создана");
                    return;
                }
            }

            var dn = fi.FullName;
            if (!fi.Exists || (fi.Attributes & FileAttributes.Directory) == 0)
            {
                dn = fi.DirectoryName;
            }
            var packetDirectory = new DirectoryInfo(dn).Parent;

            byte[] packet = pack(dn);

            if (create)
            {
                CngKeyCreationParameters keyCreateParms = new CngKeyCreationParameters();
                keyCreateParms.ExportPolicy = CngExportPolicies.AllowPlaintextExport;

                using (CngKey DSKey = CngKey.Create(CngAlgorithm.ECDsaP521, null, keyCreateParms))
                {
                    byte[] dsKeyBlob;
                    /*byte[] dsKeyBlob = DSKey.Export(CngKeyBlobFormat.Pkcs8PrivateBlob);
                    File.WriteAllBytes(file + ".private", dsKeyBlob);*/

                    dsKeyBlob = DSKey.Export(CngKeyBlobFormat.EccPrivateBlob);
                    File.WriteAllBytes(file + ".private", dsKeyBlob);

                    dsKeyBlob = DSKey.Export(CngKeyBlobFormat.EccPublicBlob);
                    File.WriteAllBytes(file + ".pub", dsKeyBlob);
                }
            }

            if (!File.Exists(file + ".private"))
                throw new FileNotFoundException(file + ".private");

            byte[] ecdsaSignature, ecdsaSignatureLower;
            using (CngKey DSKey = CngKey.Import(File.ReadAllBytes(file + ".private"), CngKeyBlobFormat.EccPrivateBlob))
            {
                using (var ecdsa = new ECDsaCng(DSKey))
                {
                    ecdsa.HashAlgorithm = CngAlgorithm.Sha512;
                    ecdsaSignature = ecdsa.SignData(packet);

                    ecdsa.HashAlgorithm = CngAlgorithm.MD5;
                    ecdsaSignatureLower = ecdsa.SignData(packet);
                }
            }

            using (var resultStream = new MemoryStream())
            {
                var headSignature = Encoding.UTF8.GetBytes("\r\nFDSC PACK / prg.8vs.ru\r\n");
                var keyName = Encoding.UTF8.GetBytes(file);

                int size = 8 + 4 + headSignature.Length + 4 + keyName.Length + 4 + ecdsaSignature.Length + 4 + ecdsaSignatureLower.Length + 4 + packet.Length;

                writeInt(resultStream, size);
                writeInt(resultStream, 0 + 4 + headSignature.Length + 4 + keyName.Length + 4 + ecdsaSignature.Length + 4 + ecdsaSignatureLower.Length);   // всё, что до размера пакета
                writeAllBytesWithLength(resultStream, headSignature);
                writeAllBytesWithLength(resultStream, keyName);
                writeAllBytesWithLength(resultStream, ecdsaSignature);
                writeAllBytesWithLength(resultStream, ecdsaSignatureLower);
                writeAllBytesWithLength(resultStream, packet);

                var result = resultStream.ToArray();
                if (result.Length != size)
                    throw new Exception("Что-то общий размер файла неправильно посчитался...");

                var n = DateTime.Now;
                File.WriteAllBytes(
                                    Path.Combine(packetDirectory.FullName,
                                    packetName + "-" + n.Year.ToString("D4") + n.Month.ToString("D2") + n.Day.ToString("D2") + ".updt"),
                                    result
                                    );

                resultStream.Close();
            }
        }

        private static void writeAllBytesWithLength(MemoryStream resultStream, byte[] bytes)
        {
            writeInt(resultStream, bytes.Length);
            resultStream.Write(bytes, 0, bytes.Length);
        }

        private static byte[] pack(string dirName)   // :обновление :безопасность
        {
            byte[] data;
            byte[] zipData;
            using (var resultStream = new MemoryStream())
            {
                var separators = new char[] {Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar};
                using (var ms       = new MemoryStream())
                {
                    var fileNames = Directory.GetFiles(dirName, "*", SearchOption.AllDirectories);
                    foreach (var fileName in fileNames)
                    {
                        if (fileName.EndsWith(".private"))
                        {
                            MessageBox.Show("В директории архивации обнаружен файл приватного ключа");
                            continue;
                        }

                        if (fileName.EndsWith(".new"))
                        {
                            MessageBox.Show("В директории архивации обнаружен файл с расширением .new - недопустимо");
                            continue;
                        }

                        int i = fileName.IndexOfAny(separators, dirName.Length - 1) + 1;
                        var relativeFileName = fileName.Substring(i, fileName.Length - i);
                        addFileToMs(File.ReadAllBytes(fileName), relativeFileName, ms);
                    }

                    ms.Flush();
                    data = ms.ToArray();
                }

                using (var zs = new MemoryStream())
                {
                    using (var zip = new GZipStream(zs, CompressionMode.Compress))
                    {
                        zip.Write(data, 0, data.Length);
                        zip.Flush();
                    }
                    zipData = zs.ToArray();
                }

                // :проверкаПодлинности :безопасность
                var hash512 = sha512(zipData);
                var hash160 = MD160(zipData);
                File.WriteAllText("last512Hash.txt", Convert.ToBase64String(hash512));
                File.WriteAllText("last160Hash.txt", Convert.ToBase64String(hash160));

                writeAllBytesWithLength(resultStream, hash512);
                writeAllBytesWithLength(resultStream, hash160);

                writeInt(resultStream, data.Length);
                resultStream.Write(zipData, 0, zipData.Length);

                return resultStream.ToArray();
            }
        }

        public static void writeInt(Stream s, int data)
        {
            uint mask = 0xFF000000;
            for (int i = 0; i < 4; i++)
            {
                byte b = (byte) (   (data & mask) >> ((3 - i) << 3)   );
                s.WriteByte(b);
                mask = mask >> 8;
            }
        }

        // нулевые 4 байта
        // 4 байта длинны имени файла
        // имя файла
        // 4 байта длинны файла в неархивированном виде
        // неархивированное содержимое файла
        // длинна хэша
        // MD160 хэш архивированных данных (архивированного файла)
        public static void addFileToMs(byte[] data, string fileName, Stream s)
        {
            var fName = Encoding.UTF8.GetBytes(fileName);
            if (fName.Length < 1)
                throw new Exception("имя файла должно иметь положительную длину");

            writeInt(s, 0);

            writeInt(s, fName.Length);
            s.Write(fName, 0, fName.Length);

            writeInt(s, data.Length);
            s.Write (data, 0, data.Length);

            byte[] hash = MD160(data);
            writeInt(s, hash.Length);
            s.Write (hash, 0, hash.Length);
        }

        // :$$$.контрольнаясумма
        public static byte[] MD160(byte[] toHash)
        {
            using (RIPEMD160 myRIPEMD160 = RIPEMD160Managed.Create())
            {
                return myRIPEMD160.ComputeHash(toHash);
            }
        }

        // :$$$.контрольнаясумма
        public static byte[] sha512(byte[] toHash)
        {
            using (SHA512 sha = new SHA512Managed())
            {
                return sha.ComputeHash(toHash);
            }
        }
    }
}
