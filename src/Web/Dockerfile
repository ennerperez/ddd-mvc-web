FROM mcr.microsoft.com/dotnet/sdk:8.0.401 AS build

ARG configuration=Debug
WORKDIR /app
COPY . .
RUN chmod +x build.sh
RUN ./build.sh --configuration $configuration --target Publish
RUN dotnet dev-certs https -ep /app/publish/Web/certificate.pfx -p 0ca271cdf1764804a136eba180f02673

FROM mcr.microsoft.com/dotnet/aspnet:8.0.401 AS run
RUN apt-get update && apt-get install -y libfontconfig1
EXPOSE 5000/tcp
EXPOSE 5001/tcp
WORKDIR /app
COPY --from=build /app/publish/Web .
ENV ASPNETCORE_URLS=http://+:5000;https://+:5001
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="0ca271cdf1764804a136eba180f02673"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=certificate.pfx
ENTRYPOINT ["dotnet", "Web.dll"]
