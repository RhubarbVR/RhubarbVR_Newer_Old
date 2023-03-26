dotnet publish "HPRApplication.csproj" -c Release
docker build -t rhubarb_rhp .
docker save -o ./RhubarbRHPImage rhubarb_rhp