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


if __name__ == "__main__":
    unittest.main()
