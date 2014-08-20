using System.IO;

namespace CountryByIp
{
    public interface IResourceManager
    {
        string GetResource(string resourceName);
    }
    internal sealed class ResourceManager : IResourceManager
    {
        string IResourceManager.GetResource(string resourceName)
        {
            using (Stream stream = typeof(ResourceManager).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}