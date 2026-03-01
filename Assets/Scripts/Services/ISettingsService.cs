namespace Services
{
    public interface ISettingsService
    {
        SettingsData CurrentSettings { get; }

        void LoadOrDefault();
        void Save(SettingsData settingsData);
    }
}