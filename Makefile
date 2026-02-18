SHELL := bash
.DEFAULT_GOAL:=help

TEST_PROJECT := Tests/Tests.csproj
MAIN_PROJECT := Src/PokerGame.csproj
COVERAGE_DIR := Tests/TestResults
REPORT_DIR := Tests/coveragereport
BADGE_SVG := README.md

.PHONY: help
help: Makefile
	@echo "PokerGame: a Texas Hold 'Em Poker engine written in C#."
	@echo "-------------------"
	@echo "Usage:"
	@sed -n 's/^##//p' $<

## coverage    Run tests with coverage and update badge in README.md
.PHONY: coverage
coverage:
	rm -rf coverage.log Tests/coveragereport
	cd Tests && dotnet test \
		-p:CollectCoverage=true \
		-p:CoverletOutputFormat=cobertura \
		-p:CoverletOutput=../coverage.cobertura.xml | tee ../coverage.log
	@COVERAGE_PCT=$$(grep "PokerGame" coverage.log | grep -o '[0-9]\+\.[0-9]\+%' | head -1 | sed 's/%//' | awk '{printf "%d", $$1}' || echo "0") && \
	COLOR=$$(case $$COVERAGE_PCT in \
		[8-9][0-9]) echo "brightgreen" ;; \
		[6-7][0-9]) echo "yellowgreen" ;; \
		[5][0-9]) echo "orange" ;; \
		*) echo "red" ;; \
	esac) && \
	sed -i.bak 's|Coverage-[0-9]\+%-[a-z]\+|Coverage-'"$$COVERAGE_PCT"'%-'"$$COLOR"'|g' README.md && \
	rm -rf README.md.bak coverage.cobertura.xml coverage.log Tests/coveragereport

## run    Default run
.PHONY: run
run :
	cd Src && dotnet run

## test    Run unit tests
.PHONY: test
test :
	cd Tests && dotnet test
