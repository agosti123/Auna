import unittest

from auna.events import EventBus


class EventBusTests(unittest.TestCase):
    def test_publish_subscribe(self) -> None:
        bus: EventBus[str] = EventBus()
        received: list[str] = []

        bus.subscribe("symbol_detected", received.append)
        bus.publish("symbol_detected", "HOLA")

        self.assertEqual(received, ["HOLA"])


if __name__ == "__main__":
    unittest.main()
