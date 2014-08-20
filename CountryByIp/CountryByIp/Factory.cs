using System.Configuration;

namespace CountryByIp
{
    public interface IFactory
    {
        IDataUpdater Updater { get; }
    }
    public sealed class Factory : IFactory
    {
        private readonly IDataUpdater _updater;
        public static readonly IFactory Get = new Factory();
        

        private Factory()
        {
            IConfig conf = new Config();

            if (!conf.HasDbConnectionString)
            {
                throw new ConfigurationErrorsException("Missing CountryByIp.exe.config with connectionStrings name='CountryByIpDb'");
            }
            IResourceManager res = new ResourceManager();
            ICountriesList countries = new CountriesList(res);
            IDbManager db = new DbManager(conf);
            ICsvParser csv = new CsvParser();
            ILog log = new Log();
            _updater = new DataUpdater(db, countries, csv, res, conf, log);
        }

        IDataUpdater IFactory.Updater { get { return _updater; } }
    }
}