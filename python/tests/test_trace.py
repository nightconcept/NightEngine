"""Both implementations use the shared RNG and canonical state fixtures."""

import json
import unittest
from pathlib import Path

from nightengine.trace import vectors


class ContractTest(unittest.TestCase):
    def test_rng_and_trace_match_shared_vectors(self):
        path = Path(__file__).resolve().parents[2] / "contract" / "rng.json"
        self.assertEqual(vectors(), json.loads(path.read_text()))
