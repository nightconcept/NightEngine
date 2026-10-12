"""A game's justfile exposes the standard surface and nothing more."""

import shutil
import tempfile
import unittest
from pathlib import Path

from nightengine.recipes import check


def recipe(doc="Does a thing.", params=(), private=False):
    return {"doc": doc, "private": private, "parameters": [{"name": n, "kind": k} for n, k in params]}


def standard(**changes):
    recipes = {
        "setup": recipe(),
        "check": recipe(),
        "lint": recipe(),
        "fmt": recipe(),
        "run": recipe(params=[("args", "star")]),
        "dev": recipe(doc="Start a state: level=N", params=[("args", "star")]),
    }
    recipes.update(changes)
    return {"recipes": {k: v for k, v in recipes.items() if v is not None}, "aliases": {}, "modules": {}}


class SurfaceTest(unittest.TestCase):
    def test_standard_surface_passes(self):
        self.assertEqual(check.problems(standard()), [])

    def test_optional_and_private_recipes_pass(self):
        dump = standard(
            build=recipe(params=[("targets", "star")]),
            replay=recipe(params=[("file", "singular")]),
            _helper=recipe(doc=None, private=True),
        )
        self.assertEqual(check.problems(dump), [])

    def test_missing_required_recipe_fails(self):
        self.assertIn("missing recipe `dev`", check.problems(standard(dev=None)))

    def test_extra_public_recipe_fails(self):
        [problem] = check.problems(standard(**{"mg-run": recipe()}))
        self.assertIn("`mg-run` is not a standard recipe", problem)

    def test_run_must_be_variadic(self):
        [problem] = check.problems(standard(run=recipe(params=[("seed", "singular")])))
        self.assertIn("variadic", problem)

    def test_public_recipes_need_docs(self):
        [problem] = check.problems(standard(lint=recipe(doc=None)))
        self.assertIn("doc comment", problem)

    def test_dev_doc_lists_keys(self):
        [problem] = check.problems(standard(dev=recipe(doc="Developer mode.", params=[("args", "star")])))
        self.assertIn("must list its keys", problem)

    def test_aliases_and_modules_fail(self):
        dump = standard()
        dump["aliases"] = {"r": {"target": "run"}}
        dump["modules"] = {"mg": {}}
        self.assertEqual(len(check.problems(dump)), 2)

    @unittest.skipIf(shutil.which("just") is None, "just is not installed")
    def test_reads_a_real_justfile(self):
        with tempfile.TemporaryDirectory() as tmp:
            Path(tmp, "justfile").write_text(
                "# Install.\nsetup:\n\n# Test.\ncheck:\n\n# Lint.\nlint:\n\n# Format.\nfmt:\n\n"
                "# Play.\nrun *args:\n\n# Start a state: level=N.\ndev *args:\n\n_helper:\n\n# Extra.\nextra:\n"
            )
            found = check.problems(check.dump(Path(tmp)))
        self.assertEqual(len(found), 1)
        self.assertIn("`extra`", found[0])


if __name__ == "__main__":
    unittest.main()
