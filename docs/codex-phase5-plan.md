# MouseSelectTranslator Phase 5 执行方案

## 1. 方案结论

本轮后续推进固定采用：

```text
方案 B：桌面端 + Edge 双通道版
```

选择原因：

- 当前最痛点不是通用 OCR，而是 Edge 文本框和浏览器 PDF 失败率高。
- 只强化桌面端无法稳定覆盖 `contenteditable` 和浏览器内 PDF 文本层。
- 先把浏览器专用通道补上，再给 OCR 留正式接口，复杂度更可控。

## 2. 本轮目标

本轮只解决以下三件事：

1. 增强 `Ctrl+C` 兜底
2. 提升 Edge 浏览器文本框 / PDF 成功率
3. 给 OCR 预留正式接口

本轮不做：

- 完整 OCR 识别产品化
- Oxford 官方词典离线下载
- 浏览器扩展商店发布
- 历史记录、账号、同步

## 3. 实施原则

1. 保持现有 WPF 主程序为主链路，Edge 扩展只负责浏览器专用文本采集。
2. Core 只放契约、流程和策略，不直接依赖 Edge 扩展实现或 OCR 平台实现。
3. 先做 Phase 5A，再做 5B，再做 5C，最后只做 5D 的接口预留。
4. 每个阶段都必须有独立验收出口，未通过则不能进入下一阶段。
5. 不把 OCR 做成当前默认强依赖。

## 4. 目标架构

执行后形成的提取链路：

```text
UI Automation -> Clipboard fallback -> Edge extension channel -> OCR hook point
```

职责边界：

- Desktop app
  - 鼠标拖选监听
  - 提取策略调度
  - 翻译调用
  - Overlay 展示
- Edge extension
  - 读取网页正文选区
  - 读取 `input` / `textarea` / `contenteditable`
  - 读取浏览器 PDF viewer 的文本层选区
  - 把选中文本回传给桌面端
- OCR contracts
  - 只定义接口和挂点
  - 不在本轮落地真实 OCR 引擎

## 5. 模块清单

### Core

- `ITextExtractor`
- `TextExtractionResult`
- `SelectionCoordinator`
- 新增或扩展提取失败原因分类
- 预留 `IOcrTextExtractor`
- 预留 OCR 调度挂点

### Infrastructure / Desktop

- `UIAutomationTextExtractor`
- `ClipboardTextExtractor`
- `CompositeTextExtractor`
- `SendInputService`
- 新增剪贴板轮询与变化检测
- 新增 Edge 通道接入适配器

### Edge Extension

- `manifest.json`
- `content-script`
- 页面选区提取逻辑
- 文本框选区提取逻辑
- PDF viewer 文本层提取逻辑
- 与桌面端的数据交换协议说明

### Docs

- `docs/development-plan.md`
- `docs/manual-test-checklist.md`
- `docs/limitations.md`
- 本文档

## 6. 分阶段执行

### Phase 5A：桌面端提取增强

范围：

- 把 `ClipboardTextExtractor` 从固定等待改成短轮询等待
- 引入剪贴板文本变化检测，而不是只读一次结果
- 把 `UIAutomationTextExtractor` 从只看 `TextPattern` 扩展到 `TextPattern + ValuePattern`
- 为提取失败建立更细的原因分类

不做：

- Edge 扩展
- OCR 实现

验收标准：

- `dotnet build MouseSelectTranslator.slnx` 通过
- `dotnet test MouseSelectTranslator.slnx` 通过
- 至少补齐剪贴板等待策略和失败分类的单元测试
- 记事本和普通网页正文的成功率高于当前版本

### Phase 5B：Edge 扩展最小版

范围：

- 新增最小可加载的 Edge 扩展
- 内容脚本读取 `window.getSelection()`
- 读取 `input`、`textarea`、`contenteditable` 当前选区
- 定义扩展到桌面端的数据交换协议
- 桌面端能识别并优先使用来自 Edge 的选区文本

不做：

- PDF 专项适配
- OCR 实现

验收标准：

- 扩展可在本地开发模式加载
- Edge 普通网页正文选区可稳定回传
- Edge 文本框选区可稳定回传
- 桌面端在收到扩展文本后仍走现有翻译和 overlay 主链路

### Phase 5C：Edge PDF 专项适配

范围：

- 针对浏览器 PDF viewer 增加文本层选区提取逻辑
- 明确 `file://` 权限要求、启用方法和手测步骤
- 对浏览器内 PDF 和本地 PDF 分开记录限制

不做：

- OCR 实现

验收标准：

- 浏览器内 PDF 文本层选区可在至少一个样例文件上稳定回传
- 文档中明确写出 `file://` 权限要求
- `docs/manual-test-checklist.md` 和 `docs/limitations.md` 同步更新

### Phase 5D：OCR 接口预留

范围：

- 定义 `IOcrTextExtractor`
- 定义 `IScreenCaptureService`
- 在 `CompositeTextExtractor` 或等价调度点中预留第三通道挂点
- 在文档中写明 OCR 尚未启用的原因和后续接入条件

不做：

- 真实 OCR 识别
- 截屏 UI

验收标准：

- 不接入 OCR 实现时，现有主链路行为不变
- Core 中 OCR 契约不依赖具体平台实现
- 文档清楚写明未来接入边界

## 7. Codex 执行约束

1. 先读 `docs/development-plan.md`、`docs/limitations.md`、`docs/manual-test-checklist.md`。
2. 每次只推进一个阶段，不跨阶段偷做后续内容。
3. Phase 5B 之前不要提前写 PDF 专项逻辑。
4. Phase 5D 之前不要把 OCR 接口变成运行时强依赖。
5. 每完成一个阶段，都必须更新：
   - `docs/development-plan.md`
   - `docs/manual-test-checklist.md`
   - `docs/limitations.md`
   - `checkpoints/`

## 8. 风险与已知限制

- 只增强桌面端并不能根治浏览器 `contenteditable` 和 PDF viewer。
- Edge 扩展引入后，安装、权限和版本兼容会成为新的运维点。
- `file://` PDF 需要用户显式允许扩展访问本地文件 URL。
- `Windows.Media.Ocr` 在桌面应用上涉及包标识约束，因此本轮只留接口，不直接承诺内置 OCR 落地。
- Oxford 官方词典内容不能默认作为本地离线库处理，除非单独取得授权。

## 9. 阶段出口后的下一决策

本方案固化后，下一个关键决策点是：

```text
是否立即进入 Phase 5A：桌面端提取增强
```

如果继续，Codex 应直接从以下顺序开始：

1. 盘点现有 `ClipboardTextExtractor`、`UIAutomationTextExtractor`、`CompositeTextExtractor`
2. 定义 Phase 5A 的最小代码改动面
3. 先补测试，再落实现有桌面端增强

## 10. 参考资料

- Microsoft UI Automation `TextPattern` Overview  
  https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/ui-automation-textpattern-overview
- Microsoft UI Automation `ValuePattern`  
  https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/implementing-the-ui-automation-value-control-pattern
- Microsoft Edge content scripts  
  https://learn.microsoft.com/en-us/microsoft-edge/extensions-chromium/developer-guide/content-scripts
- Microsoft Edge match patterns / `file://` access  
  https://learn.microsoft.com/en-us/microsoft-edge/extensions-chromium/developer-guide/match-patterns
- Windows `Windows.Media.Ocr` namespace  
  https://learn.microsoft.com/en-us/uwp/api/windows.media.ocr?view=winrt-28000
- Oxford Dictionaries FAQ  
  https://developer.oxforddictionaries.com/faq
- Oxford Dictionaries API Terms and Conditions  
  https://developer.oxforddictionaries.com/api-terms-and-conditions?tab=commercial
