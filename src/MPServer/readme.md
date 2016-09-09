# MPServer

## Compile

Download and install .Net Core from [https://www.microsoft.com/net/core](https://www.microsoft.com/net/core)

Run following commends

```bash
git clone https://github.com/arition/MPServer
cd MPServer/src/MPServer
dotnet restore
dotnet run
```

You can now visit the site on ```http://localhost:5000/```

First run will generated a file called ```_default-user.json``` which contains all the account info. Please ensure to keep the file safe in some other place and delete the generated file inside the folder.

Use nginx as a reverse proxy server to ```http://localhost:5000/``` is recommended.