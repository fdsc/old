using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using keccak;
using System.Threading;
using System.Diagnostics;

namespace testKeccak
{
    partial class Program
    {

        public static byte[] HexStringToBytes(string hexMessage)
        {
            var bt = new BytesBuilder();

            var sb = new StringBuilder();
            for (int i = 0; i < hexMessage.Length; i += 2)
            {
                bt.addByte(  Convert.ToByte(hexMessage.Substring(i, 2), 16)  );
            }

            return bt.getBytes();
        }

         static string text = "The quick brown fox jumps over the lazy dog."; 
//         static string text = "Мне холодно, мне одиноко."; 
 //       static string text = "Знаешь, мне холодно. Мне одиноко. Стужёный ветер несёт льдинки града мне прямо в лицо, неприятно царапая и даже раня кожу.";
  //      static string text = "Знаешь, мне холодно. Мне одиноко. Стужёный ветер несёт льдинки града мне прямо в лицо, неприятно царапая и даже раня кожу. Я смотрю сквозь прищуренные веки, и бесконечная мёрзлая пустота проникает в меня и растворяет в себе, унося далеко за линию горизонта.";


        class WorkTask: IDisposable
        {
            public WorkTask(string name)
            {
                this.name = name;
            }

            public readonly string name;
            public bool completed = false;

            public void Dispose()
            {
                completed = true;
            }
        }

        static unsafe int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-s")
            {
                staticticTests();
                return 1;
            }

            /*
            byte[] ST = new byte[3];
            byte[] R = null;
            long stk = 0;
            fixed (byte* st = ST)
            {
                while (true)
                {
                    stk++;
                    Gost28147Modified.addConstant(st, ST.Length);
                    if (R == null)
                        R = BytesBuilder.CloneBytes(st, 0, ST.Length);
                    else
                    if (BytesBuilder.Compare(ST, R))
                        break;
                }
            }

            Console.WriteLine("ST count " + Math.Log(stk-1)/Math.Log(2));
            */

            int errorflag = 0;


            var b1_gost = HexStringToBytes("323130393837363534333231303938373635343332313039383736353433323130393837363534333231303938373635343332313039383736353433323130");
            var t = keccak.Gost_34_11_2012_safe.getHash512((byte[]) b1_gost.Clone(), true);
            var a = BitConverter.ToString(t).Replace("-", "").ToLower();
            if (a != "486f64c1917879417fef082b3381a4e211c324f074654c38823a7b76f830ad00fa1fbae42b1285c0352f227524bc9ab16254288dd6863dccd5b9f54a1ad0541b")
            {
                Console.WriteLine("GOST 34.11-2012 first test 512 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = keccak.Gost_34_11_2012_safe.getHash256((byte[]) b1_gost.Clone(), true);
            a = BitConverter.ToString(t).Replace("-", "").ToLower();
            if (a != "00557be5e584fd52a449b16b0251d05d27f94ab76cbaa6da890b59d8ef1e159d")
            {
                Console.WriteLine("GOST 34.11-2012 first test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            b1_gost = HexStringToBytes("fbe2e5f0eee3c820fbeafaebef20fffbf0e1e0f0f520e0ed20e8ece0ebe5f0f2f120fff0eeec20f120faf2fee5e2202ce8f6f3ede220e8e6eee1e8f0f2d1202ce8f0f2e5e220e5d1");
            t = keccak.Gost_34_11_2012_safe.getHash512((byte[]) b1_gost.Clone(), true);
            a = BitConverter.ToString(t).Replace("-", "").ToLower();
            if (a != "28fbc9bada033b1460642bdcddb90c3fb3e56c497ccd0f62b8a2ad4935e85f037613966de4ee00531ae60f3b5a47f8dae06915d5f2f194996fcabf2622e6881e")
            {
                Console.WriteLine("GOST 34.11-2012 second test 512 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = keccak.Gost_34_11_2012_safe.getHash256((byte[]) b1_gost.Clone(), true);
            a = BitConverter.ToString(t).Replace("-", "").ToLower();
            if (a != "508f7e553c06501d749a66fc28c6cac0b005746d97537fa85d9e40904efed29d")
            {
                Console.WriteLine("GOST 34.11-2012 second test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            b1_gost = new UTF8Encoding().GetBytes("984121654654674241321f32a132165465498987732132465467987654324132432454798732432432416547987546579873241211100000000000000000987413210659870102109879854604749871054064098710987109870610210987409871098719870510201065409879877105402102103210654064987987987109871098710640321301206540987098710987102103210321065498710987106540321098710987106540321065498798798798710lasdjkcfbnaklrjkcf'wp829v5qycnxhf9870987409");
            t = keccak.Gost_34_11_2012_safe.getHash512((byte[]) b1_gost.Clone(), true);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "0634EB7C52DD8AEE59DBE8B600DD07B8B830D26D3C8C4319946A4C49E746D9A8F7C88D01F3C1F3153BCAD9961F24661AD4F6909B37F9A942D8E0BE9EF9A013E5")
            {
                Console.WriteLine("GOST 34.11-2012 test 512 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = keccak.Gost_34_11_2012_safe.getHash256((byte[]) b1_gost.Clone(), true);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "A2D8115F8A51A0B8DF835FCDF3C3E06A3E1A1EFD449CABAFA7D71534E01FB34B")
            {
                Console.WriteLine("GOST 34.11-2012 test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            b1_gost = new UTF8Encoding().GetBytes("984121654654674241321f32a132165465498987732132465467987654324132432454798732432432416547987546579873241211100000000000000000987413210659870102109879854604749871054064098710987109870610210987409871098719870510201065409879877105402102103210654064987987987109871098710640321301206540987098710987102103210fd321065498710987106540321098710987106540321065498798798798710lasdjkcfbnaklrjkcf7j;");
            t = keccak.Gost_34_11_2012_safe.getHash512((byte[]) b1_gost.Clone(), true);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "84B0E47EBACC7510AD04541012C80448E61F70D8D9C9A790FF39EDCA3BA15C327F65ADC38936CAC9FE30D77F20EB8BECE9DEB6579A40B45AF3100DB847B91E16")
            {
                Console.WriteLine("GOST 34.11-2012 test 512 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = keccak.Gost_34_11_2012_safe.getHash256((byte[]) b1_gost.Clone(), true);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "08D6DBA016218560AC4A8671D00812724B12E58B34D4F40A8D91A100FCDED26D")
            {
                Console.WriteLine("GOST 34.11-2012 test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            b1_gost = new UTF8Encoding().GetBytes("1984121654654674241321f32a132165465498987732132465467987654324132432454798732432432416547987546579873241211100000000000000000987413210659870102109879854604749871054064098710987109870610210987409871098719870510201065409879877105402102103210654064987987987109871098710640321301206540987098710987102103210fd321065498710987106540321098710987106540321065498798798798710lasdjkcfbnaklrjkcf7j;");
            t = keccak.Gost_34_11_2012_safe.getHash512((byte[]) b1_gost.Clone(), true);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "07C8BD5BB94F66ECBB8C5CC250707D331924AB639AA510AC84E8AD1256514791A1A5F866CB852E24DF37CCE7F774FDD4AA2ABF58DA432D316DDA88E227B43F93")
            {
                Console.WriteLine("GOST 34.11-2012 test 512 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = keccak.Gost_34_11_2012_safe.getHash256((byte[]) b1_gost.Clone(), true);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "9C61783CF222906422467DE969BDE3B868871572CBB211FB05A6E599F98AC844")
            {
                Console.WriteLine("GOST 34.11-2012 test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }


            
            b1_gost = HexStringToBytes("323130393837363534333231303938373635343332313039383736353433323130393837363534333231303938373635343332313039383736353433323130");
            t = keccak.Gost_34_11_2012.getHash512(b1_gost, true, false, false);
            a = BitConverter.ToString(t).Replace("-", "").ToLower();
            if (a != "486f64c1917879417fef082b3381a4e211c324f074654c38823a7b76f830ad00fa1fbae42b1285c0352f227524bc9ab16254288dd6863dccd5b9f54a1ad0541b")
            {
                Console.WriteLine("GOST 34.11-2012 first test 512 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = keccak.Gost_34_11_2012.getHash256(b1_gost, true, false, false);
            a = BitConverter.ToString(t).Replace("-", "").ToLower();
            if (a != "00557be5e584fd52a449b16b0251d05d27f94ab76cbaa6da890b59d8ef1e159d")
            {
                Console.WriteLine("GOST 34.11-2012 first test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }


            b1_gost = HexStringToBytes("fbe2e5f0eee3c820fbeafaebef20fffbf0e1e0f0f520e0ed20e8ece0ebe5f0f2f120fff0eeec20f120faf2fee5e2202ce8f6f3ede220e8e6eee1e8f0f2d1202ce8f0f2e5e220e5d1");
            //Array.Reverse(b1_gost, true);
            t = keccak.Gost_34_11_2012.getHash512(b1_gost, true, false, false);
            a = BitConverter.ToString(t).Replace("-", "").ToLower();
            if (a != "28fbc9bada033b1460642bdcddb90c3fb3e56c497ccd0f62b8a2ad4935e85f037613966de4ee00531ae60f3b5a47f8dae06915d5f2f194996fcabf2622e6881e")
            {
                Console.WriteLine("GOST 34.11-2012 second test 512 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = keccak.Gost_34_11_2012.getHash256(b1_gost, true, false, false);
            a = BitConverter.ToString(t).Replace("-", "").ToLower();
            if (a != "508f7e553c06501d749a66fc28c6cac0b005746d97537fa85d9e40904efed29d")
            {
                Console.WriteLine("GOST 34.11-2012 second test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            b1_gost = new UTF8Encoding().GetBytes("984121654654674241321f32a132165465498987732132465467987654324132432454798732432432416547987546579873241211100000000000000000987413210659870102109879854604749871054064098710987109870610210987409871098719870510201065409879877105402102103210654064987987987109871098710640321301206540987098710987102103210321065498710987106540321098710987106540321065498798798798710lasdjkcfbnaklrjkcf'wp829v5qycnxhf9870987409");
            t = keccak.Gost_34_11_2012.getHash512(b1_gost, true, false, false);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "0634EB7C52DD8AEE59DBE8B600DD07B8B830D26D3C8C4319946A4C49E746D9A8F7C88D01F3C1F3153BCAD9961F24661AD4F6909B37F9A942D8E0BE9EF9A013E5")
            {
                Console.WriteLine("GOST 34.11-2012 test 512 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = keccak.Gost_34_11_2012.getHash256(b1_gost, true, false, false);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "A2D8115F8A51A0B8DF835FCDF3C3E06A3E1A1EFD449CABAFA7D71534E01FB34B")
            {
                Console.WriteLine("GOST 34.11-2012 test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            b1_gost = new UTF8Encoding().GetBytes("984121654654674241321f32a132165465498987732132465467987654324132432454798732432432416547987546579873241211100000000000000000987413210659870102109879854604749871054064098710987109870610210987409871098719870510201065409879877105402102103210654064987987987109871098710640321301206540987098710987102103210fd321065498710987106540321098710987106540321065498798798798710lasdjkcfbnaklrjkcf7j;");
            t = keccak.Gost_34_11_2012.getHash512(b1_gost, true, false, false);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "84B0E47EBACC7510AD04541012C80448E61F70D8D9C9A790FF39EDCA3BA15C327F65ADC38936CAC9FE30D77F20EB8BECE9DEB6579A40B45AF3100DB847B91E16")
            {
                Console.WriteLine("GOST 34.11-2012 test 512 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = keccak.Gost_34_11_2012.getHash256(b1_gost, true, false, false);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "08D6DBA016218560AC4A8671D00812724B12E58B34D4F40A8D91A100FCDED26D")
            {
                Console.WriteLine("GOST 34.11-2012 test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            b1_gost = new UTF8Encoding().GetBytes("1984121654654674241321f32a132165465498987732132465467987654324132432454798732432432416547987546579873241211100000000000000000987413210659870102109879854604749871054064098710987109870610210987409871098719870510201065409879877105402102103210654064987987987109871098710640321301206540987098710987102103210fd321065498710987106540321098710987106540321065498798798798710lasdjkcfbnaklrjkcf7j;");
            t = keccak.Gost_34_11_2012.getHash512(b1_gost, true, false, false);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "07C8BD5BB94F66ECBB8C5CC250707D331924AB639AA510AC84E8AD1256514791A1A5F866CB852E24DF37CCE7F774FDD4AA2ABF58DA432D316DDA88E227B43F93")
            {
                Console.WriteLine("GOST 34.11-2012 test 512 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = keccak.Gost_34_11_2012.getHash256(b1_gost, true, false, false);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "9C61783CF222906422467DE969BDE3B868871572CBB211FB05A6E599F98AC844")
            {
                Console.WriteLine("GOST 34.11-2012 test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            var g3411 = new Gost_34_11_2012();
            t = g3411.getHash_(b1_gost, false);
            t = g3411.getHash_(b1_gost, true);
            t = g3411.getHash_(b1_gost, false);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "9C61783CF222906422467DE969BDE3B868871572CBB211FB05A6E599F98AC844")
            {
                Console.WriteLine("GOST 34.11-2012 double test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }

            t = g3411.getHash_(b1_gost, true);
            t = g3411.getHash_(b1_gost, false);
            t = g3411.getHash_(b1_gost, true);
            if (a != "9C61783CF222906422467DE969BDE3B868871572CBB211FB05A6E599F98AC844")
            {
                Console.WriteLine("GOST 34.11-2012 test 256 bit incorrect");
                Interlocked.Increment(ref errorflag);
            }


            long GT1 = DateTime.Now.Ticks;
            long gcounter = 0;
            do
            {
                keccak.Gost_34_11_2012_safe.getHash512(Encoding.UTF8.GetBytes(text), false);
                gcounter++;
            }
            while (DateTime.Now.Ticks - GT1 < 1000 * 10000);

            Console.WriteLine("GOST 34.11-2012 safe: " + gcounter + " in second");

            GT1 = DateTime.Now.Ticks;
            gcounter = 0;
            do
            {
                keccak.Gost_34_11_2012.getHash512(Encoding.UTF8.GetBytes(text), false);
                gcounter++;
            }
            while (DateTime.Now.Ticks - GT1 < 1000 * 10000);

            Console.WriteLine("GOST 34.11-2012 unsafe: " + gcounter + " in second");



            var sha3 = new keccak.SHA3(8192);





            if (args.Length >= 0)
            {
                var strings = File.ReadAllLines("./files/ShortMsgKAT_512.txt");
                var separator = new string[] {"="};
                int errCount = 0;
                for (int i = 5; i < strings.Length; i += 4*8)
                {
                    var s1 = strings[i + 0].Split(separator, StringSplitOptions.None)[1].Trim();
                    var s2 = strings[i + 1].Split(separator, StringSplitOptions.None)[1].Trim();

                    if (i == 5)
                        s1 = "";

                    var tbs = HexStringToBytes(s1);
                    var res = BitConverter.ToString(sha3.getHash512(tbs)).Replace("-", "");
                    // var res = hash.GetHash(s1, 576, 1024, 64);
                    if (s2 != res)
                    {
                        Console.WriteLine("failed " + i);
                        Console.WriteLine(res);
                        Console.WriteLine( s2);

                        errCount++;
                        if (errCount > 3)
                            break;
                    }
                }

                errorflag += errCount > 0 ? 1 : 0;

                Console.WriteLine("ENDED short 512 / error count " + errCount);

                strings = File.ReadAllLines("./files/ShortMsgKAT_384.txt");
                errCount = 0;
                for (int i = 5; i < strings.Length; i += 4*8)
                {
                    var s1 = strings[i + 0].Split(separator, StringSplitOptions.None)[1].Trim();
                    var s2 = strings[i + 1].Split(separator, StringSplitOptions.None)[1].Trim();

                    if (i == 5)
                        s1 = "";

                    var tbs = HexStringToBytes(s1);
                    var res = BitConverter.ToString(sha3.getHash384(tbs)).Replace("-", "");
                    // var res = hash.GetHash(s1, 576, 1024, 64);
                    if (s2 != res)
                    {
                        Console.WriteLine("failed " + i);
                        Console.WriteLine(res);
                        Console.WriteLine( s2);

                        errCount++;
                        if (errCount > 3)
                            break;
                    }
                }

                errorflag += errCount > 0 ? 1 : 0;
                Console.WriteLine("ENDED short 384 / error count " + errCount);

                strings = File.ReadAllLines("./files/ShortMsgKAT_256.txt");
                errCount = 0;
                for (int i = 5; i < strings.Length; i += 4*8)
                {
                    var s1 = strings[i + 0].Split(separator, StringSplitOptions.None)[1].Trim();
                    var s2 = strings[i + 1].Split(separator, StringSplitOptions.None)[1].Trim();

                    if (i == 5)
                        s1 = "";

                    var tbs = HexStringToBytes(s1);
                    var res = BitConverter.ToString(sha3.getHash256(tbs)).Replace("-", "");
                    // var res = hash.GetHash(s1, 576, 1024, 64);
                    if (s2 != res)
                    {
                        Console.WriteLine("failed " + i);
                        Console.WriteLine(res);
                        Console.WriteLine( s2);

                        errCount++;
                        if (errCount > 3)
                            break;
                    }
                }

                errorflag += errCount > 0 ? 1 : 0;
                Console.WriteLine("ENDED short 256 / error count " + errCount);

                strings = File.ReadAllLines("./files/ShortMsgKAT_224.txt");
                errCount = 0;
                for (int i = 5; i < strings.Length; i += 4*8)
                {
                    var s1 = strings[i + 0].Split(separator, StringSplitOptions.None)[1].Trim();
                    var s2 = strings[i + 1].Split(separator, StringSplitOptions.None)[1].Trim();

                    if (i == 5)
                        s1 = "";

                    var tbs = HexStringToBytes(s1);
                    var res = BitConverter.ToString(sha3.getHash224(tbs)).Replace("-", "");
                    // var res = hash.GetHash(s1, 576, 1024, 64);
                    if (s2 != res)
                    {
                        Console.WriteLine("failed " + i);
                        Console.WriteLine(res);
                        Console.WriteLine( s2);

                        errCount++;
                        if (errCount > 3)
                            break;
                    }
                }

                errorflag += errCount > 0 ? 1 : 0;
                Console.WriteLine("ENDED short 224 / error count " + errCount);

                strings = File.ReadAllLines("./files/LongMsgKAT_512.txt");
                separator = new string[] {"="};
                errCount = 0;
                for (int i = 4; i < strings.Length; i += 3)
                {
                    var s0 = Int32.Parse(strings[i + 0].Split(separator, StringSplitOptions.None)[1].Trim());
                    var s1 =             strings[i + 1].Split(separator, StringSplitOptions.None)[1].Trim();
                    var s2 =             strings[i + 2].Split(separator, StringSplitOptions.None)[1].Trim();

                    if (s0 % 8 != 0)
                        continue;

                    var tbs = HexStringToBytes(s1);
                    var res = BitConverter.ToString(sha3.getHash512(tbs)).Replace("-", "");
                    // var res = hash.GetHash(s1, 576, 1024, 64);
                    if (s2 != res)
                    {
                        Console.WriteLine("failed " + i);
                        Console.WriteLine(res);
                        Console.WriteLine( s2);

                        errCount++;
                        if (errCount > 3)
                            break;
                    }
                }

                errorflag += errCount > 0 ? 1 : 0;
                Console.WriteLine("ENDED long");
            }
            else
                Console.WriteLine("test skipped");
            // GC.Collect();


            errorflag += checkGammaByHash    (sha3) ? 0 : 1;
            errorflag += checkDuplexByHash   (sha3) ? 0 : 1;
            errorflag += checkInitDuplex     (sha3) ? 0 : 1;
            errorflag += checkDuplexModByHash(sha3) ? 0 : 1;

            duplexModTest(ref errorflag);
            GammaTest(ref errorflag);


            t = SHA3.getExHash(2, new byte[0], new byte[0]);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "DC722A9D52AB45C259C82AA9D67BF4BCB6BA3170C4CD3940F5A6E77F3899811B217FD640ACF255054918023592DF4BFE31012B3B62293D33931984C59968E0C90F36F6C2B9F13ED06E1D73FC98B90C9B14D6C3C6B39CBC454C17A6DF2D87D4829F83961D57CB7E4B3955AFF22A1F8D6242F196759AD2EA21F8B99C539CFFBA6B")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}

            t = SHA3.getExHash(2, Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"), Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"));
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "33A63497DDC83D8F29877087BAB4366E00228A78A9D044D5986E17537ED7A44D24D5447178062536CDB1CFF98CA305C6DDF20A3278729F6CB0ADC22D7695453C7FF16561D07BBA1E3F659163B2F95839717CC84F33EA9A3CDC5D674C6BBEBA8D6D287DDD7127C3A98AFE824D93483F33E303AA95F6DEB8C8CD4679B777F6EE59")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}

            t = SHA3.getExHash(2, Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна."), Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"));
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "DF16B0967D681ADF6C8EEB5758DB8D5A1B846C281412CAAF8BEE9A129A1F3E5D9B9B6516AC93A55E38E6A279651DA58606BB45D44D64EB5F5C7B7EB0B2D2850BAF3F15D69A752855F003BDB67268483CED5BE6B9B5C8A87A031A456A2EB4E4593AB83EE78632F5CF090DF6115D1BF78BF27AA0C16AC1198AB99533C57E3ADFD3")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}

            t = SHA3.getExHash(2, Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"), Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна."));
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "5324B0BD6FF639A03936D293D5425E59734BFC8ED9B3A51FD38A9162139F1347847F76932650D5B2DA0D3E56250D7E5C9CF2209D779046C23E6B5D148E07D6AC217A79DB7CECC5404E8336CC71A591BA354C4797E829605B06CF70DE811730C1D83537B98D85EC9DD3EE142E42412E497B07085FD9BC225B6CFC0F0A12E74406")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}

            t = SHA3.getExHash(2, Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"), new byte[0]);
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "9E7D08075D87A02B70A7CC753D67DF35D8ED5B91AF739658654D1A597136C44E3D2A6534738764EAF91C0342FB3953CC106E1F74F2B5D4F760CB06ED34CC787FBA0E6061BC515B893A033D0F2D1F3621666405CD19DF63C47EB27B5FD89DB93DA8B04BABB93E53AB82E0F1378CA67A6E1AE179D920A265955F35423467D934D8")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}

            t = SHA3.getExHash(2, new byte[0], Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"));
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "12FB4248C9CE1788A7ABEA628D24AEE33FFC0C0A2148A63AD09ED4DF4BAB8671899D1F0BFEDD719A7B3A585AECE04B994F9491430B2E738EA55CC8A9251E54F8ED1B48A69009DB4D1043F10378AFA24B3FD671469EDF5EB5A37FE433B0C236190E8EAB52EDC51956CBA36241E045A8A5B3843213B5CEC64E6502BE0CECDFEA59")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}

            t = SHA3.getExHash(3, Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"), Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"));
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "C6E2FA9587AE58AA4E71697C71A3BE04C0E2B0063976EB4790FCA10B7E45F69579CBA55CCC9BCB7FDF1245F12F6DB7F6EC00B150F94F855B8C95249947C6764936C9CEF3A200458FE1BA5474FDE2CCE8BEE8A4B47D8D197D5745D4BAFA014BAC706B60783119FED6D37C8789F52CA7D7285E4AF53BA281589D849E699967FC4D5C1831B9EDFAF1AFDF6F1249D72EC5AD9982FC807284A740B0C3312E920C83ADBE607F1551B5F406944207D28455D7628E99E4AFD6C7B497DDBB44177EB6C094")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}

            t = SHA3.getExHash(4, Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"), Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"));
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "6ABB3495D5821BE6979B0BD437E02FAC6345C3BE3BBDD0967DCC752329812E3507BD85E19C85C6AD7C246A2A90D03E13795641E246953DE97ACF4C21CC4ED7B6BB1BAD7980695519CF7660E58ECF7AC386C546A9689DA4B5E368EAE89CD99F08C8B9D3B5A9E78A0D445EAAC7B58C2C77F3D860990E633F8EF1799669D27EE503C326030FE2165582C414B8DE2C7B8A27C04FDA43767B5B9735CF6397A3C84584D3E76E8BFE62755D4B709F91A6D28260021A84FB0617FC90BD3A2EF4A06F055232DE2F4E5A23969582D69FC18F15FE35CC4B06239860F121AC3E26D94FE40E454B8885203D6387BA91FE2F41423D70BCB504288D09CE2B995F867D2B864D6BF5")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}

            t = SHA3.getExHash(5, Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"), Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"));
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "374D16704517D39B306D1B4BFF8B99A92BB5412BA40CBED8065CA572C301E78C83EC39FD5B7B0F0FE939520AB6D3BEC09E46646B14197792EF7160D2D40D44CC9B7BC955210B10EA63FAB0EA7ACC463E24E2A9AF63B260967262DB9FFCB6FCED04B759501D02B9662A02B75D5151CD9AA98A261824F425E99C708DD6AA17BDE93578381894BFD60101A0A103CB4BC31E32587E7643F84869C56221EB8BA0916C587BF0C155A5DAD248DDD4B90DF06BC44F4B95E43E48D365888B4150CFA450AAF12D852C307BE0238D353219EA70EAC16D32E3FF996B37E3A5A46A3B1707E12DB57619CFDF9621B1BBFAEE2520AAFFE74034E44B9BB67AA10C9E55FB4F41D4A7CB02F7A1DBB7E06708F5CAFF08F4B82155869191B8362CE6F7A1A688FC3CEA1034A0C16FAA80D747BCD43943AF6AA06B49AE052B573E096C9ED44225B08075F1")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}

            t = SHA3.getExHash(8, Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"), Encoding.GetEncoding(1251).GetBytes("Ваганова Екатерина Юрьевна"));
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "22B1ABE39FB16D4DD7C790CD966E5E0F9ECD2F0E1519FEFD1477199A23EB0AEE5A98121C26CA4DF84A385E2FB72CD7B00AAFB684A84CF46C7A66B691966CDBCE4750D536BB181BD5E331EB4B25CEE881C42D60CB438743D1D6571D84F8AC291C548CC800DFA211489968E5CAD2E42DB30B5562FFEBAD9B555262EF004B6600C5899D861CE7A9EEDBC9EBFB2CAD7CD0BD8C0DF8096E05DDE884930742FD23B7F0F2DB111716746F4D1E9C4A62ED14807B8A225F5A7844EB2F496E916BDA713E31EC361357A074CF9986A94D0CD93F08E015CA9433A70509529A72D28475A46063D336389FD277B029ACADAF27B51552AF5F55A08270EE72E54D37E0E70F9CE96DB35A6E1317571F8256E3F2FFB76C859555B9DEAEE26C344246C83165732A24B983FC87614233FEE3F6BA998174C6FF979136DCF6BFEFB73C51F73A5E55802F941B78F4AC8EA0BD674540156F0B8B5434D0FA0C73ED21CFF8E2CD48AC495A67DF79C042B4F6836B3CA8238BF83C0A42B0A1132594F31D08D7D5E5E97FA3EF43C2580B4F0FE0C7C6546E4A7F1E90BC76C09AA3646451638C0D8F1984F79AF07AED9F08462F3A59B890589C31C21B7355AD242F1EA72C6D9A61758BB1F2167B853F94D21B3FC48FF92AB2774A3C2163EF055D9A29CC765A70392A5CE05075B9A52C1B6ACD49275F7D1296C0BEA6A8DB489B19A618D73EE022BC319D566801795A8F")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}

            t = SHA3.getExHash(8, Encoding.GetEncoding(1251).GetBytes("Мой дядя самых честных правил, когда не в шутку занемог он уважать себя заставил и лучше выдумать не мог. Его пример другим наука но боже мой какая скука"), Encoding.GetEncoding(1251).GetBytes("С больным сидеть и день и ночь, печально подносить лекарство, молчать и думать про себя: когда же чёрт возьмёт тебя. Так думал молодой повеса летя в пыли на почтовых"));
            a = BitConverter.ToString(t).Replace("-", "");
            if (a != "DC54E42BB544EFECF248DE0A1937F31054CC4A34221C7E7965BB9C45B88AE425D810989425C2CD769C96785672879F6D52FD6E84022070753B84EDCBDF5E7927812DBBC3388F170E514C927EB62D562051D80A8150BBE14E44934A1BD0AC8FF31F9D6AAB0C2098FDAC8E01B4A35B8B6A2CD406A1752C918E8E511B8669C42AB00EE6FA75D7080F2D7BD5DF04E9F78D8C1683155E80966F575C80D287BCBE2E634D1C884EB4C2566CBF51026A66CDE3D082A036BD32EA9B08DC440604E98B0E2C017AD92153B7D2D23AFE38F8B5AACB58272883BFC6D928510F37964B6D6D2472F16FFAB135A11687FCC852E2D35A5E5422054B35CE8AB5D708AFFD0E2927ABDFFB014B882DB0A3382BAEA4CB787C50B2A8DE89E6F0E64AB3A7DA73A25B202EA210D585B28089785E1DD9A38AD98E718A0E496DAFE674E65D71BDBFD3CF31C626C6F3228599E30D47F6E5BD17FF1BA4FB283D6AE41215A85A6FD63916FE1B871A958BF3E6160FF3370F86401F1EFC458311D8CFE2B8CBF44A715AFC53B0B6391823E471D63F90B19CC705DB376379702664336773E2750AF34678D332B1183E8FBC1CAB97ED681618850E3FA41FD5170061338C3412E7681192BF496A86DE6E0E1530F33F14B16FD32FDAE532564B71F3988B1EFB3173EE92341552F7A02349C0988B193487B83CA4FDC025CE9B67D073D1F3E5656D74115220C7145B7998C170")
                {Interlocked.Increment(ref errorflag); Console.WriteLine("getExHash error");}



            long works = 0;

            long counter = 0;

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    var sha = new keccak.SHA3(8192);
                    long T1 = DateTime.Now.Ticks;
                    do
                    {
                        string t2 = BitConverter.ToString(sha.getHash512(text)).Replace("-", "");
                        counter++;
                    }
                    while (DateTime.Now.Ticks - T1 < 1000 * 10000);

                    Interlocked.Decrement(ref works);
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    var sha512 = new System.Security.Cryptography.SHA512Managed();
                    var txt512 = Encoding.UTF8.GetBytes(text);
                    long T3 = DateTime.Now.Ticks;
                    long counter2 = 0;
                    do
                    {
                        txt512 = Encoding.UTF8.GetBytes(text);
                        string t2 = BitConverter.ToString( sha512.ComputeHash(txt512)).Replace("-", "");
                        counter2++;
                    }
                    while (DateTime.Now.Ticks - T3 < 1000 * 10000);

                    Console.WriteLine("" + counter + "/" + counter2 + " : " + (DateTime.Now.Ticks - T3) / 10000 );

                    Interlocked.Decrement(ref works);
                }
            );

            var key = /*sha3.getHash512(*/Encoding.UTF8.GetBytes("Но вот багряною рукою Заря от утренних долин Выводит с солнцем за собою Весёлый праздник именин")/*)*/;
            var initVector = /*sha3.getHash512(*/Encoding.UTF8.GetBytes("Ваше время истекло")/*)*/;
            sha3.prepareGamma(key, initVector);
            
            var dt = DateTime.Now;
            byte[] gamma = sha3.getGamma(1024*1024*10, true, 576);
            Console.WriteLine(" " + ((DateTime.Now.Ticks - dt.Ticks) / (10000)) + "ms");

            /*
            var dt2 = DateTime.Now;
            var g = new Gost28147Modified();
            var g28147 = g.getGOSTGamma(key, initVector, Gost28147Modified.CryptoProA, 1024*1024*10);
            Console.WriteLine(" " + ((DateTime.Now.Ticks - dt2.Ticks) / (10000)) + "ms");
            */

            sha3.prepareGamma(key, initVector);
            var gammaIV = sha3.getGamma(1024, true, 576);
            var dt2 = DateTime.Now;
            var g = new Gost28147Modified();
            g.prepareGamma(key, gammaIV, Gost28147Modified.CryptoProB, Gost28147Modified.CryptoProC, Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProD, Gost28147Modified.ESbox_A, Gost28147Modified.ESbox_B, false);
            g.getGamma(1024*128, 2);
            Console.WriteLine(" " + ((DateTime.Now.Ticks - dt2.Ticks) / (10000)) + "ms");

            dt2 = DateTime.Now;
            g.prepareGamma(key, gammaIV, Gost28147Modified.CryptoProB, Gost28147Modified.CryptoProC, Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProD, Gost28147Modified.ESbox_A, Gost28147Modified.ESbox_B, false);
            g.getGamma(1024*128, 12);
            Console.WriteLine(" " + ((DateTime.Now.Ticks - dt2.Ticks) / (10000)) + "ms");

            {
                long c20_e = SHA3.getHashCountForMultiHash20(71, 4);
                long c20_1 = SHA3.getHashCountForMultiHash20(71, 1);
                long c20_2 = SHA3.getHashCountForMultiHash20(71, 4, 5, 5);

                if (c20_1 < 24 || c20_1 / c20_e > 1)    // обычно 28/28
                {
                    Interlocked.Increment(ref errorflag);
                    Console.WriteLine("getHashCountForMultiHash20 time error");
                }

                // Interlocked.Decrement(ref works);
                lock (sha3)
                {
                    Monitor.Pulse(sha3);

                    Console.WriteLine("getHashCountForMultiHash20 return " + c20_1 + " / " + c20_e + ", 5ms = " + c20_2);
                }
            }

            {
                long c20_e = SHA3.getHashCountForMultiHash20(71, 4, 300, 0, 1);
                long c20_1 = SHA3.getHashCountForMultiHash20(71, 1, 300, 0, 1);
                long c20_2 = SHA3.getHashCountForMultiHash20(71, 4, 5, 5, 1);

                if (c20_1 < 11 || c20_1 / c20_e > 1)    // обычно 28/28
                {
                    Interlocked.Increment(ref errorflag);
                    Console.WriteLine("getHashCountForMultiHash20 for 40 error");
                }

                // Interlocked.Decrement(ref works);
                lock (sha3)
                {
                    Monitor.Pulse(sha3);

                    Console.WriteLine("getHashCountForMultiHash20 for 40 return " + c20_1 + " / " + c20_e + ", 5ms = " + c20_2);
                }
            }

            long c = SHA3.getHashCountForMultiHash();
            Console.WriteLine("getHashCountForMultiHash return " + c);
            if (c < 4096)
            {
                Interlocked.Increment(ref errorflag);
                Console.WriteLine("error: getHashCountForMultiHash is slow");
            }



            var openText  = File.ReadAllBytes("./files/ShortMsgKAT_224.txt"); //("./files/LongMsgKAT_512.txt");
            var openTextS = File.ReadAllBytes("./files/opentextS.bin");

            var efcrypt = errorflag;

            byte[] crypted = null, crypted2 = null, crypted3 = null, crypted4 = null, crypted5 = null, crypted6 = null, cryptedPT = null, crypted20 = null, crypted20f = null, crypted21 = null, crypted22 = null, crypted22f = null, crypted23 = null, crypted23f = null, crypted30 = null, crypted30f = null, crypted33 = null, crypted33f = null, crypted40 = null, crypted40f = null, crypted41 = null, crypted41f = null;
            byte[] decrypt = null, decrypt2 = null, decrypt3 = null, decrypt4 = null, decrypt5 = null, decrypt6 = null, decryptPT = null, decrypt20 = null, decrypt20f = null, decrypt21 = null, decrypt22 = null, decrypt22f = null, decrypt23 = null, decrypt23f = null, decrypt30 = null, decrypt30f = null, decrypt33 = null, decrypt33f = null, decrypt40 = null, decrypt40f = null, decrypt41 = null, decrypt41f = null;
            byte[] crypted_getMACHashMod = null, crypted_getDerivatoKey = null, crypted_getMultiHash40 = null, crypted_getMultiHash40_2 = null;


            List<WorkTask> worksList = new List<WorkTask>(1024);

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    WorkTask task = new WorkTask("keccak sequential");
                    try
                    {
                        lock (worksList)
                        worksList.Add(task);

                        var sha = new keccak.SHA3(8192);

                        string[] fns = {"./files/LongMsgKAT_512.txt", "./files/ShortMsgKAT_384.txt", "./files/ShortMsgKAT_256.txt", "./files/ShortMsgKAT_224.txt"};
                        //fns = Directory.GetFiles("./files/");

                        foreach (var fn in fns)
                        {
                            byte[] b = new byte[72];
                            string t1, t2;
                            using (var or = File.OpenRead(fn))
                            {
                                bool isInit = false;
                                int num = 0;
                                do
                                {
                                    num = or.Read(b, 0, b.Length);
                                    t1 = BitConverter.ToString(sha.getHash512(b, num, isInit, num != b.Length)).Replace("-", "");
                                    isInit = true;
                                }
                                while (num == b.Length);

                                or.Position = 0;
                                isInit = false;
                                b = new byte[72*2];
                                do
                                {
                                    num = or.Read(b, 0, b.Length);
                                    t2 = BitConverter.ToString(sha.getHash512(b, num, isInit, num != b.Length)).Replace("-", "");
                                    isInit = true;
                                }
                                while (num == b.Length);
                            }

                            b = File.ReadAllBytes(fn);
                            string t3 = BitConverter.ToString(sha.getHash512(b, b.Length)).Replace("-", "");

                            if (t1 != t3)
                            {
                                Interlocked.Increment(ref errorflag);
                                Console.WriteLine("ERROR: sequential keccak hash t1 != t3 for file " + fn);
                            }
                            if (t2 != t3)
                            {
                                Interlocked.Increment(ref errorflag);
                                Console.WriteLine("ERROR: sequential keccak hash t2 != t3 for file " + fn);
                            }
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref works);
                        lock (sha3)
                        {
                            task.completed = true;
                            Monitor.Pulse(sha3);
                        }
                    }
                }
            );


            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("erfc2", worksList))
                    {
                        try
                        {erfc(0.0, 1e-1); // TODO
                            // В точке 0.0 erfc2 = 1.0
                            // Вычисляем погрешность
                            var b = 1.0 - erfc_(0.0, 1e-1);
                            var d = 1.0 - erfc_(0.0, 1e-8);
                            if (Math.Abs(b) > 1e-1 || Math.Abs(d) > 1e-8)
                            {
                                Interlocked.Increment(ref errorflag);
                                Console.WriteLine("erfc2 error: tolerance!");
                            }

                            double[,] correctValues = { { -1e64, 2.0 }, { 0.0, 1.0 }, { 1e64, 0.0 }, 
                                { 1.5, 0.133614402537716 }, { -1.5, 1.866385597462284 }, { -1.9, 1.942566880367996 },
                                { +1.9,  0.057433119632004 }, { +10, 1.523970604832119e-023 }, { -4, 1.999936657516334 } };

                            for (int i = 0; i < correctValues.GetLength(0); i++)
                            {
                                double val  = erfc_(correctValues[i, 0] / 1.4142135623730950488016887242097);
                                double val2 = erfc(correctValues[i, 0] / 1.4142135623730950488016887242097);
                                var k = 1.0;
                                if (Math.Abs(correctValues[i, 1]) > 0.0)
                                    k = Math.Abs(correctValues[i, 1]);
                                if (Math.Abs(val - correctValues[i, 1]) / k > 1e-3 || Math.Abs(val2 - correctValues[i, 1]) / k > 1e-3)
                                {
                                    Interlocked.Increment(ref errorflag);
                                    Console.WriteLine("erfc2 error: ercf(" + i + ") = " + val + " " + val2 + " / " + erfc(correctValues[i, 0] / 1.4142135623730950488016887242097, 1e-6));
                                    break;
                                }
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("getMACHashMod", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            List<byte[]> keys = new List<byte[]>();
                            keys.Add(key);
                            List<byte[]> oivs = new List<byte[]>();
                            oivs.Add(Encoding.UTF8.GetBytes("Мой дядя самых честных правил когда не в шутку занемог он уважать себя заставил и лучше выдумать не мог его пример другим наука но боже мой какая скука с больным сидеть и день и ночь не отходя ни шагу прочь печально подносить"));
                            crypted_getMACHashMod = sha.getMACHashMod(openText, keys, oivs, 4, 4, 0);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("getDerivatoKey", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            var keyDK = Encoding.UTF8.GetBytes("Сплався отпечество наше отродное, сратских народов союз чумовой, седками данная пудрость народная, справся срана, мы оптимся гобой");
                            var oivDK = Encoding.UTF8.GetBytes("Мой дядя самых честных правил когда не в шутку занемог он уважать себя заставил и лучше выдумать не мог его пример другим наука но боже мой какая скука с больным сидеть и день и ночь не отходя ни шагу прочь печально подносить");
                            int pc = 4;
                            crypted_getDerivatoKey = sha.getDerivatoKey(keyDK, oivDK, 8, ref pc, 320, 4);
                            var cgdk = sha.getDerivatoKey(keyDK, oivDK, 8, ref pc, 320, 4);
                            if (!BytesBuilder.Compare(crypted_getDerivatoKey, cgdk))
                            {
                                Interlocked.Increment(ref errorflag);
                                Console.WriteLine("getDerivatoKey error");
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("getMultiHash40", worksList))
                    {
                        try
                        {
                            var keyDK = Encoding.UTF8.GetBytes("Сплався отпечество наше отродное, сратских народов союз чумовой, седками данная пудрость народная, справся срана, мы оптимся гобой");
                            int pc = 4;
                            byte[] cgmh40, cgmh40_2;
                            SHA3.getMultiHash40(keyDK, out crypted_getMultiHash40, ref pc, 8);
                            SHA3.getMultiHash40(keyDK, out cgmh40, ref pc, 8);
                            pc = 1;
                            SHA3.getMultiHash40(keyDK, out crypted_getMultiHash40_2, ref pc, 0);
                            SHA3.getMultiHash40(keyDK, out cgmh40_2, ref pc, 2);
                            if (!BytesBuilder.Compare(crypted_getMultiHash40, cgmh40) || !BytesBuilder.Compare(crypted_getMultiHash40_2, cgmh40_2))
                            {
                                Interlocked.Increment(ref errorflag);
                                Console.WriteLine("getMultiHash40 error");
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    WorkTask task = new WorkTask("keccak 256 sequential");
                    try
                    {
                        lock (worksList)
                        worksList.Add(task);

                        var sha = new keccak.SHA3(8192);

                        string[] fns = {"./files/LongMsgKAT_512.txt", "./files/ShortMsgKAT_384.txt", "./files/ShortMsgKAT_256.txt", "./files/ShortMsgKAT_224.txt"};
                        //fns = Directory.GetFiles("./files/");

                        foreach (var fn in fns)
                        {
                            byte[] b = new byte[136];
                            string t1, t2;
                            using (var or = File.OpenRead(fn))
                            {
                                bool isInit = false;
                                int num = 0;
                                do
                                {
                                    num = or.Read(b, 0, b.Length);
                                    t1 = BitConverter.ToString(sha.getHash256(b, num, isInit, num != b.Length)).Replace("-", "");
                                    isInit = true;
                                }
                                while (num == b.Length);

                                or.Position = 0;
                                isInit = false;
                                b = new byte[136*2];
                                do
                                {
                                    num = or.Read(b, 0, b.Length);
                                    t2 = BitConverter.ToString(sha.getHash256(b, num, isInit, num != b.Length)).Replace("-", "");
                                    isInit = true;
                                }
                                while (num == b.Length);
                            }

                            b = File.ReadAllBytes(fn);
                            string t3 = BitConverter.ToString(sha.getHash256(b, b.Length)).Replace("-", "");

                            if (t1 != t3)
                            {
                                Interlocked.Increment(ref errorflag);
                                Console.WriteLine("ERROR: sequential keccak hash t1 != t3 for file " + fn);
                            }
                            if (t2 != t3)
                            {
                                Interlocked.Increment(ref errorflag);
                                Console.WriteLine("ERROR: sequential keccak hash t2 != t3 for file " + fn);
                            }
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref works);
                        lock (sha3)
                        {
                            task.completed = true;
                            Monitor.Pulse(sha3);
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    WorkTask task = new WorkTask("Gost_34_11_2012 sequential");
                    try
                    {
                        lock (worksList)
                        worksList.Add(task);

                        var sha = new keccak.Gost_34_11_2012();

                        string[] fns = {"./files/LongMsgKAT_512.txt", "./files/ShortMsgKAT_384.txt", "./files/ShortMsgKAT_256.txt", "./files/ShortMsgKAT_224.txt"};
                        // fns = Directory.GetFiles("./files/");

                        foreach (var fn in fns)
                        {
                            byte[] b = new byte[64];
                            string t1 = null, t2 = null;
                            using (var or = File.OpenRead(fn))
                            {
                                bool isInit = false;
                                int num = 0;
                                long fl = 0;
                                do
                                {
                                    num = or.Read(b, 0, b.Length);
                                    fl += num;
                                    var h = keccak.Gost_34_11_2012.getHash512(b, false, true, false, num, isInit, num != b.Length, fl, sha);
                                    if (h != null)
                                    t1 = BitConverter.ToString(h).Replace("-", "");
                                    isInit = true;
                                }
                                while (num == b.Length);

                                or.Position = 0;
                                isInit = false;
                                b = new byte[64*2];
                                fl = 0;
                                do
                                {
                                    num = or.Read(b, 0, b.Length);
                                    fl += num;
                                    var h = keccak.Gost_34_11_2012.getHash512(b, false, true, false, num, isInit, num != b.Length, fl, sha);
                                    if (h != null)
                                    t2 = BitConverter.ToString(h).Replace("-", "");
                                    isInit = true;
                                }
                                while (num == b.Length);
                            }

                            b = File.ReadAllBytes(fn);
                            string t3 = BitConverter.ToString(keccak.Gost_34_11_2012.getHash512(b)).Replace("-", "");

                            if (t1 != t3)
                            {
                                Interlocked.Increment(ref errorflag);
                                Console.WriteLine("ERROR: sequential 34.11 hash t1 != t3 for file " + fn);
                            }
                            if (t2 != t3)
                            {
                                Interlocked.Increment(ref errorflag);
                                Console.WriteLine("ERROR: sequential 34.11 hash t2 != t3 for file " + fn);
                            }
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref works);
                        lock (sha3)
                        {
                            task.completed = true;
                            Monitor.Pulse(sha3);
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 2-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted = sha.multiCryptLZMA(openText, key, initVector, 2, true, 19, 1000);
                            decrypt = sha.multiDecryptLZMA(crypted, key);

                            if (decrypt != null && BytesBuilder.Compare(openText, decrypt))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 0-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted2 = sha.multiCryptLZMA(openText, Encoding.UTF8.GetBytes("Ключ"), initVector, 0, true, 19, 1000);
                            decrypt2 = sha.multiDecryptLZMA(crypted2, Encoding.UTF8.GetBytes("Ключ"));

                            if (decrypt2 != null && BytesBuilder.Compare(openText, decrypt2))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 3-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted3 = sha.multiCryptLZMA(openText, Encoding.UTF8.GetBytes("Ключ"), initVector, 3, true, 19, 1000);
                            decrypt3 = sha.multiDecryptLZMA(crypted3, Encoding.UTF8.GetBytes("Ключ"));

                            if (decrypt3 != null && BytesBuilder.Compare(openText, decrypt3))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 10-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted4 = sha.multiCryptLZMA(openText, key, initVector, 10, true, 19, 1000);
                            decrypt4 = sha.multiDecryptLZMA(crypted4, key);

                            if (decrypt4 != null && BytesBuilder.Compare(openText, decrypt4))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 11-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted5 = sha.multiCryptLZMA(openText, key, initVector, 11, true, 19, 1000);
                            decrypt5 = sha.multiDecryptLZMA(crypted5, key);

                            if (decrypt5 != null && BytesBuilder.Compare(openText, decrypt5))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 12-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted6 = sha.multiCryptLZMA(openText, key, initVector, 12, true, 19, 1000);
                            decrypt6 = sha.multiDecryptLZMA(crypted6, key);
                            if (decrypt6 != null && BytesBuilder.Compare(openText, decrypt6))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 12-false", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            cryptedPT = sha.multiCryptLZMA(openText, key, initVector, 12, false, 0, 1000);
                            decryptPT = sha.multiDecryptLZMA(cryptedPT, key);
                            if (decryptPT != null && BytesBuilder.Compare(openText, decryptPT))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 20-false", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted20 = sha.multiCryptLZMA(openText, key, initVector, 20, false, 0, 12);
                            decrypt20 = sha.multiDecryptLZMA(crypted20, key);
                            if (decrypt20 != null && BytesBuilder.Compare(openText, decrypt20))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 20-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted20f = sha.multiCryptLZMA(openText, key, initVector, 20, true, 19, 12);
                            decrypt20f = sha.multiDecryptLZMA(crypted20f, key);
                            if (decrypt20f != null && BytesBuilder.Compare(openText, decrypt20f))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 21-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted21 = sha.multiCryptLZMA(openText, key, initVector, 21, true, 19, 12);
                            decrypt21 = sha.multiDecryptLZMA(crypted21, key);
                            if (decrypt21 != null && BytesBuilder.Compare(openText, decrypt21))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 22-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted22 = sha.multiCryptLZMA(openText, key, initVector, 22, true, 19, 12);
                            decrypt22 = sha.multiDecryptLZMA(crypted22, key);
                            if (decrypt22 != null && BytesBuilder.Compare(openText, decrypt22))
                            { }
                            else
                                Interlocked.Increment(ref errorflag);
                        }
                        catch
                        {
                            Interlocked.Increment(ref errorflag);
                            lock (sha3)
                            {
                                Console.WriteLine("decrypt22 exception");
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 22-false", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted22f = sha.multiCryptLZMA(openText, key, Encoding.UTF8.GetBytes("Мой дядя самых честных правил когда не в шутку занемог он уважать себя заставил и лучше выдумать не мог его пример другим наука но боже мой какая скука с больным сидеть и день и ночь не отходя ни шагу прочь печально подносить"), 22, false, 0, 12);
                            decrypt22f = sha.multiDecryptLZMA(crypted22f, key);
                            if (decrypt22f != null && BytesBuilder.Compare(openText, decrypt22f))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt22f != openText");
                                }
                            }
                        }
                        catch
                        {
                            Interlocked.Increment(ref errorflag);
                            lock (sha3)
                            {
                                Console.WriteLine("decrypt22f exception");
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 23-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted23 = sha.multiCryptLZMA(openText, key, initVector, 23, true, 19, 12);
                            decrypt23 = sha.multiDecryptLZMA(crypted23, key);
                            if (decrypt23 != null && BytesBuilder.Compare(openText, decrypt23))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt23 != openText");
                                }
                            }
                        }
                        catch
                        {
                            Interlocked.Increment(ref errorflag);
                            lock (sha3)
                            {
                                Console.WriteLine("decrypt23 exception");
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 23-false", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted23f = sha.multiCryptLZMA(openText, key, Encoding.UTF8.GetBytes("Мой дядя самых честных правил когда не в шутку занемог он уважать себя заставил и лучше выдумать не мог его пример другим наука но боже мой какая скука с больным сидеть и день и ночь не отходя ни шагу прочь печально подносить"), 23, false, 0, 12);
                            decrypt23f = sha.multiDecryptLZMA(crypted23f, key);
                            if (decrypt23f != null && BytesBuilder.Compare(openText, decrypt23f))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt23f != openText");
                                }
                            }
                        }
                        catch
                        {
                            Interlocked.Increment(ref errorflag);
                            lock (sha3)
                            {
                                Console.WriteLine("decrypt23f exception");
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 30-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted30 = sha.multiCryptLZMA(openText, key, initVector, 30, true, 19, 12);
                            decrypt30 = sha.multiDecryptLZMA(crypted30, key);
                            if (decrypt30 != null && BytesBuilder.Compare(openText, decrypt30))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt30 != openText");
                                }
                            }
                        }
                        catch
                        {
                            Interlocked.Increment(ref errorflag);
                            lock (sha3)
                            {
                                Console.WriteLine("decrypt30 exception");
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 30-false", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted30f = sha.multiCryptLZMA(openText, key, Encoding.UTF8.GetBytes("Мой дядя самых честных правил когда не в шутку занемог он уважать себя заставил и лучше выдумать не мог его пример другим наука но боже мой какая скука с больным сидеть и день и ночь не отходя ни шагу прочь печально подносить"), 30, false, 0, 12);
                            decrypt30f = sha.multiDecryptLZMA(crypted30f, key);
                            if (decrypt30f != null && BytesBuilder.Compare(openText, decrypt30f))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt30f != openText");
                                }
                            }
                        }
                        catch
                        {
                            Interlocked.Increment(ref errorflag);
                            lock (sha3)
                            {
                                Console.WriteLine("decrypt30f exception");
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 33-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted33 = sha.multiCryptLZMA(openText, key, initVector, 33, true, 19, 12);
                            decrypt33 = sha.multiDecryptLZMA(crypted33, key);
                            if (decrypt33 != null && BytesBuilder.Compare(openText, decrypt33))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt33 != openText");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Interlocked.Increment(ref errorflag);
                            lock (sha3)
                            {
                                Console.WriteLine("decrypt33 exception " + ex.Message);
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 33-false", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted33f = sha.multiCryptLZMA(openText, key, Encoding.UTF8.GetBytes("Мой дядя самых честных правил когда не в шутку занемог он уважать себя заставил и лучше выдумать не мог его пример другим наука но боже мой какая скука с больным сидеть и день и ночь не отходя ни шагу прочь печально подносить"), 33, false, 0, 12);
                            decrypt33f = sha.multiDecryptLZMA(crypted33f, key);
                            if (decrypt33f != null && BytesBuilder.Compare(openText, decrypt33f))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt33f != openText");
                                }
                            }
                        }
                        catch
                        {
                            Interlocked.Increment(ref errorflag);
                            lock (sha3)
                            {
                                Console.WriteLine("decrypt33f exception");
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 40-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted40 = sha.multiCryptLZMA(openTextS, key, initVector, 40, true, 19, 12);
                            decrypt40 = sha.multiDecryptLZMA(crypted40, key);
                            if (decrypt40 != null && BytesBuilder.Compare(openTextS, decrypt40))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt40 != openText");
                                }
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 40-false", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted40f = sha.multiCryptLZMA(openTextS, key, Encoding.UTF8.GetBytes("Мой дядя самых честных правил когда не в шутку занемог он уважать себя заставил и лучше выдумать не мог его пример другим наука но боже мой какая скука с больным сидеть и день и ночь не отходя ни шагу прочь печально подносить"), 40, false, 0, 12);
                            decrypt40f = sha.multiDecryptLZMA(crypted40f, key);
                            if (decrypt40f != null && BytesBuilder.Compare(openTextS, decrypt40f))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt40f != openText");
                                }
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 41-true", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted41 = sha.multiCryptLZMA(openTextS, key, initVector, 41, true, 19, 12);
                            decrypt41 = sha.multiDecryptLZMA(crypted41, key);
                            if (decrypt41 != null && BytesBuilder.Compare(openTextS, decrypt41))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt41 != openText");
                                }
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );

            Interlocked.Increment(ref works);
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    using (AddTaskToList("multiCryptLZMA 41-false", worksList))
                    {
                        try
                        {
                            var sha = new keccak.SHA3(8192);
                            // Тесты дадут другие варианты на не 4-хядерном компьютере
                            crypted41f = sha.multiCryptLZMA(openTextS, key, Encoding.UTF8.GetBytes("Мой дядя самых честных правил когда не в шутку занемог он уважать себя заставил и лучше выдумать не мог его пример другим наука но боже мой какая скука с больным сидеть и день и ночь не отходя ни шагу прочь печально подносить"), 41, false, 0, 12);
                            decrypt41f = sha.multiDecryptLZMA(crypted41f, key);
                            if (decrypt41f != null && BytesBuilder.Compare(openTextS, decrypt41f))
                            { }
                            else
                            {
                                Interlocked.Increment(ref errorflag);
                                lock (sha3)
                                {
                                    Console.WriteLine("decrypt41f != openText");
                                }
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref works);
                            lock (sha3)
                            {
                                Monitor.Pulse(sha3);
                            }
                        }
                    }
                }
            );


            var dta = DateTime.Now.Ticks;

            lock (sha3)
                while (Interlocked.Read(ref works) > 0)
                {
                    Monitor.Wait(sha3, 10000);
                    if (Interlocked.Read(ref works) > 0 && DateTime.Now.Ticks - dta > 1500*10000)
                    {
                        dta = DateTime.Now.Ticks;

                        Console.WriteLine("wait for " + Interlocked.Read(ref works) + " tasks (" + DateTime.Now.ToLocalTime() + ")");
                        if (Interlocked.Read(ref works) <= 2)
                        {
                            foreach (var task in worksList)
                            {
                                if (!task.completed)
                                    Console.WriteLine("    wait for tasks " + task.name);
                            }
                        }
                    }
                }

            /*
            File.WriteAllBytes("./files/LongMsgKAT_512.crpt40", crypted40);
            File.WriteAllBytes("./files/LongMsgKAT_512.crpt40f", crypted40f); // TODO:
            File.WriteAllBytes("./files/LongMsgKAT_512.crpt41", crypted41);
            File.WriteAllBytes("./files/LongMsgKAT_512.crpt41f", crypted41f); // TODO:
            File.WriteAllBytes("./files/getDerivatoKey.crpt", crypted_getDerivatoKey);
            File.WriteAllBytes("./files/getMultiHash40.crpt", crypted_getMultiHash40);
            File.WriteAllBytes("./files/getMultiHash40_2.crpt", crypted_getMultiHash40_2);
            */
            if (errorflag != efcrypt)
                Console.WriteLine("crypted and decrypted data are not equal or not be decrypted");

            if (!File.Exists("./files/gamma.bin"))
            {
                if (args.Length > 0)
                    errorflag += 1;

                // Console.WriteLine("Gamma generation begined, in file 'gamma.bin'");
                
                File.WriteAllBytes("./files/gamma.bin",      gamma);
                /*File.WriteAllBytes("./files/gamma28147.bin", g28147);
                File.WriteAllBytes("./files/gammaMod.bin",   gmod);*/
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt",  crypted);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt2", crypted2);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt3", crypted3);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt4", crypted4);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt5", crypted5);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt6", crypted6);
                File.WriteAllBytes("./files/LongMsgKAT_512.crptpt", cryptedPT);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt20", crypted20);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt20f",crypted20f);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt21", crypted21);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt22", crypted22);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt22f", crypted22f);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt23", crypted23);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt23f", crypted23f);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt30", crypted30);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt30f", crypted30f);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt33", crypted33);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt33f", crypted33f);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt40", crypted40);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt40f", crypted40f);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt41", crypted41);
                File.WriteAllBytes("./files/LongMsgKAT_512.crpt41f", crypted41f);
                File.WriteAllBytes("./files/getMACHashMod.crpt", crypted_getMACHashMod);
                File.WriteAllBytes("./files/getDerivatoKey.crpt", crypted_getDerivatoKey);
                File.WriteAllBytes("./files/getMultiHash40.crpt", crypted_getMultiHash40);

                Console.WriteLine("!!! Gamma ended in file 'gamma.bin' !!!");

                if (args.Length == 0)
                    Console.ReadLine();
            }
            else
            {
                byte[] fileGamma1 = File.ReadAllBytes("./files/gamma.bin");
                /*byte[] fileGamma2 = File.ReadAllBytes("./files/gamma28147.bin");
                byte[] fileGamma3 = File.ReadAllBytes("./files/gammaMod.bin");*/
                byte[] fileGamma4 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt");
                byte[] fileGamma5 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt2");
                byte[] fileGamma6 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt3");
                byte[] fileGamma7 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt4");
                byte[] fileGamma8 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt5");
                byte[] fileGamma9 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt6");
                byte[] fileGammaPT = File.ReadAllBytes("./files/LongMsgKAT_512.crptpt");
                byte[] fileGamma20 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt20");
                byte[] fileGamma20f= File.ReadAllBytes("./files/LongMsgKAT_512.crpt20f");
                byte[] fileGamma21 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt21");
                byte[] fileGamma22 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt22");
                byte[] fileGamma22f = File.ReadAllBytes("./files/LongMsgKAT_512.crpt22f");
                byte[] fileGamma23 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt23");
                byte[] fileGamma23f = File.ReadAllBytes("./files/LongMsgKAT_512.crpt23f");
                byte[] fileGamma30 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt30");
                byte[] fileGamma30f = File.ReadAllBytes("./files/LongMsgKAT_512.crpt30f");
                byte[] fileGamma33 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt33");
                byte[] fileGamma33f = File.ReadAllBytes("./files/LongMsgKAT_512.crpt33f");
                byte[] fileGamma40 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt40");
                byte[] fileGamma40f = File.ReadAllBytes("./files/LongMsgKAT_512.crpt40f");
                byte[] fileGamma41 = File.ReadAllBytes("./files/LongMsgKAT_512.crpt41");
                byte[] fileGamma41f = File.ReadAllBytes("./files/LongMsgKAT_512.crpt41f");
                byte[] file_getMACHashMod = File.ReadAllBytes("./files/getMACHashMod.crpt");
                byte[] file_getDerivatoKey = File.ReadAllBytes("./files/getDerivatoKey.crpt");
                byte[] file_getMultiHash40 = File.ReadAllBytes("./files/getMultiHash40.crpt");
                byte[] file_getMultiHash40_2 = File.ReadAllBytes("./files/getMultiHash40_2.crpt");
                /*if (fileGamma1.LongLength != gamma.LongLength || /*fileGamma2.LongLength != g28147.LongLength || 
                    fileGamma3.LongLength != gmod.LongLength  || *//*fileGamma4.LongLength != crypted.LongLength || 
                    fileGamma5.LongLength != crypted2.LongLength || fileGamma6.LongLength != crypted3.LongLength ||
                    fileGamma7.LongLength != crypted4.LongLength || fileGamma8.LongLength != crypted5.LongLength ||
                    fileGamma20.LongLength != crypted20.LongLength || fileGamma21.LongLength != crypted21.LongLength ||
                    fileGamma22.LongLength != crypted22.LongLength || fileGamma20f.LongLength != crypted20f.LongLength || 
                    fileGamma22f.LongLength != crypted22f.LongLength ||
                    fileGamma23.LongLength != crypted23.LongLength ||
                    fileGamma23f.LongLength != crypted23f.LongLength ||
                    fileGamma9.LongLength != crypted6.LongLength || fileGammaPT.LongLength != cryptedPT.LongLength ||
                    fileGamma23.LongLength != crypted30.LongLength || fileGamma30f.LongLength != crypted30f.LongLength ||
                    fileGamma33.LongLength != crypted33.LongLength || fileGamma33f.LongLength != crypted33f.LongLength
                    )
                {
                    errorflag += 1;
                    Console.WriteLine("crypted files are no equal (length)");
                }
                else*/
                {
                    var ef = errorflag;
                    errorflag += BytesBuilder.Compare(fileGamma1, gamma)    ? 0 :       return1("crypted incorrect: gamma");
                    /*errorflag += BytesBuilder.Compare(fileGamma2, g28147)   ? 0 :       return1("crypted incorrect: g28147");
                    errorflag += BytesBuilder.Compare(fileGamma3, gmod)     ? 0 :       return1("crypted incorrect: gmod");*/
                    errorflag += BytesBuilder.Compare(fileGamma4, crypted)  ? 0 :       return1("crypted incorrect: crypted");
                    errorflag += BytesBuilder.Compare(fileGamma5, crypted2) ? 0 :       return1("crypted incorrect: crypted2");
                    errorflag += BytesBuilder.Compare(fileGamma6, crypted3) ? 0 :       return1("crypted incorrect: crypted3");
                    errorflag += BytesBuilder.Compare(fileGamma7, crypted4) ? 0 :       return1("crypted incorrect: crypted4");
                    errorflag += BytesBuilder.Compare(fileGamma8, crypted5) ? 0 :       return1("crypted incorrect: crypted5");
                    errorflag += BytesBuilder.Compare(fileGamma9, crypted6) ? 0 :       return1("crypted incorrect: crypted6");
                    errorflag += BytesBuilder.Compare(fileGammaPT,  cryptedPT)  ? 0 :   return1("crypted incorrect: cryptedPT");
                    errorflag += BytesBuilder.Compare(fileGamma20,  crypted20)  ? 0 :   return1("crypted incorrect: crypted20");
                    errorflag += BytesBuilder.Compare(fileGamma20f, crypted20f) ? 0 :   return1("crypted incorrect: crypted20f");
                    errorflag += BytesBuilder.Compare(fileGamma21,  crypted21)  ? 0 :   return1("crypted incorrect: crypted21");
                    errorflag += BytesBuilder.Compare(fileGamma22,  crypted22)  ? 0 :   return1("crypted incorrect: crypted22");
                    errorflag += BytesBuilder.Compare(fileGamma22f, crypted22f) ? 0 :   return1("crypted incorrect: crypted22f");
                    errorflag += BytesBuilder.Compare(fileGamma23,  crypted23)  ? 0 :   return1("crypted incorrect: crypted23");
                    errorflag += BytesBuilder.Compare(fileGamma23f, crypted23f) ? 0 :   return1("crypted incorrect: crypted23f");
                    errorflag += BytesBuilder.Compare(fileGamma30,  crypted30)  ? 0 :   return1("crypted incorrect: crypted30");
                    errorflag += BytesBuilder.Compare(fileGamma30f, crypted30f) ? 0 :   return1("crypted incorrect: crypted30f");
                    errorflag += BytesBuilder.Compare(fileGamma33,  crypted33)  ? 0 :   return1("crypted incorrect: crypted33");
                    errorflag += BytesBuilder.Compare(fileGamma33f, crypted33f) ? 0 :   return1("crypted incorrect: crypted33f");
                    errorflag += BytesBuilder.Compare(fileGamma40,  crypted40)  ? 0 :   return1("crypted incorrect: crypted40");
                    errorflag += BytesBuilder.Compare(fileGamma40f, crypted40f) ? 0 :   return1("crypted incorrect: crypted40f");
                    errorflag += BytesBuilder.Compare(fileGamma41,  crypted41)  ? 0 :   return1("crypted incorrect: crypted41");
                    errorflag += BytesBuilder.Compare(fileGamma41f, crypted41f) ? 0 :   return1("crypted incorrect: crypted41f");
                    errorflag += BytesBuilder.Compare(file_getMACHashMod, crypted_getMACHashMod) ? 0 :   return1("crypted incorrect: getMACHashMod");
                    errorflag += BytesBuilder.Compare(file_getDerivatoKey, crypted_getDerivatoKey) ? 0 :   return1("crypted incorrect: getDerivatoKey");
                    errorflag += BytesBuilder.Compare(file_getMultiHash40, crypted_getMultiHash40) ? 0 :   return1("crypted incorrect: getMultiHash40");

                    // Из-за того, что файл ShortMsgKAT_224.txt используется в двоичном виде, его нужно брать как двоичный, без преобразования, которое делает git
                    // !!!!!!!!!! git config --global core.autocrlf false

                    /*
                    if (ef != errorflag)
                        Console.WriteLine("crypted files are no equal");*/
                }
            }

            if (errorflag != 0)
            {
                Console.WriteLine("error: incorrect keccak.dll code");
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine();
                Console.WriteLine("!!!~~~ ERROR ~~~!!! error count " + errorflag);

                // if (args.Length == 0)
                    Console.ReadLine();
            }
            else
                Console.WriteLine("all tests is correct");

            System.Threading.Thread.Sleep(2000);

            if (args.Length == 0)
                Console.ReadLine();

            return errorflag;
        }

        private static void staticticTests()
        {
            try
            {
                var sha = new keccak.SHA3(8192);
                if (!testForBits_keccak(sha))
                {
                    Console.WriteLine("testForBits_keccak error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("testForBits_keccak error: " + ex.Message);
            }
            /*
            try
            {
                var sha = new keccak.SHA3(8192);
                int pc = 4;

                if (!testForBits_getMultiHash40(ref pc, 0, sha))
                {
                    Console.WriteLine("testForBits_getMultiHash40");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("testForBits_getMultiHash40 error: " + ex.Message);
            }*/

            Console.WriteLine("ended");
            Console.ReadLine();
        }

        private static WorkTask AddTaskToList(string name, List<WorkTask> worksList)
        {
            WorkTask task = new WorkTask(name);
            lock (worksList)
                worksList.Add(task);

            return task;
        }

        private static int return1(string message)
        {
            Console.WriteLine(message);
            return 1;
        }
    }
}
