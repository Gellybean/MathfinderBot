﻿using Discord;
using Gellybeans.Pathfinder;
using MongoDB.Driver;

namespace MathfinderBot
{
    public static class Characters
    {
        public static Dictionary<ulong, List<StatBlock>>    Database    = new Dictionary<ulong, List<StatBlock>>();  
        public static Dictionary<ulong, StatBlock>          Active      = new Dictionary<ulong, StatBlock>();        
        public static Dictionary<ulong, Init>               Inits       = new Dictionary<ulong, Init>();
        
        
        
        public static async Task<StatBlock> GetCharacter(ulong user)
        {
          
            if(Active.ContainsKey(user))
                return Active[user];

            var results = await Program.GetStatBlocks().FindAsync(x => x.Owner == user);
            var stats = results.ToList();
            if(stats.Count > 0)
            {
                await SetActive(user, stats[0]);
                return stats[0];
            }
                    
            else
            {
                var global = new StatBlock() { Owner = user, CharacterName = "$GLOBAL" };
                await Program.InsertStatBlock(global);
                await SetActive(user, global);
                return global;
            }
        }


         public static async Task SetActive(ulong id, StatBlock stats)
        {
            await Task.Run(() =>
            {
                if(Active.ContainsKey(id))
                    stats.ValueChanged -= UpdateValue;

                Active[id] = stats;
                stats.ValueChanged += UpdateValue;
            });
        }
        
        public static async void UpdateValue(object? sender, string value)
        {
            await Task.Run(async ()  => 
            {
                var stats = (StatBlock)sender!;
                UpdateDefinition<StatBlock> update = null!;
                switch(value)
                {
                    case "stats":
                        update = Builders<StatBlock>.Update.Set(x => x.Stats, Active[stats.Owner].Stats);
                        break;
                    case "inv":
                        update = Builders<StatBlock>.Update.Set(x => x.Inventory, Active[stats.Owner].Inventory);
                        break;
                    case string val when val.Contains("stat:"):
                        var statName = value.Split(':')[1];
                        update = Builders<StatBlock>.Update.Set(x => x.Stats[statName], Active[stats.Owner].Stats[statName]);
                        break;
                    case string val when val.Contains("expr:"):
                        var exprName = value.Split(':')[1];
                        update = Builders<StatBlock>.Update.Set(x => x.Expressions[exprName], Active[stats.Owner].Expressions[exprName]);
                        break;
                    case string val when val.Contains("row:"):
                        var rowName = value.Split(':')[1];
                        update = Builders<StatBlock>.Update.Set(x => x.ExprRows[rowName], Active[stats.Owner].ExprRows[rowName]);
                        break;
                }

                if(update != null)
                    await Program.UpdateSingleStat(update, stats.Owner);
            
            }).ConfigureAwait(false);        
        }
    }
}

