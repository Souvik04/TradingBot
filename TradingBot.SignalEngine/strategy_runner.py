from strategies import STRATEGY_REGISTRY
from strategies.config_loader import load_strategy_config
import os

if __name__ == "__main__":
    # Load config with risk_reward_ratio
    config_path = os.path.join("strategies", "sample_moving_average_strategy.json")
    schema_path = os.path.join("strategies", "strategy.schema.json")
    config = load_strategy_config(config_path, schema_path)

    strategy = STRATEGY_REGISTRY["moving_average"](config)
    sample_data = {
        "price": 150.0,
        "sma_short": 151.0,
        "sma_long": 150.0,
        "prev_sma_short": 149.0,
        "prev_sma_long": 150.5,
        "expected_risk": 5,
        "expected_reward": 12
    }
    print(strategy.generate_signal(sample_data))