import pandas as pd
from datetime import datetime
import os
import json
from .base import BaseStrategy, TradeSignal

class AIStrategy(BaseStrategy):
    def __init__(self, config=None):
        super().__init__(config)
        self.name = "AIStrategy"
        
        # Default configuration
        self.default_config = {
            "openai_api_key": os.getenv("OPENAI_API_KEY", ""),
            "model": "gpt-3.5-turbo",
            "max_tokens": 500,
            "temperature": 0.3,
            "target_percent": 0.04,   # 4% target
            "stop_loss_percent": 0.025,  # 2.5% stop loss
            "fallback_strategy": "moving_average"
        }
        
        # Merge with provided config
        if config:
            self.default_config.update(config)
        
        # Initialize OpenAI client if API key is available
        self.openai_client = None
        if self.default_config["openai_api_key"]:
            try:
                import openai
                openai.api_key = self.default_config["openai_api_key"]
                self.openai_client = openai
            except ImportError:
                print("OpenAI library not installed. Install with: pip install openai")

    def generate_signal(self, df: pd.DataFrame, symbol: str) -> TradeSignal:
        """
        Generate trading signal using AI analysis combined with technical indicators.
        
        Strategy:
        1. Calculate technical indicators
        2. Prepare market data summary
        3. Use AI to analyze and generate signal
        4. Fallback to technical analysis if AI fails
        """
        # Calculate technical indicators
        df = self.calculate_technical_indicators(df)
        
        # Try AI analysis first
        if self.openai_client:
            try:
                return self._generate_ai_signal(df, symbol)
            except Exception as e:
                print(f"AI analysis failed: {e}. Falling back to technical analysis.")
        
        # Fallback to technical analysis
        return self._generate_technical_signal(df, symbol)

    def _generate_ai_signal(self, df: pd.DataFrame, symbol: str) -> TradeSignal:
        """Generate signal using OpenAI API"""
        # Prepare market data summary
        market_summary = self._prepare_market_summary(df, symbol)
        
        # Create AI prompt
        prompt = self._create_ai_prompt(market_summary, symbol)
        
        # Get AI response
        response = self.openai_client.ChatCompletion.create(
            model=self.default_config["model"],
            messages=[
                {
                    "role": "system",
                    "content": "You are a professional stock market analyst. Analyze the provided market data and provide a trading recommendation."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            max_tokens=self.default_config["max_tokens"],
            temperature=self.default_config["temperature"]
        )
        
        # Parse AI response
        ai_response = response.choices[0].message.content
        signal_data = self._parse_ai_response(ai_response, symbol)
        
        return TradeSignal(**signal_data)

    def _generate_technical_signal(self, df: pd.DataFrame, symbol: str) -> TradeSignal:
        """Fallback technical analysis signal"""
        # Use moving average strategy as fallback
        from .moving_average import MovingAverageStrategy
        
        fallback_strategy = MovingAverageStrategy()
        return fallback_strategy.generate_signal(df, symbol)

    def _prepare_market_summary(self, df: pd.DataFrame, symbol: str) -> dict:
        """Prepare market data summary for AI analysis"""
        current = df.iloc[-1]
        previous = df.iloc[-2] if len(df) > 1 else current
        
        # Calculate key metrics
        price_change = (current['Close'] - previous['Close']) / previous['Close'] * 100
        volume_ratio = current['Volume'] / df['Volume'].rolling(window=10).mean().iloc[-1]
        
        # Price trends
        sma_20_trend = "bullish" if current['SMA_20'] > previous['SMA_20'] else "bearish"
        sma_50_trend = "bullish" if current['SMA_50'] > previous['SMA_50'] else "bearish"
        
        # RSI interpretation
        rsi = current['RSI_14']
        if rsi > 70:
            rsi_status = "overbought"
        elif rsi < 30:
            rsi_status = "oversold"
        else:
            rsi_status = "neutral"
        
        return {
            "symbol": symbol,
            "current_price": float(current['Close']),
            "price_change_percent": round(price_change, 2),
            "volume_ratio": round(volume_ratio, 2),
            "rsi": round(rsi, 2),
            "rsi_status": rsi_status,
            "sma_20": float(current['SMA_20']),
            "sma_50": float(current['SMA_50']),
            "sma_20_trend": sma_20_trend,
            "sma_50_trend": sma_50_trend,
            "macd": float(current['MACD']),
            "macd_signal": float(current['MACD_Signal']),
            "bb_position": "upper" if current['Close'] > current['BB_Upper'] else "lower" if current['Close'] < current['BB_Lower'] else "middle"
        }

    def _create_ai_prompt(self, market_summary: dict, symbol: str) -> str:
        """Create AI prompt for market analysis"""
        return f"""
Analyze the following market data for {symbol} and provide a trading recommendation:

Market Data:
- Current Price: ${market_summary['current_price']}
- Price Change: {market_summary['price_change_percent']}%
- Volume Ratio: {market_summary['volume_ratio']}x average
- RSI: {market_summary['rsi']} ({market_summary['rsi_status']})
- SMA 20: ${market_summary['sma_20']} ({market_summary['sma_20_trend']} trend)
- SMA 50: ${market_summary['sma_50']} ({market_summary['sma_50_trend']} trend)
- MACD: {market_summary['macd']:.4f}
- MACD Signal: {market_summary['macd_signal']:.4f}
- Bollinger Band Position: {market_summary['bb_position']}

Please provide your analysis in the following JSON format:
{{
    "action": "Buy|Sell|Hold",
    "confidence": 0.0-1.0,
    "reasoning": "Detailed explanation of your decision",
    "target_price": float,
    "stop_loss_price": float
}}

Focus on:
1. Technical indicator alignment
2. Volume confirmation
3. Risk/reward ratio
4. Market momentum
"""

    def _parse_ai_response(self, ai_response: str, symbol: str) -> dict:
        """Parse AI response and extract signal data"""
        try:
            # Try to extract JSON from response
            start_idx = ai_response.find('{')
            end_idx = ai_response.rfind('}') + 1
            json_str = ai_response[start_idx:end_idx]
            
            ai_data = json.loads(json_str)
            
            # Validate and convert to TradeSignal format
            action = ai_data.get('action', 'Hold').capitalize()
            confidence = float(ai_data.get('confidence', 0.5))
            reasoning = ai_data.get('reasoning', 'AI analysis')
            target_price = float(ai_data.get('target_price', 0))
            stop_loss_price = float(ai_data.get('stop_loss_price', 0))
            
            return {
                "symbol": symbol,
                "action": action,
                "target": target_price,
                "stop_loss": stop_loss_price,
                "confidence": confidence,
                "strategy": self.name,
                "reasoning": reasoning,
                "timestamp": datetime.now().isoformat()
            }
            
        except (json.JSONDecodeError, KeyError, ValueError) as e:
            # Fallback to technical analysis if AI response parsing fails
            print(f"Failed to parse AI response: {e}")
            return self._generate_technical_signal(pd.DataFrame(), symbol).dict()