using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace PostBuilder
{
    class Program
    {
        static string path7Zip
        {
            get
            {
                var str1 = @"C:\Prg\Text\7-Zip\7z.exe"; //System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) +  "\\7-Zip\\7z.exe";
                var str2 = str1; // System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)    +  "\\7-Zip\\7z.exe";
                if (File.Exists(str2))
                    return str2;
                else
                    return str1;
            }
        }

        static string signtoolPath
        {
            get
            {
                var str1 = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft SDKs\Windows\v7.0A\Bin\signtool.exe");
                var str2 = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),    @"Microsoft SDKs\Windows\v7.0A\Bin\signtool.exe");
                if (File.Exists(str2))
                    return str2;
                else
                    return str1;
            }
        }

        static int Main(string[] args)
        {
            File.WriteAllText(LogF, DateTime.Now.ToString() + "\r\n");
            if (args.Length == 0)
            {
                args = new string[] {@"Z:\BlackDisplay\", @"Z:\BlackDisplay\bin\updatorCompile\", @"E:\works\programming\IT_helpers\BlackDisplay"};
            }

            foreach (var arg in args)
            {
                log(arg);
            }
            log("args ended", true);

            int ec;
            try
            {
                ec = PostBuildAction(args);
            }
            catch (Exception e)
            {
                log("General exception " + e.Message);
                Console.WriteLine("error:" + e.Message);
                return 1;
            }

            return ec;
        }

        static string LogF = "log.txt";
        static void log(string s, bool space = false)
        {
            s = s + "\r\n";
            if (space)
                s += "\r\n";

            lock (sync)
            File.AppendAllText(LogF, s);
        }

        static object sync = new object();
        private static int PostBuildAction(string[] args)
        {
            var solution = args[0];
            var updator  = args[1];
            var BUILD    = new DirectoryInfo(Path.Combine(new DirectoryInfo(solution).Parent.FullName, "build"));
            var build    = Path.GetFullPath(Path.Combine(BUILD.FullName, "bin/Release/"));

            if (!BUILD.Exists)
                BUILD.Create();

            System.Threading.Thread.Sleep(1000);

            var now = DateTime.Now;
            var dts = now.ToString("yyyyMMdd");
            var release = Path.Combine(args[2], @"arc\release_" + dts);
            if (Directory.Exists(release))
                try
                {
                    Directory.Delete(release, true);
                }
                catch// (Exception ex)
                {
                    System.Threading.Thread.Sleep(500);     // Почему-то иногда выдаёт "папка не пуста" и оставляет пустую папку не удалённой (кажется, дело в антивирусе)
                    if (Directory.Exists(release))
                        Directory.Delete(release, true);
                }

            System.Threading.Thread.Sleep(500);

            Directory.CreateDirectory(release);

            var rtdb  = Path.Combine(release,   "rtdb");            // это для первичного архива
            var urtdb = Path.Combine(rtdb,      "updator");         // это для первичного архива, копируется обновлятель
            var rtdbA = Path.Combine(rtdb,      "Resources");       // это для первичного архива, копируются аудиофалы
            var updt  = Path.Combine(release,   "updt");            // это для архива обновления
            var ubin  = Path.Combine(release,   "bin");             // это для архива обновления
            var bind  = Path.Combine(release,   "binDebug");
            var sh    = Path.Combine(solution,  "BlackDisplay/help");
            var rhdir = Path.Combine(release,   @"h\rtdb");
            var hdir  = Path.Combine(rhdir,     "help");
            var keyd  = "z:";//Path.Combine(solution,  "key");

            var packerBin = Path.GetFullPath(Path.Combine(BUILD.FullName, "vinpacker/bin/Release/vinpacker.exe"));

            Directory.CreateDirectory(rtdb);
            Directory.CreateDirectory(urtdb);
            Directory.CreateDirectory(updt);
            Directory.CreateDirectory(ubin);

            if (Directory.Exists(bind))
                try
                {
                    Directory.Delete(bind, true);
                }
                catch
                {
                    System.Threading.Thread.Sleep(500);     // Почему-то иногда выдаёт "папка не пуста" и оставляет пустую папку не удалённой (кажется, дело в антивирусе)
                    if (Directory.Exists(bind))
                        Directory.Delete(bind, true);
                }

            var updatorFiles = new string[] { "options.dll", "umove.exe", "updatorvs8.exe", /*"updt1.pub", */"updt2013.pub" };
            var rtbdFiles    = new string[] { "options.dll", "keccak.dll", "AttentionTests.dll", "VinPlaning.dll", "BlackDisplay.exe" };

            if (File.Exists(keyd + "/key.pfx"))
            {
                log(keyd + "/key.pfx" + " exists");

                signAssemblies(args, updator,  keyd, updatorFiles);
                signAssemblies(args, build, keyd, rtbdFiles);
            }
            else
            {
                Console.WriteLine("key.pfx not found. Assemblyes does not signed");
                log(keyd + "/key.pfx" + " not exists");
            }
            /* Обновление удалено! Т.к. не поддерживается сейчас
            copyAllFiles(updator, updatorFiles, updt);
            File.Delete(updt + "/updt1.pub");           // удаляем файлы публичных ключей, т.к. они и так уже должны быть на компьютере пользователя
            File.Delete(updt + "/updt2013.pub");

            copyAllFiles(updator, updatorFiles, urtdb);
            */

            copyAllFiles(build, rtbdFiles, rtdb);
            copyAllContainsFiles(Path.Combine(build, "Resources"), rtdbA);
            copyAllFiles(build, rtbdFiles, ubin);

            copyAllContainsFiles(rtdb, bind);
            // Обновление удалено! Т.к. не поддерживается сейчас
            // copyAllFiles(updator, new string[] {"uopts.txt"}, Path.Combine(bind, "updator"));


            Directory.CreateDirectory(rhdir);
            Directory.CreateDirectory(hdir);
            copyAllContainsFiles(rtdb, rhdir);
            copyAllContainsFiles(sh,   hdir);
            
            var pi = new ProcessStartInfo(packerBin, "0 z:\\updt2013 \"" + Path.Combine(ubin, "exe") + "\" relaxtime");
            pi.CreateNoWindow = true;
            pi.WorkingDirectory = Path.GetDirectoryName(packerBin);

            var pi2 = new ProcessStartInfo(packerBin, "0 z:\\updt2013 \"" + Path.Combine(updt, "exe") + "\" update");
            pi2.CreateNoWindow = true;
            pi2.WorkingDirectory = pi.WorkingDirectory;

            /* Обновление удалено! Т.к. не поддерживается сейчас
            var p1 = Process.Start(pi);
            var p2 = Process.Start(pi2);

            p1.WaitForExit();
            p2.WaitForExit();
            if (p1.ExitCode != 0)
                throw new Exception("Ошибка " + p1.ExitCode + " при выполнении " + pi.FileName + " " + pi.Arguments);
            if (p2.ExitCode != 0)
                throw new Exception("Ошибка " + p2.ExitCode + " при выполнении " + pi2.FileName + " " + pi2.Arguments);
                */

            var arcFileName = Path.Combine(release, "rtdb_" + dts);
            pi = new ProcessStartInfo(path7Zip, "a " + arcFileName + ".7z rtdb/ -mx9");
            pi.CreateNoWindow = true;
            pi.WorkingDirectory = release;

            var p3 = Process.Start(pi);
            p3.WaitForExit();

            if (p3.ExitCode != 0)
                throw new Exception("Ошибка " + p3.ExitCode + " при выполнении " + pi.FileName + " " + pi.Arguments);

            pi = new ProcessStartInfo(path7Zip, "a " + arcFileName + ".zip rtdb/ -mx9");
            pi.CreateNoWindow = true;
            pi.WorkingDirectory = release;

            var p4 = Process.Start(pi);
            p4.WaitForExit();

            if (p4.ExitCode != 0)
                throw new Exception("Ошибка " + p4.ExitCode + " при выполнении " + pi.FileName + " " + pi.Arguments);

            pi = new ProcessStartInfo(path7Zip, "a " + arcFileName + "help" + ".7z rtdb/ -mx9");
            pi.CreateNoWindow = true;
            pi.WorkingDirectory = release + "/h";

            var p5 = Process.Start(pi);
            p5.WaitForExit();

            if (p5.ExitCode != 0)
                throw new Exception("Ошибка " + p5.ExitCode + " при выполнении " + pi.FileName + " " + pi.Arguments);

            File.WriteAllLines
            (
                release + "/lastversion.txtn",
                new string[]
                {
                    "string:version:" + dts,
                    "bool:replace:no",
                    "string:ufv:20141112",
                    "string:name:relaxtime",
                    "string:eralyes:",
                    "string:end:это поле всегда должно быть последним"
                }
            );

            File.WriteAllLines
            (
                release + "/lastversionU.txtn",
                new string[]
                {
                    "string:version:" + dts,
                    "bool:replace:no",
                    "string:ufv:20141112",
                    "string:name:update",
                    "string:eralyes:",
                    "string:end:это поле всегда должно быть последним"
                }
            );


            var testKeccak = Path.Combine(BUILD.FullName, @"testKeccak\bin\Release\testKeccak.exe");
            pi = new ProcessStartInfo(testKeccak, "-n");
            pi.CreateNoWindow = true;
            pi.WorkingDirectory = Path.GetDirectoryName(testKeccak);

            var p = Process.Start(pi);
            p.WaitForExit();
            return p.ExitCode;
        }

        /*
            L:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin>
            makecert -r -sky signature -cy authority -e "12/31/2030" -len 4096 -a sha512 -eku "1.3.6.1.5.5.7.3.3" -n "CN=Vinogradov Sergey V. <prg@vs8.ru>, OU=issuer 2014" -sv "L:\tmp\VSVIssuer2014.pvk" "L:\tmp\VSVIssuer2014.cer"

            makecert -sky signature -cy end -e "12/31/2025" -len 4096 -a sha512 -eku "1.3.6.1.5.5.7.3.3" -n "CN=Vinogradov Sergey V. <prg@vs8.ru>, C=RU, OU=blackdisplay 2014" -iky signature -ic "L:\tmp\VSVIssuer2014.cer" -iv "L:\tmp\VSVIssuer2014.pvk" -sv "L:\tmp\BlackDisplay2014.pvk" "L:\tmp\BlackDisplay2014.cer"

            pvk2pfx -po gdklcPR7U5Kr5v8hC5qE2XZfPPQIcK3S -pvk "L:\tmp\BlackDisplay2014.pvk" -spc "L:\tmp\BlackDisplay2014.cer" -pfx "L:\tmp\BlackDisplay2014.pfx"

            signtool sign /fd sha512 /p gdklcPR7U5Kr5v8hC5qE2XZfPPQIcK3S /f "D:\tmpA\BlackDisplay2014.pfx" "D:\tmpA\FileExists.exe"
         * */
        private static void signAssemblies(object sync, string dir, string keyd, string[] files)
        {
            int count = 0;
            foreach (var file in files)
            {
                if (!file.EndsWith("dll") && !file.EndsWith("exe"))
                    continue;

                Interlocked.Increment(ref count);

                var cFile = file;

                log("queue to sign " + file);

                //File.WriteAllText("tmp.txt", "");
                ThreadPool.QueueUserWorkItem
                (
                delegate
                {
                    try
                    {
                        var asm = Path.Combine(dir, cFile);
                        var str = "sign /fd sha512 /p gdklcPR7U5Kr5v8hC5qE2XZfPPQIcK3S /f \"" + keyd + "\\key.pfx\" \"" + asm + "\"";
                        /*do
                        {
                            try
                            {
                                File.AppendAllText("tmp.txt", str + "\r\n");
                                str = "";
                            }
                            catch
                            {}
                        }
                        while (str != "");*/
                        var pis = new ProcessStartInfo(signtoolPath, str);
                        pis.CreateNoWindow = true;
                        pis.UseShellExecute = false;
                        pis.RedirectStandardError  = true;
                        pis.RedirectStandardOutput = true;
                        //pis.WorkingDirectory = dir;
                        var p = Process.Start(pis);
                        log("exec to sign for file " + cFile + "\r\n" + "command " + pis.FileName + " " + pis.Arguments, true);
                        p.WaitForExit();
                        var e = p.StandardError.ReadToEnd();
                        var o = p.StandardOutput.ReadToEnd();
                        log("signed " + cFile + " with exitCode " + p.ExitCode + "\r\nerrors:\r\n" + e + "\r\noutput:\r\n" + o, true);
                    }
                    catch (Exception ex)
                    {
                        log("exception in the sign time " + cFile);
                        log(ex.Message, true);
                        throw ex;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref count);
                        lock (sync)
                            Monitor.Pulse(sync);
                    }
                }
                );
            }

            lock (sync)
            {
                while (count > 0)
                    Monitor.Wait(sync);
            }
        }

        /// <summary>
        /// Копирует файлы updatorFiles из source в target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="updatorFiles"></param>
        /// <param name="target"></param>
        private static void copyAllFiles(string source, string[] updatorFiles, string target)
        {
            foreach (var file in updatorFiles)
            {
                File.Copy(  Path.Combine(source, file), Path.Combine(target, file)  );
            }
        }

        private static void copyAllContainsFiles(string source, string dest)
        {
            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);

            var files = Directory.EnumerateFiles(source);
            foreach (var file in files)
            {
                File.Copy
                (
                    file,
                    Path.Combine(  dest, Path.GetFileName(file)  ),
                    true
                );
            }

            var dirs = Directory.EnumerateDirectories(source);
            foreach (var dir in dirs)
            {
                var newDir = Path.Combine(  dest, Path.GetFileName(dir)  );
                if (!Directory.Exists(newDir))
                    Directory.CreateDirectory(newDir);

                copyAllContainsFiles(Path.Combine(source, Path.GetFileName(dir)), newDir);
            }
        }
    }
}
