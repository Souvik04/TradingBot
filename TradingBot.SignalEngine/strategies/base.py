from abc import ABC, abstractmethod
from typing import Dict, Any

class TradingStrategy(ABC):
    def __init__(self, config: Dict[str, Any]):
        self.config = config

    @abstractmethod
    def generate_signal(self, data: Dict[str, Any], **kwargs) -> Dict[str, Any]:
        """
        Generate a trading signal from input market data.

        Args:
            data (dict): Input data (price, indicators, etc.)
            kwargs: Optional parameters (e.g., config, thresholds)

        Returns:
            dict: {
                "signal": "BUY" | "SELL" | "HOLD",
                "confidence": float,
                "reason": str
            }
        """
        pass