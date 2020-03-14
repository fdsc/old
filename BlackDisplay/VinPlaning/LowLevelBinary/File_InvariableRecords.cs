using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VinPlaning.LowLevelBinary
{
    public abstract class File_InvariableRecords: IDisposable
    {
        public readonly int    invariableRecordLength;
        public readonly long   startShift;
        public readonly string FileName;
        public File_InvariableRecords(int recordLength, string FileName, long startShift = 0)
        {
            invariableRecordLength = recordLength;
            this.FileName          = FileName;
            this.startShift        = startShift;
        }

        public abstract void   open ();
        public abstract void   close();


        public abstract byte[] getRecord     (long recordNumber, byte[] buffer = null);
        public abstract void   setRecord     (long recordNumber, byte[] record);
        public abstract void   InsertRecord  (long recordNumber, byte[] record, long fileLen = 0);
        public abstract void   addRecordToEnd(byte[] record);

        public virtual void Dispose()
        {
            close();
        }
    }

    public class File_InvariableRecordsNonReaded: File_InvariableRecords
    {

        FileStream stream = null;
        public File_InvariableRecordsNonReaded(int recordLength, string FileName): base(recordLength, FileName)
        {
        }

        public override void open()
        {
            stream = File.Open(FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
        }

        public override void close()
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
        }

        public override byte[] getRecord(long recordNumber, byte[] buffer = null)
        {
            byte[] result;
            if (buffer == null || buffer.Length < invariableRecordLength)
                result = new byte[invariableRecordLength];
            else
                result = buffer;

            bool isOpened = stream != null;
            if (!isOpened)
                open();

            try
            {
                stream.Seek(startShift + recordNumber * invariableRecordLength, SeekOrigin.Begin);
                stream.Read(result, 0, result.Length);
            }
            finally
            {
                if (!isOpened)
                    close();
            }

            return result;
        }

        public override void   setRecord(long recordNumber, byte[] record)
        {
            bool isOpened = stream != null;
            if (!isOpened)
                open();

            try
            {
                stream.Seek(startShift + recordNumber * invariableRecordLength, SeekOrigin.Begin);
                stream.Write(record, 0, invariableRecordLength);
            }
            finally
            {
                if (!isOpened)
                    close();
            }
        }

        public override void   InsertRecord(long recordNumber, byte[] record, long fileLen = 0)
        {
            bool isOpened = stream != null;
            var buffer = new byte[invariableRecordLength];

            if (!isOpened)
                open();

            if (fileLen <= 0)
                fileLen = stream.Length;

            fileLen -= invariableRecordLength;
            try
            {
                long position = startShift + recordNumber * invariableRecordLength;
                do
                {
                    getRecord(position, buffer);
                    setRecord(position, record);

                    position += invariableRecordLength;
                    record = buffer;
                }
                while (position <= fileLen);
                setRecord(position, record);        // последнюю запись дописываем уже после конца fileLen, ничего оттуда не считывая

            }
            finally
            {
                if (!isOpened)
                    close();
            }
        }

        public override void   addRecordToEnd(byte[] record)
        {
            bool isOpened = stream != null;
            if (!isOpened)
                open();

            try
            {
                stream.Seek(0, SeekOrigin.End);
                stream.Write(record, 0, invariableRecordLength);
            }
            finally
            {
                if (!isOpened)
                    close();
            }
        }
    }
}
