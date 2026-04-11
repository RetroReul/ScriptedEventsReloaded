using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.BroadcastMethods;

[UsedImplicitly]
public class AnimatedBroadcastMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Sends an animated broadcast to all players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DurationArgument("duration"),
        new TextArgument("content")
    ];
    
    public override void Execute()
    {
        var content = Args.GetText("content");
        var duration = Args.GetDuration("duration").TotalSeconds;
        Announcer.Clear();
        Announcer.Message(
            $"$SLEEP_{duration} .", 
            Helper.FormatToCassieCentralScreenSubtitles(content, true), 
            false,
            696969,
            0
        );
    }

    public string AdditionalDescription =>
        "Uses CASSIE to make an animated broadcast - if there is CASSIE playing, it will be stopped. " +
        "Keep custom formatting to a minimum, this system is very limited.";
}

public static class Helper
{
    public static string FormatToRawCassieSubtitles(string text, bool addWaits)
    {
        var result = "";
        var index = 72;
    
        foreach (var line in text.Split('\n'))
        {
            // Skip empty lines
            if (line.Length == 0)
            {
                index -= 1;
                continue;
            }
    
            // Calculate actual length excluding HTML tags
            var len = CalculateTextLength(line);
            var parts = new List<string>();
    
            // Split long lines
            if (len > 80)
            {
                SplitLongLine(line, parts);
            }
            else
            {
                parts.Add(line);
            }
    
            // Add all parts to result with proper formatting
            foreach (var part in parts)
            {
                index -= 1;
                result += FormatLine(part, index);
            }
        }
    
        return result;
    }
    
    private static int CalculateTextLength(string line)
    {
        var len = 0;
        var isTag = false;
    
        foreach (var c in line)
        {
            switch (c)
            {
                case '<':
                    isTag = true;
                    continue;
                case '>':
                    isTag = false;
                    continue;
            }
    
            if (!isTag) len++;
        }
    
        return len;
    }
    
    private static void SplitLongLine(string line, List<string> parts)
    {
        int? lastUnusedSpaceIndex = null;
        
        for (var i = 0; i < line.Length; i++)
        {
            if (!char.IsWhiteSpace(line[i])) continue;

            if (i <= 50)
            {
                lastUnusedSpaceIndex = i;
                continue;
            }
            
            var lastAvailableSpaceIndex = lastUnusedSpaceIndex ?? i;
            var leftPart = line[..lastAvailableSpaceIndex].Trim();
            parts.Add(leftPart);
            
            var rightPart = line[(lastAvailableSpaceIndex + 1)..].Trim();
            if (CalculateTextLength(rightPart) > 80)
            {
                SplitLongLine(rightPart, parts);
            }
            else
            {
                parts.Add(rightPart);
            }
            
            return;
        }
        
        parts.Add(line);
    }
    
    private static string FormatLine(string text, int index)
    {
        const string waitReplacement = "<cspace=0em><size=0>.........................</size></cspace>";
        return $"<voffset={index}em>{text.Replace("[wait]", waitReplacement)}</voffset>\\n";
    }

    public static string FormatToCassieCentralScreenSubtitles(string text, bool addWaits)
    {
        return
            @"<line-height=2800>\n</line-height></size><align=center><size=30><line-height=0%>\n"
            + FormatToRawCassieSubtitles(text, addWaits);
    }

    public static string FormatToCassieSpeechSubtitles(string text, bool addWaits)
    {
        return @"<line-height=3500>\n</line-height></size><align=left><size=25><line-height=0%>\n"
               + FormatToRawCassieSubtitles(text, addWaits);
    }
}

