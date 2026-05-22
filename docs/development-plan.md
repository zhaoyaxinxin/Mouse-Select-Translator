# MouseSelectTranslator 开发推进方案

## 1. 方案结论

当前项目采用：

```text
方案 C：按依赖与风险分层推进
```

选择原因：

- 先冻结 Core 契约，减少后续 AI 连续开发时的返工
- 先验证 Win32 Hook、UI Automation、Clipboard、Overlay 等高风险平台能力
- 把“系统能力验证”和“产品闭环编排”拆开，避免早期互相污染
- 更适合后续按阶段交接给 Codex 持续执行

## 2. 总体目标

在不扩展 MVP 范围的前提下，完成以下最小闭环：

```text
鼠标拖选 -> 读取选中文本 -> 文本校验 -> Mock 翻译 -> 悬浮窗显示
```

并在此基础上补齐：

- 托盘控制
- 快捷键暂停/恢复
- JSON 配置
- OpenAI-Compatible 翻译服务
- 隐私与限制文档

## 当前状态

```text
当前阶段：Phase 4：产品控制层
状态：已完成
结果：已完成 Win32 Hook、UIA/Clipboard 双通道提取、SelectionCoordinator、Mock/OpenAI-Compatible 翻译、Overlay、Tray、Hotkey、Settings
验证：dotnet build / dotnet test 已通过，应用可启动且不会立即崩溃
下一动作：执行 docs/manual-test-checklist.md 中的人工场景验证
```

## 3. 执行原则

1. 只推进当前阶段，不提前实现下一阶段功能。
2. 每个阶段结束必须有明确出口，不以“代码写了很多”作为完成标准。
3. Core 不引用 WPF、Win32、Infrastructure。
4. 风险验证阶段允许做最小可用实现，但不能留下长期无约束的 spike 代码。
5. 未通过当前阶段出口前，不进入下一阶段。
6. 任何无法稳定实现的系统能力，必须写入 `docs/limitations.md`。

## 4. 阶段划分

### Phase 1：稳定层

#### 目标

冻结解决方案结构、领域契约、测试骨架和基础文档，让后续开发有稳定边界。

#### 范围内

- 环境探测
- 解决方案和项目结构
- App / Core / Infrastructure / Tests 项目引用关系
- Core 中的领域对象、接口、请求/响应模型、状态机
- Core 单元测试骨架
- 基础文档骨架

#### 不做

- Win32 Hook 细节
- UI Automation 细节
- 剪贴板模拟复制
- 悬浮窗真实定位逻辑
- 真正的翻译调用

#### 对应原任务

- TASK-0001：环境探测
- TASK-0101：创建解决方案和项目结构
- TASK-0201：实现核心领域对象
- TASK-1101：补充文档的初始化部分

#### 产出模块

- `MouseSelectTranslator.sln`
- `src/MouseTranslator.App`
- `src/MouseTranslator.Core`
- `src/MouseTranslator.Infrastructure`
- `src/MouseTranslator.Tests`
- Core 中的：
  - `SelectionState`
  - `SelectionOptions`
  - `SelectionEvent`
  - `ITextExtractor`
  - `TextExtractionResult`
  - `TextValidationService`
  - `ITranslationService`
  - `TranslationRequest`
  - `TranslationResult`
  - `TranslationCache`
  - `OverlayRequest`
  - `OverlayPlacement`
- 文档：
  - `docs/development-plan.md`
  - `docs/privacy.md`
  - `docs/limitations.md`
  - `docs/manual-test-checklist.md`

#### 阶段出口

- `dotnet restore` 通过
- `dotnet build` 通过
- `dotnet test` 至少能执行当前 Core 测试
- Core 项目不引用 WPF、Win32、Infrastructure
- 基础文档已存在，后续阶段可增量更新

### Phase 2：风险验证层

#### 目标

尽早确认最难的不确定性是否可行，尤其是 Windows 平台能力。

#### 范围内

- Win32 低级鼠标 Hook
- 拖选判定
- CursorPositionService
- UI Automation 读取选中文本
- Clipboard 兜底提取
- SendInput 模拟 `Ctrl+C`
- Overlay 越界定位和隐藏规则的最低可行实现

#### 不做

- 最终产品级交互细节
- 托盘菜单
- 全量设置面板
- 真实翻译 API

#### 对应原任务

- TASK-0301：实现鼠标 Hook
- TASK-0401：实现文本提取器
- TASK-0601：实现悬浮窗口的风险能力部分

#### 产出模块

- `Win32MouseHook`
- `NativeMethods`
- `CursorPositionService`
- `UIAutomationTextExtractor`
- `ClipboardTextExtractor`
- `CompositeTextExtractor`
- `SendInputService`
- `OverlayWindow`
- `OverlayPlacement`

#### 阶段出口

- 拖选操作能被识别，单击不会误触发
- 至少在记事本完成一次 UIA 或 Clipboard 文本提取验证
- 剪贴板兜底在失败时不会导致程序崩溃
- 悬浮窗可以显示并能基本避开屏幕越界
- 退出程序后 Hook 被正确释放

#### 风险结论要求

本阶段结束时必须明确写出：

- 哪些应用支持 UI Automation
- 哪些场景只能依赖 Clipboard
- 剪贴板恢复有哪些已知不完整情况
- Overlay 在多屏/边缘位置的已知限制

### Phase 3：闭环编排层

#### 目标

把已验证的平台能力串成最小 MVP 闭环，但先只接 Mock 翻译。

#### 范围内

- SelectionCoordinator
- TextValidationService 规则落地
- TranslationCache
- MockTranslationService
- 鼠标释放后的延迟、防抖、去重
- Overlay 请求组装与显示

#### 不做

- 真实在线翻译
- 产品设置持久化
- 托盘交互完善

#### 对应原任务

- TASK-0501：实现 Mock 翻译服务
- TASK-0701：串联 SelectionCoordinator
- TASK-0401：文本提取器与协调器集成部分
- TASK-0601：Overlay 与协调器集成部分

#### 产出模块

- `SelectionCoordinator`
- `MockTranslationService`
- `TranslationCache`
- 文本清洗与去重规则的正式接入
- 端到端调用链：
  - Hook 事件
  - 提取文本
  - 校验
  - 查缓存
  - Mock 翻译
  - Overlay 显示

#### 阶段出口

- 在记事本中拖选英文后可显示悬浮翻译框
- 同一段文本在短时间内不会重复翻译
- 非法文本会被正确过滤
- 整条调用链不把原文和译文写入日志
- MVP 最小闭环成立

### Phase 4：产品控制层

#### 目标

在闭环可运行的基础上补足可控性、配置能力和真实翻译服务。

#### 范围内

- 托盘菜单
- 全局快捷键
- 启用/暂停状态切换
- `Esc` 隐藏悬浮窗
- JSON 设置存储
- OpenAI-Compatible 翻译服务
- 超时和错误提示
- 文档补齐

#### 不做

- OCR
- 历史记录
- 云同步
- 账号系统
- 阅读助手

#### 对应原任务

- TASK-0801：托盘和快捷键
- TASK-0901：配置系统
- TASK-1001：OpenAI-Compatible 翻译服务
- TASK-1101：文档补齐与限制说明

#### 产出模块

- `TrayManager`
- `GlobalHotkeyManager`
- `AppSettings`
- `JsonSettingsStore`
- `OpenAICompatibleTranslationService`
- 完整的：
  - `docs/privacy.md`
  - `docs/limitations.md`
  - `docs/manual-test-checklist.md`

#### 阶段出口

- `Ctrl+Shift+T` 可暂停 / 恢复
- `Esc` 可隐藏悬浮窗
- 托盘菜单可启用、暂停、退出
- 真实翻译服务可通过配置启用
- 错误和超时有可见反馈
- 文档明确写出隐私边界与已知限制

## 5. 阶段依赖关系

```text
Phase 1 稳定层
    ->
Phase 2 风险验证层
    ->
Phase 3 闭环编排层
    ->
Phase 4 产品控制层
```

依赖规则：

- Phase 2 不得先于 Phase 1
- Phase 3 必须建立在风险结论已明确的前提上
- Phase 4 只能在 MVP 闭环已经成立后进入

## 6. 里程碑验收

### Milestone A：结构稳定

- 解决方案结构确定
- Core 契约冻结
- 测试骨架可执行

### Milestone B：平台可行

- Hook、UIA、Clipboard、Overlay 均完成最低可行验证

### Milestone C：MVP 闭环

- 划词 -> 提取 -> Mock 翻译 -> 悬浮窗显示跑通

### Milestone D：产品可控

- Tray、Hotkey、Settings、Real API 接入完成

## 7. 当前建议的执行顺序

如果后续让 Codex 接着做代码，实现顺序建议固定为：

1. 完成 Phase 1 全部内容
2. 只做 Phase 2 的平台验证与最小实现
3. 在 Phase 2 结论明确后，再进入 Phase 3 串联闭环
4. MVP 闭环稳定后，最后进入 Phase 4

## 8. 明确不进入本轮范围

以下内容保持在后续版本，不进入本次推进方案：

- V0.4 阅读助手能力
- V0.5 自动读书助手原型
- OCR、截图识别、区域识别
- 账号、同步、多端
- 生词本、历史记录、插件系统

## 9. 后续文档更新规则

后续每进入一个新阶段，都应同步更新：

## 10. Post-MVP Extension Track

After the MVP and product-control phases are complete, the next approved extension track is fixed to:

```text
Option B: Desktop + Edge dual-channel
```

Scope of the approved extension track:

- strengthen `Ctrl+C` fallback
- improve Edge textbox extraction success rate
- improve Edge browser PDF extraction success rate
- reserve OCR interfaces without making OCR a required runtime dependency

Execution document:

- `docs/codex-phase5-plan.md`

Phase order:

1. Phase 5A: desktop extraction hardening
2. Phase 5B: minimal Edge extension
3. Phase 5C: Edge PDF specialization
4. Phase 5D: OCR interface reservation

- `docs/development-plan.md`：标注当前阶段状态
- `docs/manual-test-checklist.md`：追加对应手测项
- `docs/limitations.md`：补充发现的限制
- `checkpoints/`：新增阶段检查点
## 11. Phase 5 Status

Current extension-track status:

```text
Phase 5A complete
Phase 5B complete
Phase 5C complete
Phase 5D complete
```

Completed outcomes:

- clipboard fallback now polls for clipboard changes instead of waiting once
- UI Automation now supports `TextPattern` and `ValuePattern`
- browser extraction has a loopback bridge plus an unpacked Edge extension in `edge-extension`
- PDF-related browser extraction has a dedicated browser-source path plus updated fallback and permission documentation
- OCR fallback now has request/result contracts, region calculation, local screen capture, and a Tesseract adapter hook

Validation snapshot:

- `dotnet build MouseSelectTranslator.slnx`
- `dotnet test MouseSelectTranslator.slnx`
- browser bridge health endpoint responded after application startup

## 12. OCR Track

After Phase 5, the approved OCR track is fixed to:

```text
Option B: local OCR engine
```

Scope:

- keep the current WPF exe delivery shape
- add OCR only as the last fallback extractor
- target scanned PDFs, image text, and browser PDF failure cases

Execution documents:

- `docs/ocr-implementation-options.md`
- `docs/ocr-local-plan.md`
- `docs/ocr-decision.md`

Recommended order:

1. Phase O1: OCR contracts and region calculation
2. Phase O2: local screen capture
3. Phase O3: local OCR engine integration
4. Phase O4: extraction-chain integration
5. Phase O5: packaging and documentation

Current OCR-track status:

```text
Phase O1 complete
Phase O2 complete
Phase O3 code integration complete, external Tesseract runtime assets still required
Phase O4 complete
Phase O5 complete
```

Current OCR outcomes:

- `SelectionCoordinator` now invokes OCR only after the normal extraction chain fails validation
- `OcrRequest`, `OcrResult`, `OcrRegion`, and OCR-specific screen bounds are formalized in Core
- `GdiScreenCaptureService` captures only the drag-selected region, not the full screen
- `TesseractCommandLineOcrEngine` resolves `tesseract.exe` and `tessdata` from environment variables, app assets, or common install paths
- the app output now carries `assets/ocr/tessdata/README.md` so the runtime path is explicit

Remaining external prerequisite:

- install or supply `tesseract.exe`
- place `eng.traineddata` in `assets/ocr/tessdata` or point `MOUSE_TRANSLATOR_TESSDATA_PATH` at an existing tessdata directory
