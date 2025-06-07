# Roadmap – TradingBot

## Phase 1 – Core Infrastructure (✅)

- [x] C# API with config endpoints
- [x] Flask signal engine in Python
- [x] Docker containers for API and Engine
- [x] Local docker-compose setup
- [x] Basic signal endpoint integration

## Phase 2 – Core Logic (🚧)

- [x] Portfolio management service
- [x] Signal strategy interface (Python)
- [x] Trade enforcement rules
- [ ] Strategy plugin loader
- [ ] Configurable trade modes
- [ ] API <-> Engine HTTP invocation

## Phase 3 – Broker & AI Integration (🧠)

- [ ] IBrokerService interface & mock
- [ ] Zerodha/Groww integration
- [ ] OpenAI-based decision strategy
- [ ] Configurable strategy selection

## Phase 4 – CI/CD & Infra (⚙️)

- [ ] AKS + GitHub Actions deployment
- [ ] Environment-specific configs
- [ ] Metrics & Logging with Azure Monitor

## Phase 5 – UX & Analytics (📊)

- [ ] Admin dashboard (React or Blazor)
- [ ] Trade history/audit log
- [ ] Performance analytics