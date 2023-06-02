﻿using Newtonsoft.Json;

namespace NNR.MDK.Configuration
{
    public abstract class BaseConfig
    {
        protected static async Task<IConfig> BaseLoadAsync<TConfig>(string filePath) where TConfig : class, IConfig
        {
            if (File.Exists(filePath))
                return JsonConvert.DeserializeObject<TConfig>(
                        await File.ReadAllTextAsync(filePath)
                    );

            await (Activator.CreateInstance(typeof(TConfig)) as IConfig).SaveAsync();

            return null;
        }

        protected static Task BaseSaveAsync<TConfig>(string filePath, TConfig config) where TConfig : class, IConfig
        {
            if (!Directory.Exists(DataDirectories.CONFIG))
                Directory.CreateDirectory(DataDirectories.CONFIG);

            return File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(config, Formatting.Indented));
        }
    }
}
