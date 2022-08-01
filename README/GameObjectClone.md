# GameObjectClone

克隆 GameObject 。

根据目标物体 Target 的结构层级，检查并克隆模板 Template 的组件数据（仅复制值类型和String）。

主要用于运行时更新预制体：设置预制体为 Target ，运行时预制体对象为 Template 。
