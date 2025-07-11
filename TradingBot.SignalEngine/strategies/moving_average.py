import pandas as pd
from datetime import datetime
from .base import BaseStrategy, TradeSignal

class MovingAverageStrategy(BaseStrategy):
    def __init__(self, config=None):
        super().__init__(config)
        self.name = "MovingAverageStrategy"
        
        # Default configuration
        self.default_config = {
            "short_period": 20,
            "long_period": 50,
            "rsi_period": 14,
            "rsi_oversold": 30,
            "rsi_overbought": 70,
            "volume_threshold": 1.5,  # Volume should be 1.5x average
            "target_percent": 0.05,   # 5% target
            "stop_loss_percent": 0.03  # 3% stop loss
        }
        
        # Merge with provided config
        if config:
            self.default_config.update(config)

    def generate_signal(self, df: pd.DataFrame, symbol: str) -> TradeSignal:
        """
        Generate trading signal based on moving average crossover and RSI.
        
        Strategy:
        - Buy: Short MA > Long MA AND RSI < 70 AND Volume > Average
        - Sell: RSI > 70 OR Short MA < Long MA
        - Hold: Otherwise
        """
        # Calculate technical indicators
        df = self.calculate_technical_indicators(df)
        
        # Get latest data
        current = df.iloc[-1]
        previous = df.iloc[-2] if len(df) > 1 else current
        
        # Get configuration
        short_period = self.default_config["short_period"]
        long_period = self.default_config["long_period"]
        rsi_period = self.default_config["rsi_period"]
        rsi_oversold = self.default_config["rsi_oversold"]
        rsi_overbought = self.default_config["rsi_overbought"]
        volume_threshold = self.default_config["volume_threshold"]
        
        # Calculate average volume
        avg_volume = df['Volume'].rolling(window=20).mean().iloc[-1]
        current_volume = current['Volume']
        
        # Get moving averages
        short_ma = current[f'SMA_{short_period}']
        long_ma = current[f'SMA_{long_period}']
        rsi = current['RSI_14']
        
        # Previous values for crossover detection
        prev_short_ma = previous[f'SMA_{short_period}']
        prev_long_ma = previous[f'SMA_{long_period}']
        
        # Decision logic
        action = "Hold"
        confidence = 0.5
        reasoning = []
        
        # Check for buy signal
        if (short_ma > long_ma and  # Golden cross
            rsi < rsi_overbought and  # Not overbought
            current_volume > avg_volume * volume_threshold):  # High volume
            
            # Check for crossover (bullish)
            if prev_short_ma <= prev_long_ma and short_ma > long_ma:
                action = "Buy"
                confidence = 0.8
                reasoning.append("Golden cross detected")
            elif rsi < rsi_oversold:
                action = "Buy"
                confidence = 0.7
                reasoning.append("Oversold condition with bullish MA")
            else:
                action = "Buy"
                confidence = 0.6
                reasoning.append("Bullish MA alignment with good volume")
        
        # Check for sell signal
        elif (rsi > rsi_overbought or  # Overbought
              (short_ma < long_ma and prev_short_ma >= prev_long_ma)):  # Death cross
            
            if short_ma < long_ma and prev_short_ma >= prev_long_ma:
                action = "Sell"
                confidence = 0.8
                reasoning.append("Death cross detected")
            elif rsi > rsi_overbought:
                action = "Sell"
                confidence = 0.7
                reasoning.append("Overbought condition")
            else:
                action = "Sell"
                confidence = 0.6
                reasoning.append("Bearish MA alignment")
        
        # Hold conditions
        else:
            if short_ma > long_ma:
                reasoning.append("Bullish MA but waiting for better entry")
            else:
                reasoning.append("Bearish MA but waiting for confirmation")
        
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