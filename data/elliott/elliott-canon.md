# Elliott: canonical dialogue (Stardew Valley 1.6)

Extracted from the installed game's `Content`, read through the game's own MonoGame `ContentManager` (`tools/DialogueDump`), plus a filter pass over `Data\Events` and `Data\Festivals` for Elliott's spoken lines. This is the authoritative source text for the Elliott voice, kept verbatim with the game's dialogue markup intact. See `data/README.md` for how it was extracted and how to reproduce it.

**Totals:** 96 main dialogue entries, 54 marriage-dialogue entries, 29 festival entries, 2 engagement entries, 1 rainy, 76 event lines across 16 event keys. 258 units in all.

Elliott is a marriage candidate, so two sources exist that Linus never had: `MarriageDialogue<Name>` (post-wedding spouse dialogue) and `Data\EngagementDialogue` keys (between Mermaid's Pendant and the wedding). Dating-register lines also live INSIDE the main file (`dating_Elliott`, `dating_Elliott_memory_oneday`, `fall_Fri10`, `winter_Tue10`; undated friendship caps just below 9 hearts, so every `10`-suffix key implies a bouquet): the main file is multi-register, keyed by hearts and flags.

## Dialogue markup legend

- `#$e#` / `#$b#` : starts a new dialogue box (a pause or page break)
- `$h $s $u $l $a $k $0`..`$9` : speaker emotion, portrait, or end-of-line tokens
- `@` : the player's name
- `#$1 <flag>#` : line only shown when a mail or event flag is set
- `$y '<question>_<answer>_<response>...'` : a branching question with player choices
- `(O)166`, `[166]` : item references
- `%` variables (`%firstnameM`, `%spouse`, `%kid1`), `^` gender split, `#` field separators
- Day keys: `Mon`..`Sun` base; a digit suffix (`Mon4`, `Tue6`, `fall_Fri10`) is the heart level that unlocks the variant; season prefixes override the base set

## Main dialogue: Introduction and relationship state (14)

- `Introduction` : Ah, the new farmer we've all been expecting... and whose arrival has sparked many a conversation!#$b#I'm Elliott... I live in the little cabin by the beach. It's a pleasure to meet you.
- `firstVisit_ElliottHouse` : Ah... please excuse the mess in my cabin. I wasn't expecting anyone to enter!
- `dating_Elliott` : I only wish I had given you a bouquet first!$l
- `dating_Elliott_memory_oneday` : %Elliott whispers a sweet secret into your ear.
- `married_Elliott` : You've plucked Pelican Town's finest flower... Now, you must care for it with all your heart.
- `breakUp` : Ah... the wilted bouquet. A sight I'd hoped to never see...$s#$b#I suppose there are other horizons to set my eyes upon...$s
- `divorced` : ...$s#$e#Why do you torment me? Can't you see you've shattered all my hopes and dreams?$s
- `dumped_Guys` : Go away. I can't bear to see you.$s
- `secondChance_Guys` : You've hurt me beyond measure... but I'm willing to leave the past behind us.$s
- `FlowerDance_Accept` : Of course... it would be an honor! I look forward to it.$h
- `FlowerDance_Accept_Spouse` : Yes... could I refuse that soft, kind face? The touch of spring-time's sweet embrace?
- `FlowerDance_Decline` : Excuse me... not today.$s
- `eventSeen_39` : You've been very helpful... Now, I must think...
- `eventSeen_40_memory_oneday` : I had a good time at the saloon the other day... Though my liver is not quite so enthusiastic...

## Main dialogue: Day and seasonal greetings (53)

- `Mon` : I'm kind of new to this town myself, but I really feel at home.$h#$e#I moved here only a year before you.
- `Mon4` : I hope you've come to think of this place as 'home'.
- `Tue` : A great idea can pass through your head when you least expect it... but if your mind is too busy you might miss it.#$e#Well, I really must get back to my work.
- `Tue6` : The sweet friction of pen and paper is the music of my soul.$u#$b#That's why I chose this beach as my home, so that I could have peace and quiet to do my work.
- `Wed` : The forest is a wonderful place. Have you been there?
- `Wed8` : Sorry, I'm feeling morose today.$s#$e#I wish there was more certainty about the future.$s#$e#I don't want to grow old as a lonely hermit on this beach...$s
- `Thu` : Hello, @.#$e#Are you well?
- `Thu2` : I can't seem to find the inspiration to begin writing my novel...$s
- `Thu4` : I've been feeling hopeful lately. Perhaps the weather is changing.
- `Thu6` : Sometimes I wonder if I might just have an inflated self-image and no real skills...$s#$e#No, no... I'm not fishing for compliments. Though they are appreciated...
- `Thu8` : #$1 elliottApol#It's a little lonely out here on the beach... so I apologize if I was ever a little too forward with you when we first met. I was just eager to have a friend.$k#$e#It feels good to have a close friend like you.
- `Fri` : The fresh air of this valley is good for body and mind.#$e#A quick stroll outdoors always invigorates me.
- `Fri4` : Ah... do not get too close to my hair with a torch...$8
- `Fri8` : I'll admit... it takes me several hours each morning to make my hair look this good.
- `Sat` : You probably wouldn't like it inside my cabin. It's dark and full of spiders.$s
- `Sat4` : Please excuse the sorry state of my cabin.
- `Sat6` : Some people are shy. Keep showing interest in them and they'll get comfortable around you.#$e#Everyone likes to have friends, even that grumpy blacksmith.
- `Sun` : Hello. I hope your new farming life is panning out as you'd hoped?
- `Sun6` : Oh, @! I was hoping you'd show up.#$e#It's always a pleasure to see you.$h
- `summer_Mon` : A gentle little sunbeam woke me up this morning. I've never felt so refreshed.$u#$e#I'm sorry if that isn't very interesting to you.
- `summer_Mon4` : I know that I am kind of an 'oddball'.#$e#I hope you don't mind.$h
- `summer_Mon6` : @, I know you have faith in my abilities as a writer. That means a lot to me.$h
- `summer_Tue` : I've encountered some beautiful shells in front of my house.#$e#I would imagine the rarer varieties to be quite valuable.
- `summer_Tue6` : I write in hopes of connecting with others through time and space.$u#$e#*sigh*...times are changing, though. People don't read anymore.$s
- `summer_Wed` : It must be satisfying to follow your crop from seed to harvest.#$e#It's as if your essence is infused into the fruit.$u
- `summer_Thu` : Oh dear! My shoes are filled with sand.$s#$e#That's the trouble with living on the beach.
- `summer_Fri` : @? You look puzzled.#$b#I guess the hot summer air can make us a little dizzy.
- `summer_Sat` : The sun is angry today... My skin is a bit too delicate, I'm afraid.
- `summer_Sun` : You came all the way here just to talk to me? How kind.$h
- `summer_Sun4` : Visit my cabin whenever you like. I could use the company.$h
- `fall_Mon` : Hello @.#$e#Make sure and take breaks from your work now and then.
- `fall_Mon4` : Come visit me if you need a break from your labors.$h#$e#I'm usually at my house.
- `fall_Tue` : It's been said that a pirate's ship, full of plundered gold, shipwrecked here a long time ago.
- `fall_Tue8` : @, I was just thinking about you.$l#$e#Maybe you sensed it.$h#$e#So! Tell me about your day.
- `fall_Wed8` : I wouldn't mind trying my hand at farming.#$e#The quiet atmosphere of a farm might be a good source of literary inspiration, don't you think?$h
- `fall_Thu` : Oh dear! A tiny crab appears to have made his home in my shirt pocket.$l#$e#That's the trouble with living on the beach.
- `fall_Fri` : My legs are stiff from sitting at my writing desk all morning.$s
- `fall_Fri4` : My legs are stiff from sitting at my writing desk all night.$s#$e#Sometimes I envy you, @.
- `fall_Fri10` : @, when you're around... I feel unusually creative.$l#$e#I know... strange, but it's true.
- `fall_Sat` : It gets windy this time of year. Marvelous!$h
- `fall_Sun` : I have to remember to water my plants today. And not with sea water this time!
- `winter_Mon` : You've come to visit me in the cold?#$e#I could do with a respite from my lonely labors.
- `winter_Mon4` : @, you've trudged through the snow to visit me?#$e#I'm honored!$h
- `winter_Tue` : People have scraped a living off the sea for thousands of years.#$e#I just go to the grocery store.$s
- `winter_Tue10` : @, I had a feeling that you would show up.#$e#Perhaps we're connected by an other-worldly thread.$l
- `-winter_Wed` : A day can never be boring when you follow the whims of your imagination.$u#$e#Sorry, am I babbling on about nonsense?
- `winter_Thu` : Breathe deeply. Do you notice it? That's the smell of the sea.#$e#How does it make you feel?
- `winter_Thu6` : Breathe deeply. Do you notice it? That's the smell of the sea.#$e#Whenever I smell the sea, it reminds me of my youth. The ocean really impressed me as a child.
- `winter_Fri` : I have to brush my hair daily, or else it'll clump up into messy knots.#$e#It's a lot of work.#$e#I'm surprised I haven't just shaved it off in a fit of passion.$h#$e#I suppose I am too vain.
- `winter_Sat` : I've been doing little indoor exercises, since it's often too cold to go out.#$e#Sorry if it's a little humid in the cabin.$s
- `winter_Sun` : I really should scrub my floorboards today. I think an algae is starting to form.$s#$e#Do you make time for cleaning?
- `GreenRain` : Let's not get in a tizzy, now... Gus! I propose a round of drinks to settle the nerves!%noturn
- `GreenRain_2` : Ah, yet another unique day in the valley. Stimulating, to say the least!

## Main dialogue: Gift reactions (9)

- `AcceptBirthdayGift_Positive` : I'm honored that you would remember my birthday! Thank you!$h
- `AcceptBirthdayGift_Negative` : Another year gone by, another gray hair... I suppose this gift can commemorate my decline.$s
- `AcceptGift_(O)StardropTea` : Ah, what a gift! The aroma alone is inspiring...
- `AcceptGift_(O)814` : Ah, a bottle of fine ink. A writer can never have too much... and it's quite expensive! Thank you!$h
- `AcceptGift_(O)444` : This will make a beautiful quill! I feel inspired already...$h
- `AcceptGift_(O)154` : Agh, it's still wriggling! Get that abomination away from me!$6 [154]
- `AcceptGift_(O)155` : Agh, it's still wriggling! Get that abomination away from me!$6 [155]
- `AcceptGift_Positive_forage_item_beach` : Ah, you've been doing some beach combing... A fine hobby! Thank you.
- `AcceptGift_Positive_book_item` : Ah, a book... Yes, perhaps the prose within these pages will offer a new insight. Thank you.

## Main dialogue: Heart-event references (9)

- `event_idea1` : Mystery, huh? It's definitely an exciting genre. I'll remember that.
- `event_idea2` : Ah, one of the classic genres. I'll remember that.
- `event_idea3` : I would've never guessed! I suppose even those of the 'earthiest' profession sometimes have their heads in the stars.$h#$b#I'll remember that.
- `event_boat1` : ... I was worried you might not feel this way about another man.$l^So am I.$l
- `event_boat2` : Oh! I'm so sorry.$8%fork
- `event_toast1` : Well...okay.#$b#Here's to @!
- `event_toast2` : That's a great idea!$h#$b#Here's to us!
- `event_toast3` : Hmmph...forget it.$a
- `event_toast4` : Ah, to a harmonious future for the community... what a virtuous idea.#$b#Here's to Pelican Town!

## Main dialogue: Ginger Island resort (8)

- `Resort` : I must admit, this beach blows my own humble strand out of the water.
- `Resort_2` : Oh dear! Some seaweed has tangled itself in my hair.#$e#That's the trouble with relaxing on the beach.
- `Resort_Entering` : That was quite the ride!
- `Resort_Leaving` : I'll see you back in the valley.
- `Resort_Leaving_2` : Hopefully a crab hasn't snuck onto my clothes again.$s#$b#Imagine crawling out of a pocket and finding yourself on a beach hundreds of miles from home!$h#$e#Hmmm... Perhaps this could make for an interesting novel...
- `Resort_Chair` : I'm just taking a quick break to slather some coconut oil on my body.$h
- `Resort_Bar` : Gus, I'll order a round for everyone!#$b#*hic*... Oh... I don't have the G.$s#$b#Heh heh... Never mind!
- `Resort_Wander` : I've heard rumors of pirates coming to shore near here at night. They say it can get pretty wild!

## Main dialogue: Miscellaneous (3)

- `SquidFest` : It's rare to see so many people at the beach. All for the love of squid!
- `wonIceFishing` : Well, if it isn't the town's number one ice fisher! That was truly an impressive performance.$h#$e#I never stand a chance, but I always participate just to be sporting.
- `purchasedAnimal_Duck` : Did you know? A duck's feather makes for an excellent quill.

## Rainy day (1)

- `Elliott` : Sometimes when I lay in bed I can hear a distant foghorn cut through the rain.#$b#But when I look out the window I see only a curtain of gray.

## Event lines (76 lines, 16 keys)

Only Elliott's own spoken lines (speak / textAboveHead) were extracted from each event script; stage directions and other speakers are omitted. The full event key (trigger conditions) is kept as the heading.

### Two-heart event: the cabin, his backstory, the genre question

`ElliottHouse:39/f Elliott 500/p Elliott`

- @! Come in.
- Welcome to my humble... well, shack.
- This is my writing desk. It's where I spend most of my time.
- For as long as I can remember, I've wanted to be a writer. Have I told you that?#$b#That's why I live out here by myself. I figured a lonely life by the sea would help me focus on my literary aspirations...$s
- Everyone back home said I was nuts... that I could never make it as a writer.#$b#Can you believe it? They said 'For every successful author there's 1000 who fail miserably'. Such pessimism... it's sickening.
- I can see it in your eyes... you believe in me, @. You've got that spark.#$b#Now that's inspiring! That's what I'm looking for...
- $q 958699 null#A question... What kind of books do you like, @?#$r 958699 30 event_idea1#Mystery#$r 958700 30 event_idea2#Romance#$r 958701 30 event_idea3#Sci-Fi
- Well! Enough talk about me!
- Hmm... you probably know a lot about plants, don't you?
- Would you mind taking a look at this rose, here? I'm afraid it's not doing so well.$7

### Four-heart event: the Stardrop Saloon toast with Gus

`Saloon:40/f Elliott 1000/p Gus/t 1500 2200`

- Hello, @! What a pleasant surprise!#$b#I was just stopping in to relax after an eight hour writing session.$h
- Bartender! Two of your finest ales, please!^Bartender! Fetch me your finest ale. And bring some wine for the lady!
- $q 28376 null#Wait. I propose a toast! To...#$r 28376 25 event_toast4#To Pelican Town!#$r 28376 50 event_toast2#To our friendship!#$r 28376 -10 event_toast1#To my good health!#$r 28376 -50 event_toast3#To your doom!
- *Hic*... Strong stuff...$h

### Six-heart event: the piano, the unfinished novel

`ElliottHouse:423502/f Elliott 1500/p Elliott`

- Ah... I thought someone was there.$7
- Thank you. I'm not very good, but it's fun to play.$h

### Six-heart event branch: 'How long have you been playing?'

`ElliottHouse:howLong`

- Oh, I'm not sure... I've been dabbling in piano since I was a kid.$7#$b#I'm not very good, but it's fun.

### Six-heart event branch: piano compliment

`ElliottHouse:elliottPianoJoin`

- I've been working day and night to try and finish my book... It's been driving me insane, @.$s
- An occasional tune is the only recreation I allow myself.$7
- There's just too much work to do! And my bank account's starting to run dry.$7#$b#Sometimes I wish I could just throw it all away and become a farmer like you.$h
- You're right... that was an insensitive thing to say.$8
- What I meant is that I'd like to get away from this dark, musty prison and experience a little bit of real life... that's all.$s
- *sigh*... sorry I'm complaining like this. I just need someone to talk to now and then.$7

### Six-heart event branch: 'Come live on the farm'

`ElliottHouse:extraHelp`

- Seriously?$8
- It sounds wonderful... but I can't give up on my novel. It's already half-way done.$7
- *sigh*... sorry I'm complaining like this. I just need someone to talk to now and then.$7

### Eight-heart event: the book reading at the library (sci-fi default)

`ArchaeologyHouse:1848481/f Elliott 2000/t 1300 1900/n elliottReading`

- @, you made it!$h#$b#I feel so relieved to be done with my book... it's like an elephant's been lifted off my shoulders.
- Well, I'd better get started with the reading... Wish me luck.$8
- Good afternoon, everyone.
- Ever since I was a young boy, I've dreamt of becoming a writer.#$b#When the time came for me to leave home and start my own life, I moved here. I was drawn to the peaceful beauty of the valley, and hoped that days of quiet reflection in this idyllic atmosphere would fan the literary flames.
- After countless hours scribbling at my writing desk, I present to you my first novel: 'The Rise And Fall Of Planet Yazzo'... It's a sci-fi epic spanning thousands of years in an exotic planetary system.
- Chapter One.#$b#Commander Yutkin stepped through the golden archway as the airlock snapped shut behind him. Today was his first day on Planet Yazzo, and all 14 of the alliance delegates had been summoned to the Grand Spire...
- ...And as the 7th moon descended beneath the horizon, the planet of Yazzo would begin its sinister transformation... an event for which Commander Yutkin was completely unprepared.
- Well, that concludes my reading. I'll be selling signed copies of the book by the front desk. Thanks for listening!$h
- Well, how was it?
- Thanks. $h#$b#You know, I got the idea for making a sci-fi book from you. Do you remember?#$b#That's why I've dedicated this book to you...$l

### Eight-heart event variant: the mystery novel 'Blue Tower'

`ArchaeologyHouse:mysteryBook`

- After countless hours scribbling at my writing desk, I present to you my first book: 'Blue Tower'... It's a mystery novel set in a surreal, dystopian future.
- Chapter One.#$b#From the shadows emerged a man, radiating with enigmatic omniscience. 'Good Evening, Mr. Lu,' he said, the corners of his mouth quivering. Lu seemed astonished. 'How did you know my name?'
- Lu checked Jenu's pockets, then stood up and walked into the bedroom. He quickly found the small golden key that he was looking for and slipped it into his coat pocket.
- Well, that concludes my reading. I'll be selling signed copies of the book by the front desk. Thanks for listening!$h
- Well, how was it?
- Thanks. $h#$b#You know, I got the idea for making a mystery from you. Do you remember?#$b#That's why I've dedicated this book to you...$l

### Eight-heart event variant: the romance novel 'Camellia Station'

`ArchaeologyHouse:romanceBook`

- After countless hours scribbling at my writing desk, I present to you my first novel: 'Camellia Station'... It's a romance novel about a train stewardess who falls in love with a traveling architect...
- Chapter One.#$b#'Your ticket, sir?' Ticket collector Gozman extended a gloved hand towards the young commuter. 'Ah, yes. I have it right here,' he replied, reaching into his coat pocket. Mortified, he discovered that the ticket was missing.
- ...'Clara, there's something I must tell you,' he blurted as she turned to leave. Clara turned, slowly, and saw the look of desperation in Horatio's eye. At that moment Gozman burst into the compartment, red-faced.
- Well, that concludes my reading. I'll be selling signed copies of the book by the front desk. Thanks for listening!$h
- Well, how was it?
- Thanks. $h#$b#You know, I got the idea for writing a romance novel from you. Do you remember?#$b#That's why I've dedicated this book to you...$l

### Ten-heart event: the rowboat maiden voyage and the kiss

`Beach:43/f Elliott 2500/w sunny/t 700 1300/G !IS_PASSIVE_FESTIVAL_TODAY SquidFest`

- Hey.
- Look... I fixed up that old rowboat that's been sitting by my house. Pretty nice, huh?
- @... Would you do me the honor of joining me for her maiden voyage?$l
- So my book's been out for a while now... It's not a best-seller or anything, but it's been getting some good reviews from the critics.#$b#And I really couldn't have finished it without your moral support.$l
- Actually, that's not true at all. I would've finished it either way.$h#$b#But I am grateful that you believed in me... in my vision. And, well...
- Um... @? How do I say this...$8
- Well, we've been friends for a while now... But I'm... I'm not sure if I feel that way about you anymore.$l
- No! I'm not saying I want to cut all ties with you!$8#$b#In fact... quite the opposite.$l
- ...Let's see, how do I put this...?$l#$b#For once, I'm at a loss for words...
- $q -1 null#@? You're trembling...$l#$r -1 50 event_boat1#I'm happy.#$r -1 -50 event_boat2#You're making me very uncomfortable. Stop.
- We'd better head back before the southern wind picks up.$l^Uh oh... The vibration from your body has caught the attention of a Crimsonfish... We'd better get out of here.$l
- Look at the valley from here... it finally looks like 'Home'.
- *sigh*... What a day...$l

### Ten-heart event branch: the player refuses the boat

`Beach:NoToElliott`

- I see.$7

### Group ten-heart event (with Rabbit's Foot): the pool game

`Saloon:195099/f Shane 2500/f Sebastian 2500/f Sam 2500/f Harvey 2500/f Alex 2500/f Elliott 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 911526/e 528052/e 9581348/e 43/e 384882/e 233104/i 446/k 195013`

- Don't worry, I've never played before, either.$h

### Group ten-heart event (no Rabbit's Foot): the confrontation

`Saloon:195013/f Shane 2500/f Sebastian 2500/f Sam 2500/f Harvey 2500/f Alex 2500/f Elliott 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 911526/e 528052/e 9581348/e 43/e 384882/e 233104/k 195099`

- For once, I'm at a loss for words... $a
- Absolutely.$a

### Group ten-heart branch: the player tries to explain

`Saloon:choseToExplain`

- Don't blame others for your mistakes! You're just losing more respect with us...$a

### Fourteen-heart event: the book-signing tour departure

`Farm:3912125/f Elliott 3500/O Elliott/t 500 1500/p Elliott/U 8`

- @! I've just received the most exciting news!
- It's very short notice... but I've been invited to do a reading tour for my book, '%book'!
- ...I'll be out of town for a week. Will you be okay?$s
- I'll miss you very much, my dear.#$b#And I'll write to you every day, of course!
- Hah... good one. Now, don't get too excited...$8#$b#I'll be sure to write to you every day. I'll miss you!
- But, my dear... This is a rare opportunity for me. I must go!$s#$b#Please don't make me feel guilty about this. I'll write to you every day, and I'll be back before you know it.
- I leave early tomorrow morning...#$b#I need to pack!$8

### Fourteen-heart event: the return home

`FarmHouse:3912132/e 3912126/O Elliott/A elliottGone/B`

- @... I'm back...
- Ah... I've missed you.$l#$b#One drop of the big city and I'm quenched... I much prefer being back here with you!
- Well, I guess it's back to the old routine once again!$h#$b#I've got some chores to catch up on...


## Festival dialogue (29)

### Egg Festival (Spring 13)

- `Elliott_spouse` : I enjoy seeing you so relaxed, my dear.$h
- `Elliott` : Taking breaks from work can make you more productive in the long run.
- `Elliott_y2` : Be careful you don't accidentally take a raven's egg. They'll hold a grudge against you for the rest of their days!

### Flower Dance (Spring 24)

- `Elliott` : I wore my best shirt for the dance... This sort of thing doesn't happen very often!
- `Elliott_spouse_y2` : May I have this dance?$4
- `Elliott_y2` : To dance among a whimsical backdrop of flowers is an uplifting experience.$1

### Luau (Summer 11)

- `Elliott_spouse` : The old cabin seems to be holding up well. Every time I return I half expect the thing to be rotted...
- `Elliott` : I woke up late, stepped out of the door and found myself in the middle of all this hubbub!#$e#I forgot that today was the Luau.$h
- `Elliott_spouse_y2` : I would suggest we make time for a little 'rendezvous' in my old cabin... but I'm afraid it's become rather... musty... in my absence.
- `Elliott_y2` : Ah, the Luau... all the hustle and bustle of town brought to my very doorstep!$0

### Dance of the Moonlight Jellies (Summer 28)

- `Elliott` : If we keep polluting the oceans, the jellies will surely go extinct. It's already in the process of happening.#$e#What a shame... we have no respect for nature anymore.
- `Elliott_spouse_y2` : Ah, I was wondering when you would arrive!$0#$e#Let's head to the docks soon, and welcome our aquatic guests!$1
- `Elliott_y2` : The candle lights shimmering on the water are simply breathtaking. It looks like a painting coming to life.$0

### Stardew Valley Fair (Fall 16)

- `Elliott_spouse` : The smokey aroma drew me here, yet 'twas the zesty sauce that truly sealed my fate... *gurgle*$s
- `Elliott` : I'm trying to get Gus to tell me his sauce recipe, but he won't budge.
- `Elliott_spouse_y2` : Victory! I achieved a score of '216' with five fish caught... a new personal best!$1#$e#Now, I must choose a fitting prize to purchase for you with these star tokens...$0
- `Elliott_y2` : Ah, don't mind the smell... I just emerged from the fishing booth with a sizable haul!$1#$e#Now, I must decide what to do with all these star tokens...$0

### Spirit's Eve (Fall 27)

- `Elliott_spouse` : Mmph... I believe I've eaten a few too many slices of pumpkin pie.
- `Elliott` : Why, hello @. It's chilly, isn't it?
- `Elliott_spouse_y2` : I shudder to think what would happen if these got loose...%noturn$2#$e#Fear not, my dear! I would run the cursed wretch through, before it could lay a finger on you.
- `Elliott_y2` : Oh pitiful wretch... in what fetid grotto lies your kingdom?%noturn$0#$e#Sometimes, one must stare into the abyss to stir a languid muse...

### Festival of Ice (Winter 8)

- `Elliott_spouse` : I know you'll beat me in the fishing competition, but I don't mind. I'm just here for the fun of it.$h
- `Elliott` : I'm entering the ice fishing competition today. Why not?#$e#It's rare that Willy ever loses, though.
- `Elliott_spouse_y2` : I've been practicing my fishing technique. I won't hold back against you, my dear.$1
- `Elliott_y2` : Look here, it's my secret weapon... a flask of fine spirits, to wet my lips a moment before the whistle sounds...

### Feast of the Winter Star (Winter 25)

- `Elliott_spouse` : You don't need to get me anything, you've already given me the greatest gift of all...$l
- `Elliott` : Why, hello @. It's chilly, isn't it?
- `Elliott_spouse_y2` : My dear, I require nothing at all on this day... you are already the greatest gift a man could hope for.$h
- `Elliott_y2` : *urp*$8#$b#Ahh... excuse me. Where are my manners? This spiced cider has quite a potent fizz to it.$4


## Marriage dialogue (54)

Spouse-register lines (`MarriageDialogueElliott`), shown only after the wedding. `_0`..`_4` are random variants; dated keys fire on that calendar day.

### Rainy days

- `Rainy_Day_0` : I feel inspired today... I think I'll do some writing.
- `Rainy_Day_1` : The sound of rain, in some tiny way, reminds me of my cabin by the beach.#$e#The farm is wonderful, but I do miss the sound of the ocean.
- `Rainy_Day_2` : I got up early and made coffee.[395]#$e#I find that a cup of this exquisite brew makes the early morning a lot more pleasant.
- `Rainy_Day_3` : I think I'll remain indoors today, my dear. The rain causes my hair to go limp.
- `Rainy_Day_4` : My inspiration for writing is like the weather... it comes and goes at random. Today I feel entirely dull.$s#$e#Perhaps I'll read one of the classics to get my creative juices flowing.

### Rainy nights

- `Rainy_Night_0` : Good evening. Did you have a productive day, @?^Good evening. Did you have a productive day, my dear?
- `Rainy_Night_1` : You're ice cold! Let me keep you warm.$l
- `Rainy_Night_2` : My skill with words is unmatched, yet I can't find the way to properly describe your allure.$l^My skill with words is unmatched, yet I can't find the way to properly describe your beauty.$l
- `Rainy_Night_3` : What did I do today? I'll just say this... it takes a lot of work to maintain this rugged physique!
- `Rainy_Night_4` : I spent the afternoon daydreaming about the ocean. So I decided to cook some seafood. [198 202 727 728]$h

### Indoor days

- `Indoor_Day_0` : I've been taking much better care of myself now that we're together. The bachelor life wasn't particularly healthy for me.
- `Indoor_Day_1` : Good morning, @! I made us a pot of coffee.[395]#$e#I find myself craving this robust flavor nearly every morning.
- `Indoor_Day_2` : If you find a spider in the house, don't squash it! Just let me know and I'll take the poor thing outside.
- `Indoor_Day_3` : Today is going to be a fantastic day... I can feel it!$h#$e#I get this special feeling in my nose...
- `Indoor_Day_4` : I've had this recurring nightmare that you gave me a buzz cut... You wouldn't ever do that to me, would you?$s#$e#I trust you.

### Indoor nights

- `Indoor_Night_0` : From the brightest winter star, to the shimmer of an iridium vein... nothing can compare to my wonderful man.$l^From the brightest winter star, to the fragrant fairy rose... nothing can compare with your captivating beauty.$l
- `Indoor_Night_1` : A crackling fire adds wonderful ambience to the house... every piece of wood burns in a unique way.
- `Indoor_Night_2` : I never had much success growing plants in my old beach house. Hopefully I can pick up a thing or two by watching you.#$e#In this respect, you are the master and I only a humble apprentice.
- `Indoor_Night_3` : When I behold thy wondrous face, a precious jewel of form and grace, my heart... torn by the dread of night, is purified with golden light.#$e#Poetry is the only way I can begin to describe my feelings for you.$l
- `Indoor_Night_4` : My love... I wouldn't trade you for 100 iridium bars.#$e#Nor 1000...#$e#Not even 10,000 bars, no.$7#$e#...$7#$e#No, not even 100,000 bars!$a#$e#...$s#$e#Wa... one million bars of pure iridium...? Don't make me do this...$8

### Outdoor days

- `Outdoor_0` : My wildest dreams have come true... just look at this incredible landscape!
- `Outdoor_1` : Fertile soil beneath my feet, fresh air to fill my lungs, and the sun's warmth to delight my skin... Life is going well.
- `Outdoor_2` : I might stay here and write some poetry. I'm feeling a sudden surge of creativity! You go on ahead and take care of business.
- `Outdoor_3` : If I stay perfectly still, perhaps a resplendent butterfly will bless my nose with a landing.
- `Outdoor_4` : I drank too much coffee... my mouth feels about as dry as the Calico Desert.$s#$e#Oh... sorry about the coffee breath.$7
- `patio_Elliott` : Ah, what a lovely day to read a book... don't you think, my dear?

### High-hearts spouse lines

- `Good_0` : @... I'm so proud of all your hard work. I'm very lucky to have you.$l
- `Good_1` : I made a whole secret book of poems expressing my love for you.$l
- `Good_2` : I came to the valley to find the ivory tower from which my talents could reign supreme. But what I really found was a dungeon of loneliness. You saved me from that.$l
- `Good_3` : Wow, you look really handsome today! Did you shave? Your jawline is perfect.$l^Your feminine allure is irresistible today. I can't keep my eyes off you.$l
- `Good_4` : I feel burnt out in your absence. But when I hear that sound of muddy boots on wood, my heart rises from the ash.
- `Good_5` : I'm doing a walking meditation. It's good for creativity.
- `Good_6` : Be careful out there! I do worry about you sometimes. There are people in the world who would take advantage of you.

### Children

- `OneKid_0` : Have you had any time to entertain little %kid1 today? Maybe a second child will make things easier for both of us.
- `OneKid_1` : Little %kid1 is going to have the perfect childhood here. There's so much to explore.
- `OneKid_3` : Caring for babies isn't exactly my strong suit... but I'll do my best to be a good father.
- `TwoKids_0` : I already gave %kid1 and %kid2 their food. They eat a lot for such small creatures!
- `TwoKids_1` : I was carrying %kid1 earlier, and I could've sworn I heard a "Da...da".$h
- `TwoKids_2` : I'm going to teach %kid1 to read as soon as possible! That's a great way for children to learn about the world.
- `TwoKids_3` : We've done well, @. The farm is doing excellent and our healthy children are a great joy. I couldn't be happier.

### Going out and returning

- `funLeave_Elliott` : I think I'll walk down to the beach today. It's always nice to see the ocean again.
- `funReturn_Elliott` : I had a nice time at the beach by myself. I watched the waves come and go, just like old times.

### Dated spouse lines

- `spring_1` : My new year's resolution is to write, write, and write! I can never stop improving my skills.
- `spring_12` : It's strange, but I often have cravings for pomegranate in the spring.
- `spring_23` : I look forward to dancing with you tomorrow, my dear.
- `summer_1` : It's hard to be in a foul mood when the sun is beaming and the butterflies are dancing on a spice-berry breeze.$h
- `summer_10` : I was going to grab an extra bottle of ink from my cabin today, but then I remembered the Luau is tomorrow. I'll just do it then.
- `summer_27` : All good things must come to an end, including the joyful days of summer. The key to happiness is to accept change without judgment.
- `fall_1` : The splendor of fall... a verdant banquet for all the senses to enjoy... has inspired some of the greatest poetry of all time.
- `fall_15` : I might set my pride aside and sink my eager teeth into a sloppy, saucy barbecue sandwich tomorrow.
- `fall_26` : Would you still love me if I guzzled two gallons of pumpkin ale at the Spirit's Eve festival? Sometimes a man has primitive urges...
- `winter_1` : Winter is a great time to read books and play the piano. Remember to pause and enjoy a quiet moment or two.
- `winter_7` : I'm a little out of practice, but I plan on entering the fishing contest tomorrow! You've got some stiff competition, dear.
- `winter_28` : Happy new year's eve, my dear. Please accept this to celebrate. [348]


## Engagement dialogue (2)

`Data\EngagementDialogue` keys, shown between accepting the Mermaid's Pendant and the wedding.

- `Elliott0` : The farm will be such a lovely place to write. I'm really looking forward to this.$h#$e#We're going to make a great pair.$l
- `Elliott1` : This is going to be such an adventure! I'm really excited. Aren't you?$h#$e#We can use my cottage as a beach house.$l
