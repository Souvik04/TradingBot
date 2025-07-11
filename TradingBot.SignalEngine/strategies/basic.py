import pandas as pd
from datetime import datetime
from .base import BaseStrategy, TradeSignal

class BasicStrategy(BaseStrategy):
    def __init__(self, config=None):
        super().__init__(config)
        self.name = "BasicStrategy"
        
        # Default configuration
        self.default_config = {
            "price_change_threshold": 0.02,  # 2% price change
            "volume_threshold": 1.2,         # 20% above average volume
            "target_percent": 0.03,          # 3% target
            "stop_loss_percent": 0.02        # 2% stop loss
        }
        
        # Merge with provided config
        if config:
            self.default_config.update(config)

    def generate_signal(self, df: pd.DataFrame, symbol: str) -> TradeSignal:
        """
        Generate trading signal based on simple price action and volume.
        
        Strategy:
        - Buy: Price up > threshold AND Volume > average
        - Sell: Price down > threshold OR RSI > 70
        - Hold: Otherwise
        """
        # Calculate technical indicators
        df = self.calculate_technical_indicators(df)
        
        # Get latest data
        current = df.iloc[-1]
        previous = df.iloc[-2] if len(df) > 1 else current
        
        # Get configuration
        price_threshold = self.default_config["price_change_threshold"]
        volume_threshold = self.default_config["volume_threshold"]
        
        # Calculate price change
        price_change = (current['Close'] - previous['Close']) / previous['Close']
        price_change_percent = price_change * 100
        
        # Calculate volume ratio
        avg_volume = df['Volume'].rolling(window=10).mean().iloc[-1]
        volume_ratio = current['Volume'] / avg_volume if avg_volume > 0 else 1
        
        # Get RSI
        rsi = current['RSI_14']
        
        # Decision logic
        action = "Hold"
        confidence = 0.5
        reasoning = []
        
        # Check for buy signal
        if (price_change > price_threshold and  # Price up significantly
            volume_ratio > volume_threshold and  # High volume
            rsi < 70):  # Not overbought
            
            action = "Buy"
            confidence = 0.7
            reasoning.append(f"Price up {price_change_percent:.1f}% with high volume")
        
        # Check for sell signal
        elif (price_change < -price_threshold or  # Price down significantly
              rsi > 70):  # Overbought
            
            if price_change < -price_threshold:
                action = "Sell"
                confidence = 0.7
                reasoning.append(f"Price down {abs(price_change_percent):.1f}%")
            else:
                action = "Sell"
                confidence = 0.6
                reasoning.append("RSI overbought")
        
        # Hold conditions
        else:
            if abs(price_change) < price_threshold:
                reasoning.append("Price change below threshold")
            if volume_ratio < volume_threshold:
                reasoning.append("Volume below threshold")
            if 30 <= rsi <= 70:
                reasoning.append("RSI in neutral range")
        
        # Calculate target and stop loss
        current_price = self._get_current_price(df)
        target, stop_loss = self._calculate_target_and_stop_loss(
            current_price, 
            action,
            self.default_config["target_percent"],
            self.default_config["stop_loss_percent"]
        )
        
        # Create reasoning string
        reasoning_str = "; ".join(reasoning)
        
        return TradeSignal(
            symbol=symbol,
            action=action,
            target=target,
            stop_loss=stop_loss,
            confidence=confidence,
            strategy=self.name,
            reasoning=reasoning_str,
            timestamp=datetime.now().isoformat()
        )