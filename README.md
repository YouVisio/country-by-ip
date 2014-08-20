country-by-ip
=============

This project is a command line too that takes the data generously http://www.nirsoft.net 
and srores all the data into a single SQL Server table. On my 8 core machine and a remote Sql Server
it takes less than 30 seconds to execute.

Then you can check the country of a particular IP as such:

```sql
SELECT TOP 1 Country FROM [dbo].[yv_country_by_ip] WHERE FromIp <= 3625172095 AND ToIp >= 3625172095
```

The values are stored as BIGINT (sql equivalent of .NET Int64) 
Some helper functions you may need:

```c#
long IpStringToLong(string ipStr)
{
    var parts = ipStr.Trim().Split('.');
    if(parts.Length < 4) throw new ArgumentException("IP not as expected '"+ipStr+"'");
    var a = Byte.Parse(parts[0]);
    var b = Byte.Parse(parts[1]);
    var c = Byte.Parse(parts[2]);
    var d = Byte.Parse(parts[3]);
    return ((long)a << (8*3)) + (b << (8*2)) + (c << (8*1)) + (d << (8*0));
}
string IpLongToString(long ipLong)
{
    var b = BitConverter.GetBytes(ipLong);
    return b[3] + "." + b[2] + "." + b[1] + "." + b[0];
}
```
