# SER Scripting Engine: Developer Guide and Architecture Overview

> [!WARNING]
> This document has been AI-generated and should not be regarded as the definitive source of truth. Always verify implementation details against the current codebase in `SER.sln`.

Welcome to the developer guide for the SER (Scripted Events Reloaded) scripting engine. This document provides a deep technical breakdown of the engine's architecture, lifecycle, and extension systems.

---

## 1. Script Lifecycle: From Text to Execution

The SER engine transforms raw text into a hierarchical tree of executable contexts.

### 1.1. Phase 1: Tokenization (The Lexer)
The `Tokenizer.cs` breaks raw lines into "Slices" (see `Slice.cs`), which are then converted into `BaseToken` instances.
*   **Slices**: 
    *   **SingleSlice**: Simple words or symbols.
    *   **CollectionSlice**: Groups like `"..."`, `(...)`, or `{...}`.
*   **Token Importance**: `OrderedImportanceTokensFromSingleSlices` in `Tokenizer.cs` defines a hierarchy. The engine prioritizes specific tokens (like `MethodToken` or `KeywordToken`) over generic identifiers.

### 1.2. Phase 2: Contexting (The Compiler)
The `Contexter.cs` builds the execution tree using a **Statement Stack** (`Stack<StatementContext>`).
*   **Blocks**: `IContextableToken`s (like `IfToken`) push a new context onto the stack. All following lines are added as children until an `EndKeyword` pops the stack.
*   **Inline Extensions**: Keywords like `with` allow line-level context modification before a statement is finalized.

### 1.3. Phase 3: Execution (The Runtime)
Execution is managed via MEC Coroutines to prevent server hangs during long-running scripts.
*   **Yielding vs Synchronous**: `SynchronousMethod` runs instantly. `YieldingMethod` can pause (e.g., `yield return Timing.WaitForSeconds()`).

---

## 2. Automated Discovery and Reflection

SER uses C# reflection to automatically find and register Methods and Arguments.

### 2.1. Discovery Logic
*   **Methods**: Any non-abstract class inheriting from `Method` is automatically registered (see `MethodIndex.cs`).
*   **Arguments**: Methods define `ExpectedArguments`. The `MethodArgumentDispatcher.cs` uses reflection to find the `GetConvertSolution` method on the argument's type.

### 2.2. The Requirement of [UsedImplicitly]
Because many classes are instantiated via reflection, you **MUST** mark them with `[UsedImplicitly]` from `JetBrains.Annotations` to prevent IDEs from flagging them as unused.
*   Apply to: Classes inheriting from `Method`, `Argument`, `BaseFlag`, or `Value`.
*   Apply to: The `GetConvertSolution` method within custom `Argument` classes.

---

## 3. Style Guide and Best Practices

### 3.1. Error Handling: The Result Pattern
We use `Result` and `TryGet<T>` (found in `Result.cs` and `TryGet.cs`) instead of exceptions.
*   **Implicit Conversions**: The `Result` struct supports implicit conversion from `bool` (true = Success) and `string` (Error message). 
    *   `return true;` is equivalent to `Result.Success()`.
    *   `return "Error happened";` is equivalent to `Result.Error("Error happened")`.

### 3.2. Variable Prefixes
The engine uses specific prefixes to identify variable types:
*   `$` : **Literal Variables** (strings, numbers).
*   `@` : **Player Variables** (references to in-game players).
*   `*` : **Reference Variables** (wrappers for C# objects).
*   `&` : **Collection Variables** (lists/groups of data).

---

## 4. Extending the Engine: Practical Steps

### 4.1. Creating a New Method
1.  Inherit from `SynchronousMethod` or `YieldingMethod`.
2.  Override `Description` and `ExpectedArguments`.
3.  Implement `Execute()` or `Run()`.

### 4.2. Creating a New Argument Type
1.  Inherit from `Argument`.
2.  Implement `GetConvertSolution`.
3.  **CRITICAL**: You must also add a corresponding `Get[Type]` method to `ProvidedArguments.cs` to allow methods to fetch the parsed value.

### 4.3. Adding a New Token
1.  Inherit from `BaseToken`.
2.  Manually add the type to `Tokenizer.OrderedImportanceTokensFromSingleSlices`.

---

## 5. The Value System: Under the Hood

The Value System is a complex hierarchy designed to bridge script variables with C# objects. It is divided into two primary categories.

### 5.1. LiteralValue (Constants and Data)
Classes inheriting from `LiteralValue<T>` represent "static" data that can be parsed directly from a script.
*   Examples: `NumberValue`, `TextValue`, `BoolValue`, `ColorValue`.
*   **Dynamic Discovery**: `LiteralValue.Subclasses` automatically finds all classes inheriting from `LiteralValue` that implement `IValueWithProperties`. This allows literals to have built-in properties (e.g., `$count -> abs`).

### 5.2. ReferenceValue (C# Object Wrappers)
`ReferenceValue` wraps external C# objects (like `Player` or `Item`).
*   **Property Registry**: Properties for these values are defined in `ReferencePropertyRegistry.cs`.
*   **Arrow Operator (`->`)**: Accessing properties on any value is done via the arrow operator.
    *   Example: `@player -> health` or `*item -> type`.

### 5.3. IValueWithProperties
Values that implement this interface provide a `Properties` dictionary containing `PropInfo` objects. This allows the script to query metadata about the value.
*   `NumberValue` has properties like `abs`, `round`, `floor`.
*   `PlayerValue` has properties like `health`, `role`, `name`.

---

## 6. Event System and EventHandler

The `EventHandler.cs` connects game events to scripts.
*   **Binding**: Use the `!-- OnEvent EventName` flag.
*   **Extraction**: The handler extracts event data into `@` variables (e.g., `@evPlayer`).

---

## 7. Performance and Debugging

*   **Yielding in Loops**: Any loop (`while`, `repeat`, `forever`) MUST contain a yielding method (like `Wait`) to prevent server freezing.
*   **Logging**: Use `Log.ScriptWarn` for errors that script writers need to see.

---

## 8. File System

The engine scans for both `.ser` and `.txt` files within the script directory (see `FileSystem.cs`). Files starting with `#` are ignored.

---

## 9. Core Type Reference

| Type | Role |
| :--- | :--- |
| `Script` | Root container for state. |
| `Tokenizer` | Lexer (text to tokens). |
| `Contexter` | Compiler (tokens to execution tree). |
| `Value` | Base for all data (`LiteralValue`, `ReferenceValue`). |
| `ProvidedArguments` | Service for fetching typed arguments in methods. |

---

## 10. Implementation Example: Property Access

When a user writes `@plr -> health`, the `ValueExpressionContext` uses the `ValuePropertyHandler`:
1.  It resolves `@plr` to a `PlayerValue`.
2.  It sees the `->` (arrow operator).
3.  It looks up the `health` key in the `PlayerValue.Properties` dictionary.
4.  It executes the `PropInfo.GetValue()` function to return a `NumberValue`.

---
*Technical Note: This guide contains ~150 lines of instructions. Architectural details verified against SER codebase.*
