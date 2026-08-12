.DEFAULT_GOAL := help

COMPOSE       := docker-compose
COMPOSE_INFRA := docker-compose -f docker-compose.infrastructure.yml
SERVICE       ?=
NAME          ?=

## ---------------------------------------------------------------------------
## Environment
## ---------------------------------------------------------------------------

.PHONY: env
env: ## Create .env from .env.sample if it doesn't exist yet
	@test -f .env || cp .env.sample .env

## ---------------------------------------------------------------------------
## Quick Start - full stack via Docker Compose (App.Admin + Catalog + observability)
## ---------------------------------------------------------------------------

.PHONY: build up start ps logs down down-v restart rebuild
build: ## Build all images in parallel
	$(COMPOSE) build --parallel

up: ## Start all services (detached)
	$(COMPOSE) up -d

start: ## Build and start all services in one command
	$(COMPOSE) up --build -d

ps: ## Show status of all services
	$(COMPOSE) ps

logs: ## Tail logs for all services
	$(COMPOSE) logs -f

logs-%: ## Tail logs for a specific service, e.g. make logs-catalog-api
	$(COMPOSE) logs -f $*

down: ## Stop all services
	$(COMPOSE) down

down-v: ## Stop all services and remove volumes (clean slate)
	$(COMPOSE) down -v

restart: ## Restart all services
	$(COMPOSE) restart

restart-%: ## Restart a specific service, e.g. make restart-catalog-api
	$(COMPOSE) restart $*

rebuild: ## Rebuild and restart all services (force recreate)
	$(COMPOSE) up --build -d --force-recreate

## ---------------------------------------------------------------------------
## Development Mode - infrastructure services only
## ---------------------------------------------------------------------------

.PHONY: infra-up infra-ps infra-logs infra-down infra-down-v
infra-up: ## Start infrastructure services only (Postgres, MinIO, Keycloak, observability)
	$(COMPOSE_INFRA) up -d

infra-ps: ## Show status of infrastructure services
	$(COMPOSE_INFRA) ps

infra-logs: ## Tail logs for all infrastructure services
	$(COMPOSE_INFRA) logs -f

infra-logs-%: ## Tail logs for a specific infrastructure service
	$(COMPOSE_INFRA) logs -f $*

infra-down: ## Stop infrastructure services
	$(COMPOSE_INFRA) down

infra-down-v: ## Stop infrastructure services and remove volumes
	$(COMPOSE_INFRA) down -v

infra-restart-%: ## Restart a specific infrastructure service
	$(COMPOSE_INFRA) restart $*

## ---------------------------------------------------------------------------
## Building Docker Images
## ---------------------------------------------------------------------------

.PHONY: build-nocache
build-%: ## Build a specific service, e.g. make build-catalog-api
	$(COMPOSE) build $*

up-%: ## Build and start a specific service, e.g. make up-catalog-api
	$(COMPOSE) up --build -d $*

build-nocache: ## Force rebuild all images without cache
	$(COMPOSE) build --no-cache --parallel

## ---------------------------------------------------------------------------
## Running Catalog (dotnet run / hot reload)
## ---------------------------------------------------------------------------

.PHONY: run-catalog-api watch
run-catalog-api: ## Run Catalog Service API
	cd src/Services/Catalog/Api/Catalog.Api && dotnet run

watch: ## Run Catalog with hot reload
	cd src/Services/Catalog/Api/Catalog.Api && dotnet watch run

## ---------------------------------------------------------------------------
## Running App.Admin
## ---------------------------------------------------------------------------

.PHONY: run-app-admin
run-app-admin: ## Install deps and run App.Admin (http://localhost:7002)
	cd src/Apps/App.Admin && npm install && npm run dev

## ---------------------------------------------------------------------------
## Development helpers
## ---------------------------------------------------------------------------

.PHONY: test
test: ## Run all tests
	dotnet test

## ---------------------------------------------------------------------------
## Docker Maintenance
## ---------------------------------------------------------------------------

.PHONY: prune prune-all stats df
prune: ## Remove stopped containers and unused images
	docker system prune -f

prune-all: ## Remove all unused images, containers, networks, and volumes
	docker system prune -af --volumes

stats: ## Show live container resource usage
	docker stats

df: ## Show Docker disk usage
	docker system df

## ---------------------------------------------------------------------------
## Help
## ---------------------------------------------------------------------------

.PHONY: help
help: ## Show this help
	@grep -E '^[a-zA-Z0-9_.%-]+:.*?## ' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-24s\033[0m %s\n", $$1, $$2}'
