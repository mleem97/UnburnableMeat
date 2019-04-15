using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Unburnable Meat", "S642667", "1.0.0")]
    [Description("Prevent cooked meats from burning")]
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

        ItemModCookable GetCookable (string shortname)
        {
            var definition = ItemManager.FindItemDefinition(shortname);
            if (definition == null)
            {
                Puts($"Uknown definition for {shortname}");
                return null;
            }
            var cookable = definition.GetComponent<ItemModCookable>();
            if (cookable == null)
            {
                Puts($"Uknown cookable for {shortname}");
                return null;
            }
            return cookable;
        }

        void OnServerInitialized()
        {
            foreach (var shortname in CookedItems)
            {
                var cookable = getCookable(shortname);
                if (cookable == null)
                {
                    continue;
                }
                lowTemps.Add(shortname, cookable.lowTemp);
                highTemps.Add(shortname, cookable.highTemp);
                cookable.lowTemp = -1;
                cookable.highTemp = -1;
            }
        }

        void Unload()
        {
            foreach (KeyValuePair<string, int> item in lowTemps)
            {
                var cookable = getCookable(item.Key);
                if (cookable == null)
                {
                    continue;
                }
                cookable.lowTemp = item.Value;
                cookable.highTemp = highTemps[item.Key];
            }
        }
    }
}
