# TradingBot System Architecture

## Overview

The TradingBot is an AI‑powered, auto‑trading platform built for the Indian stock market. It integrates with brokers like Zerodha and Groww and aims to maximize profit with minimal human intervention through configurable, intelligent decision‑making.

The system is modular, with two primary components:

* **`TradingBot.Api` (C#)** – Orchestration layer that exposes REST APIs, enforces risk limits, and places orders.
* **`TradingBot.SignalEngine` (Python)** – Machine‑learning signal engine that produces buy/sell/hold recommendations.

---

## High‑Level Architecture

```
           +---------------------------+
           |     User Configuration    |
           +---------------------------+
                        |
                        v
              +------------------+
              |  TradingBot.Api  |  (C#, ASP.NET Core)
              +------------------+
              |  - REST APIs      |
              |  - Trade Manager  |-------------------+
              |  - Config Store   |                   |
              +------------------+                   |
                        |                            |
                        v                            v
              +------------------+         +---------------------------+
              |  Zerodha/Groww   |         |  TradingBot.SignalEngine  |
              |     Broker API   |         |   (Python, FastAPI)       |
              +------------------+         +---------------------------+
                                             |  - ML Models
                                             |  - Technical Indicators
                                             |  - Risk/Reward Logic
                                             +---------------------------+
```

---

## Key Components

### 1. TradingBot.Api (C#)
* ASP.NET Core REST API
* Manages configuration, risk checks, and order routing
* Calls Python engine for trading signals
* Stores user preferences and trade history

### 2. TradingBot.SignalEngine (Python)
* FastAPI micro‑service
* Computes technical indicators with TA‑Lib / Pandas
* Runs ML or rule‑based models to emit signals
* Can be extended with LLM sentiment or reinforcement learning

---

## Data Flow

1. User adjusts settings in `TradingBot.Api`.
2. `Api` requests fresh signals from `SignalEngine`.
3. `SignalEngine` pulls market data → generates signal JSON.
4. `Api` merges signal with risk/config → decides execution.
5. `Api` places orders via Zerodha/Groww and logs the result.

---

## Deployment Topology

| Resource | Service | Azure Product |
|----------|---------|---------------|
| Container images | All services | **Azure Container Registry** (`souvikdevacr`) |
| API hosting | `TradingBot.Api` | **App Service** or **Container Apps** |
| Signal engine hosting | `TradingBot.SignalEngine` | **Container Apps** |
| Data storage | Config & trade history | **Azure SQL (serverless)** |
| Secrets | Broker & OpenAI keys | **Azure Key Vault** |
| Monitoring | Logs & metrics | **Azure Monitor / App Insights** |

---

## Scalability & Extensibility

* **Horizontal scaling** – Each container scales independently on AKS/App Service.
* **Plugin interface** – New ML models can be mounted into the Python engine.
* **Future upgrades** – Add websocket market feeds, live dashboards, or reinforcement agents.

---

## Roadmap Snapshot

* **Phase 1** – Local integration with Docker Compose.
* **Phase 2** – CI build & tests via GitHub Actions.
* **Phase 3** – Containerise and push to ACR.
* **Phase 4** – Azure dev deployment (App Service + Container Apps).
* **Phase 5** – Live broker integration & risk engine.
