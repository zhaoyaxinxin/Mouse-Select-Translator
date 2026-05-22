# MouseSelectTranslator 本地 OCR 实施方案

## 1. 方案结论

当前项目的 OCR 路线固定为：

```text
方案 B：本地 OCR 引擎版
```

采用原因：

- 保持当前 WPF exe 交付方式不变
- 不再依赖 Edge PDF viewer 是否暴露选区
- 可同时覆盖扫描版 PDF、图片文字和浏览器 PDF 失效场景

## 2. 本轮目标

本轮 OCR 只做兜底能力，不做独立产品。

固定目标：

1. 在现有提取链最后增加 OCR 通道
2. 基于鼠标拖选区域截图，而不是整屏 OCR
3. 先支持英文识别，再决定是否补中文模型
4. OCR 失败不能影响现有 `UIA / Clipboard / Edge extension` 链路

本轮不做：

- 整页 PDF 批量识别
- OCR 历史记录
- OCR 结果编辑器
- 复杂图像增强工作流
- 云端 OCR 服务

## 3. 总体架构

执行后链路固定为：

```text
UI Automation -> Clipboard -> Edge extension -> OCR
```

OCR 子链路为：

```text
SelectionEvent
  -> OcrRegionCalculator
  -> IScreenCaptureService
  -> LocalOcrTextExtractor
  -> TesseractOcrEngine
  -> TextExtractionResult
```

## 4. 模块设计

### Core

- `IOcrTextExtractor`
- `IScreenCaptureService`
- 新增 `OcrRequest`
- 新增 `OcrResult`
- 新增 `OcrRegion`

### Infrastructure

- `OcrRegionCalculator`
- `GdiScreenCaptureService`
- `LocalOcrTextExtractor`
- `TesseractOcrEngine`
- `ImagePreprocessService`
- `OcrLanguageResolver`

### Assets

- `assets/ocr/tessdata/eng.traineddata`
- 可选后续：`assets/ocr/tessdata/chi_sim.traineddata`

## 5. 契约演进

当前 `IOcrTextExtractor : ITextExtractor` 太弱，不足以表达 OCR 输入区域。

建议固化为以下方向：

### OcrRequest

包含：

- 截图字节
- 图片格式
- 语言
- 来源说明

### OcrResult

包含：

- 是否成功
- 识别文本
- 失败原因
- 可选置信度

### OcrRegion

包含：

- `X`
- `Y`
- `Width`
- `Height`
- `Padding`

## 6. 区域计算规则

默认区域来自拖选矩形：

```text
X = min(StartX, EndX)
Y = min(StartY, EndY)
Width = abs(EndX - StartX)
Height = abs(EndY - StartY)
```

最小规则：

- 当拖选区域过小，应用有限 padding
- 不允许默认整屏截图
- 不允许越出当前屏幕边界

建议最小值：

- 最小宽度：`80px`
- 最小高度：`32px`
- 默认 padding：`12px`

这些值先固化为实现常量，不先做设置项。

## 7. 引擎路线

本地 OCR 引擎首选：

```text
Tesseract
```

选择原因：

- 本地可离线
- 与当前 exe 交付方式兼容
- 不要求 Windows package identity
- 文档和生态成熟

当前阶段建议：

1. 先做 `eng`
2. 手测稳定后再考虑 `chi_sim`
3. 不先做多语言自动检测

## 8. 分阶段任务

### Phase O1：契约与区域计算

目标：

- 固化 OCR 请求/结果契约
- 增加截图区域模型与计算器

相关文件：

- `src/MouseTranslator.Core/Selection/*`
- `src/MouseTranslator.Tests/*`

验收标准：

- 区域计算有单元测试
- 小拖选、反向拖选、边界拖选都能给出有效区域

### Phase O2：本地截图

目标：

- 完成 `IScreenCaptureService`
- 能从区域稳定产出图片字节

相关文件：

- `src/MouseTranslator.Infrastructure/Ocr/*`

验收标准：

- 对任意有效区域能生成非空图片数据
- 多显示器边界不直接崩溃

### Phase O3：OCR 引擎接入

目标：

- 接入 Tesseract
- 跑通一张英文测试图

相关文件：

- `src/MouseTranslator.Infrastructure/Ocr/*`
- `assets/ocr/tessdata/*`

验收标准：

- 已知英文样图可返回可读文本
- 引擎初始化失败时能给出明确错误

### Phase O4：链路接入

目标：

- 将 OCR 放到 `CompositeTextExtractor` 最后一级
- OCR 失败不影响前面三段提取链

相关文件：

- `src/MouseTranslator.Infrastructure/TextExtraction/*`
- `src/MouseTranslator.App/CompositionRoot.cs`

验收标准：

- 普通网页仍优先走现有链路
- 扫描版 PDF 或图片文字可通过 OCR 路径拿到文本
- OCR 失败时主程序不崩溃

### Phase O5：发布与文档

目标：

- 固化 OCR 资源分发方式
- 更新启动和手测说明

相关文件：

- `启动.md`
- `docs/limitations.md`
- `docs/manual-test-checklist.md`

验收标准：

- 文档明确写出模型文件位置
- 用户按文档能完成 OCR 手测

## 9. 验收标准

实现完成时，至少满足：

1. 普通网页正文继续优先走非 OCR 链路
2. 扫描版 PDF 至少能在一个样例上识别出可翻译文本
3. OCR 不可用时不会影响现有翻译功能
4. `dotnet build MouseSelectTranslator.slnx` 通过
5. `dotnet test MouseSelectTranslator.slnx` 通过

## 10. 禁止事项

- 不要先引入云端 OCR
- 不要先做设置界面
- 不要先做复杂图像预处理
- 不要在第一轮就支持大量语言模型
- 不要让 OCR 成为默认第一通道

## 11. 交付影响

会新增：

- OCR 引擎依赖
- 模型文件
- 发布体积

不会改变：

- 当前 WPF 主程序形态
- 当前 DeepSeek 翻译链
- 当前 Edge 扩展桥接逻辑

## 12. 参考资料

- Tesseract 官方文档  
  https://tesseract-ocr.github.io/
- Windows `Windows.Media.Ocr` namespace  
  https://learn.microsoft.com/en-us/uwp/api/windows.media.ocr?view=winrt-28000
