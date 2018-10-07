#!/usr/bin/env
# uncomment below to stop script from running
#exit 0

#set -o nounset
set -o errexit
set -o pipefail
[[ "${DEBUG}" == 'true' ]] && set -o xtrace

cd src/eaTest
dotnet run --no-build --summary
cd ../earTest
dotnet run --no-build --summary