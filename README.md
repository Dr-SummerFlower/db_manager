# DB manager 数据库管理工具

[![Avalonia](https://img.shields.io/badge/Avalonia-11-blueviolet)](https://avaloniaui.net/)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)
[![GitHub License](https://img.shields.io/github/license/Dr-SummerFlower/db_manager)](https://github.com/Dr-SummerFlower/db_manager/blob/main/LICENSE.md)
> 项目背景：因为本地开发经常需要用到数据库，
> 但是本地部署数据库又比较占用本地资源，而手动启动更加麻烦，
> 所以就诞生了开发这款软件的想法。

### 支持数据库

|    数据库    |    版本    |
|:---------:|:--------:|
|   MySQL   |  v9.2.0  |
|  MongoDB  |  v8.0.6  |
|   Redis   |  v7.4.2  |

> 兼容性：只要数据库的启动命令和上述版本一样，即可兼容使用。

### 安装方式
1. 二进制安装
前往[Release](https://github.com/Dr-SummerFlower/db_manager/releases)页面下载最新版本的构建并双击安装
2. 源码安装
安装[.NET 9 SDK](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)，
```cmd
git clone https://github.com/Dr-SummerFlower/db_manager.git
cd db_manager

# 还原依赖包
dotnet restore

# 运行应用
dotnet run --project .\db_manager\

# 构建可执行文件
dotnet publish -c Release -r <RID> 
# 示例：dotnet publish -c Release -r win-x64
```

### 使用方法
安装之后双击启动（需管理员权限），点击启动即可启动数据库。
mysql的用户名和密码均为`root`

### 常见问题
1. 软件双击后无法启动
请检查[.NET Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0#:~:text=winget%20instructions-,.NET%20Runtime%209.0.7,-The%20.NET%20Runtime)是否安装，
其次检查软件是否以管理员身份运行
2. 软件打开空白
    * 请检查`软件安装目录\config\`下是否存在文件，如果没有则拷贝项目目录中`lib/config`下的三个json文件到`软件安装目录\config\`下
    * 请检查`软件安装目录\utils\`下是否存在数据库程序文件，目录结构为`utils\mysql`,`utils\mongodb`,`utils\redis`

### 开发指南
* [.NET 9.0 SDK](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)
* [Avalonia](https://avaloniaui.net/)
* [Visual Studio 2022](https://visualstudio.microsoft.com/zh-hans/downloads/) 或 
 [Visual Studio Code](https://code.visualstudio.com/Download/) 或 
 [Jetbrains Rider](https://www.jetbrains.com/rider/download/) 以及相关扩展
