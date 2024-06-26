﻿using Discord;
using Gellybeans.Pathfinder;
using MongoDB.Driver;

namespace MathfinderBot
{
    public static class Characters
    {
        public static Dictionary<ulong, List<StatBlock>>    Database    = new Dictionary<ulong, List<StatBlock>>();  
        public static Dictionary<ulong, StatBlock>          Active      = new Dictionary<ulong, StatBlock>();
        public static Dictionary<ulong, string>             WebKeys     = new Dictionary<ulong, string>();

        public static async Task<StatBlock> GetCharacter(ulong user)
        {         
            if(Active.ContainsKey(user))
                return Active[user];

            var results = await Program.GetStatBlocks().FindAsync(x => x.Owner == user && x.CharacterName == "$GLOBAL" );
            var stats = results.ToList();
            
            //if there is a $GLOBAL, set it
            if(stats.Count > 0)
            {
                await SetActive(user, stats[0]);
                return stats[0];
            }                    
            //else create a new one
            else
            {
                var global = new StatBlock() { Owner = user, CharacterName = "$GLOBAL" };
                await Program.InsertStatBlock(global);
                await SetActive(user, global);
                return global;
            }
        }

        public static async Task SetActive(ulong user, StatBlock stats)
        {
            await Task.Run(async ()  =>
            {
                //if active character exists, unsubscribe to events
                if(Active.ContainsKey(user))
                    stats.ValueChanged -= UpdateValue;

                //set new active and subscribe to events
                Active[user] = stats;
                WebKeys[user] = Utility.Hash(Active[user].CharacterName);
                if(stats.CharacterName != "$GLOBAL")
                {
                    var results = await Program.GetStatBlocks().FindAsync(x => x.Owner == user && x.CharacterName == "$GLOBAL");
                    var global = results.ToList();
                    if(global.Count > 0)
                        stats.Global = global[0];
                }                         

                stats.ValueChanged += UpdateValue;
                
            });
        }
        
        public static async void UpdateValue(object? sender, string value)
        {
            //parse subscribed event string, update database record
            await Task.Run(()  => 
            {
                var stats = (StatBlock)sender!;
                UpdateDefinition<StatBlock> update = null!;
                switch(value)
                {
                    case "var":
                        update = Builders<StatBlock>.Update.Set(x => x.Vars, Active[stats.Owner].Vars);
                        break;
                    case string val when val.Contains("row:"):
                        update = Builders<StatBlock>.Update.Set(x => x.ExprRows, Active[stats.Owner].ExprRows);
                        break;
                }

                if(update != null)
                    Program.Database.UpdateOne(stats,update);
            
            }).ConfigureAwait(false);        
        }
    }
}

