FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS dev

WORKDIR /app

COPY . ./

EXPOSE 8081

CMD ["dotnet", "run", "--urls", "http://0.0.0.0:8081"]
