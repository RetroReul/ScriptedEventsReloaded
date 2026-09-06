# SER capability and reconstruction map

This document records what SER 1.0 can do and the behavior a replacement must
either preserve or deliberately change. It is written for a future rewrite,
not as a first tutorial.

Snapshot: 6 September 2026, repository commit
`de43fa73f31c371d10cc68ca6100ad69e7533a78`, including the working-tree ammo
methods present when this map was made.

## What counts as the complete snapshot

This page maps the product boundaries, language rules, built-in systems, and
known implementation-shaped behavior. Three other files complete the exact
symbol-level record:

- [`website/data/ser-truth-table.json`](../website/data/ser-truth-table.json)
  is the machine-readable inventory. At this snapshot it contains 301 methods,
  23 keywords, 8 flags, 47 predefined variables, 374 LabAPI events, 4
  ProjectMER events, and 6 UCR events. Each method includes its exact syntax,
  return type, arguments, defaults, options, framework requirement, and known
  errors. Each event includes its injected variables and whether it can be
  cancelled.
- [`language_specification.md`](../language_specification.md) records the
  grammar and short examples.
- [`PROJECT_GUIDE.md`](../PROJECT_GUIDE.md) records the current class layout,
  registration order, and extension workflow.

Do not rebuild SER from this page alone while dropping the truth table. The
truth table is generated from the same runtime metadata used by `serhelp`, SER
Blocks, and the VS Code extension, so it is the most precise contract for the
hundreds of methods and events whose details would make this page unreadable.

## Product boundary

SER is a .NET Framework 4.8 SCP: Secret Laboratory server plugin. It executes a
custom, line-oriented scripting language from text files without compiling each
script into a DLL. The same source builds two distributions:

| Build | Host | Output |
|---|---|---|
| `Release` | LabAPI | `SER.dll` |
| `EXILED` | EXILED | `SER-Exiled.dll` |

The core product includes:

- script discovery, compilation, hot reload, execution, stopping, and status;
- bindings for game events, commands, custom triggers, SER custom roles,
  interactable toys, ProjectMER events, and UCR events;
- a typed value and variable system with chained properties;
- control flow, waits, functions, callbacks, and catchable runtime errors;
- 301 game, player, map, network, persistence, audio, and integration methods;
- round-scoped data and rules plus JSON database files and YAML config files;
- server-side help, validated examples, a standalone block editor, and a VS
  Code extension.

## Script files, identity, and reloads

### Discovery contract

- The main directory is LabAPI's config directory plus
  `Scripted Events Reloaded`.
- `.ser` is preferred. `.txt` has identical meaning.
- Discovery is recursive.
- A filename beginning with `#` disables that file. A directory name beginning
  with `#` disables its complete subtree.
- Symbolic-link and junction directories are skipped. Data-file paths reject
  traversal, unsafe Windows names, and linked path segments.
- The base filename is the global identity. Relative folders and extensions are
  not part of the name. If two files have the same base filename, neither is
  loaded.
- Script and audio filename matching is case-insensitive on Windows. Audio
  filenames must also be unique across the complete SER data tree.

### One file, several handlers

Every line beginning with `!--` starts a new independent section. It ends just
before the next `!--`. A file with three declarations therefore exposes
`filename:1`, `filename:2`, and `filename:3`; the bare filename is ambiguous and
cannot be run manually. A file with zero or one section keeps the bare name.

Only blank lines and comments may appear before the first declaration in a
flagged file. Source line numbers remain physical-file line numbers in errors.

This is the current answer to “multi-flag scripts”: one physical file can hold
many flagged handlers, but each handler is a separate runtime script section.
All eight built-in flags implement the major-behavior marker, and a runtime
script may not combine two major behavior flags. The flag registry can store a
list, but the file splitter normally leaves one declaration in each section.
A rewrite can model handlers directly and keep `filename:N` only as a
compatibility address.

### Reload contract

- SER scans and loads everything when its runtime initializes and on round
  restart.
- `serreload` forces a complete refresh.
- Every file-backed execution request first refreshes that physical file. This
  covers `serrun`, events, custom commands, triggers, callbacks, and scripts
  started by other scripts.
- A physical file is transactional: every section must split, compile, parse
  its flags, and bind successfully before any accepted section is replaced.
- A failed edit leaves the last accepted version active. A newly invalid file
  has no active version. Deleting a file unloads its bindings.
- Stable reads compare write time and length before and after reading, retrying
  up to three times if an editor is still writing.
- `serstatus` reports accepted files, failed candidates, disabled files,
  excluded folders, skipped links, and every duplicate-name path.

## Execution model

A script is compiled from raw lines into slices, tokens, and then a tree of
contexts. The tokenizer is whitespace-oriented. Quoted text and `(...)` or
`{...}` are grouped slices; the first token type that recognizes a slice wins,
so token precedence is part of compatibility.

Runtime work uses MEC coroutines. Statements and methods can be synchronous or
yielding. A script tracks its name, physical filename, executor, caller, run
reason, current line, start time, local variables, functions, cancellation
decision, and killed state. Run reasons are `Unknown`, `Script`, `Event`,
`BaseCommand`, `CustomCommand`, and `FunctionCallback`.

The executor controls where `Reply`, warnings, and errors go: server console,
Remote Admin, or the player's client console. A player-backed command executor
automatically receives `@sender`.

`SafeScripts` adds safety yields around method execution. This helps prevent a
tight script from freezing the server, but it means the game event continues
before `IsAllowed` can change it. Infinite and long loops still need explicit
waits. Stopping a script marks it killed and removes it from the running set;
new asynchronous work must stay on the script-aware coroutine path to stop with
its owner.

## Language surface

### Values and variables

| Prefix | Family | Holds |
|---|---|---|
| `$` | literal | text, number, boolean, duration, enum, enum flags, or color |
| `@` | player | zero, one, or many ready players |
| `*` | reference | a live CLR, game, JSON, integration, or SER object |
| `&` | collection | an ordered, normally single-compatible-type list of SER values |

`name = value` creates or replaces a local variable. `global` writes a
round-wide variable and `delete` removes a variable. The same spelling with a
different prefix is a different variable, but SER rejects an active local and
global collision with the same prefix and name.

There is no lexical scope table for blocks. Every execution has one local
table. `ephm` values use that table but are removed when their owning statement
finishes, and loop ephemeral values are removed after each iteration. Function
arguments are also temporarily added to the same local table and removed when
the call finishes. Locals disappear with the execution; globals are reset on
map generation.

Every read of a player variable removes entries not present in
`Player.ReadyList`. Removed players are not added back. Predefined player groups
are live getters and are recalculated when read; assigning one to another
variable takes a selection that can later only shrink through readiness
sanitizing.

Collections are one-based when fetched by index. They expose insert, remove,
remove-at, contains, join, and subtract operations. Empty collections can adopt
their first value type. Mixed unrelated value types fail when materialized;
compatible subclasses may share a common base. `sum` and `average` consider
only number values.

### Literal syntax and expressions

- Text uses double quotes. `{expression}` interpolates inside text and `~`
  escapes interpolation characters. `<br>` is the common game-text newline.
- Comments begin with `#`; the documented form uses a following space.
- Numbers use decimal semantics inside SER. Durations accept units such as
  `ms`, `s`, `m`, and `h`. Colors accept SER's color token formats.
- `+`, `-`, `*`, `/`, and `%` are passed through NCalc 1.3.8. A numeric token
  ending in `%` is parsed as a fraction, so `50%` is `0.5`.
- Conditions accept `=`, `is`, `==`, `!=`, `isnt`, `isn't`, `isnot`, `>`,
  `>=`, `<`, `<=`, `and`/`&&`, and `or`/`||`. `!` and `not` are not logical
  negation operators. The special raw value `invalid` is accepted by the
  expression resolver.
- A standalone assignment already supplies the expression boundary. Inline
  multi-token method calls and property chains normally need `{...}`. Plain
  prefixed variables do not.
- Method arguments are whitespace-separated. `_` explicitly skips an optional
  argument when the method allows it. `*` is the non-player “all values” joker
  for compatible arguments. A final variadic argument consumes the rest of the
  line.

### Control flow and keywords

The 23 registered keywords are:

| Kind | Keywords | Contract |
|---|---|---|
| branches | `if`, `elif`, `else`, `chance`, `end` | Nested statement tree; `elif` and `else` extend the previous failed branch. |
| loops | `repeat`, `while`, `over`, `forever`, `with`, `break`, `continue` | Optional `with` variables expose a one-based iteration or current value; `over` can also expose a one-based index. |
| timing | `wait`, `wait_until` | Pause for a duration or poll a boolean expression once per frame. |
| variables | `global`, `ephm`, `delete` | Change storage/lifetime as described above. |
| inline functions | `func`, `run`, `return` | Functions must appear above the call. The function-name prefix declares its return family; no prefix means no value. |
| errors | `attempt`, `on_error` | Catch SER `ScriptRuntimeError` failures. Optional `with` variables receive message, exception type, and stack trace. |
| termination | `stop` | End the current script execution. |

`break` ends the nearest loop or inline function. `return` ends the inline
function with a value. Functions execute in the caller's script instance and
temporarily add their arguments to its local table. Callback-taking methods are
different: they copy the function body into a new script execution when the
callback fires, refresh the source first, and inject the callback values there.

### Properties

`value -> property` reads a property, and chains can continue through returned
values. Property names are case-insensitive. Player properties require exactly
one player even though the `@` family can normally hold many.

Only reference variables support assignment through a property chain:

```ser
*object -> writableProperty = true
```

The last property must be a public settable CLR property or mutable public
field, and the SER value must convert to its CLR type. Built-in safe properties
are read-only.

Guaranteed value properties are:

| Value | Properties |
|---|---|
| text and enum | `length`, `upper`, `lower`, `trim`, `isEmpty`, `valType` |
| number | `abs`, `round`, `floor`, `ceil`, `isEven`, `isOdd`, `sign`, `valType` |
| bool | `not`, `asNumber`, `asText`, `valType` |
| duration | `h`, `m`, `s`, `ms`, `totalH`, `totalM`, `totalS`, `totalMs`, `valType` |
| color | `r`, `g`, `b`, `a`, `hex`, `valType` |
| collection | `length`, `isEmpty`, `first`, `last`, `random`, `sum`, `average`, `valType` |
| reference | `isValid`, `isInvalid`, `refName`, `refAssembly`, `valType`, plus the referenced object properties |

Player properties are:

`name`, `displayName`, `role`, `roleRef`, `team`, `inventory`, `itemCount`,
`heldItem`, `heldItemRef`, `isAlive`, `userId`, `playerId`, `customInfo`,
`roomRef`, `health`, `maxHealth`, `artificialHealth`, `maxArtificialHealth`,
`humeShield`, `maxHumeShield`, `humeShieldRegenRate`, `effects`,
`effectReferences`, `groupName`, `posX`, `posY`, `posZ`, `isDisarmed`,
`isMuted`, `isIntercomMuted`, `isGlobalModerator`, `isNorthwoodStaff`,
`isBypassEnabled`, `isGodModeEnabled`, `isNoclipEnabled`, `gravity`,
`roleChangeReason`, `roleSpawnFlags`, `auxiliaryPower`, `emotion`, `experience`,
`maxAuxiliaryPower`, `sizeX`, `sizeY`, `sizeZ`, `accessTier`, `relativeX`,
`relativeY`, `relativeZ`, `isNpc`, `isDummy`, `isSpeaking`, `isSpectatable`,
`isJumping`, `isGrounded`, `stamina`, `movementState`, `roleColor`, `lifeId`,
`unitId`, `unit`, `cRole`, `isTransmitting`, `hasRemoteAdminAccess`, and
`valType`.

The explicit reference additions are:

| Reference type | Added properties |
|---|---|
| item | `inInventory` |
| door | `remainingHealth`, `maxHealth`, `isGate`, `isBreakable`, `isCheckpoint`, `isBroken`, `isMoving` |
| pickup | `posX`, `posY`, `posZ` |
| room | `posX`, `posY`, `posZ` |
| damage handler | `damage`, `hitbox`, `firearmUsed`, `attacker` |
| respawn wave | `respawnTokens`, `influence`, `timeLeft` |
| JSON object | `value`, plus any JSON key as a dynamic property |
| JSON token | `type`, `path`, `root`, `parent`, `children`, `asText`, `asNumber`, `asBool` |
| IP information | `isVPN`, `isHosting`, `provider`, `country`, `type`, `riskScore`, `confidence`, `firstSeen`, `lastSeen` |

Every reference also reflects all public instance properties and fields of its
runtime CLR type. Names are lower-camel-cased from the CLR member name. Public
setters and mutable fields become writable. Base-class registrations are
included. Framework shell references expose both the shell and wrapped
framework object, and forward writes when supported.

This reflection fallback is the largest unstable part of the language: the
exact surface changes with SCP:SL, LabAPI, EXILED, ProjectMER, UCR, and other
dependency versions. A rewrite should keep the explicit properties above as a
stable contract, then either version and generate the reflected surface or
replace it with declared adapters. `serhelp properties [type]` and the
`#properties.txt` report produced by `serdocs` on the exact server dependency
set are the only reliable records of reflected properties.

## Predefined player groups

SER currently generates 47 `@` variables from the installed game enums and the
ready-player list:

- Other: `@all`, `@allPlayers`, `@alivePlayers`, `@npcPlayers`, `@empty`,
  `@emptyPlayers`.
- Zones: `@lightContainmentPlayers`, `@heavyContainmentPlayers`,
  `@entrancePlayers`, `@surfacePlayers`, `@otherPlayers`.
- Teams/factions: `@scpPlayers`, `@foundationForcePlayers`,
  `@chaosInsurgencyPlayers`, `@otherAlivePlayers`, and `@deadPlayers`.
- Roles in this snapshot: `@nonePlayers`, `@scp173Players`, `@classDPlayers`,
  `@spectatorPlayers`, `@scp106Players`, `@ntfSpecialistPlayers`,
  `@scp049Players`, `@scientistPlayers`, `@scp079Players`,
  `@chaosConscriptPlayers`, `@chaosMarauderPlayers`,
  `@chaosRepressorPlayers`, `@chaosRiflemanPlayers`, `@scp096Players`,
  `@scp0492Players`,
  `@ntfSergeantPlayers`, `@ntfCaptainPlayers`, `@ntfPrivatePlayers`,
  `@tutorialPlayers`, `@facilityGuardPlayers`, `@scp939Players`,
  `@customRolePlayers`, `@overwatchPlayers`, `@filmmakerPlayers`,
  `@scp3114Players`, `@destroyedPlayers`, `@alphaFlamingoPlayers`,
  `@zombieFlamingoPlayers`, `@flamingoPlayers`, `@chaosFlamingoPlayers`,
  `@ntfFlamingoPlayers`, and `@deadPlayers`.

The runtime derives these names from `RoleTypeId`, `FacilityZone`, and `Team`.
That means enum additions or renames change the predefined surface. Preserve the
generation algorithm or freeze an explicit compatibility list in a rewrite.

## Entry points and flags

Every flag name is reflection-discovered from a `Flag` subclass. External
plugins can register more flags, but the built-in eight are:

| Flag | Starts when | Inputs and injected values |
|---|---|---|
| `CustomCommand` | A registered command is used | Required command name. Options: `availableFor`, `requireSender`, `description`, `neededPermission`, `noPermissionMessage`, `neededRank`, `invalidRankMessage`, `cooldown`, `onCooldownMessage`, `globalCooldown`, `onGlobalCooldownMessage`, `maxUses`, `onMaxUsesMessage`, `globalMaxUses`, `onGlobalMaxUsesMessage`, `arguments`. Injects `@sender` when player-backed, `*command`, and `$argumentName` values. |
| `OnEvent` | A named LabAPI event fires | Required event name; optional `require` variables. Event properties become `ev` variables. A missing required variable silently skips the section. |
| `OnPMER` | A supported ProjectMER event fires | Same `require` behavior. ProjectMER schematics use SER's shell reference. |
| `OnUCR` | A supported UCR lifecycle event fires | Same `require` behavior. Injects the player, role definition, or active role instance when supplied by UCR. |
| `OnCRole` | A SER custom role is spawned or removed | Inline `Spawned` or `Removed`; optional `forRoles`. Injects `@evPlayer` and `*evCRole`. |
| `OnCustomTrigger` | `Trigger "name"` fires | Required trigger name. |
| `InteractableToyEvent` | A SER-created interactable toy is used | No arguments. Injects `@evPlayer` and `*evToy`. |
| `Function` | `RunFunc` calls a legacy cross-file function | Optional globally unique inline function name and one or more required `-- argument` variable declarations. Only this call path is allowed. |

`CustomCommand` can be exposed to player console, Remote Admin, server console,
or a combination. Required command arguments must precede optional names ending
in `?`. Per-player and global cooldowns and use counts are held by the flag
instance, so a reload or round reset clears them. Permission and rank lists use
OR matching. `requireSender` is incompatible with server-console availability.

`OnEvent`, `OnPMER`, and `OnUCR` reject manual execution because their run
reason must be `Event`. `Function` rejects missing or wrong-family variables.
Function names are globally unique across all files; unnamed legacy functions
remain addressable by their file or section selector.

## Events

SER discovers LabAPI's static event handlers at runtime. At this snapshot there
are 374 events across 15 groups:

| Group | Count |
|---|---:|
| Objective | 12 |
| Player | 186 |
| SCP-049-2 | 4 |
| SCP-049 | 11 |
| SCP-079 | 24 |
| SCP-096 | 14 |
| SCP-106 | 10 |
| SCP-127 | 5 |
| SCP-173 | 14 |
| SCP-3114 | 10 |
| SCP-914 | 10 |
| SCP-939 | 7 |
| shared SCP | 1 |
| Server | 60 |
| Warhead | 6 |

The exact names, descriptions, event argument types, cancellability, and
injected variables are in `eventDetails` in the truth table. The discovery
algorithm turns each public event-argument property into a local name prefixed
with `ev`, then converts its CLR value into the nearest SER value family.
Consequently, the event surface follows the installed LabAPI version.

`IsAllowed` can change only the event which started the current execution and
only while the original cancellable callback is still waiting. `DisableEvent`
and `EnableEvent` globally veto or restore a named cancellable event and return
whether state changed. `AddEventHandler` attaches an inline function callback;
calling it again for the same function replaces that handler.

ProjectMER adds `ButtonInteracted`, `SchematicDestroyed`, `SchematicSpawned`,
and cancellable `SchematicSpawning`. UCR 9.6.0 or newer adds `Registered`,
cancellable `Registering`, `Removed`, `Spawned`, cancellable `Spawning`, and
`Unregistered`.

## Built-in method families

The complete signatures and contracts are in `methods` in the truth table.
This index states the capability represented by each family and gives every
method name, so a rewrite cannot silently lose a subsystem.

| Family | Count | Capability and methods |
|---|---:|---|
| Admin toys | 16 | Create, destroy, move, rotate, scale, parent, inspect, and teleport toys; set camera, interactable, light, primitive, shooting-target, and text properties. `Toy.Create`, `Toy.Destroy`, `Toy.Info`, `Toy.Move`, `Toy.SetParent`, `Toy.SetRotation`, `Toy.SetScale`, `Toy.TPPlayer`, `Toy.TPPosition`, `Toy.TPRoom`, `SetCameraProperties`, `SetInteractableProperties`, `SetLightSourceProperties`, `SetPrimitiveObjectProperties`, `SetShootingTargetProperties`, `SetTextProperties`. |
| Audio | 10 | Load mono 48 kHz `.ogg` files; create global, positional, or player-attached speakers; play, stop, query, and destroy them. `Audio.Load`, `Audio.Play`, `Audio.Stop`, `Audio.IsLoaded`, `Audio.IsPlaying`, `Speaker.CreateGlobal`, `Speaker.CreateOnPosition`, `Speaker.CreatePlayerAttached`, `Speaker.Destroy`, `Speaker.Exists`. |
| Broadcast | 6 | Broadcasts, animated broadcasts, hints, countdowns, and clearing. `Broadcast`, `AnimatedBroadcast`, `Hint`, `Countdown`, `ClearCountdown`, `ClearBroadcasts`. |
| CASSIE | 4 | Global or per-player announcements and speaking state. `Cassie`, `PlayerCassie`, `ClearCassie`, `IsCassieSpeaking`. |
| Player selection | 9 | Count, take, parse, filter, union, intersection, overlap, exclusion, and membership. `AmountOf`, `Take`, `ParsePlayers`, `Filter`, `Join`, `Intersect`, `Overlapping`, `Except`, `Contains`. |
| Player control | 27 | Ban/kick/mute, roles and appearance, rank/group and shown info, movement and physics, stamina, emotes, noclip/bypass/god mode, intercom, hit markers, permissions, ammo limits, and ProxyCheck IP information. `Ban`, `Kick`, `Mute`, `ResetMute`, `SetRole`, `SetAppearance`, `SetGroup`, `SetDisplayName`, `SetCustomInfo`, `SetShownPlayerInfo`, `SetSize`, `SetGravity`, `Jump`, `Stamina`, `SetNoclip`, `SetBypass`, `SetGodMode`, `SetSpectatability`, `SetPlayerIntercom`, `SetEmote`, `Show`, `ShowHitMarker`, `HasPermission`, `Explode`, `GetAmmoLimit`, `SetAmmoLimit`, `GetIPInfo` (alias `GetIPInfoWithKey`). |
| Health | 9 | Damage, kill, heal, normal/max health, artificial health, Hume shield, and regeneration. `Damage`, `Kill`, `Heal`, `SetHealth`, `SetMaxHealth`, `SetAHP`, `SetHumeShield`, `AddHume`, `SetRegeneration`. |
| Items and pickups | 25 | Give/drop/destroy items and ammo; loadouts, candy, radio range, force-equip, firearm/usable inspection; create/spawn/destroy pickups and grenades. `GiveItem`, `DropItem`, `DestroyItem`, `AdvGiveItem`, `AdvDropItem`, `AdvDestroyItem`, `GiveAmmo`, `DropAmmo`, `DestroyAmmo`, `GetAmmo`, `GiveCandy`, `GrantLoadout`, `ClearInventory`, `ForceEquip`, `SetRadioRange`, `FirearmItemInfo`, `UsableItemInfo`, `CreatePickup`, `CreateGrenade`, `SpawnPickupPlayer`, `SpawnPickupPos`, `SpawnPickupRoom`, `AddPickupToInventory`, `DestroyPickup`, `PickupExists`. |
| Doors, rooms, elevators, lights | 20 | Door state, locks, permissions, health and repair; room lookup/blackout; elevator movement/locks/text; room lights. `OpenDoor`, `CloseDoor`, `LockDoor`, `UnlockDoor`, `SetDoorPermission`, `SetDoorHealth`, `SetDoorMaxHealth`, `BreakDoor`, `RepairDoor`, `PryGate`, `GetRandomDoor`, `GetRoomByName`, `Blackout`, `SendElevator`, `LockElevator`, `UnlockElevator`, `SetElevatorText`, `DisableLights`, `SetLightColor`, `ResetLightColor`. |
| Teleport | 5 | Move players to a player, position, room-relative position, door, or role spawn. `TPPlayer`, `TPPosition`, `TPRoom`, `TPDoor`, `TPSpawn`. |
| Map | 7 | Read map objects/pickups, decontamination control/status, ragdoll creation and cleanup. `GetFromMap`, `GetPickups`, `Decontamination`, `DecontaminationInfo`, `CreateRagdoll`, `CleanupRagdolls`, `CleanupPickups`. |
| Round/server/warhead/intercom | 13 | Start/end/restart and lock rounds, server information and friendly fire, full warhead control/status, intercom state/text/status. `StartRound`, `EndRound`, `RestartRound`, `RoundInfo`, `SetRoundLock`, `SetLobbyLock`, `ServerInfo`, `SetFriendlyFire`, `Warhead`, `WarheadInfo`, `SetIntercomState`, `SetIntercomText`, `IntercomInfo`. Respawn methods are listed separately below. |
| SCP-specific | 5 | SCP-096 targets, SCP-173 observers, and SCP-079 power/experience/tier. `Get096Targets`, `Get173Observers`, `Set079AuxPower`, `Set079Exp`, `Set079AccessTier`. |
| Respawn | 6 | Wave timer, effect, manual spawn, tokens, influence, and time. `GetWaveTimer`, `PlayWaveEffect`, `SpawnWave`, `WaveRespawnTokens`, `WaveInfluence`, `WaveRespawnTime`. |
| Output and command | 6 | Console output/replies/errors and executing commands with optional sender; reset custom-command cooldowns. `Print`, `Reply`, `Error`, `Command`, `ResetPlayerCommandCooldown`, `ResetGlobalCommandCooldown`. |
| Script/event | 13 | Run, wait for, stop, inspect, and detect scripts; transfer variables, invoke legacy functions, fire triggers; event cancellation, global event enable/disable, and dynamic handlers. `RunScript`, `RunScriptAndWait`, `RunFunc`, `StopScript`, `IsRunning`, `ScriptExists`, `ThisInfo`, `TransferVariables`, `Trigger`, `IsAllowed`, `EnableEvent`, `DisableEvent`, `AddEventHandler`. |
| Variables/collections | 12 | Inspect variables and edit collection variables. `VarExists`, `GetVariableByName`, `GlobalVariables`, `LogVar`, `Coll.Create`, `Coll.Fetch`, `Coll.Insert`, `Coll.Remove`, `Coll.RemoveAt`, `Coll.Contains`, `Coll.Join`, `Coll.Subtract`. |
| Number/text/time | 15 | Random and chance, rounding, hex conversion; text contains/replace/slice/pad/trim; current date, offsets, time information, duration construction and formatting. `Random`, `Chance`, `Round`, `IntToHex`, `HexToInt`, `Text.Contains`, `Text.Replace`, `Text.Slice`, `Text.Pad`, `Text.Trim`, `Date.Current`, `Date.Offset`, `TimeInfo`, `ToDuration`, `FormatDuration`. |
| Data structures and files | 17 | Persistent databases, in-memory dictionaries, JSON create/parse/edit/format, and YAML custom config reads. `DB.Create`, `DB.Exists`, `DB.Add`, `DB.Get`, `DB.Contains`, `DB.Remove`, `Dict.Create`, `Dict.Add`, `Dict.Get`, `Dict.Contains`, `Dict.Remove`, `JSON.Create`, `JSON.Parse`, `JSON.Add`, `JSON.FormatToReadable`, `Config.Read`, `Config.GetOption`. HTTP requests are listed separately below. |
| Network and Discord | 12 | JSON HTTP GET/POST/PATCH with policy limits; Discord webhook messages and embeds, including edit/delete and wait-for-response. `HTTP.Get`, `HTTP.Post`, `HTTP.Patch`, `Discord.CreateMessage`, `Discord.SendMessage`, `Discord.SendMessageAndWait`, `Discord.EditMessage`, `Discord.DeleteMessage`, `Embed.Create`, `Embed.CreateAuthor`, `Embed.CreateFooter`, `Embed.CreateField`. |
| Persistent player data | 4 | Round-scoped values keyed by one player and text key. `SetPlayerData`, `GetPlayerData`, `HasPlayerData`, `ClearPlayerData`. |
| Damage and Tesla rules | 4 | Add/remove multiplying attacker or receiver damage rules and player Tesla-ignore rules. `AddDamageRule`, `RemoveDamageRule`, `AddTeslaIgnoreRule`, `RemoveTeslaIgnoreRule`. |
| SER custom roles | 9 | Define a display role over a base role, assign/unregister it, callbacks, and chance/procedural/bracket round-start spawning. `CRole.Register`, `CRole.Unregister`, `CRole.IsRegistered`, `CRole.Set`, `CRole.SetCallbacks`, `CRole.CreateChanceSpawnSystem`, `CRole.CreateProceduralSpawnSystem`, `CRole.CreateSpawnBracket`, `CRole.CreateBracketSpawnSystem`. |
| Dummy | 2 | Create and destroy dummy players. `AddDummy`, `DestroyDummy`. |
| Effects | 3 | Apply, clear, and query status effects. `GiveEffect`, `ClearEffect`, `HasEffect`. |
| Callvote integration | 3 | Build options and start votes synchronously or with a wait. `Vote.CreateOption`, `Vote.Start`, `Vote.StartAndWait`. |
| ProjectMER integration | 28 | List/load/save/unload/merge maps; read map data and object definitions; create/delete/rename/move/refresh/transform objects; spawn/destroy/show/hide schematics; inspect blocks/animators and control animations. `MER.LoadMap`, `MER.UnloadMap`, `MER.SaveMap`, `MER.MergeMaps`, `MER.GetAvailableMaps`, `MER.GetAvailableSchematics`, `MER.GetLoadedMaps`, `MER.GetLoadedMap`, `MER.ReadMapData`, `MER.GetObjects`, `MER.GetObjectDefinition`, `MER.CreateObject`, `MER.DeleteObject`, `MER.RenameObject`, `MER.MoveObjectToMap`, `MER.RefreshObject`, `MER.SetPosition`, `MER.SetRotation`, `MER.SetScale`, `MER.SpawnSchematic`, `MER.DestroySchematic`, `MER.GetSchematicBlocks`, `MER.GetAnimators`, `MER.PlayAnimation`, `MER.SetAnimationState`, `MER.StopAnimation`, `MER.ShowSchematic`, `MER.HideSchematic`. |
| UCR integration | 11 | List and resolve UCR roles, instances, players and spawn counts; assign/remove roles and test registration. `UCR.GetRoles`, `UCR.GetRoleById`, `UCR.GetRoleByName`, `UCR.GetRole`, `UCR.GetRoleInstance`, `UCR.GetRoleInstances`, `UCR.GetPlayersWithRole`, `UCR.GetSpawnedCount`, `UCR.IsRegistered`, `UCR.SetRole`, `UCR.RemoveRole`. |

The table intentionally describes outcomes. For exact spelling, argument order,
defaults, enum choices, return families, and errors, use the truth table rather
than inferring them from these summaries.

## State and persistence

| System | Lifetime and behavior |
|---|---|
| local variables | One script execution. Callback executions get their own table. |
| global variables | Shared by all scripts until map generation resets the runtime. |
| predefined variables | Live round state generated on read. |
| player data | In-memory per-player/key values; cleared on map generation and plugin disable. |
| SER custom roles | Registered and assigned for the round; cleared on map generation/disable. Assignment is tracked by player life ID and normally removed on death or role change. |
| damage/Tesla rules | In-memory lists, cleared on map generation/disable. Optional IDs remove matching rules; no ID clears all. Dynamic player getters can refresh targets. |
| custom-command counts/cooldowns | Bound flag state; cleared when the binding reloads or the runtime resets. |
| databases | JSON under `Databases`. Stores literal values, colors in stable text form, and player account IDs. Live references and collections are rejected. Player reads return only currently ready matching accounts. Writes roll back in memory if saving fails. |
| YAML configs | Read-only `.yml`/`.yaml` under `Custom Configs`. `Config.GetOption` currently accepts one key per call even though the internal reader can traverse nested paths. |
| audio | Clip/speaker state managed through AudioPlayerApi. Files are searched recursively under SER's data directory. |

## Optional integrations and external effects

SER can send arbitrary HTTP GET, POST, and PATCH requests expecting JSON,
operate Discord webhooks, and query ProxyCheck.io for player IP information.
All use the configured request timeout and maximum response size. Local network
addresses return a local result without calling ProxyCheck.

Framework-dependent methods register only when their owner is detected:

- EXILED: `GetAmmoLimit`, `SetAmmoLimit`, and `SetAppearance`. They are always
  present in the EXILED build and appear in the LabAPI build only when Exiled
  Loader is active.
- Callvote: the three `Vote.*` methods. Callvote is detected only as a LabAPI
  plugin.
- ProjectMER: 28 `MER.*` methods and four `OnPMER` events. LabAPI and supported
  EXILED-hosted ProjectMER plugins can be detected.
- UncomplicatedCustomRoles: 11 `UCR.*` methods and six `OnUCR` events. The
  lifecycle bridge targets UCR 9.6.0 or newer.

External plugins can add methods through `MethodIndex.AddAllDefinedMethodsInAssembly`
or `AddMethod`, and flags through `Flag.RegisterFlagsAsExternalPlugin`. Method
class names become script names by removing `Method` and changing underscores
to dots. Aliases share one method instance.

## Server-owner controls

| Command | Permission | Capability |
|---|---|---|
| `serrun <name>` | `ser.run` | Refresh and start one file or numbered section. |
| `sermethod <line>` | `ser.run` | Compile and run one SER line; immediately return a synchronous method result when applicable. |
| `serrunning` | `ser.run` | List active executions. |
| `serstop <name>` | `ser.stop` | Stop every matching instance; a physical filename matches all its sections. |
| `serstopall` | `ser.stop` | Stop all running scripts. |
| `serreload` | `ser.reload` | Transactionally refresh the complete script tree and report failures. |
| `serstatus [all]` (alias `serlist`) | `ser.run` | Inspect accepted, failed, disabled, excluded, linked, and conflicting paths. |
| `serhelp [topic]` | none | Browse start help, methods, flags, events, variables, properties, enums, and integrations. |
| `serexamples` | none | Write missing bundled examples with `#`-disabled names. Existing files are not overwritten. |
| `serdocs` | `ser.docs` | Generate plain-text methods, variables, events, keywords, enums, properties, and general help under SER's `Documentation` folder. |

Configuration options are:

| Setting | Default | Effect |
|---|---:|---|
| `IsEnabled` | `true` | Enables SER. |
| `Debug` | `false` | EXILED build only; enables EXILED debug logging. |
| `SendInitMessage` | `true` | Prints a concise ready/status message. |
| `SafeScripts` | `true` | Adds safety pauses; keep it on unless every script is checked. Prevents current-event cancellation. |
| `SendLogo` | `false` | Prints the large logo and contributor list at enable. |
| `ShowContributorBadges` | `false` | Gives recognized contributors without a rank a temporary SER badge. |
| `NetworkRequestTimeoutSeconds` | `15` | Cancels slow HTTP, Discord, and IP-information requests. |
| `MaxNetworkResponseBytes` | `1,048,576` | Rejects oversized network response bodies. |

## Tooling and distribution capabilities

- Every plugin build loads the built DLL, indexes methods, initializes
  registries, and compiles every embedded example. It also regenerates the
  method/truth-table data and visual editor.
- SER Blocks is a generated standalone HTML block editor. The VS Code extension
  exposes completions, hovers, diagnostics, language configuration, and the same
  block editor.
- `Tooling/shared/ser-language-core.js` is shared between editor surfaces.
  Tooling tests validate the manifest, language core, editor logic, and extension
  configuration.
- The release packager produces LabAPI and EXILED DLLs, examples/docs, editor,
  extension, a complete bundle, and SHA-256 sums.
- `serhelp`, website reference data, standalone editor data, and extension data
  are generated from one runtime metadata source. A rewrite should retain this
  single-source property even if it changes formats.

## Rewrite seams and compatibility decisions

These are capabilities today, but their current implementation should not be
copied without an explicit decision:

1. **Physical files, sections, and flags are three overlapping identities.**
   Model a file as a module and each declaration as a named handler. Preserve
   `filename:N`, bare-name ambiguity, and file-wide stop/reload rules only at
   the compatibility boundary.
2. **All flags are “major,” while the registry supports several.** Define flag
   composition in the language model. Today the observable contract is one
   behavior flag per section.
3. **Properties mix a stable SER API with unbounded CLR reflection.** Declare a
   versioned property schema. Put unsafe/reflected access behind an explicit
   compatibility or advanced mode.
4. **The `$` family is a union rather than one type.** Decide whether literal
   variables remain dynamically typed or gain type inference/annotations, while
   preserving accepted values and conversions.
5. **Scopes are lifetime cleanup on one dictionary.** A new runtime can use real
   lexical/function frames as long as collision behavior, `global`, `ephm`, and
   callback isolation are either emulated or migrated clearly.
6. **Synchronous event cancellation conflicts with safety yielding.** Give event
   handlers an explicit synchronous decision phase or separate cancellable
   predicates from asynchronous actions.
7. **Runtime discovery makes the API dependency-version-shaped.** Generate and
   version event, enum, predefined-variable, and reflected-property schemas for
   each supported server baseline.
8. **Method metadata and execution live in reflection-created mutable method
   objects.** A rewrite should use immutable method descriptors plus per-call
   argument state.
9. **Persistence formats contain CLR type names.** Keep a migration reader for
   existing database JSON, but write a stable, implementation-neutral type tag
   format.
10. **Round reset is the cleanup boundary for many unrelated systems.** Give
    globals, player data, rules, custom roles, command limits, audio, callbacks,
    and coroutines explicit owners and lifetimes.
11. **Callbacks recompile copied function bodies.** Use callable objects or AST
    references with source revision tracking, while preserving the current rule
    that the latest accepted file is used when the callback fires.
12. **Several lists are generated from external enums.** Preserve names through
    aliases or a compatibility manifest when game updates rename roles, teams,
    effects, items, events, or lock reasons.

## Minimum parity test for a replacement

A replacement is capability-complete only when it can:

1. Load nested `.ser` and `.txt` files with the same disable, link, duplicate,
   section, and transaction rules.
2. Compile every script under `Example Scripts` and report the same physical
   source lines for failures.
3. Expose every method, keyword, flag, predefined variable, event, and enum in
   the truth table, plus every stable property listed here, or publish an
   intentional migration for each omission.
4. Run the same script from server console, Remote Admin, player console,
   another script, a legacy function, an inline callback, a game event, a custom
   trigger, a custom command, a custom role, an interactable toy, PMER, and UCR.
5. Match variable families, player readiness pruning, collection indexing,
   interpolation, expression aliases, property chaining, waits, control flow,
   error catching, and stop behavior.
6. Preserve cancellable-event timing or provide a documented replacement that
   works safely with script throttling.
7. Preserve round reset, reload, last-known-good rollback, database migration,
   custom config, player data, command limits, rules, audio, network policy, and
   optional-framework behavior.
8. Generate server help and editor metadata from the same descriptors used by
   the runtime.

Freeze a copy of the truth table and run `serdocs` against the final supported
SCP:SL, LabAPI, EXILED, ProjectMER, and UCR assemblies before beginning a
rewrite. Together, those capture the dependency-shaped event, enum, and
reflected-property surface which source-only summaries cannot make stable.
