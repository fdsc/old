using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;

namespace updator
{
    class response
    {
        static private void setReferer(String referer, HttpWebRequest myHttpWebRequest)
        {
            if (!String.IsNullOrEmpty(referer))
                myHttpWebRequest.Referer = referer;
        }

        static private void setUAandLanguage(HttpWebRequest myHttpWebRequest)
        {
            myHttpWebRequest.Headers.Add("Accept-Language: ru");
            myHttpWebRequest.Headers.Add("Accept-Charset: windows-1251,utf-8");
            myHttpWebRequest.UserAgent = String.Format("vs8.ru updator/{0}", updator.version);
        }

        private void getCookies(HttpWebResponse myHttpWebResponse)
        {
            cookies.Add(myHttpWebResponse.Cookies);
        }

        private string getMovedLocation(HttpWebResponse myHttpWebResponse)
        {
            String hh = myHttpWebResponse.ResponseUri.AbsolutePath;
            Uri host = new Uri("http://" + myHttpWebResponse.ResponseUri.Host + hh);
            String loc = myHttpWebResponse.Headers["Location"];
            return new Uri(host, loc).AbsoluteUri;
        }

        public String getLocation(HttpWebResponse myHttpWebResponse)
        {
            //Uri host = new Uri("http://" + myHttpWebResponse.ResponseUri.Host);
            String hh = myHttpWebResponse.ResponseUri.AbsolutePath;
            Uri host = new Uri("http://" + myHttpWebResponse.ResponseUri.Host + hh);
            String loc = myHttpWebResponse.Headers["Location"];
            return host.AbsoluteUri + loc; //new Uri(host, loc).AbsoluteUri;
        }

        private void setCookies(HttpWebRequest myHttpWebRequest)
        {
            myHttpWebRequest.CookieContainer = new CookieContainer();
            myHttpWebRequest.CookieContainer.Add(cookies);
        }

        private void setHeaders(HttpWebRequest myHttpWebRequest, string referer)
        {
            // myHttpWebRequest.Proxy = New WebProxy("127.0.0.1", 8888);

            setReferer(referer, myHttpWebRequest);
            setUAandLanguage(myHttpWebRequest);
            setCookies(myHttpWebRequest);
        }

        CookieCollection cookies = new CookieCollection();
        public String lastEncoding = "windows-1251";
        public string getPage(string hostName, string referer)
        {
            lastPage = hostName;

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(hostName);
            // myHttpWebRequest.KeepAlive = true;
            setHeaders(myHttpWebRequest, referer);

            HttpWebResponse myHttpWebResponse;
            try
            {
                myHttpWebRequest.Timeout = 50 * 1000;
                myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.NameResolutionFailure)
                    return "\\error\\nameresolution";

                return "\\error\\" + e.Message;
            }
            catch (Exception e)
            {
                return "\\error\\" + e.Message;
            }
            getCookies(myHttpWebResponse);

            /*if (myHttpWebResponse.StatusCode == HttpStatusCode.Redirect)
            {
                String newLoc = getLocation(myHttpWebResponse);
                myHttpWebResponse.Close();
                return getPage(newLoc, hostName);
            }*/
            lastPage = myHttpWebResponse.ResponseUri.AbsoluteUri;

            String encoding = GetEncoding(myHttpWebResponse);

            using (StreamReader myStreamReader = new StreamReader(
                                            myHttpWebResponse.GetResponseStream(), Encoding.GetEncoding(encoding)
                                                           ))
            {
                String result = /*"";//*/ myStreamReader.ReadToEnd();
                return result;
            }
        }

        public byte[] getFile(string hostName, string referer)
        {
            HttpStatusCode status = 0;
            long ctLen = 0;
            var a = 0;
            BytesBuilder bb;
            byte[] bt = null;
            try
            {
                bt = getFile(hostName, referer, out ctLen, out status);

                if (bt.Length >= ctLen)
                    return bt;

                bb = new BytesBuilder();
                bb.add(bt);

                a = bt.Length;
            }
            catch
            {
                bb = new BytesBuilder();
            }

            var size = a;
            if (size < 1400)
                size = 64*1024;

            var tryCount = 0;
            do
            {
                long ctLen2 = 0;
                
                try
                {
                    updator.toUpdateLog("try to partial download from " + a + ", with size " + size + "\r\n\r\n");

                    status = 0;
                    bt = getFile(hostName, referer, out ctLen2, out status, a, (a + size < (int) ctLen) ? a + size : (int) ctLen);
                    a += bt.Length;
                    tryCount = 0;
                }
                catch (WebException e)
                {
                    tryCount++;

                    try
                    {
                        if (e.Response != null && ((HttpWebResponse) e.Response).StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
                            break;
                    }
                    catch (Exception ex)
                    {
                        updator.toUpdateLog("error (getFile) with message " + ex.Message + "\r\n" + ex.StackTrace + "\r\n");
                    }

                    updator.toUpdateLog("error (getFile) with message " + e.Message + "\r\n" + e.StackTrace + "\r\n\r\n");

                    size -= size >> 2;
                    if (size < 1400*4)
                        size = 1400*4;

                    if (tryCount > 16)
                        throw;

                    continue;
                }
                catch (Exception e)
                {
                    tryCount++;

                    updator.toUpdateLog("error (getFile) with message " + e.Message + "\r\n" + e.StackTrace + "\r\n\r\n");

                    size -= size >> 2;
                    if (size < 1400*4)
                        size = 1400*4;

                    if (tryCount > 16)
                        throw;

                    continue;
                }

                if (bt.Length <= 0)
                    break;

                updator.toUpdateLog("success partial download for one part with size " + bt.Length + "\r\n\r\n");
                bb.add(bt);
            }
            while (a < ctLen || ctLen == 0);

            return bb.getBytes();
        }

        public byte[] getFile(string hostName, string referer, out long ctLen, out HttpStatusCode status, int start = -1, int end = 0)
        {
            lastPage = hostName;

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(hostName);
            // myHttpWebRequest.KeepAlive = true;
            setHeaders(myHttpWebRequest, referer);

            HttpWebResponse myHttpWebResponse;
            try
            {
                myHttpWebRequest.Timeout = 25000;
                myHttpWebRequest.ReadWriteTimeout = 25000;
                if (start >= 0)
                    if (end == 0)
                        myHttpWebRequest.AddRange(start);
                    else
                        myHttpWebRequest.AddRange(start, end);
                    //myHttpWebRequest.Headers.Add("Range: bytes=" + start + "-");

                myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
            }
            catch (Exception e)
            {
                updator.toUpdateLog("error with message " + e.Message + "\r\n" + e.StackTrace + "\r\n\r\n");
                ctLen = 0;
                status = 0;
                return null;
            }
            getCookies(myHttpWebResponse);

            /*if (myHttpWebResponse.StatusCode == HttpStatusCode.Redirect)
            {
                String newLoc = getLocation(myHttpWebResponse);
                myHttpWebResponse.Close();
                return getPage(newLoc, hostName);
            }*/
            lastPage = myHttpWebResponse.ResponseUri.AbsoluteUri;

            String encoding = GetEncoding(myHttpWebResponse);

            status = myHttpWebResponse.StatusCode;

            // Content-Length: 8982968
            ctLen = myHttpWebResponse.ContentLength; //Int32.Parse(myHttpWebResponse.Headers["Content-Length"]);

            using (  var s = myHttpWebResponse.GetResponseStream()  )
            using (  var ms = new MemoryStream()  )
            {
                s.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private string GetEncoding(HttpWebResponse myHttpWebResponse)
        {
            lastEncoding = myHttpWebResponse.CharacterSet;

            if (lastEncoding == null || lastEncoding.Equals("ISO-8859-1"))  // Вообще, это 1252 - "по-умолчанию"
                return "windows-1251";

            return lastEncoding;
            /*
            String cntType = myHttpWebResponse.ContentType;
            int index = cntType.IndexOf("charset=") + "charset=".Length;
            if (index == -1)
                return myHttpWebResponse.CharacterSet;

            int y = cntType.IndexOf(";", index + 1);
            y = y == -1 ? cntType.Length : y;
            return cntType.Substring(index, y - index);
            */
        }

        private void action_postStringToServer(HttpWebRequest myHttpWebRequest, string acceptLine)
        {
            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            myHttpWebRequest.Timeout = 8000;
            byte[] ByteArr = System.Text.Encoding.GetEncoding(1251).GetBytes(acceptLine);
            myHttpWebRequest.ContentLength = ByteArr.Length;
            using (Stream webStream = myHttpWebRequest.GetRequestStream())
            {
                webStream.Write(ByteArr, 0, ByteArr.Length);
            }
        }

        Random rnd = new Random();
        private void postString(HttpWebRequest myHttpWebRequest, string referer, string acceptLine)
        {
            myHttpWebRequest.KeepAlive = true;
            myHttpWebRequest.Method = "POST";
            setHeaders(myHttpWebRequest, referer);
        //    myHttpWebRequest.Headers.Add("X-Forwarded-For: " + rnd.Next(255) + "." + rnd.Next(255) + "." + rnd.Next(255) + "." + rnd.Next(255));
            action_postStringToServer(myHttpWebRequest, acceptLine);
        }

        public string postString(string hostName, string referer, string acceptLine)
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(hostName);
            myHttpWebRequest.AllowAutoRedirect = false;

            postString(myHttpWebRequest, referer, acceptLine);

            return getResponse(myHttpWebRequest, hostName);
        }

        public String lastPage = "";
        private string getResponse(HttpWebRequest myHttpWebRequest, string hostName)
        {
            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
            getCookies(myHttpWebResponse);

            if (myHttpWebResponse.StatusCode == HttpStatusCode.Redirect)
            {
                String newLoc = getLocation(myHttpWebResponse);
                myHttpWebResponse.Close();
                return getPage(newLoc, hostName);
            }
            else if (myHttpWebResponse.StatusCode == HttpStatusCode.Moved)
            {
                String newLoc = getMovedLocation(myHttpWebResponse);
                myHttpWebResponse.Close();
                return getPage(newLoc, hostName);
            }
            else
            {
                lastPage = hostName;
                using (StreamReader myStreamReader = new StreamReader(
                                                myHttpWebResponse.GetResponseStream(), Encoding.GetEncoding(lastEncoding)
                                                               ))
                {
                    return /*"";//*/ myStreamReader.ReadToEnd(); // "" - ERROR!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                }
            }
        }

        public static String convert(String str, Encoding srcE, Encoding dstE)
        {
            return dstE.GetString(
                Encoding.Convert(srcE, dstE, srcE.GetBytes(str))
                );
        }
    }













    // Копия в updator
    public class BytesBuilder
    {
        public List<byte[]> bytes = new List<byte[]>();

        public long Count
        {
            get
            {
                return count;
            }
        }

        public byte[] getBlock(int number)
        {
            return bytes[number];
        }

        long count = 0;
        public void add(byte[] bytesToAdded, int index = -1, bool isNoConstant = false)
        {
            if (isNoConstant)
            {
                var b = new byte[bytesToAdded.LongLength];
                BytesBuilder.CopyTo(bytesToAdded, b);
                bytesToAdded = b;
            }

            if (index == -1)
                bytes.Add(bytesToAdded);
            else
                bytes.Insert((int) index, bytesToAdded);

            count += bytesToAdded.LongLength;
        }

        public void addCopy(byte[] bytesToAdded, int index = -1)
        {
            add(bytesToAdded, index, true);
        }

        public void addByte(byte number, int index = -1)
        {
            var n = new byte[1];
            n[0] = number;
            add(n, index);
        }

        public void addUshort(ushort number, int index = -1)
        {
            var n = new byte[2];
            n[1] = (byte) (number >> 8);
            n[0] = (byte) (number     );
            add(n, index);
        }

        public void addInt(int number, int index = -1)
        {
            var n = new byte[4];
            n[3] = (byte) (number >> 24);
            n[2] = (byte) (number >> 16);
            n[1] = (byte) (number >> 8);
            n[0] = (byte) (number     );

            add(n, index);
        }

        public void addULong(ulong number, int index = -1)
        {
            var n = new byte[8];
            n[7] = (byte) (number >> 56);
            n[6] = (byte) (number >> 48);
            n[5] = (byte) (number >> 40);
            n[4] = (byte) (number >> 32);
            n[3] = (byte) (number >> 24);
            n[2] = (byte) (number >> 16);
            n[1] = (byte) (number >> 8);
            n[0] = (byte) (number     );

            add(n, index);
        }

        public void addVariableULong(ulong number, int index = -1)
        {
            byte[] target = null;
            BytesBuilder.VariableULongToBytes(number, ref target);

            add(target, index);
        }

        public void add(string utf8String, int index = -1)
        {
            add(UTF8Encoding.UTF8.GetBytes(utf8String), index);
        }

        public void clear(bool fast = false)
        {
            if (!fast)
            {
                foreach (byte[] e in bytes)
                    BytesBuilder.ToNull(e);
            }

            count = 0;
            bytes.Clear();
        }

        public long RemoveLastBlock()
        {
            if (count == 0)
                return 0;
            
            long removedLength = bytes[bytes.Count - 1].LongLength;
            var tmp = bytes[bytes.Count - 1];
            bytes.RemoveAt(bytes.Count - 1);
            BytesBuilder.BytesToNull(tmp);

            count -= removedLength;
            return removedLength;
        }

        public long RemoveBlockAt(int position)
        {
            if (count == 0 || position < 0 || position >= bytes.Count)
                return 0;

            long removedLength = bytes[position].LongLength;
            var tmp = bytes[position];
            bytes.RemoveAt(position);
            BytesBuilder.BytesToNull(tmp);

            count -= removedLength;
            return removedLength;
        }

        public long RemoveBlocks(int position, int endPosition)
        {
            if (count == 0 || position < 0 || position >= bytes.Count || position > endPosition || endPosition >= bytes.Count)
                return 0;

            long removedLength = 0;

            for (int i = position; i <= endPosition; i++)
            {
                var tmp = bytes[position];
                removedLength += RemoveBlockAt(position);
                BytesBuilder.BytesToNull(tmp);
            }

            return removedLength;
        }

        public byte[] getBytes(long resultCount = -1)
        {
            if (resultCount == -1 || resultCount > count)
                resultCount = count;

            byte[] result = new byte[resultCount];

            long cursor = 0;
            for (int i = 0; i < bytes.Count; i++)
            {
                if (cursor >= result.LongLength)
                    break;

                CopyTo(bytes[i], result, cursor);
                cursor += bytes[i].LongLength;
            }

            return result;
        }

        public byte[] getBytes(long resultCount, long index)
        {
            if (resultCount == -1 || resultCount > count)
                resultCount = count;

            byte[] result = new byte[resultCount];

            long cursor = 0;
            long tindex = 0;
            for (int i = 0; i < bytes.Count; i++)
            {
                if (cursor >= result.Length)
                    break;

                if (tindex + bytes[i].LongLength < index)
                {
                    tindex += bytes[i].LongLength;
                    continue;
                }

                CopyTo(bytes[i], result, cursor, resultCount - cursor, index - tindex);
                cursor += bytes[i].LongLength;
                tindex += bytes[i].LongLength;
            }

            return result;
        }

        public static unsafe byte[] CloneBytes(byte[] B, long start, long PostEnd)
        {
            var result = new byte[PostEnd - start];
            fixed (byte * r = result, b = B)
                BytesBuilder.CopyTo(PostEnd, PostEnd - start, b, r, 0, -1, start);

            return result;
        }

        public static unsafe byte[] CloneBytes(byte * b, long start, long PostEnd)
        {
            var result = new byte[PostEnd - start];
            fixed (byte * r = result)
                BytesBuilder.CopyTo(PostEnd, PostEnd - start, b, r, 0, -1, start);

            return result;
        }

        /// <summary>
        /// Копирует массив source в массив target. Если запрошенное количество байт скопировать невозможно, копирует те, что возможно
        /// </summary>
        /// <param name="source">Источник копирования</param>
        /// <param name="target">Приёмник</param>
        /// <param name="targetIndex">Начальный индекс копирования в приёмник</param>
        /// <param name="count">Максимальное количество байт для копирования (-1 - все доступные)</param>
        /// <param name="index">Начальный индекс копирования из источника</param>
        public unsafe static long CopyTo(byte[] source, byte[] target, long targetIndex = 0, long count = -1, long index = 0)
        {
            long sl = source.LongLength;
            if (count < 0)
                count = sl;

            /*
            long firstUncopied = index + count;
            if (firstUncopied > source.Length)
                firstUncopied = source.Length;*/

            fixed (byte * s = source, t = target)
            {
                return CopyTo(sl, target.LongLength, s, t, targetIndex, count, index);
            }
        }

        unsafe public static long CopyTo(long sourceLength, long targetLength, byte* s, byte* t, long targetIndex = 0, long count = -1, long index = 0)
        {
            byte* se = s + sourceLength;
            byte* te = t + targetLength;

            if (count == -1)
            {
                count = Math.Min(sourceLength - index, targetLength - targetIndex);
            }

            byte* sec = s + index + count;
            byte* tec = t + targetIndex + count;

            byte* sbc = s + index;
            byte* tbc = t + targetIndex;

            if (sec > se)
            {
                tec -= sec - se;
                sec = se;
            }

            if (tec > te)
            {
                sec -= tec - te;
                tec = te;
            }

            if (tbc < t)
                throw new ArgumentOutOfRangeException();

            if (sbc < s)
                throw new ArgumentOutOfRangeException();

            if (sec - sbc != tec - tbc)
                throw new OverflowException("BytesBuilder.CopyTo: fatal algorithmic error");


            ulong* sbw = (ulong*)sbc;
            ulong* tbw = (ulong*)tbc;

            ulong* sew = sbw + ((sec - sbc) >> 3);

            for (; sbw < sew; sbw++, tbw++)
                *tbw = *sbw;

            byte toEnd = (byte)(((int)(sec - sbc)) & 0x7);

            byte* sbcb = (byte*)sbw;
            byte* tbcb = (byte*)tbw;
            byte* sbce = sbcb + toEnd;

            for (; sbcb < sbce; sbcb++, tbcb++)
                *tbcb = *sbcb;


            return sec - sbc;
        }

        unsafe public static long ToNull(byte[] t, long index = 0, long count = -1)
        {
            fixed (byte* tb = t)
            {
                return ToNull(t.LongLength, tb, index, count);
            }
        }

        unsafe public static long ToNull(long targetLength, byte* t, long index = 0, long count = -1)
        {
            if (count < 0)
                count = targetLength;

            byte* te = t + targetLength;

            byte* tec = t + index + count;
            byte* tbc = t + index;

            if (tec > te)
            {
                tec = te;
            }

            if (tbc < t)
                throw new ArgumentOutOfRangeException();

            ulong* tbw = (ulong*)tbc;

            ulong* tew = tbw + ((tec - tbc) >> 3);

            for (; tbw < tew; tbw++)
                *tbw = 0;

            byte toEnd = (byte)(((int)(tec - tbc)) & 0x7);

            byte* tbcb = (byte*)tbw;
            byte* tbce = tbcb + toEnd;

            for (; tbcb < tbce; tbcb++)
                *tbcb = 0;


            return tec - tbc;
        }

        public unsafe static void UIntToBytes(uint data, ref byte[] target, long start = 0)
        {
            if (target == null)
                target = new byte[4];

            if (start < 0 || start + 4 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start; i < start + 4; i++)
                {
                    *(t + i) = (byte) data;
                    data = data >> 8;
                }
            }
        }

        public unsafe static void ULongToBytes(ulong data, ref byte[] target, long start = 0)
        {
            if (target == null)
                target = new byte[8];

            if (start < 0 || start + 8 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start; i < start + 8; i++)
                {
                    *(t + i) = (byte) data;
                    data = data >> 8;
                }
            }
        }

        public unsafe static void BytesToULong(out ulong data, byte[] target, long start)
        {
            data = 0;
            if (start < 0 || start + 8 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start + 8 - 1; i >= start; i--)
                {
                    data <<= 8;
                    data += *(t + i);
                }
            }
        }

        public unsafe static int BytesToVariableULong(out ulong data, byte[] target, long start)
        {
            data = 0;
            if (start < 0)
                throw new IndexOutOfRangeException();

            int j = 0;
            for (long i = start; i < target.LongLength; i++, j++)
            {
                int b = target[i] & 0x80;
                if (b == 0)
                    break;
            }

            if ((target[start + j] & 0x80) > 0)
                throw new IndexOutOfRangeException();

            for (long i = start + j; i >= start; i--)
            {
                byte b = target[i];
                int  c = b & 0x7F;

                data <<= 7;
                data += (byte) c;
            }

            return j + 1;
        }

        public unsafe static void VariableULongToBytes(ulong data, ref byte[] target, long start = 0)
        {
            if (start < 0)
                throw new IndexOutOfRangeException();

            BytesBuilder bb = new BytesBuilder();
            for (long i = start; ; i++)
            {
                byte b = (byte) (data & 0x7F);

                data >>= 7;
                if (data > 0)
                    b |= 0x80;

                if (target == null)
                    bb.addByte(b);
                else
                    target[i] = b;

                if (data == 0)
                    break;
            }

            if (target == null)
            {
                target = new byte[bb.Count];
                BytesBuilder.CopyTo(bb.getBytes(), target, start);
            }
            /*else
            if (start + bb.Count > target.LongLength)
                throw new IndexOutOfRangeException();*/
        }

        public unsafe static void BytesToNull(byte[] bytes, long firstNotNull = long.MaxValue, long start = 0)
        {
            if (firstNotNull > bytes.LongLength)
                firstNotNull = bytes.LongLength;

            if (start < 0)
                start = 0;

            fixed (byte * b = bytes)
            {
                ulong * lb = (ulong *) (b + start);

                ulong * le = lb + ((firstNotNull - start) >> 3);

                for (; lb < le; lb++)
                    *lb = 0;

                byte toEnd = (byte) (  ((int) (firstNotNull - start)) & 0x7  );

                byte * bb = (byte *) lb;
                byte * be = bb + toEnd;

                for (; bb < be; bb++)
                    *bb = 0;
            }
        }

        public unsafe static bool Compare(byte[] wellHash, byte[] hash)
        {
            if (wellHash.LongLength != hash.LongLength || wellHash.LongLength < 0)
                return false;

            fixed (byte * w1 = wellHash, h1 = hash)
            {
                byte * w = w1, h = h1, S = w1 + wellHash.LongLength;

                for (; w < S; w++, h++)
                {
                    if (*w != *h)
                        return false;
                }
            }

            return true;
        }

        public unsafe static bool Compare(byte[] wellHash, byte[] hash, out int i)
        {
            i = -1;
            if (wellHash.LongLength != hash.LongLength || wellHash.LongLength < 0)
                return false;

            i++;
            fixed (byte * w1 = wellHash, h1 = hash)
            {
                byte * w = w1, h = h1, S = w1 + wellHash.LongLength;

                for (; w < S; w++, h++, i++)
                {
                    if (*w != *h)
                        return false;
                }
            }

            return true;
        }

        unsafe public static void ClearString(string resultText)
        {
            if (resultText == null)
                return;

            fixed (char * b = resultText)
            {
                for (int i = 0; i < resultText.Length; i++)
                {
                    *(b + i) = ' ';
                }
            }
        }
    }
}
