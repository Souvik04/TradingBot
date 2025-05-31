import yfinance as yf
import pandas as pd
import pandas_ta as ta

def generate_signal(stock_symbol: str):
    # Download last 90 days of 30 min data
    df = yf.download(stock_symbol, period="90d", interval="30m")
    if df.empty:
        return {"prediction": "HOLD", "confidence": 0, "reason": "No data found."}

    # Calculate indicators
    df["EMA20"] = ta.ema(df["Close"], length=20)
    df["RSI14"] = ta.rsi(df["Close"], length=14)
    macd = ta.macd(df["Close"])
    df["MACD"] = macd["MACD_12_26_9"]
    df["MACDh"] = macd["MACDh_12_26_9"]

    # Use the last row for analysis
    last = df.iloc[-1]

    reasons = []
    score = 0

    # Sample logic: EMA crossover + RSI + MACD
    if last["Close"] > last["EMA20"]:
        reasons.append("Price above EMA20 (bullish).")
        score += 1
    else:
        reasons.append("Price below EMA20 (bearish).")
        score -= 1

    if last["RSI14"] > 60:
        reasons.append("RSI indicates strength.")
        score += 1
    elif last["RSI14"] < 40:
        reasons.append("RSI indicates weakness.")
        score -= 1

    if last["MACD"] > 0 and last["MACDh"] > 0:
        reasons.append("MACD bullish.")
        score += 1
    elif last["MACD"] < 0 and last["MACDh"] < 0:
        reasons.append("MACD bearish.")
        score -= 1

    if score >= 2:
        prediction = "BUY"
        confidence = min(1.0, 0.7 + 0.1 * (score - 2))  # e.g., 0.7 - 0.9
    elif score <= -2:
        prediction = "SELL"
        confidence = min(1.0, 0.7 + 0.1 * (abs(score) - 2))
    else:
        prediction = "HOLD"
        confidence = 0.5

    return {
        "prediction": prediction,
        "confidence": round(confidence, 2),
        "reason": " ".join(reasons)
    }