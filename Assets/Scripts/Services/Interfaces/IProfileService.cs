using System;
using Data;

namespace Services.Interfaces
{
    public interface IProfileService
    {
        event Action<ProfileData> ProfileChanged;

        ProfileData ProfileData { get; }
        bool HasProfile { get; }

        void SetLoadedProfile(ProfileData profileData);
        
        bool HasAvatar(string avatarId);
        bool HasBackground(string backgroundId);
    }
}
