import unittest

from auna.translator import GenericTranslator


class GenericTranslatorTests(unittest.TestCase):
    def test_translate_known_symbol(self) -> None:
        translator = GenericTranslator[str]({"HOLA": "Hola"})
        self.assertEqual(translator.translate("HOLA"), "Hola")

    def test_translate_unknown_symbol_falls_back_to_symbol(self) -> None:
        translator = GenericTranslator[str]({"HOLA": "Hola"})
        self.assertEqual(translator.translate("XYZ"), "XYZ")


if __name__ == "__main__":
    unittest.main()
