using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Unburnable Meat", "S642667", "1.0.0")]
    class UnburnableMeat : RustPlugin
    {
        public static readonly Dictionary<string, string> UnburntItems = new Dictionary<string, string>()
        {
            { "bearmeat.burned", "bearmeat.cooked" },
            { "chicken.burned", "chicken.cooked" },
            { "deermeat.burned", "deermeat.cooked" },
            { "horsemeat.burned", "horsemeat.cooked" },
            { "humanmeat.burned", "humanmeat.cooked" },
            { "meat.pork.burned", "meat.pork.cooked" },
            { "wolfmeat.burned", "wolfmeat.cooked" }
        };

        Item OnFindBurnable(BaseOven oven)
        {
            List<Item> toRemove = new List<Item>();
            foreach (var item in oven.inventory.itemList)
            {
                if (item.info.shortname == "bearmeat.burned")
                {
                    toRemove.Add(item);
                }
            }
            foreach (var item in toRemove)
            {
                item.DoRemove();
                Item unburnt = ItemManager.CreateByName(UnburntItems[item.info.shortname], 1);
                if (!unburnt.MoveToContainer(oven.inventory))
                {
                    unburnt.Drop(oven.inventory.dropPosition, oven.inventory.dropVelocity);
                }
            }
            return null;
        }
    }
}
