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
├── REQUIREMENTS.md           # Functional & Non-functional requirements
├── ARCHITECTURE.md           # System architecture overview
