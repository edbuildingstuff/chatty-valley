"""Single definition of per-category turn caps and category targets for the dataset tools.

build_batch.py, assemble_dataset.py and sweep_dataset.py all import from here. Before 2026-09-24
MAX_TURNS was defined in three files with a comment claiming they matched; one definition removes
the drift instead of documenting it.
"""

# category -> max assistant turns; anything absent defaults to 3. depth, perspective, rumor and
# romance train holding a position under sustained pressure. long (Elliott v2) makes 6 to 10 turn
# conversations in-distribution, because the 2026-09-24 play-test ran nine turns against a corpus
# where 460 of 577 conversations stopped at two. repair trains restating plainly after "what?".
MAX_TURNS = {"depth": 6, "perspective": 6, "rumor": 6, "romance": 6, "long": 10, "repair": 4}


def max_turns(category):
    return MAX_TURNS.get(category or "", 3)


TARGETS = {
    "linus": {"voice": 210, "lore": 90, "state": 120, "place": 30, "deflection": 90, "crossover": 60,
              "identity": 46, "nonsense": 40, "reference": 55, "depth": 30, "casual": 57, "townsfolk": 19,
              "farewell": 42, "perspective": 40, "rumor": 73},
    # Elliott v2 (gtm spec 2026-09-24-elliott-v2-data-round-design.md): v1's 640 plus the new blocks.
    # casual +40 greetings; lore +50 fact sheet; rumor +60 (gift and activity premises, multi-turn
    # pressure, place rumors); perspective +15 companions; romance +25; townsfolk +20 Tier 1 warmth;
    # identity +10 bare openers; deflection +15 bare modern nouns; state +10 rows that must not end;
    # long 80; repair 40. "romance" is the conditional category for marriage candidates.
    # v2b (replay findings, 2026-09-24): townsfolk +16 contrastive Tier 1 identity; lore +8 main
    # characters; romance +8 poem-about-you; state +10 town events from his vantage; rumor +6
    # denial-first answers to "everyone's saying it".
    "elliott": {"voice": 110, "state": 85, "lore": 108, "romance": 78, "deflection": 65, "rumor": 108,
                "reference": 42, "perspective": 47, "crossover": 32, "identity": 38, "place": 28,
                "casual": 68, "farewell": 24, "nonsense": 24, "townsfolk": 54, "depth": 22,
                "long": 80, "repair": 40},
}
