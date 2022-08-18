﻿namespace MathfinderBot
{
    public static class Stuff
    {
        static Dictionary<int, string> Quotes = new Dictionary<int, string>()
        {
            {0,  @"Remind yourself, $, that overconfidence is a slow and insidious killer."},
            {1,  @"I remember days when the sun shone and laughter could be heard from the tavern."},
            {2,  @"Ruin has come to our family."},
            {3,  @"Prodigious size alone does not dissuade the sharpened blade."},
            {4,  @"Leave nothing unchecked, there is much to be found in forgotten places."},
            {5,  @"Behold the heart of the world! Progenitor of life, father and mother, alpha and omega! Our creator—and our destroyer, $!"},
            {6,  @"The mighty sword arm anchored by holy purpose—a zealous warrior."},
            {7,  @"Many fall in the face of chaos, but not $. Not today."},
            {8,  @"The cost of preparedness—measured now in gold, later in blood."},
            {9,  @"Monstrous size has no intrinsic merit, unless inordinate exsanguination be considered a virtue."},
            {10, @"We are the flame"",they cry,""and darkness fears us!"},
            {11, @"This sprawling estate—a mecca of madness and morbidity. $'s work begins."},
            {12, @"The front line of this war is not in the dungeon, but rather, inside the mind."},
            {13, @"Welcome home, $, such as it is. This squalid hamlet, these corrupted lands, they are yours now, and you are bound to them."},
            {14, @"The bellows blast once again! The forge stands ready to make weapons of war." },
            {15, @"And now the true test—hold fast or expire?" },
            {16, @"There is a sickness in the ancient pitted cobbles of the old road, and on its writhing path you will face viciousness, violence, and perhaps other damnably transcendent terrors." },
            {17, @"Great adversity has beauty—it is the fire that tempers the blade." },
            {18, @"Slowly, gently, this is how life is taken." },
            {19, @"Where there is no peril in the task, there can be no glory in its accomplishment." },
            {20, @"Do not ruminate on this fleeting failure—the campaign is long, and victory will come." },
            {21, @"A moment of valor shines brightest against a backdrop of despair." },
            {22, @"Good fortune and hard work may yet arrest this plague." },
            {23, @"You cannot learn a thing you think you know." },
            {24, @"Ignorance of your enemy and of yourself will invariably lead to defeat." },
            {25, @"This expedition, at least, promises success." },
            {26, @"The great ruins belong to us, and we will find whatever secrets they hold." },
            {27, @"This one understands that adversity and existence are one and the same." },
            {28, @"At home in wild places, they are a stalwart survivor and a strict instructor." },
            {29, @"Curiosity, interest, and obsession—mile markers on $'s road to damnation." },
            {30, @"Alone in the woods or tunnels, survival is the same. Prepare, persist, and overcome." },
            {31, @"Success depends on survival." },
            {32, @"Shoot, bandage, and pillage—the dancing steps of war." },
            {33, @"Failure tests the mettle of the heart, brain, and body." },
            {34, @"True desperation is known only when escape is impossible." },
            {35, @"To those with a keen eye, gold gleams like a dagger’s point." },
            {36, @"Experimental techniques and tonics can overcome things a sharpened sword cannot." },
            {37, @"Every creature has a weakness. The wise hero trains for what she will face." },
            {38, @"Gilded icons and dogmatic rituals—for some, a tonic against the bloodshed." },
            {39, @"A sharper sword, a stronger shield. Anything to prolong a soldier’s life." },
            {40, @"A strict regimen is paramount, if one is to master the brutal arithmetic of combat." },
            {41, @"To fight the abyss, $ must know it." },
            {42, @"$ searches where others will not go, to see what others will not see." },
            {43, @"A little hope, however desperate, is never without worth." },
            {44, @"Adversity can foster hope and resilience." },
            {45, @"The cobwebs have been dusted, the pews set straight. The Abbey calls to the faithful." },
            {46, @"I see something long-absent in the sunken faces of passersby—a glimmer of hope." },
            {47, @"I hope you see things that startle you. I hope you feel things you never felt before. I hope you meet people with a different point of view. I hope you live a life you’re proud of. If you find that you’re not, I hope you have the strength to start all over again." },
            {48, @"Every cleared path and charted route reduces the isolation of our troubled estate." },
            {49, @"Shattered and unmade! Or, perhaps, reborn?"},
            {50, @"“The way is lit. The path is clear. We require only the strength to follow it."},
            {51, @"As the light gains purchase, spirits are lifted and purpose is made clear."},
            {52, @"Trouble yourself not with the cost of this crusade—its noble end affords you broad tolerance in your choice of means."},
            {53, @"$ may fall, but their knowledge lives on."},
            {54, @"Send $ to journey elsewhere, for we have a need of sterner stock."},
            {55, @"Driving out corruption is an endless battle, but one that must be fought."},
            {56, @"“All manner of diversion and dalliance await those who cross the threshold with coin in hand."},
            {57, @"More arrive, foolishly seeking fortune and glory in this domain of the damned."},
            {58, @"Tortured and reclusive—this one is more dangerous than they seem."},
            {59, @"What better laboratory than the blood-soaked battlefield?"},
            {60, @"Suffer not the lame horse—nor the broken man."},
            {61, @"Wealth beyond measure, awarded to the brave and the foolhardy alike."},
            {62, @"Those without a stomach for this place must move on."},
            {63, @"Life—the greatest treasure of all."},
            {64, @"All my life, I could feel an insistent gnawing at the back of my mind. It was a yearning—a thirst for discovery—which could be neither numbed nor sated."},
            {65, @"The raw strength of youth may be spent, but $'s eyes hold the secrets of a hundred campaigns."},
            {66, @"We are born of this thing, made from it, and we will be returned to it in time"},
            {67, @"Death is patient, it will wait."},
            {68, @"Death and demise—cause for celebration!"},
            {69, @"As soon as I saw $, I knew adventure was going to happen."},
            {70, @"Most will end up here, covered in the poisoned earth, awaiting merciful oblivion."},
            {71, @"Even reanimated bones can fall—even the dead can die again."},
            {72, @"Let me share with you the terrible wonders I have come to know."},
            {73, @"The plume and the pistol—a fitting end to folly, and a curse upon us all."},
            {74, @"Even the aged oak will fall to the tempest’s winds."},
            {75, @"More dust, more ashes—more disappointment."},
            {76, @"$ does this because it irritates you."},
            {77, @"I smell brimstone. Or is that $?"},
            {78, @"$ answered the letter—now like me, you are part of this place."},
            {79, @"Curious methodologies and apparatus can calm even the most tormented soul."},
            {80, @"Fan the flames! Mold the metal! We are raising an army!"},
            {81, @"Great heroes can be found even here, in the mud and rain."},
            {82, @"As life ebbs, terrible vistas of emptiness reveal themselves."},
            {83, @"Strong drink, a game of chance, and companionship—the rush of life."},
            {84, @"There is power in symbols. Collect the scattered scraps of faith and give comfort to the masses."},
            {85, @"Can you feel it? The walls between the sane world and that unplumbed dimension of delirium is tenuously thin here."},
            {86, @"Success, so clearly in view. Or, is it merely a trick of the light?"},
            {87, @"So many young adventurers, so little propriety."},
            {88, @"Be wary, triumphant pride precipitates a dizzying fall."},
            {89, @"For my part, I travel not to go anywhere but to go—I travel for travel’s sake. the great affair is to move." },
            {90, @"A whole new world. A dazzling place I never knew, but when I’m way up here, it’s crystal clear that now I’m in a whole new world with you."},
            {91, @"When’s it my turn? Wouldn’t I love, love to explore that shore above? Out of the sea, wish I could be, part of that world."},
            {92, @"There’s no mountain too great."},
            {93, @"Adventure is worthwhile in itself."},
            {94, @"Avoiding danger is no safer in the long run than outright exposure. Life is either a daring adventure or nothing."},
            {95, "Would you tell me, please, which way I ought to go from here?”\n“That depends a good deal on where you want to get to.”\n“I don’t much care where –”\n“Then it doesn’t matter which way you go.”"},
            {96, @"If we were meant to stay in one place, we’d have roots instead of feet."},
            {97, @"The danger of adventure is worth a thousand days of ease and comfort."},
            {98, @"Travel is fatal to prejudice, bigotry, and narrow-mindedness."},
            {99, @"Why do you go away? So that you can come back. So that you can see the place you came from with new eyes and extra colors. And the people there see you differently, too. Coming back to where you started is not the same as never leaving."},
            {100, @"Let us step into the night and pursue that flighty temptress, adventure."},
            {101, @"It’s a dangerous business, $, going out your door. You step onto the road, and if you don’t keep your feet, there’s no knowing where you might be swept off to."},
            {102, @"Traveling – it leaves you speechless, then turns you into a storyteller."},
            {103, @"Jobs fill your pockets, but adventures fill your soul."},
            {104, @"$ may not have gone where they intended to go, but I think they have ended up where they intended to be."},
            {105, @"Cover the earth before it covers you."},
            {106, @"And into the forest $ goes, to lose their mind and find their soul."},
            {107, @"Shut up, live, travel, adventure, bless, and don’t be sorry."},
            {108, @"Life shrinks or expands in proportion to one’s courage."},
            {109, @"Sometimes getting lost is not a waste of time."},
            {110, @"I travel because I become uncomfortable being too comfortable."},
            {113, @"Go where you feel most alive."},
            {114, @"It feels good to be lost in the right direction."},
            {115, @"Life should not be a journey to the grave with the intention of arriving safely in a pretty and well-preserved body, but rather to skid in broadside in a cloud of smoke, thoroughly used up, totally worn out, and loudly proclaiming “Wow! What a Ride!"""},
            {116, @"The gladdest moment in human life is a departure into unknown lands."},
            {117, @"If you want to know the truth of who you are, walk until not a person knows your name. Travel is the great leveler, the great teacher, bitter as medicine, crueler than mirror-glass. A long stretch of road will teach you more about yourself than a hundred years of quiet."},
            {118, @"It is never too late to be who you might have been."},
            {119, @"Death may be the greatest of all human blessings."},
            {120, @"We all die. The goal isn’t to live forever, the goal is to create something that will."},
            {130, @"Remembering that you are going to die is the best way I know to avoid the trap of thinking you have something to lose."},
            {131, @"To die will be an awfully big adventure."},
            {132, @"I would rather die a meaningful death than to live a meaningless life."},
            {133, @"There are far, far better things ahead than any we leave behind."},
            {134, @"There is no cure for birth and death save to enjoy the interval. The dark background which death supplies brings out the tender colours of life in all their purity."},
            {135, @"Death is more universal than life; everyone dies but not everyone lives."},
            {136, @"You never know how much you really believe anything until its truth or falsehood becomes a matter of life and death to you."},
            {137, @"The hour of departure has arrived, and we go our separate ways, I to die, and you to live. Which of these two is better only Gods know."},
            {138, @"All goes onward and outward, nothing collapses, and to die is different from what any one supposed, and luckier."},
            {139, @"When the ancients said a work well begun was half done, they meant to impress the importance of always endeavoring to make a good beginning."},
            {140, @"Dare to be wise; begin! He who postpones the hour of living rightly is like the rustic who waits for the river to run out before he crosses."},
            {141, @"Affairs are easier of entrance than exit; and it is but common prudence to see our way out before we venture in."},
            {142, @"Amusement that is excessive and followed only for its own sake, allures and deceives us."},
            {143, @"The world is satisfied with words, few care to dive beneath the surface."},
            {144, @"Do you shovel to survive, or survive to shovel?"},
            {145, "“I’ve never once thought about how I was going to die,” she said. \n “I can’t think about it. I don’t even know how I’m going to live.”"},
            {146, @"It’s not down on a map, true places never are." },
            {147, @"Asps... very dangerous. You go first." },
            {148, @"What is $? A miserable little pile of secrets!" },
            {149, @"Hide a stone among stones and a man among men." },
            {150, @"The Runelords will return." },
            {151, @"Keep watching the skies, $" },
            {152, @"$ loses saving throw vs. shiny with a penalty of -5. $ takes 2d8 damage to the credit card." },
            {153, @"Better to have one's skull crushed than to have one's spirit enslaved!" },
            {154, @"Only the dead have seen the end of war." },
            {154, @"It is better to be feared than loved, if you cannot be both." },
            {154, @"It’s time to kick ass and chew bubble gum…and $'s all outta gum." },
            {154, @"It’s a-me, $io!" },
            {154, @"The sun went down with practiced bravado. Twilight crawled across the sky, laden with foreboding. I didn’t like the way the show started. But they had given me the best seat in the house. Front row center." },
            {154, @"The right man in the wrong place can make all the difference in the world." },
            {154, @"Would you kindly…" },
            {154, @"Nothing is true, everything is permitted." },
            {154, @"It’s dangerous to go alone, take $!" },
            {154, @"Praise the sun!" },
            {154, @"Do you like hurting other people, $?" },
            {155, @"Stop right there, criminal scum!" },
            {155, @"Do a barrel roll!" },
            {155, @"A man chooses; a slave obeys." },
            {155, @"Grass grows, birds fly, the sun shines, and brother, $ hurts people." },
            {155, @"Stay awhile and listen!" },
            {155, @"Good men mean well. We just don’t always end up doing well." },
            {156, @"You have died of dysentery." }
        };
    
        public static string GetRandomQuote(string name)
        {
            return Quotes[new Random().Next(Quotes.Count + 1)].Replace("$", name);
        }
    }
}
