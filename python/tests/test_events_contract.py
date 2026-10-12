"""Both languages preserve the same sparse host event recording."""

import unittest
from pathlib import Path

from nightengine import Recording


class EventsContractTest(unittest.TestCase):
    def test_shared_event_recording_round_trips(self):
        text = (Path(__file__).resolve().parents[2] / "contract/events.json").read_text().strip()
        self.assertEqual(Recording.from_json(text).to_json(), text)
