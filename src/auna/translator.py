from __future__ import annotations

from dataclasses import dataclass, field
from typing import Generic, TypeVar

T = TypeVar("T")


@dataclass
class GenericTranslator(Generic[T]):
    dictionary: dict[T, str] = field(default_factory=dict)

    def translate(self, symbol: T) -> str:
        return self.dictionary.get(symbol, str(symbol))
