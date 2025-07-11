# TradingBot

**TradingBot** is an AI-powered automated trading system for the Indian stock market, designed to work with brokers like Zerodha and Groww. It consists of a C# API backend and a Python-based signal engine, connected via Docker, and is highly configurable and extendable.

---

## 🚀 Features

- 📈 AI-driven signal generation (Python, OpenAI API)
- 💼 Execute buy/sell orders via broker APIs (e.g., Kite Connect)
- 🔁 Configurable modes: Auto trade / Signal-only
- ⚙️ Trading strategy options: intraday, long-term, swing, options, etc.
- 🔒 Risk management: daily limits, trade caps, risk/reward ratio
- 🔌 Modular architecture for plug-in strategy and AI service integration
- 🧪 Local testing and Docker support for development

---

## 🗂️ Project Structure

```bash
TradingBot/
├── TradingBot.Api/           # C# ASP.NET Core backend (REST API)
├── TradingBot.SignalEngine/  # Python signal engine (AI, ML logic)
├── TradingBot.Tests/         # C# Unit Tests
├── docker-compose.yml        # Orchestrates multi-container dev environment
├── docs/                     # Project documentation
└── README.md                 # This file
```

---

## 🛠️ Quick Start

### Prerequisites

- Docker and Docker Compose
- .NET 8.0 SDK (for local development)
- Python 3.11+ (for local development)

### 1. Clone and Setup

```bash
git clone <repository-url>
cd TradingBot
```

### 2. Environment Setup

Create a `.env` file in the root directory:

```bash
# Optional: OpenAI API Key for AI strategy
OPENAI_API_KEY=your_openai_api_key_here

# Optional: Database connection (if using external DB)
DB_CONNECTION_STRING=your_db_connection_string
```

### 3. Run with Docker

```bash
# Build and start all services
docker-compose up --build

# Or run in background
docker-compose up -d --build
```

### 4. Access Services

- **API Documentation**: http://localhost:5000/swagger
- **Signal Engine**: http://localhost:8000/docs
- **API Health**: http://localhost:5000/health
- **Signal Engine Health**: http://localhost:8000/health

---

## 📖 API Usage

### Get Trading Configuration

```bash
curl http://localhost:5000/api/trade/config
```

### Generate Trading Signals

```bash
curl -X POST http://localhost:5000/api/trade/signal \
  -H "Content-Type: application/json" \
  -d '{
    "symbols": ["AAPL", "MSFT"],
    "strategy": "moving_average",
    "tradeMode": "paper"
  }'
```

### Execute Trade

```bash
curl -X POST http://localhost:5000/api/trade/execute \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "AAPL",
    "quantity": 10,
    "price": 150.00,
    "side": "buy",
    "orderType": "market",
    "tradeType": "intraday"
  }'
```

### Get Portfolio

```bash
curl http://localhost:5000/api/portfolio
```

---

## 🧠 Signal Engine Strategies

The Python signal engine supports multiple trading strategies:

### 1. Moving Average Strategy
- Uses SMA crossover with RSI confirmation
- Volume-based validation
- Configurable periods and thresholds

### 2. Basic Strategy
- Simple price action analysis
- Volume confirmation
- RSI overbought/oversold signals

### 3. AI Strategy
- OpenAI GPT integration for advanced analysis
- Technical indicator synthesis
- Fallback to technical analysis

### Using Different Strategies

```bash
# Moving Average Strategy
curl -X POST http://localhost:8000/signal \
  -H "Content-Type: application/json" \
  -d '{"symbol": "AAPL", "strategy": "moving_average"}'

# AI Strategy (requires OpenAI API key)
curl -X POST http://localhost:8000/signal \
  -H "Content-Type: application/json" \
  -d '{"symbol": "AAPL", "strategy": "ai"}'

# Basic Strategy
curl -X POST http://localhost:8000/signal \
  -H "Content-Type: application/json" \
  -d '{"symbol": "AAPL", "strategy": "basic"}'
```

---

## ⚙️ Configuration

### Trading Settings

Edit `TradingBot.Api/appsettings.Development.json`:

```json
{
  "TradeSettings": {
    "EnableAutoBuy": true,
    "EnableAutoSell": true,
    "EnableSignalOnly": false,
    "MaxDailyBuyAmount": 10000.00,
    "MaxDailySellAmount": 10000.00,
    "MaxDailyTrades": 10,
    "TradeTypesEnabled": ["Intraday", "Swing", "LongTerm"]
  }
}
```

### Strategy Configuration

Each strategy can be configured with custom parameters. See individual strategy files in `TradingBot.SignalEngine/strategies/`.

---

## 🔧 Development

### Local Development Setup

1. **C# API Development**:
   ```bash
   cd TradingBot.Api
   dotnet restore
   dotnet run
   ```

2. **Python Signal Engine Development**:
   ```bash
   cd TradingBot.SignalEngine
   pip install -r requirements.txt
   uvicorn main:app --reload --host 0.0.0.0 --port 8000
   ```

### Adding New Strategies

1. Create a new strategy class in `TradingBot.SignalEngine/strategies/`
2. Extend `BaseStrategy` class
3. Implement `generate_signal()` method
4. Register in `main.py` STRATEGIES dictionary

Example:
```python
from .base import BaseStrategy, TradeSignal

class MyCustomStrategy(BaseStrategy):
    def generate_signal(self, df: pd.DataFrame, symbol: str) -> TradeSignal:
        # Your custom logic here
        return TradeSignal(...)
```

---

## 🧪 Testing

### Run C# Tests
```bash
cd TradingBot.Tests
dotnet test
```

### Test Signal Engine
```bash
# Test health endpoint
curl http://localhost:8000/health

# Test signal generation
curl -X POST http://localhost:8000/signal \
  -H "Content-Type: application/json" \
  -d '{"symbol": "AAPL"}'
```

---

## 📊 Monitoring

### Health Checks
- API: `GET /health`
- Signal Engine: `GET /health`

### Logs
```bash
# View all logs
docker-compose logs

# View specific service logs
docker-compose logs tradingbot-api
docker-compose logs signal-engine
```

---

## 🚀 Deployment

### Production Deployment

1. **Azure Container Registry**:
   ```bash
   # Build and push images
   docker build -t yourregistry.azurecr.io/tradingbot-api .
   docker build -t yourregistry.azurecr.io/tradingbot-signal-engine .
   docker push yourregistry.azurecr.io/tradingbot-api
   docker push yourregistry.azurecr.io/tradingbot-signal-engine
   ```

2. **Azure Kubernetes Service (AKS)**:
   - Deploy using provided Kubernetes manifests
   - Configure secrets for API keys
   - Set up monitoring and logging

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `OPENAI_API_KEY` | OpenAI API key for AI strategy | No |
| `DB_CONNECTION_STRING` | Database connection string | Yes (prod) |
| `BROKER_API_KEY` | Broker API credentials | Yes (live) |

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

---

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 🆘 Support

- **Documentation**: Check the `docs/` folder
- **Issues**: Create an issue on GitHub
- **Discussions**: Use GitHub Discussions

---

## 🗺️ Roadmap

See [docs/roadmap.md](docs/roadmap.md) for detailed roadmap and upcoming features.