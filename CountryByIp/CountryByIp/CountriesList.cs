using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CountryByIp
{
    public class CountryInfo
    {
        public string Code;
        public string Name;
    }
    public interface ICountriesList
    {
        CountryInfo[] GetCountries();
    }
    internal sealed class CountriesList : ICountriesList
    {
        private readonly IResourceManager _res;

        internal CountriesList(IResourceManager res)
        {
            _res = res;
        }

        CountryInfo[] ICountriesList.GetCountries()
        {
            var xml = _res.GetResource("CountryByIp.Files.Countries.xml");

            return GetXmlValuesByXPath(
                        XDocument.Parse(xml),
                        "/table/tbody/tr/td/a",
                        xo =>
                        {
                            var elem = (XElement)xo;
                            return new CountryInfo
                                   {
                                       Code = elem.Attribute("href").Value.Replace(".html", ""),
                                       Name = elem.Value
                                   };
                        })
                        .ToArray();
        }

        
        private static IEnumerable<T> GetXmlValuesByXPath<T>(XDocument projXml, string xPath, Func<XObject, T> getter)
        {
            if (projXml.Root == null) yield break;
            var ns = projXml.Root.Name.Namespace;
            var r = new XmlNamespaceManager(new NameTable());
            r.AddNamespace("n", ns.NamespaceName);
            var objects = projXml.XPathEvaluate(xPath, r) as IEnumerable<object>;
            if (objects == null) yield break;
            foreach (var obj in objects)
            {
                var xObj = obj as XObject;
                yield return getter(xObj);
            }
        }
    }
}