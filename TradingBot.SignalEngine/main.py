from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import yfinance as yf
import pandas_ta as ta
import logging
import sys
from typing import List, Optional
import json
from strategies.base import BaseStrategy
from strategies.moving_average import MovingAverageStrategy
from strategies.ai_strategy import AIStrategy
from strategies.basic import BasicStrategy

# Configure console logging (Docker-friendly)
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
    handlers=[logging.StreamHandler(sys.stdout)]
)

logger = logging.getLogger(__name__)
app = FastAPI(title="TradingBot Signal Engine", version="1.0.0")

class SignalRequest(BaseModel):
    symbol: str
    strategy: Optional[str] = "moving_average"
    timeframe: Optional[str] = "1d"
    period: Optional[str] = "3mo"

class TradeSignal(BaseModel):
    symbol: str
    action: str  # Buy, Sell, or Hold
    target: float
    stop_loss: float
    confidence: float
    strategy: str
    reasoning: str
    timestamp: str

class BatchSignalRequest(BaseModel):
    symbols: List[str]
    strategy: Optional[str] = "moving_average"
    timeframe: Optional[str] = "1d"
    period: Optional[str] = "3mo"

class BatchSignalResponse(BaseModel):
    signals: List[TradeSignal]
    summary: dict

# Strategy registry
STRATEGIES = {
    "moving_average": MovingAverageStrategy(),
    "ai": AIStrategy(),
    "basic": BasicStrategy()
}

@app.get("/health", tags=["Health"])
async def health_check():
    return {"status": "ok", "service": "signal-engine", "version": "1.0.0"}

@app.get("/strategies", tags=["Strategies"])
async def get_available_strategies():
    return {
        "available_strategies": list(STRATEGIES.keys()),
        "default_strategy": "moving_average"
    }

@app.post("/signal", response_model=TradeSignal, tags=["Signals"])
def generate_signal(request: SignalRequest):
    try:
        logger.info(f"Generating signal for {request.symbol} using {request.strategy} strategy")
        
        # Get strategy
        strategy = STRATEGIES.get(request.strategy)
        if not strategy:
            raise HTTPException(status_code=400, detail=f"Strategy '{request.strategy}' not found")
        
        # Download market data
        ticker = yf.Ticker(request.symbol)
        df = ticker.history(period=request.period, interval=request.timeframe)
        
        if df.empty:
            raise HTTPException(status_code=404, detail=f"No data found for symbol {request.symbol}")
        
        # Generate signal using the strategy
        signal = strategy.generate_signal(df, request.symbol)
        
        logger.info(f"Signal generated for {request.symbol}: {signal.action} (confidence={signal.confidence})")
        return signal
        
    except Exception as e:
        logger.error(f"Error generating signal for {request.symbol}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error generating signal: {str(e)}")

@app.post("/signals/batch", response_model=BatchSignalResponse, tags=["Signals"])
def generate_batch_signals(request: BatchSignalRequest):
    try:
        logger.info(f"Generating batch signals for {len(request.symbols)} symbols using {request.strategy} strategy")
        
        signals = []
        successful = 0
        failed = 0
        
        for symbol in request.symbols:
            try:
                signal = generate_signal(SignalRequest(
                    symbol=symbol,
                    strategy=request.strategy,
                    timeframe=request.timeframe,
                    period=request.period
                ))
                signals.append(signal)
                successful += 1
            except Exception as e:
                logger.warning(f"Failed to generate signal for {symbol}: {str(e)}")
                failed += 1
        
        summary = {
            "total_requested": len(request.symbols),
            "successful": successful,
            "failed": failed,
            "strategy_used": request.strategy
        }
        
        return BatchSignalResponse(signals=signals, summary=summary)
        
    except Exception as e:
        logger.error(f"Error generating batch signals: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error generating batch signals: {str(e)}")

@app.get("/market-data/{symbol}", tags=["Market Data"])
def get_market_data(symbol: str, period: str = "1mo", interval: str = "1d"):
    try:
        ticker = yf.Ticker(symbol)
        df = ticker.history(period=period, interval=interval)
        
        if df.empty:
            raise HTTPException(status_code=404, detail=f"No data found for symbol {symbol}")
        
        # Convert to JSON-serializable format
        data = {
            "symbol": symbol,
            "period": period,
            "interval": interval,
            "data_points": len(df),
            "latest_price": float(df['Close'].iloc[-1]),
            "price_change": float(df['Close'].iloc[-1] - df['Close'].iloc[0]),
            "price_change_percent": float(((df['Close'].iloc[-1] - df['Close'].iloc[0]) / df['Close'].iloc[0]) * 100),
            "high": float(df['High'].max()),
            "low": float(df['Low'].min()),
            "volume": int(df['Volume'].sum())
        }
        
        return data
        
    except Exception as e:
        logger.error(f"Error fetching market data for {symbol}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error fetching market data: {str(e)}")

@app.get("/", tags=["Root"])
async def root():
    return {
        "message": "TradingBot Signal Engine",
        "version": "1.0.0",
        "endpoints": {
            "health": "/health",
            "strategies": "/strategies",
            "single_signal": "/signal",
            "batch_signals": "/signals/batch",
            "market_data": "/market-data/{symbol}"
        }
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)