# Deployment Guide – TradingBot

## Requirements
- Azure CLI
- Docker & Docker Compose
- PowerShell or Bash
- Azure Container Registry (ACR)
- Azure Kubernetes Service (AKS)

## Steps

### 1. Build and Push Docker Images

```bash
docker build -t souvikdevacr.azurecr.io/tradingbot-api ./TradingBot.Api
docker build -t souvikdevacr.azurecr.io/tradingbot-engine ./TradingBot.SignalEngine
docker push souvikdevacr.azurecr.io/tradingbot-api
docker push souvikdevacr.azurecr.io/tradingbot-engine
```

### 2. Provision AKS (Simplified)

```bash
az aks create   --resource-group rg-tradingbot   --name tradingbot-aks   --node-count 1   --generate-ssh-keys   --attach-acr souvikdevacr
```

### 3. Deploy with kubectl

```bash
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/engine-deployment.yaml
kubectl get services
```

## Local Deployment (Optional)

```bash
docker compose up
```

## Future Plans

- Add Helm charts
- Setup GitHub Actions CI/CD
- Enable autoscaling & DNS