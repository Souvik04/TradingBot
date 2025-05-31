import json
import os
from jsonschema import validate, ValidationError

def load_strategy_config(json_path: str, schema_path: str) -> dict:
    with open(schema_path, "r") as schf:
        schema = json.load(schf)
    with open(json_path, "r") as f:
        config = json.load(f)
    try:
        validate(instance=config, schema=schema)
    except ValidationError as ve:
        raise ValueError(f"Invalid strategy config: {ve.message}")
    return config

# Example usage:
if __name__ == "__main__":
    config = load_strategy_config(
        os.path.join(os.path.dirname(__file__), "sample_moving_average_strategy.json"),
        os.path.join(os.path.dirname(__file__), "strategy.schema.json")
    )
    print("Loaded strategy config:", config)
