using CountryByIp;
using NUnit.Framework;

namespace CountryByIpTests
{
    [TestFixture]
    public class CountriesListTests
    {
        [Test]
        public void CanGetCountries()
        {
            ICountriesList cl = new CountriesList(new ResourceManager());
            var list = cl.GetCountries();

            Assert.That(list, Is.Not.Null);
            Assert.That(list.Length, Is.GreaterThan(1));
            Assert.That(list[0], Is.Not.Null);
            Assert.That(list[0].Code, Is.Not.Null);
            Assert.That(list[0].Name, Is.Not.Null);
        }
    }
}
