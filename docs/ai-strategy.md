# AI Strategy Interface – TradingBot.SignalEngine

## Overview
This document defines the strategy interface to be used for signal generation in Python. The interface supports multiple strategies: rule-based, statistical, and AI/ML-powered.

## Strategy Base Class

```python
# tradingbot/strategies/base.py

from abc import ABC, abstractmethod

class Strategy(ABC):
    @abstractmethod
    def generate_signal(self, market_data, **kwargs):
        pass
```

## Example Strategies

### Basic Strategy

```python
class BasicStrategy(Strategy):
    def generate_signal(self, market_data, **kwargs):
        return {"action": "HOLD", "confidence": 0.5, "reason": "No signal triggered."}
```

### Moving Average Crossover

```python
class MovingAverageStrategy(Strategy):
    def generate_signal(self, market_data, **kwargs):
        # Apply SMA crossover logic
        return {"action": "BUY", "confidence": 0.8, "reason": "SMA crossover detected."}
```

## Strategy Loader

```python
def get_strategy(name: str) -> Strategy:
    strategies = {
        "basic": BasicStrategy(),
        "moving_average": MovingAverageStrategy(),
    }
    return strategies.get(name, BasicStrategy())
```

## Roadmap
- ✅ Implement base interface and dummy strategy
- ✅ Pluggable loader pattern
- 🚧 AI-enhanced strategies (OpenAI/GPT-based decision logic)
- 🚧 Unit tests for strategies