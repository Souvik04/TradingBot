from fastapi import FastAPI
from pydantic import BaseModel
import yfinance as yf
import pandas_ta as ta
import logging
import sys

# Configure console logging (Docker-friendly)
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
    handlers=[logging.StreamHandler(sys.stdout)]
)

app = FastAPI()

class SignalRequest(BaseModel):
    symbol: str

class TradeSignal(BaseModel):
    symbol: str
    action: str  # Buy, Sell, or Hold
    target: float
    stop_loss: float

@app.get("/health", tags=["Health"])
async def health_check():
    return {"status": "ok"}

@app.post("/signal", response_model=TradeSignal)
def generate_signal(request: SignalRequest):
    df = yf.download(request.symbol, period="3mo", interval="1d")
    df.ta.sma(length=50, append=True)
    df.ta.sma(length=200, append=True)
    df.ta.rsi(length=14, append=True)

    last = df.iloc[-1]

    if last["SMA_50"] > last["SMA_200"] and last["RSI_14"] < 60:
        action = "Buy"
        target = round(last["Close"] * 1.05, 2)
        stop_loss = round(last["Close"] * 0.97, 2)
    elif last["RSI_14"] > 70:
        action = "Sell"
        target = round(last["Close"] * 0.95, 2)
        stop_loss = round(last["Close"] * 1.03, 2)
    else:
        action = "Hold"
        target = last["Close"]
        stop_loss = last["Close"]

    logging.info(f"Signal generated for {request.symbol}: {action} (target={target}, stop_loss={stop_loss})")
    return TradeSignal(
        symbol=request.symbol,
        action=action,
        target=target,
        stop_loss=stop_loss
    )