#!/usr/bin/env bash
# scripts/check.sh: the single "definition of done" check (CLAUDE.md section 8).
set -u
cd "$(git rev-parse --show-toplevel)"

fail() { echo "FAIL: $1"; exit 1; }

# Prints the rule and its matches, then fails, when a forbidden-pattern grep found anything.
check_none() {  # $1 = rule name, $2 = grep output (file:line:text)
  if [ -n "$2" ]; then echo "$1:"; echo "$2"; fail "$1"; fi
}

BASE=$(git merge-base HEAD origin/master) || fail "no merge-base with origin/master (run git fetch)"

# Counts test declarations. $1 = --untracked (working tree) or a commit (e.g. $BASE).
count_tests() {
  { git grep -E -e '\[(Fact|Theory)' "$1" -- 'EquipmentCalibrationTracker/Tests/*.cs'
    git grep -E -e '^[[:space:]]*(it|test)\(' "$1" -- ':(glob)equipment-tracker-ui/src/**/*.spec.ts'
  } | wc -l
}

echo "== Test guard"
before=$(count_tests "$BASE")
now=$(count_tests --untracked)
echo "test declarations: $before at base, $now now"
[ "$now" -ge "$before" ] || fail "test count dropped from $before to $now"
# shortcut: git diff misses brand-new untracked files; they are covered once staged or committed.
added=$(git diff "$BASE" -- EquipmentCalibrationTracker/Tests equipment-tracker-ui/src \
  | grep -E '^\+.*(Skip *=|it\.skip|xit\(|describe\.skip|\.only\()')
check_none "skipped or focused test added" "$added"

echo "== Forbidden patterns"
EXCLUDE=(--exclude-dir=bin --exclude-dir=obj --exclude-dir=node_modules --exclude-dir=dist)
check_none "No sync-over-async" \
  "$(grep -rnE "${EXCLUDE[@]}" --include='*.cs' '\.Wait\(\)|\.GetAwaiter\(\)\.GetResult\(\)' .)"
check_none "Controllers stay thin" \
  "$(grep -rnE 'AppDbContext|I?EquipmentRepository' EquipmentCalibrationTracker/Api/Controllers)"
check_none "No outward project references (Domain)" \
  "$(grep -Hn '<ProjectReference' EquipmentCalibrationTracker/Domain/Domain.csproj)"
check_none "No outward project references (Application)" \
  "$(grep -Hn '<ProjectReference' EquipmentCalibrationTracker/Application/Application.csproj | grep -v 'Domain\.csproj')"
check_none "No secrets in config" \
  "$(grep -rniE "${EXCLUDE[@]}" --include='appsettings*.json' 'Password=|Pwd=|User ID=' .)"
# -w: matches the HttpClient class, not provideHttpClient() used in spec setup.
check_none "HTTP only in services (UI)" \
  "$(grep -rnw HttpClient equipment-tracker-ui/src/app/features)"

echo "== Backend"
cd EquipmentCalibrationTracker
dotnet restore                                                    || fail "dotnet restore failed"
dotnet build --no-restore --configuration Release                 || fail "dotnet build failed"
dotnet test --no-build --verbosity normal --configuration Release || fail "dotnet test failed"
cd ..

echo "== Frontend"
cd equipment-tracker-ui
npm ci                     || fail "npm ci failed"
npm test -- --watch=false  || fail "npm test failed"
npm run build              || fail "npm run build failed"
npm run e2e                || fail "e2e tests failed"
cd ..

echo "ALL CHECKS PASSED"
