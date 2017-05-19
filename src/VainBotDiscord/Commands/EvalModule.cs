﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

// https://github.com/Bond-009/iTool.DiscordBot/blob/master/src/iTool.DiscordBot/Modules/Dev.cs

namespace VainBotDiscord.Commands
{
    public class EvalModule : ModuleBase
    {
        [Command("eval", RunMode = RunMode.Async)]
        [Alias("evaluate", "cs", "c#", "csharp")]
        public async Task Evaluate([Remainder]string input)
        {
            if (Context.Guild.Id != 268547141721522178)
            {
                await ReplyAsync("Can't use that in this server.");
                return;
            }

            var index1 = input.IndexOf('\n', input.IndexOf("```") + 3) + 1;
            var index2 = input.LastIndexOf("```");
            if (index1 == -1 || index2 == -1)
            {
                await ReplyAsync("Code must be wrapped in a code block.");
                return;
            }

            var code = input.Substring(index1, index2 - index1);

            var msg = ReplyAsync("", embed: new EmbedBuilder
            {
                Title = "Evaluation",
                Color = new Color(205, 101, 28),
                Description = "Evaluating..."
            });

            try
            {
                var options = ScriptOptions.Default
                    .AddReferences(new[]
                    {
                        typeof(object).GetTypeInfo().Assembly.Location,
                        typeof(Object).GetTypeInfo().Assembly.Location,
                        typeof(Enumerable).GetTypeInfo().Assembly.Location,
                        typeof(DiscordSocketClient).GetTypeInfo().Assembly.Location,
                        typeof(IMessage).GetTypeInfo().Assembly.Location
                    })
                    .AddImports(new[]
                    {
                        "Discord",
                        "Discord.Commands",
                        "Discord.WebSocket",
                        "System",
                        "System.Linq",
                        "System.Collections",
                        "System.Collections.Generic"
                    });

                var result = await CSharpScript.EvaluateAsync(code, options, globals:
                    new RoslynGlobals
                    {
                        Client = Context.Client as DiscordSocketClient,
                        Channel = Context.Channel as SocketTextChannel
                    });

                await (await msg).ModifyAsync(m => m.Embed = new EmbedBuilder
                {
                    Title = "Evaluation",
                    Description = result?.ToString() ?? "Success, nothing returned.",
                    Color = new Color(205, 101, 28)
                }.Build());
            }
            catch (Exception ex)
            {
                await (await msg).ModifyAsync(m => m.Embed = new EmbedBuilder
                {
                    Title = "Evaluation Failure",
                    Description = $"**{ex.GetType()}**: {ex.Message}",
                    Color = new Color(205, 101, 28)
                }.Build());
            }
        }

        public class RoslynGlobals
        {
            public DiscordSocketClient Client { get; set; }
            public SocketTextChannel Channel { get; set; }
        }
    }
}
