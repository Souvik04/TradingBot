FROM python:3.11-slim

WORKDIR /app

# requirements
COPY TradingBot.SignalEngine/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# source
COPY TradingBot.SignalEngine/ .

ENV PYTHONUNBUFFERED=1
EXPOSE 5001
CMD ["python", "app.py"]
