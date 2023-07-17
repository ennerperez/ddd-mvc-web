FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY publish/Web/ ./
ENTRYPOINT ["dotnet", "Web.dll"]