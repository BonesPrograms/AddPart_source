using XRL.Wish;
using XRL.World;
using XRL;
using System;
using System.Linq;

namespace AddPartWishCommand
{
    [HasWishCommand]

    [HasGameBasedStaticCache]
    static class AddPartWishCommand
    {
        static Type[] IParts => _iParts ??= GatherIParts();

        [GameBasedStaticCache(false)]
        static Type[] _iParts = Array.Empty<Type>();

        [WishCommand("addpart")]
        public static void AddPart(string partName)
        {
            var partType = FirstTypeWithName(partName);
            if (IsValidType(partType, partName) && PickTarget(The.Player, "addart", out var pick))
            {
                if (!pick.HasPart(partType))
                {
                    IComponent<GameObject>.AddPlayerMessage("added part " + partType.Name + $" to {pick.t()}");
                    pick.AddPart((IPart)Activator.CreateInstance(partType));
                }
                else
                    IComponent<GameObject>.AddPlayerMessage($"{pick.t()} already has " + partType.Name);
            }

        }

        [WishCommand("removepart")]
        public static void RemovePart(string partName)
        {
            var partType = FirstTypeWithName(partName);
            if (IsValidType(partType, partName) && PickTarget(The.Player, "removepart", out var pick))
            {
                if (pick.HasPart(partType))
                {
                    IComponent<GameObject>.AddPlayerMessage($"{partType.Name} removed from {pick.t()}.");
                    pick.RemovePart(partType);
                }
                else
                    IComponent<GameObject>.AddPlayerMessage($"{pick.t()} does not have {partType.Name}");
            }
        }

        static Type FirstTypeWithName(string partName)
        {
            return IParts.FirstOrDefault(x => x.Name.Equals(partName, StringComparison.OrdinalIgnoreCase)); ;
        }

        static bool IsValidType(Type type, string partName)
        {
            if (type == null)
                IComponent<GameObject>.AddPlayerMessage($"{partName} could not be found in XRL.World.Parts namespace or is not an IPart.");
            return type != null;

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

        static bool PickTarget(GameObject obj, string text, out GameObject pick)
        {
            IPart part = new() { ParentObject = obj };
            Cell cell = part.PickDestinationCell(80, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: true, IgnoreLOS: true, RequireCombat: true, XRL.UI.PickTarget.PickStyle.EmptyCell, text, Snap: true);
            pick = cell?.GetCombatTarget(obj, true, true, true);
            bool value = pick != null;
            if (!value && cell != null)
                XRL.UI.Popup.ShowFail(cell.HasCombatObject() ? $"There is no one there you can {text}." : $"There is no one there to {text}");
            return value;
        }
    }
}