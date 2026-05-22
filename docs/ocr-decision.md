# OCR 决策记录

## 决策

当前项目的 OCR 实现路线固定为：

```text
方案 B：本地 OCR 引擎版
```

## 已放弃的候选

### 方案 A：Windows 原生 OCR

放弃原因：

- `Windows.Media.Ocr` 对桌面应用存在 `package identity` 约束
- 与当前普通 WPF exe 交付方式不匹配

### 方案 C：云端 OCR 服务

放弃原因：

- 会引入截图上传的隐私边界
- 依赖网络和外部服务
- 对当前桌面工具定位不是最短路径

## 采用原因

1. 最符合当前 WPF exe 形态
2. 能覆盖扫描版 PDF 和图片文字
3. 不再依赖 Edge PDF viewer 是否暴露选区
4. 可以保持 OCR 作为最后兜底，而不破坏现有链路

## 正式实施文档

- `docs/ocr-local-plan.md`
