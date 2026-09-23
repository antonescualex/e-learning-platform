using System;
using Data;
using Services.Interfaces;

namespace Services
{
    public class ProfileService : IProfileService
    {
        private ProfileData _profileData;

        public event Action<ProfileData> ProfileChanged;
        public ProfileData ProfileData => _profileData;
        public bool HasProfile => _profileData != null;

        public ProfileService() { }

        public void SetLoadedProfile(ProfileData profileData)
        {
            _profileData = profileData;
            _profileData?.EnsureDataIntegrity();
            NotifyProfileChanged();
        }
        
        public bool HasAvatar(string avatarId)
        {
            if (_profileData == null) return false;
            return _profileData.HasAvatar(avatarId);
        }

        public bool HasBackground(string backgroundId)
        {
            if (_profileData == null) return false;
            return _profileData.HasBackground(backgroundId);
        }

        private void NotifyProfileChanged()
        {
            ProfileChanged?.Invoke(_profileData);
        }
    }
}
