﻿using System;
using System.IO;

namespace Configuration
{
    public static class Config
    {
        private static readonly string CDir = $"{Directory.GetCurrentDirectory()}/";

        public static string Token
        {
            get => File.ReadAllText(Directory.GetCurrentDirectory() + "/Token.cfg");
        }

        public static string DataBaseConnectionString
        {
            get
            {
                var pwd = File.ReadAllText(Directory.GetCurrentDirectory() + "/DBPassword.cfg");
                return $"Server=joidb.ddns.net;Database=Discord;user id=DiscordUser;password={pwd}";
            }
        }

        public static class BotFileFolders
        {
            internal static readonly string MainFolder = $"{CDir}/BotFiles/";

            public static readonly string SlaveReports = $"{MainFolder}/SlaveReports";

            public static readonly string WheelResponses = $"{MainFolder}/WheelResponses";
            public static readonly string WheelLinks = $"{WheelResponses}/Links";

            public static readonly string UserData = $"{MainFolder}/UserData";
        }

        public static class Channels
        {
            public const string Instruction = "slave-instruction";
            public const string Wheel = "wheel-of-misfortune";
        }

        public static class Users
        {
            public const ulong Aki = 335437183127257089ul;
        }

        public static class Emojis
        {
            public static string Blush = ":blush:";
            public static string Heart = ":heart:";

            public static string[] Confirms = new[] { ":white_check_mark:", ":ballot_box_with_check:", ":heavy_check_mark:", ":thumbsup" };
            public static string[] Declines = new[] { ":negative_squared_cross_mark:", ":x:", ":deletdis:", ":underage:", ":no_entry_sign:" };
        }

        public static class Pornhub
        {
            public static string[] Channels = new[] { "youranimeaddiction", "damitrix", "mosbles", "milkduds46", "elukajoi", "pjsx" };

            public static ulong[] ChannelsToPostTo = new[] { 457465277395894273ul };

            public static string IndexedVideoLocation = $"{BotFileFolders.MainFolder}/PornhubVideos";
        }

        public static class Tumblr
        {
            public static string Url = "deliciousanimefeet.tumblr.com";
            public static ulong[] ChannelsToPostTo = new[] { 448417831067975680ul };
        }
    }
}