
from fastapi import FastAPI
from pydantic import BaseModel
import yfinance as yf
import pandas_ta as ta

app = FastAPI()

class SignalRequest(BaseModel):
    symbol: str

class TradeSignal(BaseModel):
    symbol: str
    action: str  # Buy or Sell
    target: float
    stop_loss: float

@app.post("/signal", response_model=TradeSignal)
def generate_signal(request: SignalRequest):
    df = yf.download(request.symbol, period="3mo", interval="1d")
    df.ta.sma(length=50, append=True)
    df.ta.sma(length=200, append=True)
    df.ta.rsi(length=14, append=True)

    last = df.iloc[-1]

    if last["SMA_50"] > last["SMA_200"] and last["RSI_14"] < 60:
        return TradeSignal(
            symbol=request.symbol,
            action="Buy",
            target=round(last["Close"] * 1.05, 2),
            stop_loss=round(last["Close"] * 0.97, 2)
        )
    elif last["RSI_14"] > 70:
        return TradeSignal(
            symbol=request.symbol,
            action="Sell",
            target=round(last["Close"] * 0.95, 2),
            stop_loss=round(last["Close"] * 1.03, 2)
        )
    else:
        return TradeSignal(
            symbol=request.symbol,
            action="Hold",
            target=last["Close"],
            stop_loss=last["Close"]
        )
