using System;
using System.Collections.Generic;
using Auth;

namespace Dto.Profile
{
    [Serializable]
    public sealed class ProfileAwardResponse
    {
        public ProfileDto Profile;
        public List<string> AwardedBadgeIds;
    }
}