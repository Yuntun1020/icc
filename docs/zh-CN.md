# Screen Exposure 中文使用文档

Screen Exposure 是一个 Windows 屏幕调色工具，适合在游戏画面过亮、室外场景曝光刺眼时快速压暗画面。它提供类似 Photoshop 曲线的调节方式，并可以把当前曲线保存为带 `vcgt` 校准表的 ICC 配置文件。

## 主要功能

- RGB 总曲线调节
- R / G / B 单通道曲线调节
- 曝光、对比度、Gamma 调节
- 室外压暗、夜间、默认预设
- 实时写入 Windows Gamma Ramp
- 恢复启动前的原始屏幕曲线
- 生成并关联带 `vcgt` 的 ICC 配置文件
- 导入外部 `.icc` / `.icm` 滤镜文件并关联到显示器
- 打包为 Windows x64 单文件 exe

## 下载与运行

从 GitHub Release 下载：

https://github.com/Yuntun1020/icc/releases/tag/v0.1.0

下载 `ScreenExposure.App.exe` 后直接运行即可。程序启动时处于中性状态，不会立刻修改屏幕曲线；点击 **应用** 或选择预设后才会写入屏幕。

## 基本用法

1. 打开 `ScreenExposure.App.exe`。
2. 在右侧调整 **曝光 Stops**、**对比度**、**Gamma**。
3. 在左侧曲线面板拖动控制点调整明暗曲线。
4. 如果需要调单独颜色通道，选择 `R`、`G` 或 `B` 后再调整曲线。
5. 点击 **应用** 写入当前屏幕曲线。
6. 如果效果不合适，点击 **恢复** 还原启动前的屏幕曲线。

## 曲线面板

- 点击曲线区域可添加控制点。
- 拖动控制点可改变曲线。
- 选择控制点后按 `Delete` 可删除中间控制点。
- 左下和右上端点只能上下移动，避免曲线输入范围失效。

## 预设说明

- **室外压暗**：用于游戏室外、雪地、沙漠、强光场景，重点压暗高亮区域。
- **夜间**：整体更暗，并略微降低蓝色通道。
- **默认**：恢复中性参数和线性曲线。

## ICC 同步

点击 **保存当前曲线为 ICC** 后，程序会：

1. 读取系统自带的 sRGB ICC 配置文件作为基础。
2. 根据当前曝光、Gamma、对比度和曲线生成 256 点 RGB Gamma Ramp。
3. 把这条曲线写入 ICC 的 `vcgt` 标签。
4. 调用 Windows 色彩管理 API 安装并关联到当前显示器。

点击 **导入 ICC 滤镜文件** 后，可以选择外部 `.icc` 或 `.icm` 文件。程序会先校验文件扩展名和 ICC `acsp` 签名，再复制到 Windows 色彩配置目录并关联到当前显示器。

注意：ICC 不是实时游戏滤镜。程序实时生效依靠 Windows Gamma Ramp；ICC 保存更适合把当前校准曲线持久化到系统色彩配置中。

## 兼容性说明

以下情况可能导致效果不明显或无效：

- 已开启 HDR。
- 游戏使用独占全屏或受保护输出。
- 显卡驱动、显示器软件或系统夜间模式覆盖了 Gamma Ramp。
- 游戏本身绕过 Windows 色彩管理。

建议先关闭 HDR，用无边框窗口或窗口模式测试。

## 构建源码

需要 .NET 8 SDK。

```powershell
dotnet build .\ScreenExposure.slnx
dotnet test .\ScreenExposure.slnx
```

生成单文件 exe：

```powershell
.\build-single-exe.ps1
```

产物会输出到 `dist/`：

- `ScreenExposure.App.exe`
- `SHA256SUMS.txt`

## 安全恢复

如果屏幕颜色异常：

1. 先点击 **恢复**。
2. 如果窗口无响应，关闭程序。
3. 如果仍未恢复，可以注销或重启 Windows，系统会重新加载显示曲线。

## 当前限制

- 暂不支持 HDR 专用色调映射。
- 暂不支持每个游戏独立自动切换预设。
- 暂不支持复杂色彩空间转换，只处理屏幕 Gamma Ramp 和 ICC `vcgt` 曲线。
