"""Auna: demo MVP de traductor LSA con eventos."""

from .events import EventBus
from .translator import GenericTranslator

__all__ = ["EventBus", "GenericTranslator"]
