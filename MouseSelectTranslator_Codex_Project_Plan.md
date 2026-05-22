# MouseSelectTranslator 项目方案（面向 Codex 自动执行）

> 项目目标：打开应用后，程序在后台自动追踪用户用鼠标选中的文本，并在鼠标上方弹出悬浮文本框显示翻译结果。  
> 第一阶段只做 Windows 桌面端 MVP，不做跨平台、不做 OCR、不做完整读书助手。

---

## 0. 给 Codex 的总要求

Codex 执行本项目时必须遵守以下规则：

1. **先完成最小闭环，再扩展功能**：
   - 鼠标拖选文本
   - 获取选中文本
   - 调用翻译服务
   - 在鼠标上方显示悬浮翻译框

2. **不要一开始做大系统**：
   - 不要做 OCR。
   - 不要做 Electron / Tauri 跨平台版本。
   - 不要做数据库。
   - 不要做生词本。
   - 不要做完整 AI 读书助手。
   - 不要做复杂账号系统。
   - 不要做云端同步。
   - 不要做自动截屏分析。

3. **必须优先保证 Windows 本地可运行**：
   - 优先使用 C# + WPF。
   - 使用 Win32 Hook 监听全局鼠标事件。
   - 使用 UI Automation 优先读取选中文本。
   - UI Automation 失败时使用 Ctrl+C 剪贴板兜底。
   - 使用 WPF 透明置顶窗口显示翻译结果。

4. **隐私默认保护**：
   - 默认不记录用户选中的原文。
   - 默认不写入翻译历史。
   - 默认不上传除当前选中文本以外的任何屏幕信息。
   - 默认不处理密码框内容。
   - 默认不在日志中打印原文和翻译结果。

5. **Codex 修改代码原则**：
   - 若项目已存在，优先增量修补，不要重写整个项目。
   - 不要无理由改变已有命名。
   - 每完成一个阶段必须保证 `dotnet build` 通过。
   - 每个核心模块必须有清晰接口，避免 UI、Hook、翻译服务互相耦合。
   - 任何无法实现的系统能力必须在 `docs/limitations.md` 中说明，不要假装完成。

---

## 1. 项目名称

建议名称：

```text
MouseSelectTranslator
```

中文名：

```text
鼠标划词翻译助手
```

一句话描述：

```text
一个 Windows 桌面后台应用：用户选中任意应用中的文本后，自动在鼠标附近显示翻译结果。
```

---

## 2. 技术栈决策

### 2.1 首选技术栈

```text
语言：C#
运行时：.NET 10 LTS 优先；若本机未安装 .NET 10，则使用 .NET 8 LTS
桌面 UI：WPF
系统能力：Win32 API + Windows UI Automation
翻译服务：先 Mock，后接 OpenAI-Compatible API / DeepSeek / OpenAI / 其他翻译 API
测试：xUnit
配置：本地 JSON 配置文件
```

### 2.2 .NET 版本选择规则

Codex 必须先执行：

```powershell
dotnet --list-sdks
```

然后按以下规则选择目标框架：

1. 如果存在 .NET 10 SDK：
   - 使用 `net10.0-windows`
2. 否则如果存在 .NET 8 SDK：
   - 使用 `net8.0-windows`
3. 如果二者都不存在：
   - 不要继续创建大量代码。
   - 输出缺失环境说明。
   - 要求用户先安装 .NET 10 SDK 或 .NET 8 SDK。

不要使用：

```text
net7.0
net6.0
netcoreapp3.1
.NET Framework 4.x
```

---

## 3. 第一阶段 MVP 功能

第一阶段只实现以下功能：

```text
1. 应用启动后后台运行。
2. 监听全局鼠标左键按下、移动、释放。
3. 判断用户是否发生了拖选行为。
4. 鼠标释放后尝试读取当前选中文本。
5. 如果读取到文本，则调用翻译服务。
6. 在鼠标上方显示一个 WPF 悬浮框。
7. 悬浮框显示原文和翻译。
8. 鼠标移开或按 Esc 后隐藏悬浮框。
9. Ctrl + Shift + T 开关自动划词翻译。
10. 避免重复翻译同一段文本。
```

第一阶段明确不做：

```text
1. OCR。
2. 截屏识别。
3. PDF 特殊解析。
4. 生词本。
5. 历史记录。
6. 账号登录。
7. 云同步。
8. 多端同步。
9. 插件系统。
10. 完整 AI 阅读问答循环。
```

---

## 4. 用户使用流程

目标体验：

```text
用户打开应用
    ↓
应用进入托盘 / 后台运行
    ↓
用户在浏览器、Word、PDF 阅读器、记事本等软件中拖选文本
    ↓
鼠标左键释放
    ↓
程序等待 120~200ms
    ↓
程序读取当前选中文本
    ↓
程序判断文本是否合法
    ↓
程序调用翻译服务
    ↓
程序在鼠标上方显示悬浮翻译框
```

示例：

```text
用户选中：

Graph neural networks aggregate information from neighboring nodes.

鼠标上方浮现：

图神经网络从邻居节点聚合信息。
```

---

## 5. 核心架构

### 5.1 分层结构

项目分为四层：

```text
MouseTranslator.App
    WPF 启动、悬浮窗、托盘、快捷键、用户配置入口

MouseTranslator.Core
    领域接口、状态机、文本清洗、翻译请求、缓存、防抖逻辑

MouseTranslator.Infrastructure
    Win32 Hook、UI Automation、剪贴板、SendInput、屏幕坐标、文件配置

MouseTranslator.Tests
    单元测试和部分集成测试
```

### 5.2 模块关系

```text
┌────────────────────────────┐
│ MouseTranslator.App         │
│ - App.xaml                  │
│ - OverlayWindow             │
│ - TrayManager               │
│ - HotkeyManager             │
└──────────────┬─────────────┘
               │
               ▼
┌────────────────────────────┐
│ MouseTranslator.Core        │
│ - SelectionCoordinator      │
│ - ITextExtractor            │
│ - ITranslationService       │
│ - TranslationCache          │
│ - TextValidationService     │
└──────────────┬─────────────┘
               │
               ▼
┌────────────────────────────┐
│ MouseTranslator.Infrastructure │
│ - Win32MouseHook             │
│ - UIAutomationTextExtractor  │
│ - ClipboardTextExtractor     │
│ - CursorPositionService      │
│ - SettingsStore              │
└────────────────────────────┘
```

---

## 6. 推荐目录结构

Codex 应创建或调整为以下结构：

```text
MouseSelectTranslator/
├── MouseSelectTranslator.sln
├── README.md
├── docs/
│   ├── architecture.md
│   ├── development-plan.md
│   ├── limitations.md
│   ├── privacy.md
│   └── manual-test-checklist.md
│
├── src/
│   ├── MouseTranslator.App/
│   │   ├── MouseTranslator.App.csproj
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── OverlayWindow.xaml
│   │   ├── OverlayWindow.xaml.cs
│   │   ├── Tray/
│   │   │   └── TrayManager.cs
│   │   ├── Hotkeys/
│   │   │   └── GlobalHotkeyManager.cs
│   │   └── CompositionRoot.cs
│   │
│   ├── MouseTranslator.Core/
│   │   ├── MouseTranslator.Core.csproj
│   │   ├── Selection/
│   │   │   ├── SelectionCoordinator.cs
│   │   │   ├── SelectionState.cs
│   │   │   ├── SelectionEvent.cs
│   │   │   ├── SelectionOptions.cs
│   │   │   ├── ITextExtractor.cs
│   │   │   ├── TextExtractionResult.cs
│   │   │   └── TextValidationService.cs
│   │   ├── Translation/
│   │   │   ├── ITranslationService.cs
│   │   │   ├── TranslationRequest.cs
│   │   │   ├── TranslationResult.cs
│   │   │   ├── TranslationOptions.cs
│   │   │   └── TranslationCache.cs
│   │   ├── Overlay/
│   │   │   ├── OverlayRequest.cs
│   │   │   └── OverlayPlacement.cs
│   │   └── Common/
│   │       ├── Result.cs
│   │       └── Clock.cs
│   │
│   ├── MouseTranslator.Infrastructure/
│   │   ├── MouseTranslator.Infrastructure.csproj
│   │   ├── Win32/
│   │   │   ├── NativeMethods.cs
│   │   │   ├── Win32MouseHook.cs
│   │   │   ├── CursorPositionService.cs
│   │   │   └── SendInputService.cs
│   │   ├── TextExtraction/
│   │   │   ├── CompositeTextExtractor.cs
│   │   │   ├── UIAutomationTextExtractor.cs
│   │   │   └── ClipboardTextExtractor.cs
│   │   ├── Translation/
│   │   │   ├── MockTranslationService.cs
│   │   │   └── OpenAICompatibleTranslationService.cs
│   │   ├── Settings/
│   │   │   ├── AppSettings.cs
│   │   │   └── JsonSettingsStore.cs
│   │   └── Logging/
│   │       └── SafeLogger.cs
│   │
│   └── MouseTranslator.Tests/
│       ├── MouseTranslator.Tests.csproj
│       ├── SelectionCoordinatorTests.cs
│       ├── TextValidationServiceTests.cs
│       ├── TranslationCacheTests.cs
│       └── OverlayPlacementTests.cs
```

---

## 7. 创建项目的建议命令

Codex 应按本机 SDK 情况替换 `net10.0-windows` 或 `net8.0-windows`。

示例以 `net10.0-windows` 为准：

```powershell
mkdir MouseSelectTranslator
cd MouseSelectTranslator

dotnet new sln -n MouseSelectTranslator

mkdir src
dotnet new wpf -n MouseTranslator.App -o src/MouseTranslator.App -f net10.0-windows
dotnet new classlib -n MouseTranslator.Core -o src/MouseTranslator.Core -f net10.0
dotnet new classlib -n MouseTranslator.Infrastructure -o src/MouseTranslator.Infrastructure -f net10.0-windows
dotnet new xunit -n MouseTranslator.Tests -o src/MouseTranslator.Tests -f net10.0

dotnet sln add src/MouseTranslator.App/MouseTranslator.App.csproj
dotnet sln add src/MouseTranslator.Core/MouseTranslator.Core.csproj
dotnet sln add src/MouseTranslator.Infrastructure/MouseTranslator.Infrastructure.csproj
dotnet sln add src/MouseTranslator.Tests/MouseTranslator.Tests.csproj

dotnet add src/MouseTranslator.App/MouseTranslator.App.csproj reference src/MouseTranslator.Core/MouseTranslator.Core.csproj
dotnet add src/MouseTranslator.App/MouseTranslator.App.csproj reference src/MouseTranslator.Infrastructure/MouseTranslator.Infrastructure.csproj

dotnet add src/MouseTranslator.Infrastructure/MouseTranslator.Infrastructure.csproj reference src/MouseTranslator.Core/MouseTranslator.Core.csproj

dotnet add src/MouseTranslator.Tests/MouseTranslator.Tests.csproj reference src/MouseTranslator.Core/MouseTranslator.Core.csproj
dotnet add src/MouseTranslator.Tests/MouseTranslator.Tests.csproj reference src/MouseTranslator.Infrastructure/MouseTranslator.Infrastructure.csproj
```

若使用托盘图标，需要在 `MouseTranslator.App.csproj` 中启用 Windows Forms：

```xml
<PropertyGroup>
  <UseWPF>true</UseWPF>
  <UseWindowsForms>true</UseWindowsForms>
</PropertyGroup>
```

---

## 8. 核心状态机

### 8.1 状态定义

```csharp
public enum SelectionState
{
    Disabled,
    Idle,
    MouseDown,
    Dragging,
    WaitingForSelection,
    ExtractingText,
    Translating,
    ShowingOverlay,
    Error
}
```

### 8.2 状态流转

```text
Disabled
  ↑  ↓ Ctrl+Shift+T
Idle
  ↓ MouseLeftDown
MouseDown
  ↓ MouseMove over threshold
Dragging
  ↓ MouseLeftUp
WaitingForSelection
  ↓ Delay 120~200ms
ExtractingText
  ↓ text extracted
Translating
  ↓ translation done
ShowingOverlay
  ↓ timeout / Esc / mouse leaves
Idle
```

### 8.3 拖选判断

必须用鼠标移动距离过滤点击误触。

建议阈值：

```text
最小拖动距离：8 px
最小文本长度：2
最大文本长度：2000
鼠标释放后等待：150 ms
重复触发冷却：300 ms
```

判断逻辑：

```csharp
private static bool IsDrag(Point start, Point end, double threshold)
{
    var dx = end.X - start.X;
    var dy = end.Y - start.Y;
    return Math.Sqrt(dx * dx + dy * dy) >= threshold;
}
```

---

## 9. 鼠标监听模块

### 9.1 接口

```csharp
public interface IMouseSelectionMonitor : IDisposable
{
    event EventHandler<MouseSelectionEventArgs> SelectionGestureCompleted;

    void Start();
    void Stop();
}
```

### 9.2 实现要求

实现类：

```text
Win32MouseHook
```

使用低级鼠标钩子：

```text
WH_MOUSE_LL
```

监听事件：

```text
WM_LBUTTONDOWN
WM_MOUSEMOVE
WM_LBUTTONUP
```

行为：

```text
1. 左键按下时记录起点。
2. 鼠标移动超过阈值时进入 Dragging。
3. 左键释放时，如果曾经 Dragging，则触发 SelectionGestureCompleted。
4. 不吞掉鼠标事件，必须继续传递给系统。
5. Dispose 时必须卸载 hook。
```

注意事项：

```text
- Hook 回调中不要执行耗时操作。
- Hook 回调只记录事件并投递到应用线程。
- 翻译请求不得在 Hook 回调中执行。
- 必须调用 CallNextHookEx。
```

---

## 10. 选中文本提取模块

选中文本读取采用“双通道”：

```text
优先：UI Automation
兜底：模拟 Ctrl+C 读取剪贴板
```

### 10.1 抽象接口

```csharp
public interface ITextExtractor
{
    Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken cancellationToken);
}
```

结果对象：

```csharp
public sealed record TextExtractionResult(
    bool Success,
    string? Text,
    string Source,
    string? ErrorMessage
);
```

### 10.2 CompositeTextExtractor

组合策略：

```text
1. 先调用 UIAutomationTextExtractor。
2. 如果成功且文本有效，直接返回。
3. 如果失败，再调用 ClipboardTextExtractor。
4. 如果都失败，返回失败结果。
```

伪代码：

```csharp
public async Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken ct)
{
    var uia = await _uiaExtractor.ExtractSelectedTextAsync(ct);
    if (uia.Success && _textValidator.IsValid(uia.Text))
    {
        return uia;
    }

    var clipboard = await _clipboardExtractor.ExtractSelectedTextAsync(ct);
    if (clipboard.Success && _textValidator.IsValid(clipboard.Text))
    {
        return clipboard;
    }

    return TextExtractionResult.Failed("No selected text found.");
}
```

---

## 11. UI Automation 文本提取

### 11.1 目标

尽量在不污染剪贴板的情况下读取当前选中文本。

### 11.2 实现逻辑

```text
1. 获取当前焦点元素。
2. 检查是否是密码框。
3. 检查是否支持 TextPattern。
4. 调用 GetSelection()。
5. 合并所有 selection ranges。
6. 返回文本。
```

伪代码：

```csharp
var element = AutomationElement.FocusedElement;
if (element == null)
{
    return Failed("No focused element.");
}

var isPassword = element.GetCurrentPropertyValue(AutomationElement.IsPasswordProperty);
if (isPassword is true)
{
    return Failed("Password field skipped.");
}

if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var patternObj))
{
    return Failed("TextPattern not supported.");
}

var textPattern = (TextPattern)patternObj;
var ranges = textPattern.GetSelection();

if (ranges == null || ranges.Length == 0)
{
    return Failed("No selected text range.");
}

var text = string.Join(Environment.NewLine, ranges.Select(r => r.GetText(-1)));
```

### 11.3 失败是正常现象

Codex 不要把 UI Automation 失败视为 bug。很多应用不会暴露 TextPattern，例如：

```text
- 某些浏览器页面控件
- 某些 PDF 阅读器
- 某些 Electron 应用
- 某些游戏窗口
- 某些自绘 UI
```

失败后进入剪贴板兜底即可。

---

## 12. 剪贴板兜底提取

### 12.1 目标

当 UI Automation 读不到选中文本时，通过模拟 Ctrl+C 获取当前选中文本。

### 12.2 实现流程

```text
1. 保存当前剪贴板内容。
2. 清空或标记剪贴板状态。
3. 通过 SendInput 模拟 Ctrl+C。
4. 等待 50~300ms。
5. 读取剪贴板文本。
6. 恢复原剪贴板内容。
7. 返回读取到的文本。
```

### 12.3 必须保护剪贴板

实现要求：

```text
- 复制前保存 IDataObject。
- 读取完成后尽量恢复原剪贴板。
- 恢复失败时不要抛出导致程序崩溃。
- 不要在日志中打印剪贴板内容。
- 如果原剪贴板包含复杂格式，至少要尽量保留文本格式。
```

伪代码：

```csharp
var oldData = Clipboard.GetDataObject();

try
{
    await _sendInput.SendCtrlCAsync(ct);
    await Task.Delay(150, ct);

    if (Clipboard.ContainsText())
    {
        var text = Clipboard.GetText();
        return Success(text, "Clipboard");
    }

    return Failed("Clipboard does not contain text.");
}
finally
{
    TryRestoreClipboard(oldData);
}
```

### 12.4 敏感信息限制

剪贴板兜底可能复制用户不想上传的内容，因此必须：

```text
- 提供全局暂停快捷键。
- 支持应用黑名单。
- 默认不保存历史。
- 默认不写原文日志。
- UIA 识别到密码框时直接跳过。
```

---

## 13. 文本校验与清洗

### 13.1 TextValidationService

职责：

```text
- 去除前后空白。
- 合并异常换行。
- 过滤空文本。
- 过滤过短文本。
- 过滤过长文本。
- 过滤重复文本。
- 过滤疑似密码 / token。
```

### 13.2 建议规则

```text
MinLength = 2
MaxLength = 2000
MaxLineCount = 50
DebounceMs = 300
DuplicateWindowSeconds = 3
```

### 13.3 清洗规则

```csharp
public string Normalize(string text)
{
    text = text.Trim();
    text = Regex.Replace(text, @"[ \t]+", " ");
    text = Regex.Replace(text, @"\r\n|\r", "\n");
    text = Regex.Replace(text, @"\n{3,}", "\n\n");
    return text;
}
```

### 13.4 疑似敏感内容过滤

第一版只做简单过滤：

```text
- 长度过短且无空格的随机字符串不翻译。
- 疑似 API Key / Token 不翻译。
- 密码框不翻译。
```

示例规则：

```text
sk-...
ghp_...
xoxb-...
AKIA...
长度超过 32 且混合大小写数字符号的单行字符串
```

---

## 14. 翻译服务模块

### 14.1 接口

```csharp
public interface ITranslationService
{
    Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken
    );
}
```

请求对象：

```csharp
public sealed record TranslationRequest(
    string Text,
    string SourceLanguage,
    string TargetLanguage
);
```

返回对象：

```csharp
public sealed record TranslationResult(
    bool Success,
    string OriginalText,
    string TranslatedText,
    string Provider,
    string? ErrorMessage
);
```

### 14.2 第一阶段先实现 MockTranslationService

为了先跑通界面和流程，第一阶段必须先实现 Mock：

```csharp
public sealed class MockTranslationService : ITranslationService
{
    public Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new TranslationResult(
            true,
            request.Text,
            $"[Mock Translation] {request.Text}",
            "Mock",
            null
        ));
    }
}
```

这样不依赖网络、不依赖 API Key，也能先完成整体闭环。

### 14.3 第二阶段实现 OpenAI-Compatible API

用于兼容：

```text
- OpenAI
- DeepSeek
- 其他兼容 OpenAI Chat Completions 格式的服务
```

配置文件示例：

```json
{
  "Translation": {
    "Provider": "OpenAICompatible",
    "BaseUrl": "https://api.example.com/v1",
    "ApiKeyEnvironmentVariable": "OPENAI_API_KEY",
    "Model": "gpt-4.1-mini",
    "TargetLanguage": "zh-CN",
    "TimeoutSeconds": 15
  }
}
```

Codex 注意：

```text
- 不要把 API Key 写死在代码中。
- 只从环境变量或本地 settings.json 读取。
- settings.json 不要提交真实密钥。
- README 只能写示例。
```

### 14.4 翻译提示词

建议系统提示词：

```text
You are a precise translation engine.
Translate the user's text into Simplified Chinese.
Preserve technical terms when necessary.
Do not add explanations unless the original text requires context.
Return only the translated text.
```

用户内容：

```text
{{selected_text}}
```

### 14.5 语言判断

第一版可使用简单规则：

```text
- 如果主要包含英文字符，则翻译为中文。
- 如果主要包含中文，则暂时不翻译，或者翻译为英文。
```

建议第一版只做：

```text
英文 → 中文
```

若选中中文：

```text
显示：当前文本已是中文，未翻译。
```

或在配置中允许：

```text
ChineseToEnglish = true
```

---

## 15. 翻译缓存

### 15.1 目标

减少重复请求，提高响应速度。

### 15.2 接口

```csharp
public interface ITranslationCache
{
    bool TryGet(string normalizedText, out TranslationResult result);
    void Set(string normalizedText, TranslationResult result);
}
```

### 15.3 规则

```text
- 第一版只做内存缓存。
- Key 使用 Normalize 后的原文。
- 缓存最大数量：200。
- 缓存不落盘。
- 程序退出后缓存消失。
```

---

## 16. 悬浮窗口模块

### 16.1 目标

在鼠标上方显示翻译结果，不抢焦点，不影响用户继续阅读。

### 16.2 WPF 窗口属性

`OverlayWindow` 建议属性：

```xml
<Window
    WindowStyle="None"
    AllowsTransparency="True"
    Background="Transparent"
    Topmost="True"
    ShowInTaskbar="False"
    ResizeMode="NoResize"
    SizeToContent="WidthAndHeight"
    ShowActivated="False">
</Window>
```

### 16.3 样式要求

第一版样式简洁即可：

```text
- 深色半透明背景。
- 圆角。
- 最大宽度 420px。
- 原文字体较小。
- 翻译字体较大。
- 可显示错误状态。
```

布局：

```text
┌──────────────────────────────────────┐
│ 原文                                  │
│ Graph neural networks aggregate...    │
│                                      │
│ 翻译                                  │
│ 图神经网络从邻居节点聚合信息。          │
└──────────────────────────────────────┘
```

### 16.4 位置计算

输入：

```text
鼠标释放位置 x, y
悬浮框宽高
当前屏幕工作区
DPI 缩放
```

策略：

```text
1. 默认显示在鼠标上方 20px。
2. 如果上方空间不足，显示在鼠标下方。
3. 如果右侧超出屏幕，向左偏移。
4. 如果左侧超出屏幕，向右偏移。
5. 不要跨出当前显示器工作区。
```

伪代码：

```csharp
var left = mouseX - overlayWidth / 2;
var top = mouseY - overlayHeight - 20;

if (top < screen.Top)
{
    top = mouseY + 20;
}

left = Clamp(left, screen.Left + 8, screen.Right - overlayWidth - 8);
top = Clamp(top, screen.Top + 8, screen.Bottom - overlayHeight - 8);
```

### 16.5 隐藏规则

```text
- Esc 隐藏。
- 鼠标移出悬浮框后 1.5 秒隐藏。
- 新的划词触发后替换旧悬浮框。
- Ctrl+Shift+T 暂停时立即隐藏。
```

### 16.6 可选交互

MVP 可暂缓，但接口预留：

```text
- 复制翻译结果。
- 固定窗口。
- 展开完整原文。
- 切换翻译方向。
```

---

## 17. 托盘和快捷键

### 17.1 托盘功能

托盘菜单：

```text
- 启用 / 暂停划词翻译
- 设置
- 退出
```

### 17.2 快捷键

使用 Win32 `RegisterHotKey`，不要用键盘 Hook 实现简单热键。

建议：

```text
Ctrl + Shift + T：启用 / 暂停
Esc：隐藏悬浮框
```

状态提示：

```text
暂停时托盘图标状态变化，或弹出短提示。
```

---

## 18. 配置系统

### 18.1 配置文件位置

建议位置：

```text
%AppData%/MouseSelectTranslator/settings.json
```

### 18.2 配置结构

```json
{
  "General": {
    "EnabledOnStartup": true,
    "StartMinimized": true,
    "HotkeyToggle": "Ctrl+Shift+T"
  },
  "Selection": {
    "MinDragDistancePx": 8,
    "DelayAfterMouseUpMs": 150,
    "MinTextLength": 2,
    "MaxTextLength": 2000,
    "DuplicateIgnoreSeconds": 3
  },
  "Translation": {
    "Provider": "Mock",
    "TargetLanguage": "zh-CN",
    "TimeoutSeconds": 15,
    "BaseUrl": "",
    "Model": "",
    "ApiKeyEnvironmentVariable": ""
  },
  "Privacy": {
    "SaveHistory": false,
    "LogSelectedText": false,
    "SkipPasswordFields": true
  },
  "Overlay": {
    "MaxWidth": 420,
    "AutoHideSeconds": 1.5,
    "OffsetY": 20
  },
  "AppBlacklist": [
    "KeePass.exe",
    "1Password.exe",
    "Bitwarden.exe"
  ]
}
```

---

## 19. 日志策略

第一版可以使用简单文件日志或 Debug 输出，但必须遵守：

```text
- 不记录选中文本。
- 不记录翻译结果。
- 不记录剪贴板内容。
- 可以记录状态：
  - Mouse gesture detected
  - UIA extraction failed
  - Clipboard extraction succeeded
  - Translation failed: timeout
```

推荐日志示例：

```text
[INFO] Selection gesture completed.
[WARN] UIAutomation extraction failed: TextPattern not supported.
[INFO] Clipboard extraction succeeded. Length=128.
[INFO] Translation completed. Provider=Mock.
```

不要输出：

```text
[INFO] SelectedText=...
[INFO] Clipboard=...
[INFO] Translation=...
```

---

## 20. 错误处理

### 20.1 文本读取失败

行为：

```text
- 不弹窗。
- 不打扰用户。
- Debug 日志记录失败原因。
```

### 20.2 翻译失败

行为：

```text
- 如果是网络错误，可在悬浮框中显示简短错误：
  “翻译失败：网络或 API 配置错误”
- 不显示异常堆栈给普通用户。
```

### 20.3 剪贴板恢复失败

行为：

```text
- 不崩溃。
- 记录安全日志。
- 后续复制仍可继续工作。
```

### 20.4 Hook 失败

行为：

```text
- 应用启动但显示禁用状态。
- 托盘提示：“鼠标监听启动失败”。
- docs/limitations.md 记录可能原因。
```

---

## 21. 安全与隐私边界

必须写入 `docs/privacy.md`：

```text
1. 本工具只处理用户主动选中的文本。
2. 默认不保存历史记录。
3. 默认不记录原文和译文日志。
4. 使用在线翻译服务时，选中文本会发送给配置的翻译 API。
5. 用户可以暂停划词翻译。
6. 密码框和黑名单应用默认跳过。
7. 剪贴板兜底模式会临时模拟 Ctrl+C，但会尽量恢复原剪贴板。
```

---

## 22. 测试计划

### 22.1 单元测试

必须测试：

```text
TextValidationService
- 空文本被拒绝
- 过短文本被拒绝
- 过长文本被拒绝
- 正常英文句子通过
- 疑似 API Key 被拒绝

TranslationCache
- 相同文本命中缓存
- 超过容量后移除旧项

OverlayPlacement
- 鼠标上方有空间时显示在上方
- 上方空间不足时显示在下方
- 左右越界时自动回收

SelectionCoordinator
- 点击不触发翻译
- 拖动超过阈值触发提取
- Disabled 状态不触发
```

### 22.2 手动测试

写入 `docs/manual-test-checklist.md`：

```text
[ ] 在记事本中选中英文，释放鼠标后显示翻译框。
[ ] 在浏览器网页中选中英文，释放鼠标后显示翻译框。
[ ] 单击鼠标不触发翻译。
[ ] 重复选中同一文本，不重复请求 API。
[ ] Ctrl+Shift+T 可以暂停 / 恢复。
[ ] Esc 可以隐藏悬浮框。
[ ] 剪贴板中原有文本在翻译后尽量恢复。
[ ] 选中超长文本时不翻译。
[ ] 选中密码框内容时不翻译。
[ ] 退出程序后不再监听鼠标。
```

---

## 23. Codex 阶段任务清单

### TASK-0001：环境探测

执行：

```powershell
dotnet --info
dotnet --list-sdks
```

判断：

```text
- 若有 .NET 10 SDK：使用 net10.0-windows。
- 若无 .NET 10 但有 .NET 8 SDK：使用 net8.0-windows。
- 若都没有：停止并输出环境缺失说明。
```

验收：

```text
- 明确目标框架。
- 不盲目创建不兼容项目。
```

---

### TASK-0101：创建解决方案和项目结构

目标：

```text
创建 WPF App、Core、Infrastructure、Tests 四个项目。
```

验收：

```powershell
dotnet restore
dotnet build
dotnet test
```

必须通过。

---

### TASK-0201：实现核心领域对象

创建：

```text
SelectionState
SelectionOptions
SelectionEvent
ITextExtractor
TextExtractionResult
TextValidationService
ITranslationService
TranslationRequest
TranslationResult
TranslationCache
OverlayRequest
OverlayPlacement
```

验收：

```text
- Core 不引用 WPF。
- Core 不引用 Win32。
- Core 不引用 Infrastructure。
- Core 单元测试通过。
```

---

### TASK-0301：实现鼠标 Hook

创建：

```text
Win32MouseHook
NativeMethods
CursorPositionService
```

实现：

```text
- WH_MOUSE_LL
- WM_LBUTTONDOWN
- WM_MOUSEMOVE
- WM_LBUTTONUP
- 拖选判断
- Dispose 卸载 Hook
```

验收：

```text
- 启动程序后拖选可以在日志中看到 gesture completed。
- 单击不会触发 gesture completed。
- 退出程序后 Hook 被释放。
```

---

### TASK-0401：实现文本提取器

创建：

```text
UIAutomationTextExtractor
ClipboardTextExtractor
CompositeTextExtractor
SendInputService
```

验收：

```text
- 记事本中选中文本可以提取。
- UIA 失败时 Clipboard 兜底。
- 剪贴板尽量恢复。
- 密码框跳过。
```

---

### TASK-0501：实现 Mock 翻译服务

创建：

```text
MockTranslationService
```

行为：

```text
输入文本后返回 [Mock Translation] + 原文。
```

验收：

```text
- 不需要网络。
- 不需要 API Key。
- 划词后可以看到悬浮框显示 Mock 译文。
```

---

### TASK-0601：实现悬浮窗口

创建：

```text
OverlayWindow.xaml
OverlayWindow.xaml.cs
OverlayPlacementService
```

验收：

```text
- 悬浮框显示在鼠标上方。
- 不抢焦点。
- 不显示在任务栏。
- 可自动隐藏。
- Esc 可隐藏。
```

---

### TASK-0701：串联 SelectionCoordinator

创建：

```text
SelectionCoordinator
```

职责：

```text
- 接收鼠标拖选完成事件。
- 延迟等待系统选区稳定。
- 提取选中文本。
- 校验文本。
- 查缓存。
- 调用翻译。
- 发出 OverlayRequest。
```

验收：

```text
- 从拖选到显示悬浮框形成完整闭环。
```

---

### TASK-0801：托盘和快捷键

创建：

```text
TrayManager
GlobalHotkeyManager
```

实现：

```text
- 托盘菜单：启用 / 暂停 / 退出。
- Ctrl+Shift+T：切换启用状态。
- Esc：隐藏悬浮框。
```

验收：

```text
- 暂停后拖选不再触发翻译。
- 恢复后拖选继续触发。
- 退出后进程关闭。
```

---

### TASK-0901：配置系统

创建：

```text
AppSettings
JsonSettingsStore
```

实现：

```text
- 启动时读取 settings.json。
- 不存在则生成默认配置。
- 配置路径在 %AppData%/MouseSelectTranslator/settings.json。
```

验收：

```text
- 修改配置后重启生效。
- 配置文件不包含真实 API Key。
```

---

### TASK-1001：OpenAI-Compatible 翻译服务

创建：

```text
OpenAICompatibleTranslationService
```

实现：

```text
- 从环境变量读取 API Key。
- 从配置读取 BaseUrl 和 Model。
- 超时控制。
- 错误处理。
```

验收：

```text
- Provider=Mock 时不调用网络。
- Provider=OpenAICompatible 且配置正确时可翻译。
- API Key 缺失时显示配置错误，不崩溃。
```

---

### TASK-1101：补充文档

创建：

```text
README.md
docs/architecture.md
docs/limitations.md
docs/privacy.md
docs/manual-test-checklist.md
```

验收：

```text
- README 能指导用户运行项目。
- limitations 明确说明哪些软件可能无法读取选中文本。
- privacy 明确说明在线翻译会上传选中文本。
```

---

## 24. 验收标准

项目完成 MVP 后，必须满足：

```text
1. `dotnet build` 成功。
2. `dotnet test` 成功。
3. 应用可以启动。
4. 应用可以后台运行。
5. 用户在记事本中选中英文后，鼠标上方出现悬浮框。
6. 用户在浏览器中选中英文后，鼠标上方出现悬浮框。
7. 单击鼠标不会触发翻译。
8. Ctrl+Shift+T 可以暂停 / 恢复。
9. Esc 可以隐藏悬浮框。
10. 退出程序后没有残留 Hook。
11. 默认不保存用户选中文本。
12. 默认不输出原文和译文日志。
```

---

## 25. 已知限制

必须写入文档，不要隐瞒：

```text
1. 某些应用不支持 UI Automation TextPattern。
2. 某些应用会拦截 Ctrl+C。
3. 某些 PDF 阅读器可能提取失败。
4. 某些 Electron 应用可能只支持剪贴板兜底。
5. 高权限应用中的文本可能需要本程序也以管理员权限运行才能读取。
6. 剪贴板兜底无法 100% 保证复杂格式完全恢复。
7. 在线翻译服务会接收用户选中的文本。
8. 本项目第一版不处理图片文字。
9. 本项目第一版不做 OCR。
10. 本项目第一版不做跨平台。
```

---

## 26. 后续扩展路线

### V0.1：MVP

```text
鼠标划词 → 提取文本 → Mock 翻译 → 悬浮框显示
```

### V0.2：真实翻译

```text
OpenAI-Compatible API
DeepSeek / OpenAI / 其他服务配置
错误提示
超时控制
```

### V0.3：体验优化

```text
托盘设置界面
应用黑名单
翻译缓存
悬浮框固定
复制译文
```

### V0.4：阅读助手能力

```text
解释术语
总结选中文本
提出问题
回答问题
保存上下文
```

### V0.5：自动读书助手原型

```text
从划词触发扩展到主动阅读：
- 截屏 OCR
- 页面区域识别
- 自动总结
- 自动发散问题
- 自动问答循环
```

注意：V0.5 不是当前 MVP 范围。

---

## 27. 给 Codex 的一次性执行提示词

可以把下面这段直接发给 Codex：

```text
你正在实现一个 Windows 桌面项目 MouseSelectTranslator。目标是：应用启动后后台监听鼠标拖选文本，用户释放鼠标后自动读取当前选中文本，调用翻译服务，并在鼠标上方显示 WPF 悬浮翻译框。

请严格按以下阶段执行，不要扩展范围：

1. 先运行 dotnet --list-sdks，判断使用 net10.0-windows 还是 net8.0-windows。如果没有 .NET 10 或 .NET 8 SDK，停止并说明环境缺失。
2. 创建解决方案 MouseSelectTranslator.sln。
3. 创建四个项目：
   - src/MouseTranslator.App：WPF 应用
   - src/MouseTranslator.Core：领域接口和状态机
   - src/MouseTranslator.Infrastructure：Win32、UI Automation、剪贴板、翻译 API
   - src/MouseTranslator.Tests：xUnit 测试
4. Core 不允许引用 WPF、Win32、Infrastructure。
5. Infrastructure 可以引用 Core。
6. App 可以引用 Core 和 Infrastructure。
7. 先实现 MockTranslationService，不要一开始接真实 API。
8. 实现 Win32 低级鼠标 Hook，监听左键按下、移动、释放。只有拖动距离超过 8px 才触发选择完成事件。
9. 实现 CompositeTextExtractor：先 UI Automation TextPattern.GetSelection，失败后用 Ctrl+C 剪贴板兜底。剪贴板兜底必须尽量保存并恢复原剪贴板内容。
10. 实现 TextValidationService：过滤空文本、过短文本、过长文本、疑似 token、密码框。
11. 实现 OverlayWindow：WPF 透明置顶、不抢焦点、不显示任务栏，显示在鼠标上方，越界时自动调整位置。
12. 实现 SelectionCoordinator 串联流程：鼠标拖选完成 → 延迟 150ms → 提取文本 → 校验 → 查缓存 → 翻译 → 显示悬浮框。
13. 实现 Ctrl+Shift+T 暂停/恢复，Esc 隐藏悬浮框。
14. 实现托盘菜单：启用/暂停、退出。
15. 默认不记录用户选中的原文和译文。
16. 创建 docs/privacy.md、docs/limitations.md、docs/manual-test-checklist.md。
17. 每完成一个阶段都运行 dotnet build 和 dotnet test。
18. 最终确保在记事本和浏览器中选中英文后，可以在鼠标上方看到 Mock 翻译悬浮框。

不要做 OCR、不要做跨平台、不要做数据库、不要做生词本、不要做完整读书助手。当前只完成“鼠标划词翻译助手”的最小闭环。
```

---

## 28. 最终交付物

Codex 最终应交付：

```text
1. 可运行的 Windows WPF 应用。
2. 清晰的解决方案结构。
3. 鼠标拖选监听。
4. 选中文本提取。
5. Mock 翻译服务。
6. 悬浮翻译框。
7. 暂停 / 恢复快捷键。
8. 托盘退出。
9. 基础单元测试。
10. README 和隐私限制文档。
```

---

## 29. 最小成功定义

只要做到下面这一点，就算 MVP 成功：

```text
打开应用 → 在记事本中用鼠标选中一句英文 → 松开鼠标 → 鼠标上方出现包含 Mock 翻译结果的悬浮框。
```

随后再替换 Mock 翻译服务为真实 API，即可变成可用的划词翻译工具。
