using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CountryByIp
{
    public interface IDataUpdater
    {
        event Action OnEnd;
        event Action<string> OnCountryComplete;
        void Execute();
    }
    internal sealed class DataUpdater : IDataUpdater
    {
        private readonly IDbManager _db;
        private readonly ICountriesList _countries;
        private readonly ICsvParser _csv;
        private readonly IResourceManager _res;
        private readonly IConfig _conf;
        private readonly ILog _log;

        internal DataUpdater(IDbManager db, ICountriesList countries, ICsvParser csv, IResourceManager res, IConfig conf, ILog log)
        {
            _db = db;
            _countries = countries;
            _csv = csv;
            _res = res;
            _conf = conf;
            _log = log;
        }


        public event Action OnEnd;
        public event Action<string> OnCountryComplete;
        private void RunOnEnd() { if (OnEnd != null) OnEnd(); }
        private void RunOnCountryComplete(string country) { if (OnCountryComplete != null) OnCountryComplete(country); }

        async void IDataUpdater.Execute()
        {
            var ddl = _res.GetResource("CountryByIp.Files.DDL.sql");
            _db.NonQuery(ddl.Args(_conf.DbTableName));

            var countries = _countries.GetCountries();

            using (var client = new HttpClient())
            {
// ReSharper disable once CoVariantArrayConversion

                await Task.WhenAll(countries.Select(ci => client
                        .GetStringAsync(_conf.CsvSiteRoot + ci.Code + ".csv") // get CSV file
                        .ContinueWith(async task => // store Ra
                            {
                                var ranges = _csv.GetRanges(task.Result).ToList();

                                const int batch = 300;
                                while (ranges.Count != 0)
                                {
                                    var currentNum = Math.Min(batch, ranges.Count);
                                    var sql = RangesToSql(ranges.Take(currentNum), ci.Name);
                                    await _db.NonQueryAsync(sql)
                                                .ContinueWith(t =>
                                                {
                                                    if (t.Exception != null)
                                                        _log.Error( ci.Name + "\n\n" + 
                                                                        sql + "\n\n" + 
                                                                        Convert.ToString(t.Exception));
                                                });

                                    ranges.RemoveRange(0, currentNum);
                                }
                            
                                RunOnCountryComplete(ci.Name);

                            })
                        .Unwrap())
                    .ToArray());

            }

            RunOnEnd();
        }

        private string RangesToSql(IEnumerable<RangeOfIps> ranges, string name)
        {
            var sb = new StringBuilder("\nINSERT INTO [dbo].[" + _conf.DbTableName + "] (Country,FromIp,ToIp,[Count],Assigned) VALUES\n");
            var first = true;
            foreach (var r in ranges)
            {
                if (first) first = false;
                else sb.Append(',').Append('\n');
                var d = r.Assigned;
                sb.Append('(').Append('\'').Append(name.Replace("'", "''")).Append('\'').Append(',')
                    .Append(r.From)
                    .Append(',')
                    .Append(r.To)
                    .Append(',')
                    .Append(r.Count)
                    .Append(',')
                    .Append("'{0}-{1}-{2}'".Args(d.Year.PadLeft(4, '0'), d.Month.PadLeft(2, '0'), d.Day.PadLeft(2, '0')))
                    .Append(')');
            }
            return sb.ToString();
        }
    }
}