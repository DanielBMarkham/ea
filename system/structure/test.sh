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
DARKGREY='\033[1;30m'
LIGHTRED='\033[1;31m'
LIGHTGREEN='\033[1;32m'
YELLOW='\033[1;33m'
LIGHTBLUE='\033[1;34m'
LIGHTPURPLE='\033[1;35m'
LIGHTCYAN='\033[1;36m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# First we smoke test from the outside
VERBOSITY=7
EMPTYFILE=EMPTYLEAVEHEREFORTESTING.amin
TEST1='Nothing-in-IMPLIES-Nothing-out'
TEST1MODELOUT="$TEST1".amout
rm -f "$TEST1MODELOUT"

cd src/ea
touch "$TEST1MODELOUT"

# The following code is a BASH escape/DOS timebomb. Don't screw with it unless you like quotes
# Do all combinations of piping and file listing to make sure nothing comes out
$(dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\$EMPTYFILE ) > $TEST1MODELOUT
cat Util.fs | (dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\$EMPTYFILE ) >> $TEST1MODELOUT
cat Util.fs | (dotnet run --no-build -- -v:1 ) >> $TEST1MODELOUT


TEST1RESULT=$(wc -l < "$TEST1MODELOUT")
if [ $TEST1RESULT -eq 1 ];
  then
    printf "${RED}SMOKETEST FAIL${NC} \n"
  else
    printf "${GREEN}SMOKETEST PASS${NC} \n"
fi

dotnet run --no-build

cd ../earTest
dotnet run --no-build