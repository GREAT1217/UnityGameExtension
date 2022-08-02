# UGUIExtension

UGUI 扩展与常用优化。

此工具主要优化 UGUI 的 Raycast Target、Mask 和 三角面绘制，包含：

> 取消不必要的 Raycast Target ，降低每帧检测射线的性能消耗。
>
> 取消 Text.RichText，减少三角面的绘制（默认取消，根据自身情况勾选）。
>
> 根据自身情况选择 Mask 或 RectMask2D：前者内容可以合批，适合一个界面中使用多个；后者产生 drawcall 少，但不能合批。

- 提供了自定义的 UGUI 创建菜单 Custom UI，用于创建 UI 时选择可优化的选项：是否勾选 RaycastTarget，是否创建 Text 子物体，使用 Mask 或 RactMask2D 。

![Custom_UI](https://gitee.com/great1217/cdn/raw/master/images/Custom_UI.jpg)

- 提供了 RaycastTarget 查看器，可在菜单栏勾选 “Game Extension / Debug Raycast Target” 即可在 Scene 窗口中查看 UI 中的 RaycastTarget 对象，支持在预制体预览场景查看。

![UI_DebugRaycast](https://gitee.com/great1217/cdn/raw/master/images/UI_DebugRaycast.jpg)

- 增加了由 UWA 开源的两个 UI 优化组件：[UGUI 降低填充率技巧两则 ](https://blog.uwa4d.com/archives/fillrate.html)。

> ```PolygnImage``` 作为 Image 组件的辅助工具，优化三角面的绘制，减少无用填充。
>
> ```RactRaycast2D``` 用于优化接收射线检测的透明图片，不绘制三角面，不增加 drawcall 、不打断合批。
>
> 可在创建菜单中使用 Custom UI 创建：透明射线遮挡矩形 RectRaycast、透明矩形按钮 RectButton。

