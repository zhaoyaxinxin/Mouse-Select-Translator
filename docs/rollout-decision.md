# MouseSelectTranslator 推进决策记录

## Decision

项目推进方式已确定为：

```text
方案 C：按依赖与风险分层推进
```

## 决策时间

2026-05-21

## 候选方案

1. 方案 A：按原任务链顺序推进
2. 方案 B：按用户闭环切片推进
3. 方案 C：按依赖与风险分层推进
4. 方案 D：双轨推进

## 最终选择

用户选择：

```text
3
```

对应：

```text
方案 C：按依赖与风险分层推进
```

## 选择原因

- 先冻结 Core 契约，降低多轮 AI 开发时的返工概率
- 先验证 Hook、UIA、Clipboard、Overlay 等平台风险点
- 把系统能力验证和产品闭环集成拆开，避免阶段互相污染
- 更适合作为后续 Codex 按阶段接力执行的主线文档

## 决策影响

- 后续开发必须按 Phase 1 到 Phase 4 顺序推进
- 未完成风险验证前，不进入完整闭环编排
- 未完成 MVP 闭环前，不进入产品控制和真实 API 接入

## 关联文档

- [docs/modular-rollout-options.md](./modular-rollout-options.md)
- [docs/development-plan.md](./development-plan.md)
- [MouseSelectTranslator_Codex_Project_Plan.md](../MouseSelectTranslator_Codex_Project_Plan.md)
