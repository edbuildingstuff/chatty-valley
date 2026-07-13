# Linus: canonical dialogue (Stardew Valley 1.6)

Extracted from the installed game's `Content`, read through the game's own MonoGame `ContentManager` (`tools/DialogueDump`). This is the authoritative source text for the Linus voice, kept verbatim with the game's dialogue markup intact. See `data/README.md` for how it was extracted and how to reproduce it.

**Totals:** 53 main dialogue entries, 1 rainy, 16 festival, 29 event lines across 7 events. 99 units in all.

## Dialogue markup legend

- `#$e#` / `#$b#` : starts a new dialogue box (a pause or page break)
- `$h $s $u $l $a $k $0`..`$5` : speaker emotion, portrait, or end-of-line tokens
- `@` : the player's name
- `#$1 <flag>#` : line only shown when a mail or event flag is set
- `$y '<question>_<answer>_<response>...'` : a branching question with player choices
- `(O)166`, `[166]` : item references
- `%` variables, `^` gender split, `#` field separators

## Main dialogue: Introduction and relationship (2)

- `Introduction` : A stranger?... Hello.#$e#Don't mind me. I just live out here alone.
- `married` : I thought about tying the knot, once. But now, I'm happily married to the wind rustling in the leaves, and the frogs croaking under the silver moon.

## Main dialogue: Day and seasonal greetings (35)

- `Mon` : The crisp air of the wilderness is all I care to know.#$e#I live out here by choice.
- `Tue` : ...Have you come to ridicule me?#$e#I'm just minding my own business.
- `Wed` : I don't know you well enough to trust you. Sorry.
- `Thu` : ...Hmm? Do you want something from me?
- `Fri` : Please don't destroy my tent.#$e#It's happened before.$s
- `Sat` : I'm happy by myself, you know.#$e#I don't need new friends.
- `Sun4` : It would be nice if the townspeople could accept me for who I am.$s#$b#I like living out here in the open air. That's what they don't understand.
- `Tue4` : You can learn to survive in the wild. I have.#$e#I think we all have a hidden urge to return to nature. It's just a little scary to make the leap.
- `Wed6` : I spend a lot of time thinking.#$e#If you can fully understand the reasons behind your thoughts, you'll have reached a new level of being.
- `Wed2` : I have to be wary of strangers. Most people don't like a 'wild man'.
- `Thu4` : The people here seem nice, but they avoid me.#$e#People are afraid of the unknown.$u
- `Fri4` : You can learn a lot from trees.#$b#Spend time with them and they might tell you their secrets.#$e#Go in peace, young one.
- `Sat4` : I have everything I need to survive, and more. Nature plays a wonderful tune if you can only learn to listen.
- `Sun` : #$1 linusVandal#Someone was throwing rocks at my tent last night... I just had to wait it out.$s$k#$e#I don't like to stay in one place for too long. There's just too much to experience in the world.
- `summer_Mon4` : During all these years I've discovered a few secrets about life.#$e#You'll have to find out for yourself.
- `summer_Mon` : #$1 LinusHeron#I saw a heron wading gracefully through the morning mist. Such are the treasures of a quiet life.$k#$e#It's so easy to get caught up in the noise of modern life. Your best years will pass you by in a formless blur. That's why you've got to learn to slow down. 
- `summer_Tue4` : How have you been, my young friend?
- `summer_Wed6` : Ah... summer. The warm sun heats up the cans for me.#$b#Then at night I've got a warm meal to look forward to.
- `summer_Thu4` : I have my own reasons for living alone like this.#$e#Some things are best left unsaid...#$e#I should do some foraging.
- `summer_Fri4` : Whenever you catch a fish, take a moment to thank it for its sacrifice.#$b#Even if you don't believe in the spirit of the fish, it's a good mental exercise. We need to be aware of our effect on the environment.
- `summer_Sat4` : You can find rainbow trout in streams right now.
- `summer_Sun` : This is an easier time of year for me. I don't have to worry about staying warm.#$e#Plus, the fruits of the wild are growing everywhere.
- `fall_Mon` : I've explored deep into the caves.#$e#They hold some hidden secrets.#$e#Just be cautious if you go in there.
- `fall_Tue4` : #$1 LinusFall1#Some joker sprayed paint all over my home during the night... It took hours to scrub it off this morning.$k#$e#I need to start gathering lumber for the winter.#$e#Do you have enough lumber to keep your house warm?
- `fall_Wed4` : I don't have many responsibilities, so I spend my time thinking.#$e#You have to understand your thoughts before you can control them.
- `fall_Sun` : I don't like to stay in one place for too long.
- `fall_Sun6` : There's nothing quite like a feast of wild mushrooms in the fall.$h
- `winter_Mon` : It can get really cold if you live in a tent.$s
- `winter_Mon4` : After you've truly unplugged from the modern way of life, you'll start to really enjoy the simple things... like the sun, a ripe fruit, or finding a stick with the exact shape you were looking for.
- `winter_Tue4` : Thanks for not shunning me, @. It takes a lot of wisdom to override your base instincts.#$e#I'm a human like everyone else. I just have a different lifestyle.
- `winter_Wed4` : I'm sure you understand why I'm cautious of strangers.#$e#But let's put that in the past, okay? You and I are friends now, I think.
- `winter_Thu4` : Winter is tough, but you can still fish and forage to get by.#$e#If you have any animals you should keep them inside.
- `winter_Fri4` : The trees go to sleep during the winter. It makes it a bit lonely out here.#$e#But I wouldn't deny them their beauty sleep.$h
- `winter_Sat4` : Thanks for stopping by. I was actually feeling a little lonely this morning.#$e#So, have you discovered anything interesting in the mines?
- `winter_Sun` : Sleeping on the ground is good for my back.#$e#It's best to look at the positive side of things.$h

## Main dialogue: Gift reactions (7)

- `AcceptGift_(O)StardropTea` : This means a lot to me, @. I'll save it for a quiet day in the tent. Thank you!
- `AcceptGift_(O)Book_Trash` : I don't usually read books... But this one is right up my 'alley'... Thank you!$h
- `AcceptGift_Positive_category_greens` : That's a good find! I'm always happy when eating wild food. Thank you.
- `AcceptGift_(O)774` : Let me see... it looks perfect. You're a good student, @.$h
- `AcceptGift_(O)166` : No, thank you. I have no desire for money... In fact, I think it's cursed. [166]
- `AcceptGift_Positive_category_fish` : Ah, that looks fresh. I'll be eating good tonight!$h
- `reject_869` : What do you have there? Looks like it would make good bait.$h

## Main dialogue: Story, world and special (9)

- `GreenRain` : All these strange trees will be gone tomorrow... it's one of the mysteries of nature.
- `GreenRainFinished` : How did the moss harvest go? Hehe... My bed is a lot softer now.
- `GreenRain_2` : I've already gathered enough moss for the entire year. How about you?
- `cc_Boulder` : My old friend, the glimmering boulder, has moved on. It may not seem like an important event, but to me it's a big change.#$b#I'm happy for the old rock to see more of the world, though.$h
- `mineArea_80` : Very few people have gone as deep into the mines as you have.
- `eventSeen_8357109` : The water here comes from glaciers high up in the mountains.#$e#I traveled to the source once, just to pay my respects to the water.
- `movieTheater` : Movie theater? Hmm... for me, the slow procession of nature is all the drama I need.
- `MovieInvitation` : Oh... me? I usually wouldn't, but... okay. It's nice of you to think about me.#$b#I'll head over there in a little while.
- `DumpsterDiveComment` : Find anything good?$h

## Rainy day (1)

- `Linus` : A warm rain is a pleasant way to get clean.$h

## Festivals (16)

- `fall16:Linus` : These animals never judge people by their looks. The same can't be said for humans.
- `fall16:Linus_y2` : Animals are kind of like little people... they also have feelings and emotions.$0
- `fall27:Linus` : Good show, old friend.
- `fall27:Linus_y2` : Haha! Another fine creation, my old friend.$1
- `spring13:Linus` : No one really talks to me... I just come for the deviled eggs.
- `spring13:Linus_y2` : I'll just slip in a little later and have some food. I'm eyeing that scrumptious looking pie!
- `spring24:Linus` : Oh, hello there. It's nice of you to talk to me.#$b#Spring is almost over... what a shame.
- `spring24:Linus_y2` : Did you have a successful salmonberry harvest?$0#$b#I picked enough to make a sweet and tangy jelly! I brought some today for everyone to taste.$1
- `summer11:Linus` : A slow, continuous rotation is key to achieving the perfect roast.
- `summer11:Linus_y2` : Ah, can you smell that?$0#$b#It's a signal that the roast is almost ready!$1
- `summer28:Linus` : I'll just sneak up when the jellies arrive... I don't want to bother anyone.
- `summer28:Linus_y2` : I noticed no one else was using this dock, so I quietly snuck down.$0#$e#I like watching the jellies, too.
- `winter25:Linus` : I'd join in... but I don't think I'm welcome.$s
- `winter25:Linus_y2` : The mayor personally invited me to his table. I couldn't turn that down!
- `winter8:Linus` : Igloo-building's an art I picked up from the tundra dwellers who live beyond the frozen sea.#$e#That was many years ago.$h#$e#An Igloo makes a nice home, but it's easier to just stay in my tent year-round.
- `winter8:Linus_y2` : Ahh... The forest has entered her long slumber for the winter...$1

## Story events (29 lines across 7)

### `IslandSouth:6497428/e 6497423/f Leo 1500/w sunny/t 600 1800/Hl leoMoved`

- Wait!
- Hello, Leo... My name's Linus.#$b#I've heard all about you, and your parrot family.#$b#It's really something special!$h
- Leo...#$b#I'd like you to come back with us, to Stardew Valley.#$b#It's a beautiful place... not as warm as here, but still full of life... and I live right in the middle of it!#$b#I know the lay of the land, and many things about the waters, the trees, the animals, and more...#$b#You see... I'm a child of nature, too. You might say we're 'birds of a feather'.$h#$b#But, I'm getting old... And I'd like to teach someone all that I've learned before moving on...$s#$b#Leo... will you come back with us?
- Well, let's not forget... the choice is really up to Leo...
- I understand. This is Leo's home, after all.$s
- ...It's hard to make changes. But sometimes it's for the best.#$b#Still... the choice is really up to Leo.
- Great!

### `Mountain:26/f Linus 1000/w sunny/t 2000 2400`

- @! Come stand next to the firepit. It feels great.
- I was hoping you'd come by sometime.
- I wanted to say sorry for mistrusting you at first.$s#$b#Most people don't treat me well, so I've learned to be cautious.$s
- But you've been uncommonly nice to me. You're a unique person.$h#$b#...And I consider you a good friend.
- Hey, I want to show you something. Come inside.$h
- Ah... there we go. See this? It's a special kind of fish bait that I make.#$b#It's top quality stuff... I'd eat it myself!#$b#Here, I want you to have the recipe.

### `Mountain:371652/f Linus 2000/w sunny/t 600 1700/a 12 26`

- That's very nice of you, but no thanks! I've had great luck foraging today.$h
- Er...$s#$b#Um... No, thanks.$s
- I appreciate the kindness... I really do...$s#$b#But... I've told you before... I choose to live this way...
- I like to be alone most of the time... I like the quiet sounds... moving with the rhythm of nature...#$b#It's a way of life that I'm comfortable with. I don't ever want to change that.
- @... I cherish our friendship very much. And I know you do, too.$h#$b#But... You don't need to try and 'help' me... I know best how to live my own life... okay?
- ...can you smell that? It's the sweet aroma of ripe berries...$h

### `Mountain:linusWell`

- Thanks, @. You had me worried, there... I thought you were going to ask me to move on to the farm with you! *wink*#$b#You know, I consider you my closest friend in the valley... you've never tried to 'fix' me... you respect my way of life, even if you don't understand it.$h#$b#I really appreciate that.
- Ah... can you smell that? It's the sweet aroma of ripe berries...$h

### `Mountain:8357109/w sunny/t 600 1900/n linusTrashCleanup`

- Ahhh... that feels good.$4
- The water's never been cleaner.$4
- ...And the same goes for me! Hehehe.$5

### `Mountain:8959199/w sunny/t 600 1900/e 6497428/f Leo 2250/F`

- Not yet...

### `Town:502969/w sunny/f Linus 50/j 7/t 2000 2400`

- It was me... I'm sorry.$s
- I find a lot of hot, fresh food in these cans... stuff that will go to waste if I don't take it.
- $y 'Do you think there's something wrong with what I'm doing?_Yes, it's disgusting._Disgusting to you, maybe. To me, it's a way of life. And I haven't gotten sick yet. I only eat things that look fresh._No. It's a shame for food to go to waste._Thanks, @. I knew you were an open-minded person. I feel good about what I'm doing. I'm not harming anyone.$h_Yes, it's illegal. That's George's private property._I don't believe in 'private property'. Besides, what do you want me to do? I need this food to survive. Have a heart.$s_No, but you should get a job and stop leeching off others._**Sigh**...#$b#Not everyone's cut out for this world, @. You don't know what it's like to be me. I'm not harming anyone, I just want to live life in my own way. Is that so wrong?'
- You can go on home. I promise I won't rummage in George's can anymore.#$b#You can tell him you scared off the raccoons for good.
