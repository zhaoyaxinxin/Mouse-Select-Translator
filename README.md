# Mouse Select Translator

Windows 划词翻译工具。  
当前支持：

- 桌面端划词提取与翻译
- Edge 网页 / 文本框 / 部分 PDF 选区桥接
- DeepSeek API 翻译
- OCR 兜底通道

## 环境要求

- Windows
- `.NET 10 SDK`

确认：

```powershell
dotnet --list-sdks
```

## 获取与构建

在项目根目录执行：

```powershell
dotnet restore MouseSelectTranslator.slnx --configfile NuGet.Config
dotnet build MouseSelectTranslator.slnx
dotnet test MouseSelectTranslator.slnx
```

## 启动

开发启动：

```powershell
dotnet run --project src/MouseTranslator.App/MouseTranslator.App.csproj
```

或直接运行已构建程序：

```powershell
.\src\MouseTranslator.App\bin\Debug\net10.0-windows\MouseTranslator.App.exe
```

## 配置 DeepSeek API

当前默认走 DeepSeek。  
启动前先设置环境变量：

```powershell
$env:DEEPSEEK_API_KEY="你的 DeepSeek API Key"
```

程序配置文件位置：

```text
%AppData%\MouseSelectTranslator\settings.json
```

关键配置应保持为：

```json
{
  "Translation": {
    "Provider": "DeepSeek",
    "BaseUrl": "https://api.deepseek.com",
    "Model": "deepseek-v4-flash",
    "ApiKeyEnvironmentVariable": "DEEPSEEK_API_KEY",
    "TargetLanguage": "zh-CN"
  }
}
```

## Edge 扩展

如果要翻译 Edge 网页、文本框或 PDF：

1. 打开 `edge://extensions`
2. 开启 `Developer mode`
3. 点击 `Load unpacked`
4. 选择项目里的 `edge-extension`
5. 如果要处理本地 `file://` 或本地 PDF，再开启 `Allow access to file URLs`

修改扩展脚本后，需要在扩展页手动 `Reload`。

## OCR

如果要处理扫描版 PDF 或图片文字，还需要：

- 本机可找到 `tesseract.exe`
- `eng.traineddata` 可用

可通过环境变量指定：

```powershell
$env:MOUSE_TRANSLATOR_TESSERACT_PATH="D:\tesseract\tesseract.exe"
$env:MOUSE_TRANSLATOR_TESSDATA_PATH="C:\path\to\tessdata"
```

## 快速验证

1. 启动程序
2. 打开记事本或 Edge
3. 鼠标拖选一段英文
4. 确认出现中文翻译悬浮框

