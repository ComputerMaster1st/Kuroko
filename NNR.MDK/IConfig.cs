namespace NNR.MDK.Configuration
{
    public interface IConfig
    {
        Task LoadAsync();

        Task SaveAsync();
    }
}
