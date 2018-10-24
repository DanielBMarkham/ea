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
rm "$TEST1MODELOUT"
touch "$TEST1MODELOUT"

#rm  "$TEST1MODELOUT".1
#rm  "$TEST1MODELOUT".2
#rm  "$TEST1MODELOUT".3


# * The BASH escape from hell section
# The following code is a BASH escape/DOS timebomb. Don't screw with it unless you like quotes
# Do all combinations of piping and file listing to make sure nothing comes out

#echo '$(dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\'"$EMPTYFILE"' ) > '"$TEST1MODELOUT".1
#echo 'cat Util.fs | (dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\'$EMPTYFILE' ) >> '"$TEST1MODELOUT".2
#echo 'cat Util.fs | (dotnet run --no-build -- -v:1 ) >> '"$TEST1MODELOUT".3 

#(dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\$EMPTYFILE ) > "$TEST1MODELOUT".1
#cat Util.fs | (dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\$EMPTYFILE ) >> "$TEST1MODELOUT".2
#cat Util.fs | (dotnet run --no-build -- -v:1 ) >> "$TEST1MODELOUT".3 


#TEST1RESULT=$(wc -l < "$TEST1MODELOUT")
# function takes a message a a bool representing pass/fail
# remember: 0=true 1=fail
function report()
{
	if [ ${2} -eq 1 ];
	  then
		printf "${GREEN}${1} PASS${NC} \n"
	  else
		printf "${RED} ${1} FAIL${NC} \n"
	fi
}
# send me testName, testDescription, fileName, and desired linecount
function testFileLineCount() 
	{
	#echo "TEST FILE LINE COUNT FUNCTION ENTER"
	local TestName="${1}"
	local TESTNAME_Result="${1}_Result"
	local Description="${2}"
	local FileName="${3}"
	local DesiredResult=${4}
	local TestNameLineCount="$TestName"_LineCount
	local TestNameResult="$TestName"_Result
	#printf "TestName : $TestName\n"
	#printf "Description : ${Description}\n"
	#printf "FileName : ${FileName}\n"
	#printf "DesiredResult : ${DesiredResult}\n"
	#temp=$"$TESTNAME_LINECOUNT"=$(wc -l < "$TESTNAMEFILE")
	#echo "$FileName"
	temp=$"$TestNameLineCount"=$(wc -l < "$FileName")
	#echo $temp
	eval $temp
	temp=$"TestNameResult"'=$(('"$DesiredResult"'==$'"$TestNameLineCount"'))'
	#echo $temp
	eval $temp
	#echo " boo "$TestNameResult
	testResult=$(eval $(echo "echo " '$'"$TestNameResult"))
	#echo "Test condition evaluates to (0 fail anything else pass) "$TestNameResult #$testResult
	report "$Description" $TestNameResult 
	}


TESTNAME=EmptyFileProducesEmptyFile
TEST_DESCRIPTION="Empty File Produces Empty File From Command Line"
DESIRED_RESULT=1
TESTNAMEFILE="XX_$TESTNAME"
TESTNAME_LINECOUNT="$TESTNAME"_LineCount
TESTNAME_RESULT="$TESTNAME"_Result

# Make file
#touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" && (dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\EMPTYLEAVEHEREFORTESTING.amin ) > "$TESTNAMEFILE"
# Store the line count of the result
#temp=$"$TESTNAME_LINECOUNT"=$(wc -l < "$TESTNAMEFILE")
#echo $temp
#eval $temp


# store whether the line count equals the desired result 
#echo "DESIRED_RESULT="$DESIRED_RESULT
#temp=$"$TESTNAME_RESULT"'=$(('"$DESIRED_RESULT"'==$'"$TESTNAME_LINECOUNT"'))'
#echo $temp
#eval $temp

#testResult=$(eval $(echo "echo " '$'"$TESTNAME_RESULT"))
#echo "Test condition evaluates to (0 fail anything else pass) "$testResult

#report "$TEST_DESCRIPTION" $testResult 

TESTNAME=NothingProvidedProducesNoResult
TESTNAMEFILE="XX_$TESTNAME"
touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" && (dotnet run --no-build -- -v:1 ) > "$TESTNAMEFILE"
testFileLineCount $TESTNAME "Nothing Provided Produces Empty File" "$TESTNAMEFILE" 1 

TESTNAME=BadFileProvidedOnCommandLineProducesNoResult
TESTNAMEFILE="XX_$TESTNAME"
touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" && (dotnet run --no-build -- -v:1 BADFILENAMEFORTESTINGNOFILESHOULDBENAMEDTHISSODONTDOTHAT ) > "$TESTNAMEFILE"
testFileLineCount $TESTNAME "Bad FileName Provided On The Command Line Produces Empty File" "$TESTNAMEFILE" 1 

#TESTNAME=BadFilePipedInProducesNoResult
#TESTNAMEFILE="XX_$TESTNAME"
# Format a little different here because the cat fails since the file doesn't exist'
#touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" || cat BADFILENAMEFORTESTINGNOFILESHOULDBENAMEDTHISSODONTDOTHAT | (dotnet run --no-build -- -v:1  ) > "$TESTNAMEFILE"
#testFileLineCount $TESTNAME "Bad FileName Piped In Produces Empty File" "$TESTNAMEFILE" 1 

#TESTNAME=BadFilePipedAndProvidedInProducesNoResult
#TESTNAMEFILE="XX_$TESTNAME"
# Format a little different here because the cat fails since the file doesn't exist'
#touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" || cat BADFILENAMEFORTESTINGNOFILESHOULDBENAMEDTHISSODONTDOTHAT | (dotnet run --no-build -- -v:1 BADFILENAMEFORTESTINGNOFILESHOULDBENAMEDTHISSODONTDOTHAT ) > "$TESTNAMEFILE"
#testFileLineCount $TESTNAME "Bad FileNames Both Piped In And Provided On The Command Line Produces Empty File" "$TESTNAMEFILE" 1 

TESTNAME=EmptyFileProvidedOnTheCommandLineProducesEmptyFile
TESTNAMEFILE="XX_$TESTNAME"
touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" && (dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\EMPTYLEAVEHEREFORTESTING.amin ) > "$TESTNAMEFILE"
testFileLineCount $TESTNAME "Empyt File Provided On The Command Line Produces Empty File" "$TESTNAMEFILE" 1 

TESTNAME=EmptyFileProducesEmptyFilePiped
TESTNAMEFILE="XX_$TESTNAME"
#touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" && (dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\EMPTYLEAVEHEREFORTESTING.amin ) > "$TESTNAMEFILE"
touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" && cat ../../../../meta/analysis/EMPTYLEAVEHEREFORTESTING.amin | (dotnet run --no-build -- -v:1  ) > "$TESTNAMEFILE"
testFileLineCount $TESTNAME "Empty File Piped In Produces Empty File" "$TESTNAMEFILE" 1 

TESTNAME=EmptyFilePipedAndProvidedProducesEmptyFile
TESTNAMEFILE="XX_$TESTNAME"
touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" && cat ../../../../meta/analysis/EMPTYLEAVEHEREFORTESTING.amin | (dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\EMPTYLEAVEHEREFORTESTING.amin  ) > "$TESTNAMEFILE"
testFileLineCount $TESTNAME "Empty Files Both Piped and Provided Produces Empty File" "$TESTNAMEFILE" 1 

TESTNAME=TenLinePipedInProducesTenLine
TESTNAMEFILE="XX_$TESTNAME"
touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" && cat ../../../../meta/analysis/TENLINEFORTESTING.amin | (dotnet run --no-build -- -v:1  ) > "$TESTNAMEFILE"
testFileLineCount $TESTNAME "Ten Line File Piped In Produces Ten Line File" "$TESTNAMEFILE" 10 

TESTNAME=TenLineProvidedOnTheCommandLineProducesTenLine
TESTNAMEFILE="XX_$TESTNAME"
touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" && (dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\TENLINEFORTESTING.amin ) > "$TESTNAMEFILE"
testFileLineCount $TESTNAME "Ten Line File Provided On The Command Line Produces Ten Line File" "$TESTNAMEFILE" 10 

TESTNAME=TenLineFileBothPipedAndProvidedEqualsTwentyLineFile
TESTNAMEFILE="XX_$TESTNAME"
touch "$TESTNAMEFILE" && rm "$TESTNAMEFILE" && cat ../../../../meta/analysis/TENLINEFORTESTING.amin | (dotnet run --no-build -- -v:1 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\TENLINEFORTESTING.amin ) > "$TESTNAMEFILE"
testFileLineCount $TESTNAME "Ten Line File Provided On The Command Line And Piped In Produces Twenty Line File" "$TESTNAMEFILE" 20 



#dotnet run --no-build

#cd ../earTest
#dotnet run --no-build
# rm Nothing-in-IMPLIES-Nothing-out.amout.1 && (dotnet run --no-build -- -v:7 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\TENLINEFORTESTING.amin ) > Nothing-in-IMPLIES-Nothing-out.amout.1 && cat Nothing-in-IMPLIES-Nothing-out.amout.1
# rm Nothing-in-IMPLIES-Nothing-out.amout.2 && cat Util.fs | (dotnet run --no-build -- -v:7 ..\\..\\..\\..\\..\\..\\..\\meta\\analysis\\TENLINEFORTESTING.amin ) >> Nothing-in-IMPLIES-Nothing-out.amout.2 && cat Nothing-in-IMPLIES-Nothing-out.amout.2
