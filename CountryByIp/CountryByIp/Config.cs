using System.Configuration;

namespace CountryByIp
{
    public interface IConfig
    {
        bool HasDbConnectionString { get; }
        string DbConnectionString { get; }
        string DbTableName { get; }
        string CsvSiteRoot { get; }
    }
    internal sealed class Config : IConfig
    {
        bool IConfig.HasDbConnectionString { get { return ConfigurationManager.ConnectionStrings["CountryByIpDb"] != null; } }
        string IConfig.DbConnectionString { get { return ConfigurationManager.ConnectionStrings["CountryByIpDb"].ConnectionString; } }
        string IConfig.DbTableName { get { return ConfigurationManager.AppSettings["DbTableName"]; } }
        string IConfig.CsvSiteRoot { get { return ConfigurationManager.AppSettings["CsvSiteRoot"]; } }
    }
}