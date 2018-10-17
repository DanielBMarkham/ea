#!/usr/bin/env
# uncomment below to stop script from running
#exit 0

#set -o nounset
set -o errexit
set -o pipefail
[[ "${DEBUG}" == 'true' ]] && set -o xtrace

# ANSI COLORS FOR PRETTY OUTPUT :)
RED='\033[0;31m'
BLACK='\033;030m'
GREEN='\033[0;32m'
ORANGE='\033[0;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
LIGHTGREY='\033[0;37m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

cd src/eaTest
dotnet run --no-build
cd ../earTest
dotnet run --no-build

VERBOSITY=7
TEST1='Nothing in IMPLIES Nothing out']
TEST1MODELOUT="$TEST1".amout
cd ../ea
rm -f "$TEST1MODELOUT"
touch "$TEST1MODELOUT"
$(dotnet run --no-build -- -v:"$VERBOSITY")>"$TEST1MODELOUT"
TEST1RESULT=$(wc -l < "$TEST1MODELOUT")
if [ $TEST1RESULT -eq 1 ];
  then
    printf "I ${RED}HATE${NC} Stack Overflow\n"
  else
    printf "I ${GREEN}LOVE${NC} Stack Overflow\n"
fi

#exec 3</dev/null
exec &1<3
exec 3<$("")
dotnet run --no-build -- -v:7)  3< #>TESTOUT.amout
exec 3<&-
TEST1RESULT=$(wc -l < TESTOUT.amout)
