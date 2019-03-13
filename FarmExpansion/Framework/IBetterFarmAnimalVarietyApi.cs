using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FarmExpansion.Framework
{
    public interface IBetterFarmAnimalVarietyApi
    {
        /// <param name="farm">StardewValley.Farm</param>
        /// <returns>Returns List<StardewValley.Object></returns>
        List<StardewValley.Object> GetAnimalShopStock(StardewValley.Farm farm);

        /// <summary>Determine if the mod is enabled.</summary>
        Dictionary<string, Texture2D> GetAnimalShopIcons();

        /// <param name="category">string</param>
        /// <param name="farmer">StardewValley.Farmer</param>
        /// <summary>Determine if the mod is enabled.</summary>
        /// <returns>Returns string</returns>
        string GetRandomAnimalShopType(string category, StardewValley.Farmer farmer);
    }
}
