using Cassie;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using Utf8Json.Internal.DoubleConversion;
using StringBuilder = System.Text.StringBuilder;

namespace SER.Code.MethodSystem.Methods.BroadcastMethods;

[UsedImplicitly]
public class AnimatedBroadcastMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Sends an animated broadcast to all players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DurationArgument("duration"),
        new TextArgument("content"),
        new IntArgument("line break length")
        {
            Description = "How many characters are needed to make a new line",
            DefaultValue = new(60, null)
        },
        new DurationArgument("time per character")
        {
            Description = "How long each character should be displayed",
            DefaultValue = new(null, "no applied slowdown")
        }
    ];
    
    public override void Execute()
    {
        var content = Args.GetText("content");
        var duration = Args.GetDuration("duration").TotalSeconds;
        var timePerChar = Args.GetNullableDuration("time per character");
        var lineBreakLength = Args.GetInt("line break length");
        
        Announcer.Clear();
        Announcer.Message(
            $"$SLEEP_{duration-1} .", 
            Helper.FormatToCassieCentralScreenSubtitles(
                content, 
                lineBreakLength,
                timePerChar
            ), 
            false,
            696969,
            0
        );
    }

    public string AdditionalDescription =>
        "Uses CASSIE to make an animated broadcast - if there is CASSIE playing, it will be stopped. " +
        "Keep custom formatting to a minimum, this system is very limited.";
    
    public static class Helper
    {
        public static string FormatToRawCassieSubtitles(string text, int lineBreakLength, TimeSpan? timePerChar)
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
                if (len > lineBreakLength)
                {
                    SplitLongLine(line, parts, lineBreakLength);
                }
                else
                {
                    parts.Add(line);
                }
        
                // Add all parts to result with proper formatting
                foreach (var part in parts)
                {
                    index -= 1;
                    result += FormatLine(part, index, timePerChar);
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
        
        private static void SplitLongLine(string line, List<string> parts, int lineBreakLength)
        {
            int? lastUnusedSpaceIndex = null;
            
            for (var i = 0; i < line.Length; i++)
            {
                if (!char.IsWhiteSpace(line[i])) continue;

                if (i <= lineBreakLength)
                {
                    lastUnusedSpaceIndex = i;
                    continue;
                }
                
                var lastAvailableSpaceIndex = lastUnusedSpaceIndex ?? i;
                var leftPart = line[..lastAvailableSpaceIndex].Trim();
                parts.Add(leftPart);
                
                var rightPart = line[(lastAvailableSpaceIndex + 1)..].Trim();
                if (CalculateTextLength(rightPart) > lineBreakLength)
                {
                    SplitLongLine(rightPart, parts, lineBreakLength);
                }
                else
                {
                    parts.Add(rightPart);
                }
                
                return;
            }
            
            parts.Add(line);
        }
        
        private static string FormatLine(string text, int index, TimeSpan? timePerChar)
        {
            if (timePerChar is { } time)
            {
                var dots = new string('c', (int)Math.Round(time.TotalMilliseconds / 20, MidpointRounding.AwayFromZero));
                StringBuilder newText = new();
                var isTag = false;
                foreach (var c in text)
                {
                    isTag = c switch
                    {
                        '<' => true,
                        '>' => false,
                        _ => isTag
                    };

                    if (!isTag)
                    {
                        newText.Append($"<size=0>{dots}</size>{c}");
                    }
                    else
                    {
                        newText.Append(c);
                    }
                }
                
                text = newText.ToString();
            }
            
            return $"<voffset={index}em>{text}</voffset>\\n";
        }

        public static string FormatToCassieCentralScreenSubtitles(string text, int lineBreakLength, TimeSpan? timePerChar)
        {
            return
                @"<line-height=2700>\n</line-height></size><align=center><size=30><line-height=0%>\n"
                + FormatToRawCassieSubtitles(text, lineBreakLength, timePerChar);
        }

        // public static string FormatToCassieSpeechSubtitles(string text, bool addWaits)
        // {
        //     return @"<line-height=3500>\n</line-height></size><align=left><size=25><line-height=0%>\n"
        //            + FormatToRawCassieSubtitles(text);
        // }
    }
}

