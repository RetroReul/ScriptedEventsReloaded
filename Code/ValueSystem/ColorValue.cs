using JetBrains.Annotations;
using UnityEngine;

namespace SER.Code.ValueSystem;

[UsedImplicitly]
public class ColorValue(Color color) : LiteralValue<Color>(color)
{
    [UsedImplicitly]
    public ColorValue() : this(Color.white) {}

    public override string StringRep => Value.ToHex();

    [UsedImplicitly]
    public new static string FriendlyName = "color value";

    public override Dictionary<string, PropInfo> Properties => [];
}