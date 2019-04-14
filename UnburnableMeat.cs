using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Unburnable Meat", "S642667", "1.0.0")]
    class UnburnableMeat : RustPlugin
    {
        public static readonly string[] CookedItems = new string[]
        {
            "bearmeat.cooked",
            "chicken.cooked",
            "deermeat.cooked",
            "horsemeat.cooked",
            "humanmeat.cooked",
            "meat.pork.cooked",
            "wolfmeat.cooked",
            "fish.cooked"
        };

        private Dictionary<string, int> lowTemps = new Dictionary<string, int>();
        private Dictionary<string, int> highTemps = new Dictionary<string, int>();

        void OnServerInitialized()
        {
            foreach (var shortname in CookedItems)
            {
                var cookable = ItemManager.FindItemDefinition(shortname).GetComponent<ItemModCookable>();
                lowTemps.Add(shortname, cookable.lowTemp);
                highTemps.Add(shortname, cookable.highTemp);
                cookable.lowTemp = -1;
                cookable.highTemp = -1;
            }
        }

        void Unload()
        {
            foreach (var shortname in CookedItems)
            {
                var cookable = ItemManager.FindItemDefinition(shortname).GetComponent<ItemModCookable>();
                cookable.lowTemp = lowTemps[shortname];
                cookable.highTemp = highTemps[shortname];
            }
        }
    }
}
