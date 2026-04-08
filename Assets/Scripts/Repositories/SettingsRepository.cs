using Data;
using Storage;

namespace Repositories
{
    public class SettingsRepository : IRepository<SettingsData>
    {
        private readonly IStorage _storage;
        private const string Path = "settings.json";

        public SettingsRepository(IStorage storage) => _storage = storage;

        public bool TryLoad(out SettingsData data) => _storage.TryLoad(Path, out data);

        public void Save(SettingsData data) => _storage.Save(Path, data);
    }
}