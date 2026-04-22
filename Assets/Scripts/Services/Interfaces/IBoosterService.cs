using System.Collections.Generic;
using Data.StaticData.Item;

namespace Services.Interfaces
{
    public interface IBoosterService
    {
        IReadOnlyList<ProfileItemDefinition> GetBoosters();
    }
}