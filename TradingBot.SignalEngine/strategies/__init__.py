from .basic import BasicStrategy
from .moving_average import MovingAverageCrossoverStrategy
from .ai_strategy import AIEnhancedStrategy

STRATEGY_REGISTRY = {
    "basic": BasicStrategy,
    "moving_average": MovingAverageCrossoverStrategy,
    "ai": AIEnhancedStrategy
}