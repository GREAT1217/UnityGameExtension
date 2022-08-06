# BatchEditor

预制体批量编辑器。

![BatchEditor](https://gitee.com/great1217/cdn/raw/master/images/BatchEditor.png)

通过菜单栏 "Game Extension/Batch Editor" 打开批量编辑器窗口，可打开多个。

基础窗口提供了编辑对象列表 Target Objects、批量辅助器接口 Batch Helper。

单选或多选对象，拖入 Target Objects 列表中。

选择不同的 Batch Helper 进行不同功能的编辑。

可继承  ```BatchEditHelperBase``` 自行扩展功能。 

目前已实现批量辅助器：批量替换 UI 字体 ```BatchEditUIFontHeper``` 、批量替换 UI Sprite ```BatchEditUISpriteHelper``` 。