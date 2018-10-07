#!/usr/bin/env
# uncomment below to stop script from running
#exit 0

#set -o nounset
set -o errexit
set -o pipefail
[[ "${DEBUG}" == 'true' ]] && set -o xtrace

cd src/ea
dotnet build
cd ../ear
dotnet build
cd ../eaTest
dotnet build
cd ../earTest
dotnet build