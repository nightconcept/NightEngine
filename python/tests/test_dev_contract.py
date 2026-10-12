"""`just dev` pairs become host events, and every runtime reads them back the same (tests/fixtures/dev.json)."""

import json
import unittest
from pathlib import Path

from nightengine import Input, dev_event

FIXTURE = json.loads((Path(__file__).resolve().parents[2] / "tests/fixtures/dev.json").read_text())


class DevContractTest(unittest.TestCase):
    def test_pairs_become_events(self):
        self.assertEqual([dev_event(p) for p in FIXTURE["pairs"]], FIXTURE["events"])

    def test_bad_pairs_fail(self):
        for pair in FIXTURE["invalid"]:
            with self.subTest(pair=pair), self.assertRaises(ValueError):
                dev_event(pair)

    def test_input_reads_the_pairs(self):
        self.assertEqual(Input(events=tuple(FIXTURE["frame"])).dev(), FIXTURE["dev"])
        self.assertEqual(Input().dev(), {})


if __name__ == "__main__":
    unittest.main()
