using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using PlayerStatsSystem;
using Respawning;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using Newtonsoft.Json.Linq;

using Result = SER.Code.MethodSystem.Structures.Result;

namespace SER.Code.ValueSystem.PropertySystem;

public static class ReferencePropertyRegistry
{
    private static readonly Dictionary<Type, Dictionary<string, IValueWithProperties.PropInfo>> RegisteredProperties = new();
    private static readonly Dictionary<Type, Dictionary<string, IValueWithProperties.PropInfo>> CachedCombinedProperties = new();

    public static IEnumerable<Type> GetRegisteredTypes() => RegisteredProperties.Keys;

    public static void Register<T, TValue>(string name, Func<T, TValue> handler, string? description = null) where TValue : Value
    {
        var type = typeof(T);
        if (!RegisteredProperties.TryGetValue(type, out var props))
        {
            props = new Dictionary<string, IValueWithProperties.PropInfo>(StringComparer.OrdinalIgnoreCase);
            RegisteredProperties[type] = props;
        }
        props[name] = new ReferencePropInfo<T, TValue>(handler, description);
        CachedCombinedProperties.Clear(); // Invalidate cache
    }

    public static Dictionary<string, IValueWithProperties.PropInfo> GetProperties(Type type)
    {
        if (CachedCombinedProperties.TryGetValue(type, out var cached))
            return cached;

        var combined = new Dictionary<string, IValueWithProperties.PropInfo>(StringComparer.OrdinalIgnoreCase);

        // 1. Add registered properties (including from base types)
        var currentType = type;
        while (currentType != null)
        {
            if (RegisteredProperties.TryGetValue(currentType, out var registered))
            {
                foreach (var kvp in registered)
                {
                    if (!combined.ContainsKey(kvp.Key))
                        combined[kvp.Key] = kvp.Value;
                }
            }
            currentType = currentType.BaseType;
        }

        // 2. Add reflected properties with '!' prefix
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var key = "!" + prop.Name.LowerFirst();
            if (!combined.ContainsKey(key))
            {
                combined[key] = new UnsafeReferencePropInfo(type, prop, null);
            }
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var key = "!" + field.Name.LowerFirst();
            if (!combined.ContainsKey(key))
            {
                combined[key] = new UnsafeReferencePropInfo(type, field, null);
            }
        }

        CachedCombinedProperties[type] = combined;
        return type == typeof(JObject) || type == typeof(JToken) || type.IsSubclassOf(typeof(JToken)) 
            ? new JTokenPropertyDictionary(combined) 
            : combined;
    }

    private class JTokenPropertyDictionary(Dictionary<string, IValueWithProperties.PropInfo> inner) 
        : Dictionary<string, IValueWithProperties.PropInfo>(inner, StringComparer.OrdinalIgnoreCase), IValueWithProperties.IDynamicPropertyDictionary
    {
        public new bool TryGetValue(string key, out IValueWithProperties.PropInfo value)
        {
            if (base.TryGetValue(key, out value)) return true;
            
            // For JObject/JToken, if it's not a registered property, it's a dynamic access to a JSON property
            value = new JTokenDynamicPropInfo(key);
            return true;
        }
    }

    private class JTokenDynamicPropInfo(string key) : IValueWithProperties.PropInfo
    {
        public override TryGet<Value> GetValue(object obj)
        {
            JToken? token = obj switch
            {
                ReferenceValue refVal => refVal.Value as JToken,
                JToken t => t,
                _ => null
            };

            if (token == null) return "Value is not a JSON token";
            if (token is not JObject jobj) return "Value is not a JSON object, cannot access properties";

            if (!jobj.TryGetValue(key, out var val)) return $"Property '{key}' not found in JSON object";

            return Value.Parse(val, null);
        }

        public override SingleTypeOfValue ReturnType => new(typeof(ReferenceValue));
        public override string Description => $"Accesses JSON property '{key}'";
    }

    private class ReferencePropInfo<T, TValue>(Func<T, TValue> handler, string? description) 
        : IValueWithProperties.PropInfo<T, TValue>(handler, description) where TValue : Value
    {
        protected override Func<object, object> Translator => 
            obj => obj switch {
                ReferenceValue refVal => refVal.Value,
                PlayerValue { Players.Length: 1 } plrVal => plrVal.Players[0],
                IValueWithProperties valWithProps and T => valWithProps,
                _ => obj
            };
    }

    private static bool _isInitialized;
    public static void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        Register<Item, EnumValue<ItemType>>("type", i => i.Type.ToEnumValue(), "The type of the item");
        Register<Item, EnumValue<ItemCategory>>("category", i => i.Category.ToEnumValue(), "The category of the item");
        Register<Item, PlayerValue>("owner", i => new PlayerValue(i.CurrentOwner), "The player who owns the item");
        Register<Item, BoolValue>("isEquipped", i => new BoolValue(i.IsEquipped), "Whether the item is currently equipped");

        Register<Door, EnumValue<DoorName>>("name", d => d.DoorName.ToEnumValue(), "The name of the door");
        Register<Door, StaticTextValue>("unityName", d => new StaticTextValue(d.Base.name), "The name of the door in Unity");
        Register<Door, BoolValue>("isOpen", d => new BoolValue(d.IsOpened), "Whether the door is open");
        Register<Door, BoolValue>("isClosed", d => new BoolValue(!d.IsOpened), "Whether the door is closed");
        Register<Door, BoolValue>("isLocked", d => new BoolValue(d.IsLocked), "Whether the door is locked");
        Register<Door, BoolValue>("isUnlocked", d => new BoolValue(!d.IsLocked), "Whether the door is unlocked");
        Register<Door, NumberValue>("remainingHealth", d => new NumberValue(d is BreakableDoor bDoor ? (decimal)bDoor.Health : -1), "The remaining health of the door");
        Register<Door, NumberValue>("maxHealth", d => new NumberValue(d is BreakableDoor bDoor ? (decimal)bDoor.MaxHealth : -1), "The maximum health of the door");
        Register<Door, EnumValue<DoorPermissionFlags>>("permissions", d => d.Permissions.ToEnumValue(), "The permissions required to open the door");

        Register<Pickup, BoolValue>("isDestroyed", p => new BoolValue(p.IsDestroyed), "Whether the pickup is destroyed");
        Register<Pickup, BoolValue>("hasSpawned", p => new BoolValue(p.IsSpawned), "Whether the pickup has spawned");
        Register<Pickup, EnumValue<ItemType>>("itemType", p => p.Type.ToEnumValue(), "The type of the pickup item");
        Register<Pickup, PlayerValue>("lastOwner", p => new PlayerValue(p.LastOwner), "The player who last owned the pickup");
        Register<Pickup, BoolValue>("isInUse", p => new BoolValue(p.IsInUse), "Whether the pickup is currently in use");
        Register<Pickup, EnumValue<ItemCategory>>("itemCategory", p => p.Category.ToEnumValue(), "The category of the pickup item");
        Register<Pickup, ReferenceValue>("room", p => new ReferenceValue(p.Room), "The room where the pickup is located");
        Register<Pickup, NumberValue>("positionX", p => new NumberValue((decimal)p.Position.x), "The X position of the pickup");
        Register<Pickup, NumberValue>("positionY", p => new NumberValue((decimal)p.Position.y), "The Y position of the pickup");
        Register<Pickup, NumberValue>("positionZ", p => new NumberValue((decimal)p.Position.z), "The Z position of the pickup");
        Register<Pickup, NumberValue>("weight", p => new NumberValue((decimal)p.Weight), "The weight of the pickup");

        Register<Room, Value>("shape", r => r.Shape.ToEnumValue(), "The shape of the room");
        Register<Room, EnumValue<RoomName>>("name", r => r.Name.ToEnumValue(), "The name of the room");
        Register<Room, EnumValue<FacilityZone>>("zone", r => r.Zone.ToEnumValue(), "The zone where the room is located");
        Register<Room, NumberValue>("xPos", r => new NumberValue((decimal)r.Position.x), "The X position of the room");
        Register<Room, NumberValue>("yPos", r => new NumberValue((decimal)r.Position.y), "The Y position of the room");
        Register<Room, NumberValue>("zPos", r => new NumberValue((decimal)r.Position.z), "The Z position of the room");

        Register<PlayerRoleBase, EnumValue<RoleTypeId>>("type", r => r.RoleTypeId.ToEnumValue(), "The role type");
        Register<PlayerRoleBase, EnumValue<Team>>("team", r => r.Team.ToEnumValue(), "The team of the role");
        Register<PlayerRoleBase, StaticTextValue>("name", r => new StaticTextValue(r.RoleName), "The name of the role");

        Register<DamageHandlerBase, NumberValue>("damage", h => new NumberValue((decimal)((h as StandardDamageHandler)?.Damage ?? 0)), "Damage amount");
        Register<DamageHandlerBase, Value>("hitbox", h => (h as StandardDamageHandler)?.Hitbox.ToEnumValue() ?? (Value)new StaticTextValue("none"), "Hitbox type");
        Register<DamageHandlerBase, ReferenceValue>("firearmUsed", h => new ReferenceValue((h as FirearmDamageHandler)?.Firearm), "Firearm used");
        Register<DamageHandlerBase, ReferenceValue>("attacker", h => new ReferenceValue((h as AttackerDamageHandler)?.Attacker), "Attacker player");

        Register<RespawnWave, EnumValue<Faction>>("faction", w => w.Faction.ToEnumValue(), "Respawn faction");
        Register<RespawnWave, NumberValue>("maxWaveSize", w => new NumberValue(w.MaxWaveSize), "Maximum wave size");
        Register<RespawnWave, NumberValue>("respawnTokens", w => new NumberValue(w.Base is Respawning.Waves.Generic.ILimitedWave limitedWave ? limitedWave.RespawnTokens : 0), "Respawn tokens");
        Register<RespawnWave, NumberValue>("influence", w => new NumberValue((decimal)FactionInfluenceManager.Get(w.Faction)), "Faction influence");
        Register<RespawnWave, DurationValue>("timeLeft", w => new DurationValue(TimeSpan.FromSeconds(w.TimeLeft)), "Time left for wave");

        Register<Result, BoolValue>("success", r => new BoolValue(r.Success), "Whether the parsing was successful");
        Register<Result, BoolValue>("failed", r => new BoolValue(!r.Success), "Whether the parsing has failed");
        Register<Result, Value>("value", r => r.Value ?? new StaticTextValue("null"), "The value that got parsed");

        Register<JObject, Value>("value", obj => Value.Parse(obj, null), "The value of the JSON object");
        
        Register<JToken, EnumValue<JTokenType>>("type", t => t.Type.ToEnumValue(), "The type of the JSON token");
        Register<JToken, StaticTextValue>("path", t => new StaticTextValue(t.Path), "The path of the JSON token");
        Register<JToken, ReferenceValue>("root", t => new ReferenceValue(t.Root), "The root of the JSON token");
        Register<JToken, ReferenceValue>("parent", t => new ReferenceValue(t.Parent), "The parent of the JSON token");
        Register<JToken, CollectionValue>("children", t => new CollectionValue(t.Children()), "The children of the JSON token");
        Register<JToken, StaticTextValue>("asString", t => new StaticTextValue(t.ToString()), "The JSON representation of the token");
        Register<JToken, NumberValue>("asNumber", t => new NumberValue(t.Type is JTokenType.Integer or JTokenType.Float ? (decimal)t : 0), "The numeric value of the token");
        Register<JToken, BoolValue>("asBool", t => new BoolValue(t.Type == JTokenType.Boolean && (bool)t), "The boolean value of the token");

        foreach (var (key, propInfo) in PlayerValue.PropertyInfoMap)
        {
            var name = key.ToString().LowerFirst();
            if (!RegisteredProperties.TryGetValue(typeof(Player), out var playerProps))
            {
                playerProps = new Dictionary<string, IValueWithProperties.PropInfo>(StringComparer.OrdinalIgnoreCase);
                RegisteredProperties[typeof(Player)] = playerProps;
            }
            playerProps[name] = propInfo;
        }
    }


    private class UnsafeReferencePropInfo : IValueWithProperties.PropInfo
    {
        private readonly Type _ownerType;
        private readonly MemberInfo _member;
        private readonly Type _guessedValueType;

        public UnsafeReferencePropInfo(Type ownerType, MemberInfo member, string? description)
        {
            _ownerType = ownerType;
            _member = member;
            Description = description!;

            Type memberType = member switch
            {
                PropertyInfo prop => prop.PropertyType,
                FieldInfo field => field.FieldType,
                _ => typeof(object)
            };
            _guessedValueType = Value.GuessValueType(memberType);
        }

        public override TryGet<Value> GetValue(object obj)
        {
            object? target = obj switch
            {
                ReferenceValue refVal => refVal.Value,
                PlayerValue { Players.Length: 1 } plrVal => plrVal.Players[0],
                _ => obj
            };

            if (target == null) return "Reference is null";
            if (!_ownerType.IsInstanceOfType(target)) 
                return $"Object is not of type {_ownerType.AccurateName}";

            try
            {
                object? result = _member switch
                {
                    PropertyInfo prop => prop.GetValue(target),
                    FieldInfo field => field.GetValue(target),
                    _ => throw new InvalidOperationException()
                };

                if (result == null) return "Value is null";
                return Value.Parse(result, null);
            }
            catch (Exception e)
            {
                return $"Failed to get unsafe property {_member.Name}: {e.Message}";
            }
        }

        public override SingleTypeOfValue ReturnType => new(_guessedValueType);
        public override bool IsUnsafe => true;

        [field: AllowNull, MaybeNull]
        public override string Description => field ?? $"Unsafe access to C# member {_member.Name}";
    }
}
