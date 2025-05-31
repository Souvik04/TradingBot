## Strategy Architecture

All trading strategies must inherit from `TradingStrategy` (defined in `strategies/base.py`).

Each strategy implements `generate_signal(data: dict) -> dict`.

Sample return format:
```json
{
  "signal": "BUY",
  "confidence": 0.87,
  "reason": "Price crossed above SMA"
}
```

**Built-in strategies:**
- BasicStrategy: Always returns HOLD.
- MovingAverageCrossoverStrategy: Implements classic moving average crossover logic.
- AIEnhancedStrategy: Simulates an AI/ML-based signal.

To add a new strategy:

* Create a file in `strategies/`
* Implement the `generate_signal` method
* Register in `STRATEGY_REGISTRY` in `__init__.py`

**Example usage:**
```python
from strategies import STRATEGY_REGISTRY

strategy = STRATEGY_REGISTRY["moving_average"]()
result = strategy.generate_signal({
    "price": 150,
    "sma_short": 151,
    "sma_long": 150,
    "prev_sma_short": 149,
    "prev_sma_long": 150.5
})
print(result)
```