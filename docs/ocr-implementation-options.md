# MouseSelectTranslator OCR 实现方案候选

## 1. 背景

当前项目已经完成：

- `UI Automation -> Clipboard -> Edge extension` 三段提取链
- Edge 普通网页与文本框基本可用
- Edge 内置 PDF viewer 仍可能拿不到可翻译选区

因此，OCR 的目标不是替换现有链路，而是补足这类场景：

- 扫描版 PDF
- 内置 PDF viewer 不暴露选区
- 图片里的文字
- 自绘界面里看得见但复制不到的文字

## 2. 本轮固定假设

以下内容为 AI 假设，等待用户确认：

1. OCR 只作为最后兜底通道，不抢占现有 `UIA / Clipboard / Edge extension`。
2. OCR 的最小范围是“鼠标拖选区域附近的可见文本”，不是整页文档批量识别。
3. OCR 结果仍然走现有翻译与 overlay 主链路，不单独设计 OCR UI 产品。
4. 当前项目仍以 Windows 本地桌面工具为主，不优先做云端重依赖方案。

## 3. 当前代码现实约束

现有 OCR 占位接口非常薄：

- [IOcrTextExtractor.cs](<C:\Users\zyx17\Desktop\AIfactory\划词翻译助手\src\MouseTranslator.Core\Selection\IOcrTextExtractor.cs:1>)
- [IScreenCaptureService.cs](<C:\Users\zyx17\Desktop\AIfactory\划词翻译助手\src\MouseTranslator.Core\Selection\IScreenCaptureService.cs:1>)
- [SelectionEvent.cs](<C:\Users\zyx17\Desktop\AIfactory\划词翻译助手\src\MouseTranslator.Core\Selection\SelectionEvent.cs:1>)
- [ScreenCaptureRequest.cs](<C:\Users\zyx17\Desktop\AIfactory\划词翻译助手\src\MouseTranslator.Core\Selection\ScreenCaptureRequest.cs:1>)

这意味着 OCR 实现方案至少要回答四个问题：

1. 由谁计算截图区域
2. 用什么引擎识别
3. 识别失败如何回退
4. 如何保持当前 exe 交付方式可用

## 4. 共同设计底线

无论选哪条路线，都建议固定以下共识：

### 4.1 调用顺序

```text
UI Automation -> Clipboard -> Edge extension -> OCR
```

### 4.2 最小新增契约

不建议保持 `IOcrTextExtractor : ITextExtractor` 这种零参数形式不变，因为 OCR 需要明确输入区域。

建议最小演进为：

```text
SelectionEvent -> ScreenCaptureRequest -> IScreenCaptureService -> IOcrTextExtractor -> TextExtractionResult
```

### 4.3 最小区域策略

建议先按拖选矩形做区域裁切，而不是整屏 OCR：

- `X = min(StartX, EndX)`
- `Y = min(StartY, EndY)`
- `Width = abs(EndX - StartX)`
- `Height = abs(EndY - StartY)`

当矩形过小，再做有限 padding，而不是直接整屏识别。

### 4.4 本轮不做

- 整页 PDF 批量 OCR
- OCR 历史记录
- OCR 结果编辑器
- 云同步
- 多语言 OCR 模型管理界面

## 5. 方案 A：Windows 原生 OCR 版

### 项目定位

使用 Windows 原生 OCR 能力，保持系统集成感最强，尽量少引第三方 OCR 引擎。

### 技术栈

- `IScreenCaptureService`：本地截图实现
- `Windows.Media.Ocr`
- 必要时引入应用打包能力以满足 package identity

### 架构模式

```text
SelectionCoordinator
  -> ScreenCaptureService
  -> WindowsOcrTextExtractor
  -> TextExtractionResult
```

### 推荐目录结构

```text
src/
├── MouseTranslator.Core/
│   └── Selection/
│       ├── IOcrTextExtractor.cs
│       ├── IScreenCaptureService.cs
│       ├── OcrRequest.cs
│       └── OcrResult.cs
├── MouseTranslator.Infrastructure/
│   └── Ocr/
│       ├── WindowsOcrTextExtractor.cs
│       └── GdiScreenCaptureService.cs
└── MouseTranslator.App/
```

### 核心模块

- `GdiScreenCaptureService`
- `WindowsOcrTextExtractor`
- `OcrRegionCalculator`
- OCR 失败原因映射

### 开发任务拆解

1. 定义 `OcrRequest / OcrResult`
2. 用拖选坐标生成截图区域
3. 完成本地截图实现
4. 接入 `Windows.Media.Ocr`
5. 把 OCR 挂到 `CompositeTextExtractor` 最后一级
6. 增加 OCR 手测清单

### 禁止事项

- 不要在未确认 package identity 路线前直接承诺可发布 exe
- 不要把 OCR 变成默认第一通道

### 适用场景

- 只面向 Windows
- 愿意接受打包/部署约束
- 更偏系统原生能力

### 优点

- 原生能力，依赖少
- 理论上对离线本地使用最自然
- 不需要外部 OCR 服务费用

### 缺点

- 官方文档对桌面应用有 `package identity` 约束
- 会直接影响你当前“普通 exe 发布”的交付路径
- 语言包和识别能力受操作系统环境影响

### 推荐指数

`6/10`

## 6. 方案 B：本地 OCR 引擎版

### 项目定位

保持当前 WPF exe 交付方式不变，引入本地 OCR 引擎作为 OCR 兜底实现。

### 技术栈

- `IScreenCaptureService`：本地截图实现
- 本地 OCR 引擎适配器
- 推荐首选：`Tesseract` 路线

### 架构模式

```text
SelectionCoordinator
  -> ScreenCaptureService
  -> LocalOcrTextExtractor
  -> TesseractAdapter
  -> TextExtractionResult
```

### 推荐目录结构

```text
src/
├── MouseTranslator.Core/
│   └── Selection/
│       ├── IOcrTextExtractor.cs
│       ├── IScreenCaptureService.cs
│       ├── OcrRequest.cs
│       └── OcrResult.cs
├── MouseTranslator.Infrastructure/
│   └── Ocr/
│       ├── LocalOcrTextExtractor.cs
│       ├── GdiScreenCaptureService.cs
│       ├── TesseractOcrEngine.cs
│       └── ImagePreprocessService.cs
└── assets/
    └── ocr/
        └── tessdata/
```

### 核心模块

- `GdiScreenCaptureService`
- `LocalOcrTextExtractor`
- `TesseractOcrEngine`
- `ImagePreprocessService`
- `OcrLanguageResolver`

### 开发任务拆解

1. 定义 OCR 请求/结果契约
2. 用拖选区域完成截图
3. 接入本地 OCR 引擎
4. 做最小图像预处理
5. 将 OCR 放在提取链最后一层
6. 为扫描版 PDF 和图片文字增加手测样例
7. 补充发布说明，明确模型文件如何随 exe 分发

### 禁止事项

- 不要先做复杂图像增强流水线
- 不要一次支持过多语言模型
- 不要把 OCR 输出缓存设计成一个新子系统

### 适用场景

- 保持当前 exe 分发模式
- 需要离线 OCR
- 需要处理扫描版 PDF 或图片文字

### 优点

- 最符合当前项目的发布方式
- 不依赖浏览器 PDF viewer 是否暴露选区
- 可以覆盖扫描版 PDF

### 缺点

- 要引入 OCR 模型文件和额外体积
- 识别准确率、速度、预处理都要自己调
- 中英混合、复杂排版效果需要更多样本验证

### 推荐指数

`9/10`

## 7. 方案 C：云端 OCR 服务版

### 项目定位

用云端 OCR 服务快速打通 PDF / 图片文字识别，把客户端复杂度降到最低。

### 技术栈

- `IScreenCaptureService`：本地截图实现
- 云端 OCR API
- 建议候选：`Azure AI Vision Read OCR`

### 架构模式

```text
SelectionCoordinator
  -> ScreenCaptureService
  -> CloudOcrTextExtractor
  -> VisionOcrClient
  -> TextExtractionResult
```

### 推荐目录结构

```text
src/
├── MouseTranslator.Core/
│   └── Selection/
├── MouseTranslator.Infrastructure/
│   └── Ocr/
│       ├── CloudOcrTextExtractor.cs
│       ├── VisionOcrClient.cs
│       └── GdiScreenCaptureService.cs
└── docs/
    └── privacy.md
```

### 核心模块

- `GdiScreenCaptureService`
- `CloudOcrTextExtractor`
- `VisionOcrClient`
- 超时、重试、隐私提示

### 开发任务拆解

1. 定义 OCR 请求/结果契约
2. 实现本地截图
3. 调云端 OCR API
4. 增加用户开关和隐私说明
5. 处理失败重试和超时反馈
6. 增加网络异常手测项

### 禁止事项

- 不要默认静默上传截图
- 不要把云端 OCR 和翻译 API 共用一个配置块

### 适用场景

- 可以接受网络依赖
- 更关心识别效果和开发速度
- 可接受额外隐私说明和潜在费用

### 优点

- 集成速度快
- 对复杂版式和图片文字通常更稳
- 不受桌面打包方式约束

### 缺点

- 截图上传带来隐私边界变化
- 依赖网络和外部服务
- 有费用和可用性风险

### 推荐指数

`5/10`

## 8. 方案对比表

| 维度 | 方案 A：Windows 原生 OCR | 方案 B：本地 OCR 引擎 | 方案 C：云端 OCR 服务 |
|---|---|---|---|
| 交付方式兼容当前 exe | 弱 | 强 | 强 |
| 离线可用 | 强 | 强 | 弱 |
| 扫描版 PDF 覆盖 | 中 | 强 | 强 |
| 工程复杂度 | 中 | 中 | 中 |
| 部署复杂度 | 高 | 中 | 低 |
| 隐私风险 | 低 | 低 | 高 |
| 对当前项目适配度 | 中 | 强 | 中 |
| 推荐场景 | Windows 原生打包版 | 当前项目主线 | 快速实验或企业服务 |

## 9. AI 推荐

推荐选择：**方案 B：本地 OCR 引擎版**。

原因：

1. 你当前项目已经是普通 WPF exe，而且还在讨论单 exe 交付；`Windows.Media.Ocr` 的 package identity 约束会直接把路线带偏。
2. 你当前真实痛点是 Edge PDF viewer 和扫描版 PDF，不是再做一层依赖浏览器内部结构的适配。
3. 本地 OCR 引擎可以在不改变整体产品形态的前提下，把“图片 / 扫描版 / viewer 不暴露选区”都纳入可覆盖范围。

## 10. 如果选择方案 B，建议固化的正式实施方案

### 10.1 总体流程

```text
SelectionEvent
  -> OcrRegionCalculator
  -> IScreenCaptureService
  -> LocalOcrTextExtractor
  -> TextExtractionResult
  -> 现有翻译链
```

### 10.2 最小模块划分

- `OcrRegionCalculator`
  - 根据拖选矩形生成截图区域
- `GdiScreenCaptureService`
  - 负责截图为 `byte[]`
- `TesseractOcrEngine`
  - 封装 OCR 引擎调用
- `LocalOcrTextExtractor`
  - 调用截图和 OCR，返回 `TextExtractionResult`

### 10.3 最小阶段拆分

#### Phase O1：契约与区域计算

- 扩展 OCR 请求/结果契约
- 新增截图区域计算

验收：

- 单元测试覆盖区域计算边界

#### Phase O2：本地截图

- 实现 `IScreenCaptureService`
- 输出 PNG 或 BMP 字节流

验收：

- 可对任意拖选区域截出有效图像

#### Phase O3：OCR 引擎接入

- 接入本地 OCR 引擎
- 先只支持 `eng`，再看是否补 `chi_sim`

验收：

- 对一张已知测试图能返回可读文本

#### Phase O4：链路接入

- 把 OCR 放到 `CompositeTextExtractor` 最后一级
- OCR 失败不影响现有三段提取链

验收：

- 普通网页仍走现有链路
- 扫描版 PDF / 图片文字在 OCR 路径上可出结果

#### Phase O5：发布与文档

- 说明 OCR 模型文件如何随 exe 分发
- 更新 `启动.md`、`limitations.md`、`manual-test-checklist.md`

验收：

- 用户按文档能完成本地 OCR 启动和手测

## 11. 需要先确认的关键决策

在真正固化正式 OCR 文档前，只有一个关键选择：

```text
你是否接受在当前项目中引入本地 OCR 引擎文件和额外发布体积
```

如果接受，最合适的下一步是把 **方案 B** 固化成正式任务文档。

## 12. 参考资料

- Windows `Windows.Media.Ocr` namespace  
  https://learn.microsoft.com/en-us/uwp/api/windows.media.ocr?view=winrt-28000
- Azure AI Vision OCR / Read  
  https://learn.microsoft.com/en-us/azure/ai-services/computer-vision/overview-ocr
- Tesseract 官方文档  
  https://tesseract-ocr.github.io/
