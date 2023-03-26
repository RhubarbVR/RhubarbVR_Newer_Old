FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /
COPY ./bin/Release/net7.0/publish/ ./
ENTRYPOINT ["dotnet", "HPRApplication.dll"]
STOPSIGNAL SIGINT
EXPOSE 7856/tcp
EXPOSE 7856/udp
EXPOSE 7857/tcp
EXPOSE 7857/udp
