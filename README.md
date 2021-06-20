# DotnetCampusP2PFileShare

基于 ASP.NET Core 和 WPF 客户端的局域网 P2P 文件分享

提供可执行文件和 SDK 和二次开发能力

## 进度

开发中……

## DotnetCampusP2PFileShare.SDK

### 打包

需要先修改 Version 版本号，然后推 Tag 打包

### 使用方法

最简单下载方法

```csharp
            var p2PProvider = new P2PProvider();
            var p2PDownloader = p2PProvider.P2PDownloader;
            await p2PDownloader.DownloadFileAsync(resourceId, directoryInfo);
```

## 埋点

请看 EventId 的内容

## 开源

已开源在 https://github.com/dotnet-campus/dotnetCampus.P2PFileShare

请以 GitHub 上开源为主
