using SER.Code.ArgumentSystem.Arguments;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

internal static class AmmoMethodHelper
{
    public const string ArgumentName = "ammo type";

    public static OptionsArgument CreateArgument() => new(
        ArgumentName,
        nameof(ItemType.Ammo9x19),
        nameof(ItemType.Ammo556x45),
        nameof(ItemType.Ammo762x39),
        nameof(ItemType.Ammo44cal),
        nameof(ItemType.Ammo12gauge));

    public static ItemType Parse(string value) =>
        (ItemType)Enum.Parse(typeof(ItemType), value, true);
}
