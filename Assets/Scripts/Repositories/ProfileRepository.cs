using Data;
using Storage;

namespace Repositories
{
    public class ProfileRepository : IRepository<ProfileData>
    {
        private readonly IStorage _storage;
        private const string Path = "profile.json";

        public ProfileRepository(IStorage storage) => _storage = storage;
        
        public bool TryLoad(out ProfileData data) => _storage.TryLoad(Path, out data);

        public void Save(ProfileData data) => _storage.Save(Path, data);
    }
}