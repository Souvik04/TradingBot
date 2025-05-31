from .base import TradingStrategy
from typing import Dict, Any

class BasicStrategy(TradingStrategy):
    def generate_signal(self, data: Dict[str, Any], **kwargs) -> Dict[str, Any]:
        # Always returns HOLD, as a basic placeholder.
        return {
            "signal": "HOLD",
            "confidence": 0.5,
            "reason": "Default basic strategy - does not analyze market data."
        }