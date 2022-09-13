﻿using System.Text;
using Discord;
using Discord.Interactions;
using Gellybeans.Pathfinder;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using Newtonsoft.Json;
using static MathfinderBot.Character;


namespace MathfinderBot
{
    public class Character : InteractionModuleBase
    {
        public enum CharacterCommand
        {            
            Set,
            New,
            List,
            Export,
            Delete
        }

        public enum GameType
        {
            None,
            Pathfinder,
            Starfinder,
            FifthEd
        }

        public enum SheetType
        {
            [ChoiceDisplay("Pathbuilder (PF)")]
            Pathbuilder,
            
            [ChoiceDisplay("HeroLabs (PF)")]
            HeroLabs,
            
            [ChoiceDisplay("PCGen (PF)")]
            PCGen
        }

        public enum CampaignOption
        {
            Set,                
            List,
            AddPlayer,
            New,
            Delete,
        }

        static Dictionary<ulong, string> lastInputs = new Dictionary<ulong, string>();
        static Regex validName = new Regex(@"^[a-zA-Z' ]{3,50}$");

        ulong user;
        IMongoCollection<StatBlock> collection;

        public  InteractionService  Service { get; set; }
        private CommandHandler      handler;

        public Character(CommandHandler handler) => this.handler = handler;

        public override void BeforeExecute(ICommandInfo command)
        {
            user = Context.Interaction.User.Id;
            collection = Program.database.GetCollection<StatBlock>("statblocks");
        }


        [SlashCommand("char", "Create, set, export/import, delete characters.")]
        public async Task CharCommand(CharacterCommand mode, string charName = "", GameType game = GameType.Pathfinder)
        {        
            var nameToUpper = charName.ToUpper();
            lastInputs[user] = charName;
            
            //find all documents that belong to user, load them into dictionary.
            Characters.Database[user] = new Dictionary<string, StatBlock>();
            var chars = await collection.FindAsync(x => x.Owner == user);
            var characters = chars.ToList();
            
            foreach(var statblock in characters)
                Characters.Database[user][statblock.CharacterName] = statblock;
        
            
            
            if(mode == CharacterCommand.Export)
            {
                if(Characters.Database[user].ContainsKey(charName))
                {
                    var json = JsonConvert.SerializeObject(Characters.Database[user][charName], Formatting.Indented);
                    using var stream = new MemoryStream(Encoding.ASCII.GetBytes(json));
                  
                    await RespondWithFileAsync(stream, $"{charName}.txt", ephemeral: true);
                    return;
                }
                await RespondAsync($"{charName} not found.", ephemeral: true);
                return;
            }

            if(mode == CharacterCommand.List)
            {
                if(Characters.Database[user].Count == 0)
                {
                    await RespondAsync("You don't have any characters.", ephemeral: true);
                    return;
                }

                var sb = new StringBuilder();
                
                foreach(var character in Characters.Database[user].Keys)
                    sb.AppendLine($"-> {character}");
                
                await RespondAsync(sb.ToString(), ephemeral: true);
            }
            
            if(mode == CharacterCommand.Set)
            {
                if(!validName.IsMatch(charName))
                {
                    await RespondAsync("Invalid character name.", ephemeral: true);
                    return;
                }

                foreach(var c in Characters.Database[user].Values)
                {
                    if(c.CharacterName.ToUpper() == charName.ToUpper())
                    {
                        Characters.SetActive(user, Characters.Database[user][charName]);
                        await RespondAsync($"{charName} set!", ephemeral: true);
                        return;
                    }
                }
                await RespondAsync("Character not found", ephemeral: true);
                return;               
            }

            if(mode == CharacterCommand.New)
            {
                if(!validName.IsMatch(charName))
                {
                    await RespondAsync("Invalid character name.", ephemeral: true);
                    return;
                }

                if(Characters.Database[user].ContainsKey(charName))
                {
                    await RespondAsync($"{charName} already exists.", ephemeral: true);
                    return;
                }

                if(Characters.Database[user].Count >= 5)
                {
                    await RespondAsync("You have too many characters. Delete one before making another.");
                    return;
                }

                StatBlock statblock = new StatBlock();
                
                switch(game)
                {
                    case GameType.None:
                        statblock = new StatBlock() { CharacterName = charName };
                        break;
                    
                    case GameType.Pathfinder:
                        statblock = StatBlock.DefaultPathfinder(charName);
                        break;

                    case GameType.FifthEd:
                        statblock = StatBlock.DefaultFifthEd(charName);
                        break;

                    case GameType.Starfinder:
                        statblock = StatBlock.DefaultStarfinder(charName);
                        break;
                }
                
                statblock.Owner = user;         
                
                await collection.InsertOneAsync(statblock);

                var quote = Quotes.Get(charName); 

                var eb = new EmbedBuilder()
                    .WithColor(Color.DarkPurple)
                    .WithTitle($"New-Character({charName})")
                    .WithDescription($"{Context.User.Mention} has created a new character, {charName}.\n\n “{quote}”");

                Characters.Active[user] = statblock;
                
                await RespondAsync(embed: eb.Build());                 
            }

            if(mode == CharacterCommand.Delete)
            {                             
                
                if(!Characters.Database[user].ContainsKey(charName))
                {
                    await RespondAsync("Character not found.", ephemeral: true);
                    return;
                }

                await RespondWithModalAsync<ConfirmModal>("confirm_delete");           
            }
            
        }

        [SlashCommand("char-update", "Update an active character")]
        public async Task UpdateCommand(SheetType sheetType, IAttachment file)
        {
            
            if(!Characters.Active.ContainsKey(user))
            {
                await RespondAsync("No active character", ephemeral: true);
                return;
            }
      

            using var client = new HttpClient();
            var data = await client.GetByteArrayAsync(file.Url);
            var stream = new MemoryStream(data);

            if(stream != null)
            {
                if(file.Filename.ToUpper().Contains(".PDF") || file.Filename.ToUpper().Contains(".XML"))
                {
                    await RespondAsync("Updating sheet...", ephemeral: true);
                    StatBlock stats = Characters.Active[user];
                    if(sheetType == SheetType.Pathbuilder)
                        stats = Utility.UpdateWithPathbuilder(stream, stats);
                    if(sheetType == SheetType.HeroLabs)
                        stats = Utility.UpdateWithHeroLabs(stream, Characters.Active[user]);
                    if(sheetType == SheetType.PCGen)
                        stats = Utility.UpdateWithPCGen(stream, Characters.Active[user]);
                    Console.WriteLine("Done!");
                    
                    stats.Id = Characters.Active[user].Id;
                    await Program.UpdateStatBlock(stats);
                    await FollowupAsync("Updated!", ephemeral: true);
                    return;
                }              
            }
            await RespondAsync("Invalid data", ephemeral: true);
        }

        [ModalInteraction("confirm_delete")]
        public async Task ConfirmDeleteChar(ConfirmModal modal)
        {       
            if(modal.Confirm != "CONFIRM")
            {
                await RespondAsync("You didn't die. Try again. (make sure type 'CONFIRM' in all caps)", ephemeral: true);
                return;
            }

            string name = lastInputs[user];

            Characters.Database[user].Remove(name);
            await collection.DeleteOneAsync(x => x.Owner == user && x.CharacterName == name);
      
            await RespondAsync($"{lastInputs[user]} removed", ephemeral: true);
        }

        
        

        [RequireRole("DM")]
        [SlashCommand("camp", "Campaign")]
        public async Task CampaignCommand(CampaignOption option, int index = 0, IUser player = null)
        {
            var campaigns = Program.database.GetCollection<CampaignBlock>("campaigns");

            if(option == CampaignOption.List)
            {
                var camps = await campaigns.FindAsync(x => x.Owner == user);
                var list = await camps.ToListAsync();
                var sb = new StringBuilder();
                for(int i = 0; i < list.Count; i++)
                    sb.AppendLine($"{i,-2}| {list[i].CampaignName,15}|");

                var eb = new EmbedBuilder()
                    .WithTitle("List-Campaigns()")
                    .WithDescription(sb.ToString());

                await RespondAsync(embed: eb.Build());
                return;
            }

            if(option == CampaignOption.Set)
            {
                var camps = await campaigns.FindAsync(x => x.Owner == user);
                var list = await camps.ToListAsync();
                if(index >= 0 && index < list.Count)
                {
                    Characters.Campaigns[user] = list[index];
                    await RespondAsync($"{Characters.Campaigns[user].CampaignName} set", ephemeral: true);
                    return;
                }
                await RespondAsync("Campaign not found", ephemeral: true);
                return;
            }

            if(option == CampaignOption.New)
                await RespondWithModalAsync<CampaignNameModal>("new_campaign");

            if(option == CampaignOption.Delete)
                await RespondWithModalAsync<CampaignNameModal>("delete_campaign");


            if(!Characters.Campaigns.ContainsKey(user))
            {
                await RespondAsync("No active campaign", ephemeral: true);
                return;
            }

            if(option == CampaignOption.AddPlayer)
            {
                if(player == null)
                {
                    await RespondAsync("No player selected. User the `player` field to add one");
                    return;
                }
                Characters.Campaigns[user].Players.Add(player.Id);
                await RespondAsync($"{player.Username} added");
                return;
            }
        }

        [SlashCommand("camp-entry", "Add an entry to any active campaign journal")]
        public async Task CampaignEntryCommand() => await RespondWithModalAsync<CampaignEntryModal>("new_entry");

        [ModalInteraction("new_entry")]
        public async Task NewEntryCommand(CampaignEntryModal modal)
        {
            if(!Characters.Campaigns.ContainsKey(user))
            {
                await RespondAsync("No active campaign", ephemeral: true);
                return;
            }

            if(!Characters.Active.ContainsKey(user))
            {
                await RespondAsync("No active character", ephemeral: true);
                return;
            }

            var charName = Characters.Active[user].CharacterName;
            var date = DateTime.Now.ToString("d");


        }    
    }
}