"""Unit tests for the dataset tools. Run from the repo root: python tools/test_dataset_tools.py"""
import os, sys, unittest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import dataset_config
import assemble_dataset as A


def row(rid, cat, opener, turns=1):
    msgs = [{"role": "system", "content": "sys"}]
    for k in range(turns):
        msgs += [{"role": "user", "content": opener if k == 0 else f"more {k}"},
                 {"role": "assistant", "content": "reply."}]
    return {"id": rid, "category": cat, "context": "spring, clear morning, the beach, 2 hearts", "messages": msgs}


def by_cat(r):
    return r["category"]


def always(r):
    return True


class ConfigTests(unittest.TestCase):
    def test_new_categories_have_turn_caps(self):
        self.assertEqual(dataset_config.max_turns("long"), 10)
        self.assertEqual(dataset_config.max_turns("repair"), 4)
        self.assertEqual(dataset_config.max_turns("casual"), 3)
        self.assertEqual(dataset_config.max_turns(None), 3)

    def test_one_definition(self):
        import build_batch, sweep_dataset
        self.assertIs(build_batch.MAX_TURNS, dataset_config.MAX_TURNS)
        self.assertIs(A.MAX_TURNS, dataset_config.MAX_TURNS)
        self.assertIs(sweep_dataset.MAX_TURNS, dataset_config.MAX_TURNS)

    def test_elliott_v2_targets(self):
        t = dataset_config.TARGETS["elliott"]
        self.assertEqual(t["long"], 80)
        self.assertEqual(t["repair"], 40)
        self.assertEqual(sum(t.values()), 1005)


class SplitTests(unittest.TestCase):
    def base(self, n=20, cat="casual"):
        return [row(f"e-{cat}-{k:03}", cat, f"{cat} opener {k}") for k in range(n)]

    def test_unpinned_split_is_deterministic_and_ten_percent(self):
        tr1, ev1 = A.split_rows(self.base(), by_cat, always)
        tr2, ev2 = A.split_rows(self.base(), by_cat, always)
        self.assertEqual([r["id"] for r in ev1], [r["id"] for r in ev2])
        self.assertEqual(len(ev1), 2)
        self.assertEqual(len(tr1) + len(ev1), 20)

    def test_pinned_sides_hold_when_the_stratum_grows(self):
        base = self.base()
        tr, ev = A.split_rows(base, by_cat, always)
        pin = {"train": [r["id"] for r in tr], "eval": [r["id"] for r in ev]}
        grown = base + [row(f"e-casual-n{k:02}", "casual", f"new opener {k}") for k in range(20)]
        tr2, ev2 = A.split_rows(grown, by_cat, always, pin=pin)
        ev_ids, tr_ids = {r["id"] for r in ev2}, {r["id"] for r in tr2}
        self.assertTrue(set(pin["eval"]) <= ev_ids)
        self.assertTrue(set(pin["train"]) <= tr_ids)
        self.assertEqual(len(ev2), 4)  # 10% of 40, two pinned plus two drawn from new rows
        self.assertTrue(all(i.startswith("e-casual-n") for i in ev_ids - set(pin["eval"])))

    def test_new_stratum_gets_its_own_holdout(self):
        base = self.base()
        tr, ev = A.split_rows(base, by_cat, always)
        pin = {"train": [r["id"] for r in tr], "eval": [r["id"] for r in ev]}
        grown = base + [row(f"e-long-{k:03}", "long", f"long opener {k}", turns=6) for k in range(10)]
        _, ev2 = A.split_rows(grown, by_cat, always, pin=pin)
        self.assertEqual(sum(r["category"] == "long" for r in ev2), 1)

    def test_pinned_eval_row_that_became_ineligible_is_an_error(self):
        base = self.base()
        tr, ev = A.split_rows(base, by_cat, always)
        pin = {"train": [r["id"] for r in tr], "eval": [r["id"] for r in ev]}
        leaked = ev[0]["id"]
        with self.assertRaises(ValueError) as cm:
            A.split_rows(base, by_cat, lambda r: r["id"] != leaked, pin=pin)
        self.assertIn(leaked, str(cm.exception))

    def test_pinned_id_missing_is_an_error(self):
        base = self.base()
        tr, ev = A.split_rows(base, by_cat, always)
        pin = {"train": [r["id"] for r in tr] + ["e-gone-001"], "eval": [r["id"] for r in ev]}
        with self.assertRaises(ValueError):
            A.split_rows(base, by_cat, always, pin=pin)


import re
import sweep_dataset as S


class DodgeTests(unittest.TestCase):
    def test_harvested_dodges_fail(self):
        for bad in ["Both, badly, in alternation.",
                    "I sit down every day, which is a different claim.",
                    "Looking at the water, which is a different kind of looking.",
                    "If I could answer that in one breath I should have finished it.",
                    "It is about half done, depending on which half you ask."]:
            self.assertRegex(bad, re.compile(S.DODGE, re.I), bad)

    def test_ordinary_both_passes(self):
        for ok in ["We both like the sea.", "I use both hands for the oars.", "Both of us were soaked."]:
            self.assertIsNone(re.search(S.DODGE, ok, re.I), ok)


class GreetingTests(unittest.TestCase):
    CTX = "spring, clear afternoon, the beach, 0 hearts"

    def test_wrong_time_greeting_fails(self):
        self.assertIsNotNone(S.greeting_mismatch(self.CTX, "Good morning to you!"))
        self.assertIsNotNone(S.greeting_mismatch(self.CTX, "Ah, good evening, @."))

    def test_matching_or_absent_greeting_passes(self):
        self.assertIsNone(S.greeting_mismatch(self.CTX, "Good afternoon, @. The tide is out."))
        self.assertIsNone(S.greeting_mismatch(self.CTX, "Hello! The tide is out."))
        self.assertIsNone(S.greeting_mismatch(self.CTX, "Good day to you."))

    def test_good_night_is_not_a_greeting(self):
        self.assertIsNone(S.greeting_mismatch(self.CTX, "Good night, @. [end]"))


class NameTests(unittest.TestCase):
    def convo(self, player, reply):
        return {"messages": [{"role": "system", "content": "s"}, {"role": "user", "content": player},
                             {"role": "assistant", "content": reply}]}

    def test_unprompted_tier2_name_is_reported(self):
        self.assertEqual(S.unprompted_names(self.convo("hi", "Haley walked her dog past."), "elliott"), ["Haley"])

    def test_player_named_or_tier1_is_fine(self):
        self.assertEqual(S.unprompted_names(self.convo("seen haley?", "Haley passed by."), "elliott"), [])
        self.assertEqual(S.unprompted_names(self.convo("hi", "Willy is out on the pier."), "elliott"), [])

    def test_self_is_not_reported(self):
        self.assertEqual(S.unprompted_names(self.convo("hi", "Elliott, at your service."), "elliott"), [])


class LongTests(unittest.TestCase):
    def test_long_count(self):
        rows = [row("a", "long", "x", turns=6), row("b", "long", "y", turns=10), row("c", "casual", "z", turns=2)]
        self.assertEqual(S.long_count(rows), 2)


import copy
import check_frozen as F


class FrozenTests(unittest.TestCase):
    def setUp(self):
        self.before = [row("e-cas-001", "casual", "hello", turns=2)]

    def test_assistant_only_change_is_counted_not_flagged(self):
        after = copy.deepcopy(self.before)
        after[0]["messages"][2]["content"] = "Hello, @. The tide is out."
        problems, changed = F.compare(self.before, after)
        self.assertEqual(problems, [])
        self.assertEqual(changed, 1)

    def test_player_line_change_is_flagged(self):
        after = copy.deepcopy(self.before)
        after[0]["messages"][1]["content"] = "hi"
        problems, _ = F.compare(self.before, after)
        self.assertTrue(any("e-cas-001" in p for p in problems))

    def test_context_and_turn_count_changes_are_flagged(self):
        a1 = copy.deepcopy(self.before); a1[0]["context"] = "fall, clear morning, the beach, 2 hearts"
        a2 = copy.deepcopy(self.before); a2[0]["messages"] = a2[0]["messages"][:3]
        self.assertTrue(F.compare(self.before, a1)[0])
        self.assertTrue(F.compare(self.before, a2)[0])

    def test_missing_row_is_flagged_and_new_row_is_allowed(self):
        self.assertTrue(F.compare(self.before, [])[0])
        grown = copy.deepcopy(self.before) + [row("e-long-001", "long", "x", turns=6)]
        self.assertEqual(F.compare(self.before, grown)[0], [])


if __name__ == "__main__":
    unittest.main()
