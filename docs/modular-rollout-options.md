# MouseSelectTranslator 模块化推进方案候选

## 背景

基于 [MouseSelectTranslator_Codex_Project_Plan.md](../MouseSelectTranslator_Codex_Project_Plan.md) 当前内容，项目已经明确了：

- MVP 范围：鼠标划词 -> 文本提取 -> 翻译 -> 悬浮窗显示
- 分层结构：App / Core / Infrastructure / Tests
- 顺序任务：TASK-0001 到 TASK-1101
- 扩展路线：V0.1 到 V0.5

但原始计划更偏功能清单和模块清单，还没有把“如何按模块推进”明确成一个对后续 Codex 持续执行更稳定的方案。

本文只解决一个问题：如何把当前计划改写成模块化、可检查、可分阶段暂停的推进路径。

## 约束前提

- 不扩展到 OCR、跨平台、账号系统、云同步
- 第一阶段只做 Windows 桌面端 MVP
- 每一阶段结束都必须有可验证出口
- Core 继续保持不依赖 WPF、Win32、Infrastructure
- 优先保住最小闭环，不为了“工程完整感”牺牲可运行性

## 方案 A：按原任务链顺序推进

### 适用场景

你希望最大程度贴合现有项目计划，不重排任务，只把任务补成模块化阶段。

### 推进思路

按现有 TASK 顺序推进，但把相邻任务并成更高一级阶段：

1. 环境与骨架阶段
   - TASK-0001
   - TASK-0101
2. 核心契约阶段
   - TASK-0201
3. 系统输入阶段
   - TASK-0301
   - TASK-0401
4. 翻译与输出阶段
   - TASK-0501
   - TASK-0601
5. 闭环编排阶段
   - TASK-0701
6. 运行控制阶段
   - TASK-0801
   - TASK-0901
7. 增强与收尾阶段
   - TASK-1001
   - TASK-1101

### 模块边界

- 环境骨架模块：解决方案、项目引用、构建链
- 核心契约模块：状态机、请求对象、缓存、校验、接口
- 输入模块：Hook、UIA、Clipboard、SendInput
- 输出模块：Mock 翻译、Overlay
- 编排模块：SelectionCoordinator
- 运行模块：Tray、Hotkey、Settings
- 扩展模块：真实翻译、文档补齐

### 阶段出口

- 阶段 1：`dotnet build`、`dotnet test` 可通过
- 阶段 2：Core 类型稳定，测试可跑
- 阶段 3：能捕获拖选并读到文本
- 阶段 4：能显示 Mock 翻译悬浮框
- 阶段 5：形成最小闭环
- 阶段 6：具备暂停/恢复和配置能力

### 优点

- 最接近原文档，执行阻力最小
- 任务编号和推进顺序一致，便于追踪
- 对后续 AI 最友好，不需要重新解释计划

### 缺点

- 前期更偏“搭架子”，首个可见演示出现稍晚
- 某些高风险能力验证偏后，若文本提取出问题，会在中期暴露

## 方案 B：按用户闭环切片推进

### 适用场景

你想尽快看到可演示版本，优先让“从划词到看到结果”尽早跑通。

### 推进思路

按用户链路拆成 4 个垂直切片，每个切片都以可演示体验结束：

1. 最小演示切片
   - 固定文本 -> Mock 翻译 -> Overlay 显示
2. 输入接入切片
   - Mouse Hook -> 拖选判定 -> 触发协调器
3. 文本获取切片
   - UIA -> Clipboard 兜底 -> 文本校验
4. 运行完善切片
   - Tray -> Hotkey -> Settings -> Real API

### 模块边界

- 显示切片：OverlayWindow、OverlayPlacement、基础 ViewModel
- 触发切片：Win32MouseHook、CursorPositionService、SelectionEvent
- 提取切片：ITextExtractor、UIAutomationTextExtractor、ClipboardTextExtractor
- 控制切片：Tray、Hotkey、Settings、Translation Provider

### 阶段出口

- 切片 1：程序能显示假数据翻译框
- 切片 2：真实拖选能触发假数据翻译框
- 切片 3：真实拖选能显示真实提取文本的 Mock 翻译
- 切片 4：具备暂停、托盘、配置、真实接口

### 优点

- 最早得到“可看见”的产品反馈
- 很容易做里程碑演示和手测
- 能更快暴露悬浮窗体验问题

### 缺点

- 为了快速闭环，前几阶段会暂时带一点替身实现
- 若管理不好，容易在后面补契约时出现返工

## 方案 C：按依赖与风险分层推进

### 适用场景

你希望降低 AI 连续开发时的返工率，先把最容易影响全局的契约和高风险能力定住。

### 推进思路

先拆“稳定层”，再拆“风险层”，最后做编排和交互：

1. 稳定层
   - 解决方案结构
   - Core 契约
   - 测试骨架
   - docs 基础文档
2. 风险验证层
   - Mouse Hook
   - UI Automation
   - Clipboard 恢复
   - Overlay 越界定位
3. 闭环编排层
   - SelectionCoordinator
   - MockTranslationService
   - TranslationCache
4. 产品控制层
   - Tray
   - Hotkey
   - Settings
   - OpenAI-Compatible API

### 模块边界

- 稳定层模块只定义接口、状态和可测试规则
- 风险层模块只验证平台能力，不急着做完整产品体验
- 编排层只负责串联，不直接持有 Win32/WPF 细节
- 控制层最后接入用户可操作功能

### 阶段出口

- 层 1：契约冻结，可安全并行开发
- 层 2：确认最难的 Windows 能力可行
- 层 3：最小闭环打通
- 层 4：产品化控制能力补齐

### 优点

- 最适合长期让 AI 按文档持续接力
- 模块边界最清楚，返工概率最低
- 风险暴露比方案 A 更早

### 缺点

- 早期成果更偏工程验证，不如方案 B 直观
- 文档与阶段定义会更重一些

## 方案 D：双轨推进

### 适用场景

你想同时兼顾“尽快看到效果”和“保持工程边界”，接受主线加验证支线的管理方式。

### 推进思路

把工作拆成主线和验证线：

1. 主线
   - 解决方案结构
   - Core 契约
   - SelectionCoordinator
   - MockTranslationService
   - Overlay
2. 验证线
   - Mouse Hook spike
   - UIA spike
   - Clipboard 恢复 spike
3. 合流阶段
   - 用已验证的能力替换主线中的占位实现
4. 产品化阶段
   - Tray
   - Hotkey
   - Settings
   - Real API

### 模块边界

- 主线模块保证代码骨架可集成
- 验证线模块只回答“这项系统能力是否可靠”
- 合流阶段统一接口，不保留长期 spike 代码

### 阶段出口

- 主线早期能跑出假闭环
- 验证线分别给出可行/不可行结论
- 合流后才进入 MVP 定稿

### 优点

- 风险暴露早，演示能力也早
- 适合人机协作或多轮 Codex 接力

### 缺点

- 过程管理最复杂
- 如果没有严格文档约束，容易留下 spike 代码污染正式结构

## 对比表

| 维度 | 方案 A | 方案 B | 方案 C | 方案 D |
|---|---|---|---|---|
| 对原计划改动 | 最小 | 中等 | 中等 | 较大 |
| 首次可演示速度 | 中 | 最快 | 较慢 | 快 |
| 模块边界清晰度 | 中 | 中 | 最高 | 高 |
| 风险前置程度 | 中 | 中 | 高 | 最高 |
| 后续 AI 接力稳定性 | 高 | 中 | 最高 | 中 |
| 管理复杂度 | 低 | 中 | 中 | 最高 |

## 建议的统一阶段检查点

无论选哪种方案，都建议固定以下检查点：

1. 骨架检查点
   - 解决方案结构完成
   - 项目引用正确
   - `dotnet build` 通过
2. 契约检查点
   - Core 接口和对象稳定
   - Core 测试通过
3. 平台能力检查点
   - Hook、UIA、Clipboard 至少各自完成一次人工验证
4. MVP 闭环检查点
   - 划词 -> 提取 -> Mock 翻译 -> 悬浮窗显示跑通
5. 产品控制检查点
   - Hotkey、Tray、Settings 可用
6. 真实服务检查点
   - OpenAI-Compatible API 接入成功
   - 失败提示、超时、隐私边界明确

## 后续固化方式

用户选定方案后，再继续输出：

- 正式阶段拆解文档
- 每阶段任务清单
- 每阶段验收标准
- 对应 changelog / decision 记录
