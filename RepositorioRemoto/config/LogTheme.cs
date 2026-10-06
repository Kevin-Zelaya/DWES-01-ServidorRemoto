using Serilog.Sinks.SystemConsole.Themes;

public static class LogTheme
{
    public static readonly SystemConsoleTheme Theme =
        new SystemConsoleTheme(
            new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
            {
                [ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle
                {
                    Foreground = ConsoleColor.Gray
                },

                [ConsoleThemeStyle.SecondaryText] = new SystemConsoleThemeStyle
                {
                    Foreground = ConsoleColor.DarkGray
                },

                [ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle
                {
                    Foreground = ConsoleColor.Gray
                },

                [ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle
                {
                    Foreground = ConsoleColor.Green
                },

                [ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle
                {
                    Foreground = ConsoleColor.Yellow
                },

                [ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle
                {
                    Foreground = ConsoleColor.Red
                },

                [ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle
                {
                    Foreground = ConsoleColor.DarkRed
                }
            });
}