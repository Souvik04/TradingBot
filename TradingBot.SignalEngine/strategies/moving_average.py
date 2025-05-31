from .base import TradingStrategy
from typing import Dict, Any

class MovingAverageCrossoverStrategy(TradingStrategy):
    def generate_signal(self, data: Dict[str, Any], **kwargs) -> Dict[str, Any]:
        sma_short = data.get("sma_short")
        sma_long = data.get("sma_long")
        prev_sma_short = data.get("prev_sma_short")
        prev_sma_long = data.get("prev_sma_long")

        risk_reward_ratio = self.config.get("risk_reward_ratio", 1.0)
        expected_risk = data.get("expected_risk")    # e.g., stop loss distance
        expected_reward = data.get("expected_reward")  # e.g., target profit distance

        if None in (sma_short, sma_long, prev_sma_short, prev_sma_long, expected_risk, expected_reward):
            return {"signal": "HOLD", "confidence": 0.0, "reason": "Insufficient data for MA crossover or R/R."}

        rr = expected_reward / expected_risk if expected_risk else 0

        if rr < risk_reward_ratio:
            return {
                "signal": "HOLD",
                "confidence": 0.4,
                "reason": f"Risk/reward ({rr:.2f}) below required ({risk_reward_ratio:.2f})"
            }

        if prev_sma_short <= prev_sma_long and sma_short > sma_long:
            return {
                "signal": "BUY",
                "confidence": 0.85,
                "reason": f"Short MA crossed above Long MA with R/R={rr:.2f}"
            }
        elif prev_sma_short >= prev_sma_long and sma_short < sma_long:
            return {
                "signal": "SELL",
                "confidence": 0.85,
                "reason": f"Short MA crossed below Long MA with R/R={rr:.2f}"
            }
        else:
            return {
                "signal": "HOLD",
                "confidence": 0.65,
                "reason": "No crossover detected."
            }