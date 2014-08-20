using System;
using System.Linq;
using CountryByIp;
using NUnit.Framework;

namespace CountryByIpTests
{
    [TestFixture]
    public class CsvParserTests
    {
        [Test]
        public void CanGetRanges()
        {
            const string csvStr = @"
21.0.0.0,21.255.255.255,16777216,01/07/91,
22.0.0.0,22.255.255.255,16777216,26/06/89,
23.0.0.0,23.15.255.255,1048576,17/12/10,
23.18.0.0,23.18.255.255,65536,12/09/11,
23.19.0.0,23.19.255.255,65536,25/04/11,
23.20.0.0,23.23.255.255,262144,19/09/11,
23.24.0.0,23.25.255.255,131072,13/01/12,
23.26.0.0,23.26.255.255,65536,03/02/12,
23.27.0.0,23.27.255.255,65536,13/02/12,
23.28.0.0,23.28.255.255,65536,21/02/12,
23.30.0.0,23.31.255.255,131072,30/04/12,
23.32.0.0,23.63.255.255,2097152,16/05/11,
";

            ICsvParser csv = new CsvParser();
            var ranges = csv.GetRanges(csvStr);

            Assert.That(ranges, Is.Not.Null);
            var list = ranges.ToList();

            Assert.That(list.Count, Is.EqualTo(12));
            Assert.That(list[3].From, Is.EqualTo(387055616));//23.18.0.0
            Assert.That(list[3].To, Is.EqualTo(387121151));//23.18.255.255
            Assert.That(list[3].Count, Is.EqualTo(65536));
            Assert.That(list[3].Assigned, Is.EqualTo(new DateTime(2011,09,12)));
        }

        [Test]
        [TestCase("216.19.185.124", 3625171324)]
        [TestCase("24.60.0.0", (uint)406585344)]
        [TestCase("192.168.0.1", 3232235521)]
        [TestCase("198.61.161.118", 3325927798)]
        [TestCase("0.0.0.0", (uint)0)]
        [TestCase("255.255.255.255", 4294967295)]
        public void CanParseIp(string ipStr, uint expectedInt)
        {
            ICsvParser csv = new CsvParser();
            var result = csv.IpStringToLong(ipStr);

            Assert.That(result, Is.EqualTo(expectedInt));
        }

        [Test]
        [TestCase("216.19.185.124", 3625171324)]
        [TestCase("24.60.0.0", (uint)406585344)]
        [TestCase("192.168.0.1", 3232235521)]
        [TestCase("198.61.161.118", 3325927798)]
        [TestCase("0.0.0.0", (uint)0)]
        [TestCase("255.255.255.255", 4294967295)]
        public void CanStringifyIp(string expectedStr, uint ipInt)
        {
            ICsvParser csv = new CsvParser();
            var result = csv.IpLongToString(ipInt);

            Assert.That(result, Is.EqualTo(expectedStr));
        }
    }
}