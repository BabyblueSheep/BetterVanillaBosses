using BetterVanillaBosses.Core;
using ReLogic.Content.Sources;
using Terraria.ModLoader;

namespace BetterVanillaBosses
{
	public class BetterVanillaBosses : Mod
	{
        public override IContentSource CreateDefaultContentSource()
        {
            var source = new SmartContentSource(base.CreateDefaultContentSource());

            // Redirects requests for ModName/Content/... to ModName/Assets/...
            source.AddDirectoryRedirect("Content", "Assets");

            return source;
        }
    }
}
