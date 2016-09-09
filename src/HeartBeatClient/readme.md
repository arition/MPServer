# HeartBeatClient

## Compile

Download and install .Net Core from [https://www.microsoft.com/net/core](https://www.microsoft.com/net/core)

Run following commends

```bash
git clone https://github.com/arition/MPServer
cd MPServer/src/HeartBeatClient
dotnet restore
dotnet build
cd bin/Debug/netcoreapp1.0
dotnet ./HeartBeatClient.dll --username heartbeat --password passwordGeneratedByServer4423 --tokenEndPoint "http://myaddress.com/api/account/token" --heartBeatEndPoint "http://myaddress.com/api/heartBeat" --device deviceName
```

You can also write a config file

```bash
vim config.json
```

```json
{
    "username": "heartbeat",
    "password": "passwordGeneratedByServer4423",
    "token_end_point": "http://myaddress.com/api/account/token",
    "heart_beat_end_point": "http://myaddress.com/api/heartBeat",
    "device": "deviceName"
}
```

and run

```bash
dotnet ./HeartBeatClient.dll -c config.json
```