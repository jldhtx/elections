# run the below to get the report generator installed then also
# export PATH="$PATH:/Users/<you>/.dotnet/tools"
# assuming you're on a Mac, of course
# dotnet tool install -g dotnet-reportgenerator-globaltool\n
rm -rf ./TestResults

dotnet test --collect:"XPlat Code Coverage"

reportgenerator \
-reports:"./TestResults/*/coverage.cobertura.xml" \
-targetdir:"coveragereport" \
-reporttypes:Html

open ./coverageReport/index.html