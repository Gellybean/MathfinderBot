﻿using System.Text;
using System.Text.RegularExpressions;
using Discord.Interactions;
using Gellybeans.Pathfinder;
using Gellybeans.Expressions;
using MongoDB.Driver;
using Discord;
using Discord.WebSocket;

namespace MathfinderBot
{
    public class Variable : InteractionModuleBase
    {
        public enum AbilityScoreDmg
        {
            [ChoiceDisplay("None")]
            BONUS,
            
            STR,
            DEX,
            CON,
            INT,
            WIS,
            CHA
        }

        public enum ModAction
        {
            List,
            Add,
            Remove
        }
    
        public enum AbilityScoreHit
        {          
            STR,
            DEX,
            CON,
            INT,
            WIS,
            CHA
        }

        public enum EquipAction
        {
            Add,
            List
        }

        public enum InfoAction
        {
            Set,
            List,
            Remove
        }

        public enum VarAction
        {
            [ChoiceDisplay("Set-Var")]
            SetVar,

            [ChoiceDisplay("Set-Row")]
            SetRow,
            
            [ChoiceDisplay("List-Vars")]
            ListVars,
        }
        
        static readonly Dictionary<string, int> sizes = new Dictionary<string, int>(){
            { "Fine",        0 },
            { "Diminutive",  1 },
            { "Tiny",        2 },
            { "Small",       3 },
            { "Medium",      4 },
            { "Large",       5 },
            { "Huge",        6 },
            { "Gargantuan",  7 },
            { "Colossal",    8 }};

        static readonly Regex validVar  = new Regex(@"^[^\[\]<>(){}@:+*/%=!&|;$#?\-'""]*$");
        static readonly Regex validExpr = new Regex(@"^(.*){1,400}$");
              
        public static ExprRow                   exprRowData = null!;
        ulong                                   user;       

        static byte[] bestiary  = null!;
        static byte[] items     = null!;
        static byte[] rules     = null!;
        static byte[] shapes    = null!;       
        static byte[] spells    = null!;

        public async override void BeforeExecute(ICommandInfo command)
        { 
            user = Context.Interaction.User.Id;
            await Characters.GetCharacter(user);
        }

        async Task VarList(string varName = "", bool isHidden = true)
        {
            var sb = new StringBuilder();

            if(varName != "")
            {
                varName = varName.ToUpper();
                if(Characters.Active[user].Vars.TryGetValue(varName, out var var))
                {
                    sb.AppendLine($"|{varName,-15} |{var.GetType().Name.Replace("Value", ""),-15} |{var,-50}");
                    await RespondAsync($"```{sb}```", ephemeral: true);
                }

            }
            else
            {
                var ordered = Characters.Active[user].Vars.OrderBy(x => x.Value.GetType().ToString()).ThenBy(x => x.Key);
                sb.AppendLine($"|{"VAR",-15} |{"TYPE",-15} |{"VALUE",-50}\n");
                foreach(var var in ordered)
                {
                    sb.AppendLine($"|{var.Key,-15} |{var.Value.GetType().Name.Replace("Value", ""),-15} |{var.Value,-50}");
                }

                using var stream = new MemoryStream(Encoding.Default.GetBytes(sb.ToString()));
                await RespondWithFileAsync(stream, $"Vars.{Characters.Active[user].CharacterName}.txt", ephemeral: isHidden);
            }
        }


        async Task VarSetVar(string varName, IAttachment file)
        {
            var stats = await Characters.GetCharacter(user);

            if(file != null && file.ContentType.Contains("text/plain"))
            {
                var sb = new StringBuilder();

                using var client = new HttpClient();
                var data = await client.GetByteArrayAsync(file.Url);
                var str = Encoding.Default.GetString(data);

                var evals = str.Split(Environment.NewLine);

                int i;
                for(i = 0; i < evals.Length && i < 1500; i++)
                {
                    if(evals[i] == "")
                        continue;
                    Parser.Parse(evals[i], this, sb, stats).Eval(0, this, sb, stats);
                }

                sb.AppendLine();
                sb.AppendLine($"{i} lines evaluated.");

                var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
                await RespondWithFileAsync(stream, "results.txt", ephemeral: true);
                return;
            }
        }


        [SlashCommand("var", "Manage variables.")]
        public async Task Var(VarAction action, string varName = "", bool isHidden = true, IAttachment file = null!)
        {
            user = Context.Interaction.User.Id;         

            var varToUpper = varName.ToUpper().Replace(' ', '_');
            if(varName != "" && !validVar.IsMatch(varToUpper))
            {
                await RespondAsync($"Invalid variable `{varToUpper}`. Numbers and most special characters are forbidden.", ephemeral: true);
                return;
            }

            switch(action)
            {
                case VarAction.ListVars:
                    await VarList(varToUpper, isHidden);
                    return;
                case VarAction.SetVar:
                    await VarSetVar(varToUpper, file); 
                    return;
                case VarAction.SetRow:
                    await RespondWithModalAsync<ExprRowModal>("set_row");
                    return;
            }
        }

        [ModalInteraction("set_row")]
        public async Task NewRow(ExprRowModal modal)
        {
            user = Context.Interaction.User.Id;

            var exprs = modal.Expressions.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var row = new ExprRow() { RowName = modal.Name };
            
            for(int i = 0; i < exprs.Length; i++)
            {
                if(validExpr.IsMatch(exprs[i]))
                {
                    var split = exprs[i].Split('#');
                    if(split.Length == 2)
                        row.Set.Add(new Expr { Name = split[0], Expression = split[1] });
                    else if(split.Length == 1)
                        row.Set.Add(new Expr { Name = split[0], Expression = split[0] });
                }
                else
                {
                    await RespondAsync($"Invalid Input @ Expression {i + 1}", ephemeral: true);
                    return;
                }
            }                                 

            Characters.Active[user].AddExprRow(row);
            var eb = new EmbedBuilder()
                .WithColor(Color.Gold)
                .WithTitle($"New-Row({row.RowName})");

            for(int i = 0; i < row.Set.Count; i++)
                if(!string.IsNullOrEmpty(row.Set[i].Name))
                    eb.AddField(name: row.Set[i].Name, value: row.Set[i].Expression, inline: true);

            await RespondAsync(embed: eb.Build(), ephemeral: true);
        }

        [SlashCommand("row", "Get one or many rows (up to 5)")]
        public async Task GetRowCommand([Autocomplete] string rowName)
        {
            user = Context.Interaction.User.Id;
            var character = await Characters.GetCharacter(user);
           
            if(character.ExprRows.Keys.Any(x => x.Replace(" ", "_").ToUpper() == rowName.Replace(" ", "_").ToUpper()))
            {
                var cb = BuildRow(character.ExprRows[rowName]);
                await RespondAsync(components: cb.Build(), ephemeral: true);
            }
            else
                await RespondAsync("Row now found", ephemeral: true);    
        }  
      
        [SlashCommand("best", "List creature by name or index number")]
        public async Task BestiaryCommand([Summary("creature_name"), Autocomplete] string nameOrNumber = "", bool showInfo = false)
        {
            user = Context.User.Id;
            var ctx = await Characters.GetCharacter(user);

            if(nameOrNumber == "")
            {
                if(bestiary == null)   
                    bestiary = Encoding.ASCII.GetBytes(DataMap.BaseCampaign.ListBestiary());
                using var stream = new MemoryStream(bestiary);
                await RespondWithFileAsync(stream, $"Bestiary.txt", ephemeral: true);
                return;
            }      

            var outVal = -1;
            var nameVal = DataMap.BaseCampaign.Bestiary.FirstOrDefault(x => x.Name!.ToUpper() == nameOrNumber.ToUpper());
            if(nameVal != null)
                outVal = DataMap.BaseCampaign.Bestiary.IndexOf(nameVal);
            else if(!int.TryParse(nameOrNumber, out outVal))
            {
                await RespondAsync($"{nameOrNumber} not found", ephemeral: true);
                return;
            }

            if(outVal >= 0 && outVal < DataMap.BaseCampaign.Bestiary.Count)
            {
                var creature = DataMap.BaseCampaign.Bestiary[outVal];
                Embed[] ebs = null;
                var sa = creature.GetSpecialAbilities();

                ctx["LAST_CREATURE"] = creature.ToArray();


                if(showInfo)
                {
                    if(sa != null)
                        ebs = new Embed[2] { new EmbedBuilder().WithDescription(creature.ToString()).Build(), new EmbedBuilder().WithDescription(sa).Build() };
                    else
                        ebs = new Embed[1] { new EmbedBuilder().WithDescription(creature.ToString()).Build() };
                }
                else ebs = new Embed[1] { new EmbedBuilder().WithDescription(creature.GetSmallBlock()).Build()};


                var regex = new Regex(@"(^(?:or )?[+]?[0-9a-z ]*)(?:([-+][0-9]{1,2})?[/]?)* \(([0-9]{1,2}d[0-9]{1,2}(?:[-+][0-9]{1,3})?)(?:.*([+][0-9]{1,2}d[0-9]{1,2}).*\))*");
                string[] melee  = new string[5] { creature.MeleeOne!, creature.MeleeTwo!, creature.MeleeThree!, creature.MeleeFour!, creature.MeleeFive! };
                string[] ranged = new string[2] { creature.RangedOne!, creature.RangedTwo! };

                
                var cb = new ComponentBuilder();
                if(melee[0] != "")
                    for(int i = 0; i < melee.Length; i++)
                        if(melee[i] != "")
                        {
                            var match = regex.Match(melee[i]);
                            if(match.Success && match.Groups.Count > 3)
                            {
                                var row = new ActionRowBuilder();
                                for(int j = 0; j < match.Groups[2].Captures.Count; j++)
                                {
                                    Console.WriteLine($"rowbest:{creature.Name!.Replace(" ", "")},1d20{match.Groups[2].Captures[j].Value},{j + i + i * i}");
                                    if(j == 0)
                                        row.WithButton(customId: $"rowbest:{creature.Name!.Replace(" ", "")},1d20{match.Groups[2].Captures[j].Value},{j + i}", label: $"{match.Groups[1].Value} {match.Groups[2].Captures[j].Value}");
                                    else if(j < 4)
                                        row.WithButton(customId: $"rowbest:{creature.Name!.Replace(" ", "")},1d20{match.Groups[2].Captures[j].Value},{j + i}", label: match.Groups[2].Captures[j].Value);
                                }
                                row.WithButton(customId: $"rowbest:{creature.Name!.Replace(" ", "")},{match.Groups[3].Value}{(match.Groups[4].Success ? match.Groups[4].Value : "")},Damage{i}", label: $"{match.Groups[3].Value}{(match.Groups[4].Success ? match.Groups[4].Value : "")}");
                                cb.AddRow(row);
                            }
                        }
                

                await RespondAsync(embeds: ebs, components: cb.Build(), ephemeral: true);

                if(ranged[0] != "")
                {                    
                    cb = new ComponentBuilder();
                    for(int i = 0; i < ranged.Length; i++)
                        if(ranged[i] != "")
                        {
                            var match = regex.Match(ranged[i]);
                            if(match.Success) Console.WriteLine("match");
                            if(match.Success && match.Groups.Count > 3)
                            {
                                var row = new ActionRowBuilder();
                                for(int j = 0; j < match.Groups[2].Captures.Count; j++)
                                {
                                    if(j == 0)
                                        row.WithButton(customId: $"rowbest:{creature.Name!.Replace(" ", "")},1d20{match.Groups[2].Captures[j].Value},{j + i}", label: $"{match.Groups[1].Value} {match.Groups[2].Captures[j].Value}");
                                    else
                                        row.WithButton(customId: $"rowbest:{creature.Name!.Replace(" ", "")},1d20{match.Groups[2].Captures[j].Value},{j + i}", label: match.Groups[2].Captures[j].Value);
                                }
                                row.WithButton(customId: $"rowbest:{creature.Name!.Replace(" ", "")},{match.Groups[3].Value}{(match.Groups[4].Success ? match.Groups[4].Value : "")},Damage{i}", label: $"{match.Groups[3].Value}{(match.Groups[4].Success ? match.Groups[4].Value : "")}");
                                cb.AddRow(row);
                            }
                        }
                    await FollowupAsync(components: cb.Build(), ephemeral: true);
                }             
                
                return;            
            }
        }
        
        [ComponentInteraction("rowbest:*,*,*")]
        public async Task ButtonPressedBest(string creatureName, string expr, string name)
        {
            user = Context.Interaction.User.Id;

            var sb = new StringBuilder();
            var result = Parser.Parse(expr, null, sb).Eval(0, this, sb, null!);

            var ab = new EmbedAuthorBuilder()
                .WithName(Context.Interaction.User.Username)
                .WithIconUrl(Context.Interaction.User.GetAvatarUrl());

            var builder = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithAuthor(ab)
                .WithTitle($"{result}")
                .WithDescription($"{creatureName}")
                .WithFooter($"{expr}");

            if(sb.Length > 0) builder.AddField($"__Events__", $"{sb}");

            await RespondAsync(embed: builder.Build());
        }


        [SlashCommand("item", "Item and inventory management")]
        public async Task ItemCommand([Summary("item_name"), Autocomplete] string nameOrNumber = "", SizeType size = SizeType.Medium, bool isHidden = true)
        {
            user = Context.User.Id;
            var ctx = await Characters.GetCharacter(user);
            var index = -1;    

            if(nameOrNumber != "")
            {
                if(int.TryParse(nameOrNumber, out int outVal) && outVal >= 0 && outVal < DataMap.BaseCampaign.Items.Count)
                    index = outVal;
                else
                    index = DataMap.BaseCampaign.Items.FindIndex(x => x.Name!.ToUpper() == nameOrNumber.ToUpper())!;              
            }

            if(index == -1)
            {
                if(items == null)
                    items = Encoding.ASCII.GetBytes(DataMap.BaseCampaign.ListItems());
                using var stream = new MemoryStream(items!);
                await RespondWithFileAsync(stream, $"Items.txt", ephemeral: true);
            }
            else
            {
                var item = DataMap.BaseCampaign.Items[index];

                ctx.Vars["LAST_ITEM"] = item.ToArrayValue();

                var eb = new EmbedBuilder()
                    .WithDescription(item.ToString());             
              
                await RespondAsync(embed: eb.Build(), ephemeral: true);
            }
            return;
        }

        static ModalBuilder CreateRowModal(string name, string exprs)
        {         
            var mb = new ModalBuilder()
                .WithCustomId($"new_row")
                .WithTitle("New-Row")
                .AddTextInput("Name", "row_name", value: name.ToUpper())
                .AddTextInput("Expressions", "item_exprs", TextInputStyle.Paragraph, value: exprs);
            return mb;
        }

        static ModalBuilder CreateBaseItemModal(Item item)
        {
            var mb = new ModalBuilder()
                    .WithCustomId($"base_item:{item.Name}")
                    .WithTitle($"Add-Item: {item.Name}")
                    .AddTextInput("Custom Name", "item_custom", value: item.Name, maxLength: 50, required: false)
                    .AddTextInput("Quantity", "item_qty", value: "1")                    
                    .AddTextInput("Weight", "item_weight", value: item.Weight.ToString(), maxLength: 20)
                    .AddTextInput("Value", "item_value", value: item.Price)                    
                    .AddTextInput("Notes", "item_notes", TextInputStyle.Paragraph, required: false);
            return mb;        
        }

        [SlashCommand("rule", "General rules, conditions, class abilities")]
        public async Task RuleCommand([Summary("rule_name"), Autocomplete] string name = "", bool isHidden = true)
        {
            if(name == "")
            {
                if(rules == null)
                    rules = Encoding.ASCII.GetBytes(DataMap.BaseCampaign.ListRules());
                using var stream = new MemoryStream(rules);
                await RespondWithFileAsync(stream, $"Rules.txt", ephemeral: true);
                return;
            }

            var toUpper = name.ToUpper().Replace('_', ' ');
            var outVal = -1;
            var nameVal = DataMap.BaseCampaign.Rules.FirstOrDefault(x => x.Name!.ToUpper() == toUpper);
            if(nameVal != null)
                outVal = DataMap.BaseCampaign.Rules.IndexOf(nameVal);
            else if(!int.TryParse(toUpper, out outVal))
            {
                await RespondAsync($"{toUpper} not found", ephemeral: true);
                return;
            }

            if(outVal >= 0 && outVal < DataMap.BaseCampaign.Rules.Count)
            {
                var rule = DataMap.BaseCampaign.Rules[outVal];

                var eb = new EmbedBuilder()
                    .WithColor(255, 130, 130)
                    .WithDescription(rule.ToString());
                
                var cb = await BuildFormulaeComponents(rule.Formulae!);

                await RespondAsync(embed: eb.Build(), components: cb.Build(), ephemeral: isHidden);
            }
        }

        [SlashCommand("shape", "Generate attacks based on a creature's shape")]
        public async Task PresetShapeCommand([Summary("shape_name"), Autocomplete] string nameOrNumber = "", AbilityScoreHit hitMod = AbilityScoreHit.STR, bool multiAttack = false)
        { 
            if(nameOrNumber == "")
            {
                if(shapes == null)
                    shapes = Encoding.ASCII.GetBytes(DataMap.BaseCampaign.ListShapes());
                using var stream = new MemoryStream(shapes);
                await RespondWithFileAsync(stream, $"Shapes.txt", ephemeral: true);
                return;
            }
   
            user = Context.Interaction.User.Id;

            var toUpper = nameOrNumber.ToUpper().Replace(' ', '_');
            var outVal = -1;
            var nameVal = DataMap.BaseCampaign.Shapes.FirstOrDefault(x => x.Name.ToUpper() == toUpper);
            if(nameVal != null)
                outVal = DataMap.BaseCampaign.Shapes.IndexOf(nameVal);
            else if(!int.TryParse(toUpper, out outVal))
            {
                await RespondAsync($"{toUpper} not found", ephemeral: true);
                return;
            }

            if(outVal >= 0 && outVal < DataMap.BaseCampaign.Shapes.Count)
            {
                var shape = DataMap.BaseCampaign.Shapes[outVal];

                var primary = new List<(string, string)>();
                var secondary = new List<(string, string)>();

                if(shape.Bite != "")        primary.Add(("bite", shape.Bite!));
                if(shape.Claws != "")       primary.Add(("claw", shape.Claws!));
                if(shape.Gore != "")        primary.Add(("gore", shape.Gore!));
                if(shape.Slam != "")        primary.Add(("slam", shape.Slam!));
                if(shape.Sting != "")       primary.Add(("sting", shape.Sting!));
                if(shape.Talons != "")      primary.Add(("talon", shape.Talons!));

                if(shape.Hoof != "")        secondary.Add(("hoof", shape.Hoof!));
                if(shape.Tentacle != "")    secondary.Add(("tentacle", shape.Tentacle!));
                if(shape.Wing != "")        secondary.Add(("wing", shape.Wing!));
                if(shape.Pincers != "")     secondary.Add(("pincer", shape.Pincers!));
                if(shape.Tail != "")        secondary.Add(("tail", shape.Tail!));

                if(shape.Other != "")
                {
                    var oSplit = shape.Other!.Split('/');
                    for(int i = 0; i < oSplit.Length; i++)
                    {
                        var split = oSplit[i].Split(':');
                        if(split.Length > 2)
                            primary.Add((split[1], split[2]));
                        else if(split.Length > 1)
                            secondary.Add((split[0], split[1]));
                        else
                            secondary.Add(("other", split[0]));
                    }
                }

                var list = new List<ExprRow>();

                if(primary.Count > 0)
                {
                    var row = new ExprRow();
                    row.Set.Add(new Expr()
                    {
                        Name = $"primary",
                        Expression = $"ATK_{Enum.GetName(typeof(AbilityScoreHit), hitMod)}"
                    });

                    for(int i = 0; i < primary.Count; i++)
                    {
                        var split = primary[i].Item2.Split('/');
                        for(int j = 0; j < split.Length; j++)
                        {

                            var splitCount = split[j].Split(':');
                            if(splitCount.Length > 1) row.Set.Add(new Expr() { Name = $"{splitCount[0]} {primary[i].Item1}s ({splitCount[1]})", Expression = $"{splitCount[1]}+DMG_STR" });
                            else row.Set.Add(new Expr() { Name = $"{primary[i].Item1} ({splitCount[0]})", Expression = $"{splitCount[0]}+DMG_STR" });
                        }
                    }
                    list.Add(row);
                }
                if(secondary.Count > 0)
                {
                    var row = new ExprRow();
                    var secondaryMod = multiAttack ? "2" : "5";
                    row.Set.Add(new Expr()
                    {
                        Name = "secondary",
                        Expression = $"ATK_{Enum.GetName(typeof(AbilityScoreHit), hitMod)}-{secondaryMod}"
                    });

                    for(int i = 0; i < secondary.Count; i++)
                    {
                        var split = secondary[i].Item2.Split('/');
                        for(int j = 0; j < split.Length; j++)
                        {
                            Console.WriteLine(split[j]);
                            var splitCount = split[j].Split(':');
                            if(splitCount.Length > 1) row.Set.Add(new Expr() { Name = $"{splitCount[0]} {secondary[i].Item1}s ({splitCount[1]})", Expression = splitCount[1] });
                            else row.Set.Add(new Expr() { Name = $"{secondary[i].Item1} ({splitCount[0]})", Expression = splitCount[0] });
                        }
                    }
                    list.Add(row);
                }

                if(shape.Breath != "")
                {
                    var row = new ExprRow();
                    row.Set.Add(new Expr() { Name = $"breath[{shape.Breath}]", Expression = shape.Breath });
                    list.Add(row);
                }

                var cb = BuildRow(list);
                var eb = new EmbedBuilder()
                    .WithDescription(shape.ToString());

                await RespondAsync(embed: eb.Build(), components: cb.Build(), ephemeral: true);
            }
        }

        [SlashCommand("spell", "Get spell info")]
        public async Task PresetSpellCommand([Summary("spell_name"), Autocomplete] string nameOrNumber = "", uint? casterLevel = null, bool isHidden = true, bool metamagic = false)
        {     
            if(nameOrNumber == "")
            {
                if(spells == null)
                    spells = Encoding.ASCII.GetBytes(DataMap.BaseCampaign.ListSpells());
                using var stream = new MemoryStream(spells);
                await RespondWithFileAsync(stream, $"Spells.txt", ephemeral: true);
                return;
            }
                  
            var toUpper = nameOrNumber.ToUpper().Replace('_', ' ');
            var outVal = -1;
            var nameVal = DataMap.BaseCampaign.Spells.FirstOrDefault(x => x.Name!.ToUpper() == toUpper);
            if(nameVal != null)
                outVal = DataMap.BaseCampaign.Spells.IndexOf(nameVal);
            else if(!int.TryParse(toUpper, out outVal))
            {
                await RespondAsync($"{toUpper} not found", ephemeral: true);
                return;
            }

            if(outVal >= 0 && outVal < DataMap.BaseCampaign.Spells.Count)
            {                         
                if(metamagic && casterLevel != null)
                {
                    var selb = new SelectMenuBuilder()
                        .WithCustomId($"metamagic:{outVal},{casterLevel.Value}")
                        .WithMaxValues(4)
                        .AddOption("Empowered", "emp")
                        .AddOption("Enlarged", "enl")
                        .AddOption("Extended", "ext")
                        .AddOption("Intensified", "int");

                    var cb = new ComponentBuilder()
                        .WithSelectMenu(selb);

                    await RespondAsync(components: cb.Build(), ephemeral: true);
                    return;
                }
                
                var spell = DataMap.BaseCampaign.Spells[outVal];
                
                var eb = new EmbedBuilder();
                if(casterLevel != null)
                {
                    eb.WithDescription(spell.ToCasterLevel(casterLevel.Value));
                    var cb = await BuildFormulaeComponents(spell.Formulae!.Replace(".CL", casterLevel.Value.ToString()));
                    await RespondAsync(embed: eb.Build(), components: cb.Build(), ephemeral: isHidden);
                    return;
                }
                else
                {
                    eb.WithDescription(spell.ToString());
                    await RespondAsync(embed: eb.Build(), ephemeral: isHidden);
                    return;
                }
            }
        }

        [ComponentInteraction("metamagic:*,*")]
        public async Task MetamagicSelected(int spellIndex, uint casterLevel, string[] selectedMetamagic)
        {           
            var spell = DataMap.BaseCampaign.Spells[spellIndex];

            if(selectedMetamagic.Contains("emp"))
                spell = spell.Empowered();
            if(selectedMetamagic.Contains("enl"))
                spell = spell.Enlarged();
            if(selectedMetamagic.Contains("ext"))
                spell = spell.Extended();
            if(selectedMetamagic.Contains("int"))
                spell = spell.Intensified();
     
            
            var eb = new EmbedBuilder();
            eb.WithDescription(spell.ToCasterLevel(casterLevel));
            var cb = await BuildFormulaeComponents(spell.Formulae!.Replace(".CL", casterLevel.ToString()));          
            await RespondAsync(embed: eb.Build(), components: cb.Build());         
            return;

        }
      

        public static async Task<string> Evaluate(string expr, StringBuilder sb, ulong user)
        {
            var exprs = expr.Split(';');
            var result = "";
            var stats = await Characters.GetCharacter(user);
            for(int i = 0; i < exprs.Length; i++)
            {
                var node = Parser.Parse(exprs[i], stats, sb);
                result += $"{node.Eval(0, null!, sb, stats)};";
            }
            return result.Trim(';');
        }

        [ComponentInteraction("row:*,*")]
        public async Task ButtonPressedExpr(string expr, string name)
        {
            user = Context.Interaction.User.Id;
            var character = await Characters.GetCharacter(user);
            var sb = new StringBuilder();
            var result = await Evaluate(expr, sb, user);
            var ab = new EmbedAuthorBuilder()
                .WithName(Context.Interaction.User.Username)
                .WithIconUrl(Context.Interaction.User.GetAvatarUrl());

            var builder = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithAuthor(ab)
                .WithTitle($"{result.Trim(';')}")
                .WithDescription(character.CharacterName != "$GLOBAL" ? character.CharacterName : "")
                .WithFooter($"{expr}");

            if(sb.Length > 0) builder.AddField($"__Events__", $"{sb}");

            await RespondAsync(embed: builder.Build());
        }

        static ComponentBuilder BuildRow(List<ExprRow> exprRows)
        {
            var cb = new ComponentBuilder();

            for(int i = 0; i < exprRows.Count; i++)
            {
                for(int j = 0; j < exprRows[i].Set.Count; j++)
                {
                    if(!string.IsNullOrEmpty(exprRows[i].Set[j].Expression))
                        cb.WithButton(customId: $"row:{exprRows[i].Set[j].Expression.Replace(" ", "")},{exprRows[i].Set[j].Name.Replace(" ", "")}", label: exprRows[i].Set[j].Name, disabled: (exprRows[i].Set[j].Expression == "") ? true : false, row: i);
                }
            }
            return cb;
        }
        
        static ComponentBuilder BuildRow(ExprRow exprRow)
        {
            var cb = new ComponentBuilder();

            for(int i = 0; i < exprRow.Set.Count; i++)
            {
                if(!string.IsNullOrEmpty(exprRow.Set[i].Expression))
                    cb.WithButton(customId: $"row:{exprRow.Set[i].Expression.Replace(" ", "")},{exprRow.Set[i].Name.Replace(" ", "")}", label: exprRow.Set[i].Name, disabled: (exprRow.Set[i].Expression == "") ? true : false);
            }          
            return cb;
        }

        public async Task<ComponentBuilder> BuildFormulaeComponents(string formulae)
        {
            return await Task.Run(() =>
            {
                var cb = new ComponentBuilder();
                var split = formulae.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < split.Length; i++)
                {
                    var f = split[i].Split('#');
                    cb.WithButton(f[0], $"e:{f[1].Replace(" ", "")}");
                }
                return cb;
            });

        }

        [AutocompleteCommand("row-name", "row")]
        public async Task AutoCompleteRowOne()
        {                  
            var input = (Context.Interaction as SocketAutocompleteInteraction)!.Data.Current.Value.ToString();
            var results = new List<AutocompleteResult>();
            if(Characters.Active.ContainsKey(Context.User.Id))
                foreach(string name in Characters.Active[Context.User.Id].ExprRows.Keys)
                    results.Add(new AutocompleteResult(name, name));
            await (Context.Interaction as SocketAutocompleteInteraction)!.RespondAsync(results.Take(5));                            
        }          

        [AutocompleteCommand("creature_name", "best")]
        public async Task AutoCompleteBestiary()
        {
            var input = (Context.Interaction as SocketAutocompleteInteraction)!.Data.Current.Value.ToString();
            var results = DataMap.autoCompleteCreatures.Where(x => x.Name.StartsWith(input!, StringComparison.InvariantCultureIgnoreCase));
            await (Context.Interaction as SocketAutocompleteInteraction)!.RespondAsync(results.Take(5));
        }

        [AutocompleteCommand("item_name", "item")]
        public async Task AutoCompleteItem()
        {
            var input = (Context.Interaction as SocketAutocompleteInteraction)!.Data.Current.Value.ToString();
            var results = DataMap.autoCompleteItems.Where(x => x.Name.StartsWith(input!, StringComparison.InvariantCultureIgnoreCase));
            await (Context.Interaction as SocketAutocompleteInteraction)!.RespondAsync(results.Take(5));
        }

        [AutocompleteCommand("rule_name", "rule")]
        public async Task AutoCompleteRules()
        {
            var input = (Context.Interaction as SocketAutocompleteInteraction)!.Data.Current.Value.ToString();
            var results = DataMap.autoCompleteRules.Where(x => x.Name.Contains(input!, StringComparison.InvariantCultureIgnoreCase));
            await (Context.Interaction as SocketAutocompleteInteraction)!.RespondAsync(results.Take(5));
        }

        [AutocompleteCommand("shape_name", "shape")]
        public async Task AutoCompleteShape()
        {
            var input = (Context.Interaction as SocketAutocompleteInteraction)!.Data.Current.Value.ToString();
            var results = DataMap.autoCompleteShapes.Where(x => x.Name.StartsWith(input!, StringComparison.InvariantCultureIgnoreCase));
            await (Context.Interaction as SocketAutocompleteInteraction)!.RespondAsync(results.Take(5));
        }

        [AutocompleteCommand("spell_name", "spell")]
        public async Task AutoCompleteSpell()
        {
            var input = (Context.Interaction as SocketAutocompleteInteraction)!.Data.Current.Value.ToString();
            var results = DataMap.autoCompleteSpells.Where(x => x.Name.StartsWith(input!, StringComparison.InvariantCultureIgnoreCase));
            await (Context.Interaction as SocketAutocompleteInteraction)!.RespondAsync(results.Take(5));
        }
        
    }

}
