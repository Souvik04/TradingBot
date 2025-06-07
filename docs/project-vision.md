# Project Vision – TradingBot

## Overview
TradingBot is an AI-assisted, algorithmic trading platform designed for the Indian stock market. It is composed of a modular architecture including a C# ASP.NET Core API backend (`TradingBot.Api`), a Python-based signal engine (`TradingBot.SignalEngine`), and broker integration modules.

## Goals
- Automate and optimize trade decisions using AI, ML, and rule-based logic.
- Modular signal strategy interface to allow plug-and-play strategies.
- Cloud-native deployment via Azure Kubernetes Service (AKS) using Docker.
- Future-proofed broker integration: Zerodha, Groww, etc.
- Secure, config-driven trading modes: paper, backtest, live.

## Key Features
- ✅ Signal generation engine in Python (Flask-based HTTP API)
- ✅ ASP.NET Core API for orchestration and configuration
- ✅ Dockerized components with support for Azure ACR
- ✅ Mocked portfolio and broker logic for development/testing
- 🚧 Future support for AI-enhanced strategy plugins
- 🚧 Integration with real trading accounts via broker APIs

## Target Users
- Retail investors
- Algo-traders
- Developers experimenting with AI in finance

## Long-Term Vision
- Real-time trade execution with risk management
- Plug-and-play AI/ML strategy experimentation
- Web dashboard for monitoring and configuration
- Trade audit logging and analytics dashboard