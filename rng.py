"""The engine's random numbers: the same numbers in Python and in Lua (`lua/nightengine/rng.lua`).

xoshiro128** (Blackman and Vigna), seeded with splitmix32. A string seed is hashed with FNV-1a first. Every step
is a 32-bit operation, so the Lua port gives the same numbers exactly. `contract/rng.json` holds test vectors that
both sides check.

    rng = Rng(7)
    rng.randint(1, 6)       # 1 to 6, both ends included
    rng.getrandbits(32)     # a 32-bit int
    Rng("1997:40")          # a string seed
"""

MASK = 0xFFFFFFFF
GOLDEN = 0x9E3779B9


def fnv1a(data: bytes) -> int:
    """The 32-bit FNV-1a hash."""
    h = 0x811C9DC5
    for byte in data:
        h = ((h ^ byte) * 0x01000193) & MASK
    return h


def mix(z: int) -> int:
    """The splitmix32 output function."""
    z = ((z ^ (z >> 16)) * 0x85EBCA6B) & MASK
    z = ((z ^ (z >> 13)) * 0xC2B2AE35) & MASK
    return z ^ (z >> 16)


def rotl(x: int, k: int) -> int:
    return ((x << k) | (x >> (32 - k))) & MASK


class Rng:
    def __init__(self, seed: int | str = 0):
        if isinstance(seed, str):
            seed = fnv1a(seed.encode())
        x = seed & MASK
        self.s = []
        for _ in range(4):
            x = (x + GOLDEN) & MASK
            self.s.append(mix(x))
        if not any(self.s):
            self.s[0] = 1

    def next(self) -> int:
        """The next 32-bit number."""
        s0, s1, s2, s3 = self.s
        result = (rotl((s1 * 5) & MASK, 7) * 9) & MASK
        t = (s1 << 9) & MASK
        s2 ^= s0
        s3 ^= s1
        s1 ^= s2
        s0 ^= s3
        s2 ^= t
        s3 = rotl(s3, 11)
        self.s = [s0, s1, s2, s3]
        return result

    def getrandbits(self, k: int) -> int:
        """A number of `k` bits, 1 to 32."""
        if not 1 <= k <= 32:
            raise ValueError("k must be 1 to 32")
        return self.next() >> (32 - k)

    def randint(self, low: int, high: int) -> int:
        """A number from `low` to `high`, both included. Unbiased: a draw past the last whole range is drawn again."""
        n = high - low + 1
        if not 1 <= n <= 1 << 32:
            raise ValueError(f"empty or too wide range {low}..{high}")
        limit = (1 << 32) - (1 << 32) % n
        while True:
            r = self.next()
            if r < limit:
                return low + r % n

    def random(self) -> float:
        """A float in [0, 1)."""
        return self.next() / (1 << 32)
