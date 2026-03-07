using XRL.Wish;
using XRL.World;
using XRL;
using System;
using System.Linq;
using System.Reflection;

namespace AddPartWishCommand
{
    [HasWishCommand]

    [HasGameBasedStaticCache]
    public static class AddPartWishCommand
    {
        static Type[] IParts => _iParts ??= GatherIParts();

        [GameBasedStaticCache(false)]
        static Type[] _iParts = new Type[0];

        [WishCommand("addpart")]
        public static void AddPart(string partName)
        {
            Type partType = IParts.FirstOrDefault(x => x.Name.Equals(partName, StringComparison.OrdinalIgnoreCase));
            if (partType != null)
            {
                if (Activator.CreateInstance(partType) is IPart part)
                {
                    if (!The.Player.HasPart(partType))
                    {
                        IComponent<GameObject>.AddPlayerMessage("added part " + partType.Name);
                        The.Player.AddPart(part);
                    }
                    else
                        IComponent<GameObject>.AddPlayerMessage("Player already has " + partType.Name);
                }
                else
                    IComponent<GameObject>.AddPlayerMessage($"{partType.Name} does not inherit from IPart.");
            }
            else
                IComponent<GameObject>.AddPlayerMessage($"{partName} could not be found in XRL.World.Parts. Check for typos. Case does not matter.");
        }

        [WishCommand("removepart")]
        public static void RemovePart(string partName)
        {
            var part = The.Player.PartsList.FirstOrDefault(x => x.Name.Equals(partName, StringComparison.OrdinalIgnoreCase));
            if (part != null)
            {
                IComponent<GameObject>.AddPlayerMessage($"{part.Name} removed.");
                The.Player.RemovePart(part);
            }
            else
                IComponent<GameObject>.AddPlayerMessage($"{partName} does not exist in player's part list");
        }

        static Type[] GatherIParts()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.Namespace == "XRL.World.Parts" && CheckBaseType(x)).ToArray();
        }

        static bool CheckBaseType(Type type)
        {
            while (type.BaseType != null)
            {
                if (type.BaseType == typeof(IPart))
                    return true;
                type = type.BaseType;
            }
            return false;
        }
    }
}