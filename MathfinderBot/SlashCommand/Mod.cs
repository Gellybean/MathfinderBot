﻿using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Gellybeans.Pathfinder;
using MongoDB.Driver;

namespace MathfinderBot
{
    public class Mod : InteractionModuleBase
    {
       

        public enum TemplateMode
        {
            Add,
            Remove,
            List,
            ListAll
        }
        
        public enum BonusAction
        {
            Add,
            Remove
        }

        private static Dictionary<ulong, List<IUser>> lastTargets = new Dictionary<ulong, List<IUser>>();
        private static Regex targetReplace = new Regex(@"\D+");
        private ulong user;
        private IMongoCollection<StatBlock> collection;       
        
        public override void BeforeExecute(ICommandInfo command)
        {
            user = Context.User.Id;
            collection = Program.database.GetCollection<StatBlock>("statblocks");
        }

        [SlashCommand("preset-mod", "Apply or remove a specifically defined modifier to one or many targets")]
        public async Task ModifierCommand(BonusAction action, string modName, string targets = "")
        {
            var modToUpper = modName.ToUpper().Replace(' ', '_');
            var sb = new StringBuilder();
            if(!Characters.Active.ContainsKey(user) || Characters.Active[user] == null)
            {
                await RespondAsync("No active character", ephemeral: true);
                return;
            }

            if(action == BonusAction.Add)
            {
               
                if(!DataMap.Modifiers.ContainsKey(modToUpper))
                {
                    await RespondAsync("No mod by that name found", ephemeral: true);
                    return;
                }
               
                if(targets != "")
                {
                    var targetList = new List<IUser>();
                    var replace = targetReplace.Replace(targets, " ");
                    var split = replace.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    for(int i = 0; i < split.Length; i++)
                    {
                        var id = 0ul;
                        ulong.TryParse(split[i], out id);
                        var dUser = await Program.client.GetUserAsync(id);
                        if(dUser != null) targetList.Add(dUser);
                    }

                    if(targetList.Count > 0)
                    {
                        lastTargets[user] = targetList;

                        if(DataMap.Modifiers[modToUpper] == null)
                        {
                            for(int i = 0; i < targetList.Count; i++)
                                if(Characters.Active.ContainsKey(targetList[i].Id))
                                {
                                    sb.AppendLine(targetList[i].Mention);
                                    Characters.Active[targetList[i].Id].AddBonuses(StatModifier.Mods[modToUpper]);
                                    await collection.ReplaceOneAsync(x => x.Id == Characters.Active[targetList[i].Id].Id, Characters.Active[targetList[i].Id]);

                                    var eb = new EmbedBuilder()
                                        .WithTitle($"Mod({modToUpper})")
                                        .WithDescription(sb.ToString());

                                    foreach(var bonus in StatModifier.Mods[modToUpper])
                                        eb.AddField(name: bonus.StatName, value: $"{bonus.Bonus.Value} {Enum.GetName(bonus.Bonus.Type)} bonus", inline: true);

                                    await RespondAsync(embed: eb.Build());
                                }                            
                        }
                        else
                        {
                            var cb = new ComponentBuilder();
                            for(int i = 0; i < DataMap.Modifiers[modToUpper].Count; i++)
                                cb.WithButton(customId: $"mod:{DataMap.Modifiers[modToUpper][i].Item1}", label: DataMap.Modifiers[modToUpper][i].Item2);
                            await RespondAsync(components: cb.Build(), ephemeral: true);
                        }
                        return;
                    }
                }
                else
                {
                    lastTargets[user] = null;
                    if(DataMap.Modifiers[modToUpper] == null)
                    {
                        if(Characters.Active.ContainsKey(user))
                        {
                            sb.AppendLine(Characters.Active[user].CharacterName);
                            Characters.Active[user].AddBonuses(StatModifier.Mods[modToUpper]);
                            await collection.ReplaceOneAsync(x => x.Id == Characters.Active[user].Id, Characters.Active[user]);

                            var eb = new EmbedBuilder()
                                       .WithTitle($"Mod({modToUpper})")
                                       .WithDescription(sb.ToString());

                            foreach(var bonus in StatModifier.Mods[modToUpper])
                                eb.AddField(name: bonus.StatName, value: $"{bonus.Bonus.Value} {Enum.GetName(bonus.Bonus.Type).ToLower()} bonus", inline: true);

                            await RespondAsync(embed: eb.Build(), ephemeral: true);
                        }
                    }
                    else
                    {
                        var cb = new ComponentBuilder();
                        for(int i = 0; i < DataMap.Modifiers[modToUpper].Count; i++)
                            cb.WithButton(customId: $"mod:{DataMap.Modifiers[modToUpper][i].Item1}", label: DataMap.Modifiers[modToUpper][i].Item2);
                        await RespondAsync(components: cb.Build(), ephemeral: true);
                    }
                }
            }
                 
            if(action == BonusAction.Remove)
            {
                if(targets != "")
                {
                    var targetList = new List<IUser>();
                    var regex = new Regex(@"\D+");
                    var replace = regex.Replace(targets, " ");
                    var split = replace.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    for(int i = 0; i < split.Length; i++)
                    {
                        var id = 0ul;
                        ulong.TryParse(split[i], out id);
                        var dUser = await Program.client.GetUserAsync(id);
                        if(dUser != null) targetList.Add(dUser);
                    }


                    if(targetList.Count > 0)
                    {
                        for(int i = 0; i < targetList.Count; i++)
                        {
                            if(Characters.Active.ContainsKey(targetList[i].Id))
                            {
                                sb.AppendLine(targetList[i].Mention);
                                Characters.Active[targetList[i].Id].ClearBonus(modToUpper);
                                await collection.ReplaceOneAsync(x => x.Id == Characters.Active[targetList[i].Id].Id, Characters.Active[targetList[i].Id]);
                            }
                        }
                        await RespondAsync($"Removed {modToUpper} from: ```{sb}```", ephemeral: true);
                    }
                }
                else
                {
                    Characters.Active[user].ClearBonus(modToUpper);
                    await collection.ReplaceOneAsync(x => x.Id == Characters.Active[user].Id, Characters.Active[user]);
                    await RespondAsync($"{modToUpper} removed from all stats", ephemeral: true);
                }
            }
        }

        [ComponentInteraction("mod:*")]
        public async Task ModOptions(string modName)
        {
            var sb = new StringBuilder();
            if(lastTargets[user] != null)
                for(int i = 0; i < lastTargets[user].Count; i++)
                    if(Characters.Active.ContainsKey(lastTargets[user][i].Id))
                    {
                        sb.AppendLine(Characters.Active[lastTargets[user][i].Id].CharacterName);
                        Characters.Active[lastTargets[user][i].Id].AddBonuses(StatModifier.Mods[modName]);
                        await collection.ReplaceOneAsync(x => x.Id == Characters.Active[lastTargets[user][i].Id].Id, Characters.Active[user]);
                    }
            else
            {
                sb.AppendLine(Characters.Active[user].CharacterName);
                Characters.Active[user].AddBonuses(StatModifier.Mods[modName]);
                await collection.ReplaceOneAsync(x => x.Id == Characters.Active[user].Id, Characters.Active[user]);
            }

            var eb = new EmbedBuilder()
                .WithTitle($"Mod({modName})")
                .WithDescription($"```{sb}```");

            foreach(var bonus in StatModifier.Mods[modName])
                eb.AddField(name: bonus.StatName, value: $"{bonus.Bonus.Value} {Enum.GetName(bonus.Bonus.Type)} bonus", inline: true);                
            await RespondAsync(embed: eb.Build(), ephemeral: true);
        }

    }
}
