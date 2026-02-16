SHELL := bash
.DEFAULT_GOAL:=help

.PHONY: help
help: Makefile
	@echo "PokerGame: a Texas Hold 'Em Poker engine written in C#."
	@echo "-------------------"
	@echo "Usage:"
	@sed -n 's/^##//p' $<

## run    Default run
.PHONY: run
run :
	dotnet run
