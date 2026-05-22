# MouseSelectTranslator 后续 Codex 推进方案候选

## 1. 背景

当前项目已经完成：

- Windows 桌面端最小闭环
- 鼠标拖选监听
- `UI Automation -> Clipboard` 双通道提取
- Mock / DeepSeek 翻译
- 悬浮窗、托盘、热键、配置

当前暴露出的主要问题：

- `Ctrl+C` 兜底太脆弱，固定等待 `150ms` 不足以覆盖浏览器和复杂控件
- Edge 浏览器中的文本框、`contenteditable`、浏览器 PDF viewer 成功率低
- 还没有统一的 OCR 扩展接口，后续无法平滑接入图像文本兜底

本轮目标不是扩展到完整 OCR 产品，而是把下一阶段面向 Codex 的推进方案组织清楚，让后续实现可持续、可验证、可分阶段交付。

## 2. 官方资料约束

本轮方案基于以下官方资料组织：

- Microsoft UI Automation `TextPattern` 文档说明：`TextPattern` 并不能覆盖所有提供程序特有文本特性，必要时要访问其他模式或原生对象模型。
- Microsoft UI Automation `ValuePattern` 文档说明：适合“内建值可表示为字符串”的控件，可作为文本框兼容补充。
- Microsoft Edge 扩展内容脚本文档说明：内容脚本可以直接访问页面 DOM，并通过消息与扩展其他部分通信。
- Microsoft Edge `file://` 匹配规则文档说明：如果扩展要处理本地 PDF 文件，需要显式申请并启用 `file` URL 访问权限。
- Oxford Dictionaries 条款与 FAQ：默认不允许本地缓存、存储或离线保存 API 内容，除非另行取得企业许可。

## 3. 本轮固定目标

无论选哪种方向，这一轮都围绕以下三项展开：

1. 增强 `Ctrl+C` 兜底
2. 提升 Edge 浏览器文本框 / PDF 成功率
3. 为 OCR 预留正式接口

## 4. 共同技术底线

- 不破坏现有桌面端 MVP 主链路
- 保持 Core 继续只放契约、流程和策略，不混入 Edge / OCR 平台细节
- 不在这一轮直接做完整 OCR 识别产品，除非用户明确选择高覆盖方案
- 不把 Oxford 官方词典内容作为默认本地离线库方案
- 每个阶段都必须有可验证出口，而不是只增加复杂度

## 5. 方案 A：桌面端强化版

### 定位

优先补强现有 WPF 桌面程序，不引入浏览器扩展，只通过更强的 UIA 与 Clipboard 兜底提升 Edge 成功率。

### 核心内容

- `ClipboardTextExtractor`
  - 从固定等待改为短轮询
  - 检测剪贴板变更而不是只看一次结果
  - 失败原因细分
- `UIAutomationTextExtractor`
  - 在 `TextPattern` 之外补 `ValuePattern`
  - 区分只读文本框与可编辑文本框
- `CompositeTextExtractor`
  - 引入提取策略链和失败分类
- OCR
  - 只加 `IOcrTextExtractor` 接口与空实现占位

### Edge 覆盖能力

- 普通网页正文：中等提升
- 浏览器文本框：中等提升
- 浏览器 PDF：有限提升

### 优点

- 改动集中在现有桌面代码
- 后续维护成本最低
- 不需要扩展安装流程

### 缺点

- 对浏览器 PDF 的提升空间有限
- DOM / `contenteditable` 场景仍然受浏览器内部实现影响

### 推荐指数

`7/10`

## 6. 方案 B：桌面端 + Edge 双通道版

### 定位

保留桌面程序主链路，同时引入一个最小 Edge 扩展，专门处理网页 DOM、文本框、`contenteditable` 和浏览器 PDF 文本层。

### 核心内容

- 桌面端继续做：
  - `Ctrl+C` 增强轮询兜底
  - `TextPattern + ValuePattern` 扩展
  - OCR 接口占位
- 新增 Edge 扩展侧车：
  - 内容脚本读取 `window.getSelection()`
  - 读取 `input` / `textarea` / `contenteditable` 当前选区
  - 针对浏览器 PDF viewer 的文本层做专门提取
  - 通过本地桥接或剪贴板协议把选中文本回送给桌面程序
- 本地 PDF 文件访问
  - 若支持 `file://` PDF，需要在扩展权限和用户设置里显式开启

### Edge 覆盖能力

- 普通网页正文：高
- 浏览器文本框：高
- 浏览器 PDF：高于桌面-only 方案

### 优点

- 最符合“为什么浏览器里不稳定”的根因
- 对 Edge 文本框和 PDF 的成功率提升最大
- OCR 仍保持延后，不会把本轮做得太重

### 缺点

- 引入桌面端 + 扩展双交付物
- 需要额外的安装、调试和通信约束

### 推荐指数

`9/10`

## 7. 方案 C：桌面端 + OCR 优先版

### 定位

把“任意可见内容”覆盖率放在第一位，先把 OCR 兜底链建起来，Edge 专项适配只做有限补强。

### 核心内容

- `Ctrl+C` 兜底增强
- `TextPattern + ValuePattern`
- 正式定义 OCR 接口簇：
  - `IOcrTextExtractor`
  - `IScreenCaptureService`
  - `IOcrRegionSelector`
- 直接补一个最小 OCR 实现路径
  - 截屏选区附近区域
  - OCR 识别
  - 返回候选文本
- Edge 仅做桌面侧兼容增强，不引入扩展

### Edge 覆盖能力

- 普通网页正文：中等
- 浏览器文本框：中等
- 浏览器 PDF：中等
- 图片 / 扫描 PDF：开始具备能力

### 优点

- 更接近“任意勾选内容”的长期目标
- 对图片文字和扫描 PDF 更有帮助

### 缺点

- 隐私、性能、误识别复杂度会明显上升
- 对你当前最痛的 Edge 文本框 / PDF 问题，不一定是最短路径

### 推荐指数

`6/10`

## 8. 方案 D：全通道推进版

### 定位

同一轮同时做桌面增强、Edge 扩展、OCR 接口，以及最小 OCR 实现。

### 核心内容

- 方案 B 全部内容
- 再叠加方案 C 的 OCR 接口和最小 OCR 落地

### Edge 覆盖能力

- 普通网页正文：高
- 浏览器文本框：高
- 浏览器 PDF：高
- 图片 / 扫描内容：中等起步

### 优点

- 覆盖面最大
- 一轮后可形成多通道提取框架

### 缺点

- 本轮复杂度最高
- 风险、联调成本、回归面积都最大
- 不符合“最小改动先解决最高价值问题”的原则

### 推荐指数

`5/10`

## 9. 推荐排序

### 推荐 1：方案 B

原因：

1. 直接针对你当前的真实痛点：Edge 文本框和浏览器 PDF。
2. 不把 OCR 过早拉进主链路，能控制复杂度。
3. 仍然满足“增强 `Ctrl+C` 兜底 + OCR 预留接口”的要求。

### 推荐 2：方案 A

适合只想保持单一桌面程序，不想引入扩展侧车。

## 10. 选定后建议的阶段拆分

如果选择方案 B，建议后续固化为以下阶段：

### Phase 5A：桌面提取增强

- `ClipboardTextExtractor` 改成轮询等待
- 增加剪贴板变化检测
- `UIAutomationTextExtractor` 增加 `ValuePattern`
- 失败原因结构化

验收：

- 记事本、普通网页正文成功率高于当前版本
- 单元测试补齐剪贴板策略和提取链策略

### Phase 5B：Edge 扩展最小版

- 内容脚本读取网页选区
- 读取文本框 / `textarea` / `contenteditable`
- 定义与桌面端的数据交换协议

验收：

- Edge 普通网页正文可稳定把选区传回桌面端
- Edge 文本框可稳定提取选区文本

### Phase 5C：Edge PDF 专项适配

- 针对浏览器 PDF viewer 文本层补提取逻辑
- 补 `file://` 场景权限说明与手测清单

验收：

- 浏览器内 PDF 文本层可稳定获取选区
- 本地 PDF 文件访问权限说明明确

### Phase 5D：OCR 接口预留

- 定义 `IOcrTextExtractor`
- 定义 `IScreenCaptureService`
- 在 `CompositeTextExtractor` 中预留第三通道挂点

验收：

- 不接入 OCR 实现也不影响当前主链路
- 后续可以独立迭代 OCR 模块

## 11. 对 Codex 的实现约束

- 先读现有 `docs/development-plan.md`、`docs/limitations.md`、`docs/manual-test-checklist.md`
- 只实现当前选定方案，不擅自跨到更重方案
- 不直接把 OCR 做成主流程默认强依赖
- 不把 Oxford API 内容下载成本地离线库，除非用户明确提供许可路径
- 每完成一个阶段都更新：
  - `docs/development-plan.md`
  - `docs/manual-test-checklist.md`
  - `docs/limitations.md`
  - `checkpoints/`

## 12. 资料链接

- Microsoft UI Automation `TextPattern`
- Microsoft UI Automation `ValuePattern`
- Microsoft Edge 内容脚本示例
- Microsoft Edge `file://` 匹配与权限
- Oxford Dictionaries FAQ
- Oxford Dictionaries API Terms and Conditions
