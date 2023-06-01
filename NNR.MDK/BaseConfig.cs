using Newtonsoft.Json;

namespace NNR.MDK.Configuration
{
    public abstract class BaseConfig<TConfig> where TConfig : class, IConfig
    {
        protected async Task<TConfig> LoadAsync(string filePath)
        {
            if (File.Exists(filePath))
                return JsonConvert.DeserializeObject<TConfig>(
                        await File.ReadAllTextAsync(filePath)
                    );

            await (Activator.CreateInstance(typeof(TConfig)) as IConfig).SaveAsync();

            return null;
        }

        protected Task SaveAsync(string filePath)
            => File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
