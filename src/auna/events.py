from __future__ import annotations

from collections import defaultdict
from typing import Callable, DefaultDict, Generic, TypeVar

T = TypeVar("T")


class EventBus(Generic[T]):
    def __init__(self) -> None:
        self._subscribers: DefaultDict[str, list[Callable[[T], None]]] = defaultdict(list)

    def subscribe(self, event_name: str, callback: Callable[[T], None]) -> None:
        self._subscribers[event_name].append(callback)

    def publish(self, event_name: str, payload: T) -> None:
        for callback in self._subscribers[event_name]:
            callback(payload)
