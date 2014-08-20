using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CountryByIp
{
    public class RangeOfIps
    {
        public long From;
        public long To;
        public long Count;
        public DateTime Assigned;
    }

    public interface ICsvParser
    {
        IEnumerable<RangeOfIps> GetRanges(string csv);
        long IpStringToLong(string ipStr);
        string IpLongToString(long ipLong);
    }
    internal sealed class CsvParser : ICsvParser
    {
        private readonly ICsvParser _interface;

        internal CsvParser()
        {
            _interface = this;
        }

        IEnumerable<RangeOfIps> ICsvParser.GetRanges(string csv)
        {
            if(String.IsNullOrEmpty(csv)) yield break;

            foreach (var row in csv.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var rowTrimmed = row.Trim().Trim(',');
                if(rowTrimmed == "") continue;

                var parts = rowTrimmed.Trim(',').Split(',');
                RangeOfIps range;
                try
                {
                    range = new RangeOfIps
                            {
                                From = ParseIpToLong(parts, 0),
                                To = ParseIpToLong(parts, 1),
                                Count = ParseInt(parts, 2),
                                Assigned = ParseDate(parts, 3)
                            };
                }
                catch (InvalidDataException ex)
                {
                    range = null;
                }
                if (range != null)
                {
                    yield return range;
                }
            }
        }
        long ICsvParser.IpStringToLong(string ipStr)
        {
            var parts = ipStr.Trim().Split('.');
            if(parts.Length < 4) throw new ArgumentException("IP not as expected '"+ipStr+"'");
            var a = Byte.Parse(parts[0]);
            var b = Byte.Parse(parts[1]);
            var c = Byte.Parse(parts[2]);
            var d = Byte.Parse(parts[3]);
            return ((long)a << (8*3)) + (b << (8*2)) + (c << (8*1)) + (d << (8*0));
        }
        string ICsvParser.IpLongToString(long ipLong)
        {
            var b = BitConverter.GetBytes(ipLong);
            return b[3] + "." + b[2] + "." + b[1] + "." + b[0];
        }

        private long ParseIpToLong(string[] parts, int index)
        {
            VerifyIndex(parts, index);
            return _interface.IpStringToLong(parts[index]);
        }
        private long ParseInt(string[] parts, int index)
        {
            VerifyIndex(parts, index);
            return Int32.Parse(parts[index]);
        }
        private DateTime ParseDate(string[] parts, int index)
        {
            VerifyIndex(parts, index);
            return DateTime.ParseExact(parts[index], @"dd/MM/yy", null);
        }

        private void VerifyIndex(string[] parts, int index)
        {
            if (parts.Length <= index || index < 0)
                throw new InvalidDataException("Invalid Index "+index);
        }
    }
}
