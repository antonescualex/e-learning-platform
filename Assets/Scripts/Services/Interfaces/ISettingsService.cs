namespace Services.Interfaces
{
    public interface ISettingsService
    {
        SettingsData CurrentSettings { get; }

        void LoadOrDefault();
        void Save(SettingsData settingsData);
    }
}