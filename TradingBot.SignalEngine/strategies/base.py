from abc import ABC, abstractmethod
from typing import Dict, Any
import pandas as pd
from datetime import datetime
from pydantic import BaseModel

class TradeSignal(BaseModel):
    symbol: str
    action: str  # Buy, Sell, or Hold
    target: float
    stop_loss: float
    confidence: float
    strategy: str
    reasoning: str
    timestamp: str

class BaseStrategy(ABC):
    def __init__(self, config: Dict[str, Any] = None):
        self.config = config or {}
        self.name = self.__class__.__name__

    @abstractmethod
    def generate_signal(self, df: pd.DataFrame, symbol: str) -> TradeSignal:
        """
        Generate a trading signal from input market data.

        Args:
            df (pd.DataFrame): Market data with OHLCV columns
            symbol (str): Stock symbol

        Returns:
            TradeSignal: Signal object with action, confidence, reasoning, etc.
        """
        pass

    def calculate_technical_indicators(self, df: pd.DataFrame) -> pd.DataFrame:
        """
        Calculate common technical indicators for the strategy.
        
        Args:
            df (pd.DataFrame): Market data
            
        Returns:
            pd.DataFrame: Data with technical indicators added
        """
        # Add common indicators
        df = df.copy()
        
        # Moving averages
        df['SMA_20'] = df['Close'].rolling(window=20).mean()
        df['SMA_50'] = df['Close'].rolling(window=50).mean()
        df['SMA_200'] = df['Close'].rolling(window=200).mean()
        
        # RSI
        df['RSI_14'] = self._calculate_rsi(df['Close'], 14)
        
        # MACD
        df['MACD'], df['MACD_Signal'], df['MACD_Histogram'] = self._calculate_macd(df['Close'])
        
        # Bollinger Bands
        df['BB_Upper'], df['BB_Middle'], df['BB_Lower'] = self._calculate_bollinger_bands(df['Close'])
        
        return df

    def _calculate_rsi(self, prices: pd.Series, period: int = 14) -> pd.Series:
        """Calculate RSI indicator"""
        delta = prices.diff()
        gain = (delta.where(delta > 0, 0)).rolling(window=period).mean()
        loss = (-delta.where(delta < 0, 0)).rolling(window=period).mean()
        rs = gain / loss
        rsi = 100 - (100 / (1 + rs))
        return rsi

    def _calculate_macd(self, prices: pd.Series, fast: int = 12, slow: int = 26, signal: int = 9) -> tuple:
        """Calculate MACD indicator"""
        ema_fast = prices.ewm(span=fast).mean()
        ema_slow = prices.ewm(span=slow).mean()
        macd = ema_fast - ema_slow
        macd_signal = macd.ewm(span=signal).mean()
        macd_histogram = macd - macd_signal
        return macd, macd_signal, macd_histogram

    def _calculate_bollinger_bands(self, prices: pd.Series, period: int = 20, std_dev: int = 2) -> tuple:
        """Calculate Bollinger Bands"""
        sma = prices.rolling(window=period).mean()
        std = prices.rolling(window=period).std()
        upper_band = sma + (std * std_dev)
        lower_band = sma - (std * std_dev)
        return upper_band, sma, lower_band

    def _get_current_price(self, df: pd.DataFrame) -> float:
        """Get the current (latest) price"""
        return float(df['Close'].iloc[-1])

    def _calculate_target_and_stop_loss(self, current_price: float, action: str, 
                                      target_percent: float = 0.05, 
                                      stop_loss_percent: float = 0.03) -> tuple:
        """Calculate target and stop loss prices"""
        if action == "Buy":
            target = current_price * (1 + target_percent)
            stop_loss = current_price * (1 - stop_loss_percent)
        elif action == "Sell":
            target = current_price * (1 - target_percent)
            stop_loss = current_price * (1 + stop_loss_percent)
        else:  # Hold
            target = current_price
            stop_loss = current_price
            
        return round(target, 2), round(stop_loss, 2)