using System.IO;

namespace Configuration
{
    public static class Config
    {
        private static readonly string CDir = $"{Directory.GetCurrentDirectory()}/";

        public static string DataBaseConnectionString
        {
            get
            {
                var pwd = File.ReadAllText(Directory.GetCurrentDirectory() + "/DBPassword.cfg");
                return $"Server=joidb.ddns.net;Database=Discord;user id=DiscordUser;password={pwd}";
            }
        }

        public static string Token
        {
            get => File.ReadAllText(Directory.GetCurrentDirectory() + "/Token.cfg");
        }

        public static class BotFileFolders
        {
            public static readonly string SlaveReports = $"{MainFolder}/SlaveReports";
            public static readonly string UserData = $"{MainFolder}/UserData";
            public static readonly string WheelLinks = $"{WheelResponses}/Links";
            public static readonly string WheelResponses = $"{MainFolder}/WheelResponses";
            internal static readonly string MainFolder = $"{CDir}/BotFiles/";
        }

        public static class Channels
        {
            public const string Instruction = "slave-instruction";
            public const string Wheel = "wheel-of-misfortune";
        }

        public static class Emojis
        {
            public static string Blush = ":blush:";
            public static string[] Confirms = new[] { ":white_check_mark:", ":ballot_box_with_check:", ":heavy_check_mark:", ":thumbsup" };
            public static string[] Declines = new[] { ":negative_squared_cross_mark:", ":x:", ":deletdis:", ":underage:", ":no_entry_sign:" };
            public static string Heart = ":heart:";
        }

        public static class Pornhub
        {
            public static string[] Channels = new[] { "youranimeaddiction", "damitrix", "mosbles", "milkduds46", "elukajoi", "pjsx", "konekosalem" };

            public static string IndexedVideoLocation = $"{BotFileFolders.MainFolder}/PornhubVideos";

            public static ulong[] ChannelsToPostTo
            {
                get
                {
#if (DEBUG)
                    return new[] { 450793619398459432ul };
#else
                    return new[] { 457465277395894273ul };
#endif
                }
            }
        }

        public static class Tumblr
        {
            public static ulong[] ChannelsToPostTo = new[] { 448417831067975680ul };
            public static string Url = "deliciousanimefeet.tumblr.com";
        }

        public static class Users
        {
            public const ulong Aki = 335437183127257089ul;
            public const ulong Salem = 249216025931939841ul;
            public const ulong Weyui = 193029284376346624ul;
        }
    }
}