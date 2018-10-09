// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Configuration;

namespace Sabrina
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using DSharpPlus.Interactivity;
    using DSharpPlus.VoiceNext;
    using DSharpPlus.VoiceNext.Codec;

    using Sabrina.Bots;
    using Sabrina.Entities;
    using Sabrina.Pornhub;
    using Microsoft.Extensions.DependencyInjection;

    using Tables = TableObjects.Tables;

    internal class Program
    {
        private const string Prefix = "//";

        private CommandsNextExtension cmdsNext;

        private InteractivityExtension interactivity;

        private DiscordClient client;

        private SqlConnection conn;

        private TumblrBot tmblrBot;

        public object Voice { get; private set; }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public static void Main(string[] args)
        {
            var prog = new Program();
            prog.MainAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public async Task MainAsync()
        {
            this.SetConfig();
            this.SetCommands();
            this.CreateFolders();

            await this.client.ConnectAsync();
            this.client.Ready += this.OnReadyAsync;
            this.client.MessageCreated += this.ClientMessageCreated;
            this.client.MessageReactionAdded += this.ClientMessageReactionAdded;
            this.client.GuildMemberUpdated += this.ClientGuildMemberUpdated;

            this.conn = new SqlConnection(Config.DataBaseConnectionString);
            await this.conn.OpenAsync();

            await this.client.UpdateStatusAsync(new DiscordActivity("Pictures of Feet", ActivityType.Streaming), UserStatus.Online);
            
            // TODO: Looks weird, cause unused.
            PornhubBot pornhubBot = new PornhubBot(this.client);

            this.tmblrBot = new TumblrBot(this.client);

            var vcfg = new VoiceNextConfiguration { VoiceApplication = VoiceApplication.Music };
            this.Voice = this.client.UseVoiceNext(vcfg);

            var exit = false;
            while (!exit)
            {
                string command = Console.ReadLine();
                switch (command)
                {
                    case "stop":
                    case "exit":
                    case "x":
                        PornhubBot.Exit = true;
                        exit = true;
                        break;
                }
            }

            await this.client.DisconnectAsync();
        }

        /// <summary>
        /// Call when a Discord Client got updated. Used to combat nickname-rename shenanigans by Obe
        /// </summary>
        /// <param name="e">
        /// The Event Args.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here. Obe is a Name")]
        private async Task ClientGuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            if (e.Member.Id == 450771319479599114)
            {
                await(await e.Guild.GetMemberAsync(450771319479599114)).ModifyAsync(
                    model => model.Nickname = "Sabrina");
            }
        }

        /// <summary>
        /// Call when client Message is created. Logs all Messages to Database.
        /// </summary>
        /// <param name="e">
        /// The Message Event Args.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        private async Task ClientMessageCreated(MessageCreateEventArgs e)
        {
            Tables.Discord.Message msg = new Tables.Discord.Message(e.Author.Id, e.Message.Content, e.Message.Channel.Id, e.Message.CreationTimestamp.DateTime);
            var user = Tables.Discord.User.Load(e.Message.Author);
            if (user == null)
            {
                user = new Tables.Discord.User
                {
                    UserId = e.Author.Id
                };
                user.Save();
            }

            msg.Save();
        }

        /// <summary>
        /// Call when Reaction is added to Message
        /// </summary>
        /// <param name="e">
        /// The Event Args.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ClientMessageReactionAdded(MessageReactionAddEventArgs e)
        {
            await this.tmblrBot.CheckLoli(e);
        }

        /// <summary>
        /// Create missing folders.
        /// </summary>
        private void CreateFolders()
        {
            if (!Directory.Exists(Config.BotFileFolders.SlaveReports))
            {
                Directory.CreateDirectory(Config.BotFileFolders.SlaveReports);
            }

            if (!Directory.Exists(Config.BotFileFolders.WheelResponses))
            {
                Directory.CreateDirectory(Config.BotFileFolders.WheelResponses);
            }

            if (!Directory.Exists(Config.BotFileFolders.WheelLinks))
            {
                Directory.CreateDirectory(Config.BotFileFolders.WheelLinks);
            }

            if (!Directory.Exists(Config.BotFileFolders.UserData))
            {
                Directory.CreateDirectory(Config.BotFileFolders.UserData);
            }

            if (!Directory.Exists(Config.Pornhub.IndexedVideoLocation))
            {
                Directory.CreateDirectory(Config.Pornhub.IndexedVideoLocation);
            }
        }

        private Task Log(string msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        private async Task OnReadyAsync(ReadyEventArgs e)
        {
            await Task.Yield();
        }

        private void SetCommands()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddSingleton(new Dependencies(this.interactivity));
            this.cmdsNext = this.client.UseCommandsNext(
                new CommandsNextConfiguration()
                    {
                        CaseSensitive = false,
                        EnableDefaultHelp = true,
                        EnableDms = false,
                        EnableMentionPrefix = true,
                        IgnoreExtraArguments = true,
                        DmHelp = true,
                        StringPrefixes = new[] { Prefix },
                        Services = services.BuildServiceProvider()
                    });

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.Namespace == "Sabrina.Commands"))
            {
                if (type == null)
                {
                    continue;
                }

                if (IsModuleCandidateType(type))
                {
                    this.cmdsNext.RegisterCommands(type);
                }
            }
        }

        internal static bool IsModuleCandidateType(Type type)
            => IsModuleCandidateType(type.GetTypeInfo());

        internal static bool IsModuleCandidateType(TypeInfo ti)
        {
            // check if compiler-generated
            if (ti.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null)
                return false;

            // check if derives from the required base class
            var tmodule = typeof(BaseCommandModule);
            var timodule = tmodule.GetTypeInfo();
            if (!timodule.IsAssignableFrom(ti))
                return false;

            // check if anonymous
            if (ti.IsGenericType && ti.Name.Contains("AnonymousType") && (ti.Name.StartsWith("<>") || ti.Name.StartsWith("VB$")) && (ti.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
                return false;

            // check if abstract, static, or not a class
            if (!ti.IsClass || ti.IsAbstract)
                return false;

            // check if delegate type
            var dlgt = typeof(Delegate).GetTypeInfo();
            if (dlgt.IsAssignableFrom(ti))
                return false;

            // qualifies if any method or type qualifies
            return ti.DeclaredMethods.Any(xmi => IsCommandCandidate(xmi, out _)) || ti.DeclaredNestedTypes.Any(xti => IsModuleCandidateType(xti));
        }

        internal static bool IsCommandCandidate(MethodInfo method, out ParameterInfo[] parameters)
        {
            parameters = null;
            // check if exists
            if (method == null)
                return false;

            // check if static, non-public, abstract, a constructor, or a special name
            if (method.IsStatic || method.IsAbstract || method.IsConstructor || method.IsSpecialName)
                return false;

            // check if appropriate return and arguments
            parameters = method.GetParameters();
            if (!parameters.Any() || parameters.First().ParameterType != typeof(CommandContext) || method.ReturnType != typeof(Task))
                return false;

            // qualifies
            return true;
        }


        private void SetConfig()
        {
            var config = new DiscordConfiguration
                             {
                                 AutoReconnect = true,
                                 GatewayCompressionLevel = GatewayCompressionLevel.Stream,
                                 MessageCacheSize = 2048,
                                 LogLevel = LogLevel.Debug,
                                 Token = Config.Token,
                                 TokenType = TokenType.Bot,
                                 UseInternalLogHandler = true
                             };
            this.client = new DiscordClient(config);

            this.interactivity = this.client.UseInteractivity(
                new InteractivityConfiguration()
                    {
                        PaginationBehavior = TimeoutBehaviour.DeleteMessage,
                        PaginationTimeout = TimeSpan.FromSeconds(30),
                        Timeout = TimeSpan.FromSeconds(30)
                    });
        }
    }
}