using Repositories;
using Services.Interfaces;

namespace Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IRepository<SettingsData> _repository;

        public SettingsData CurrentSettings { get; private set; }

        public SettingsService(IRepository<SettingsData> repository)
        {
            _repository = repository;
            CurrentSettings = new SettingsData();
        }

        public void LoadOrDefault()
        {
            if (_repository.TryLoad(out var loadedData) && loadedData != null)
            {
                CurrentSettings = loadedData;
            }
            else
            {
                CurrentSettings = new SettingsData();
            }
        }

        public void Save(SettingsData settingsData)
        {
            CurrentSettings = settingsData.Copy();
            _repository.Save(CurrentSettings);
        }
    }
}
