# ComponentCollection

组件自动绑定工具。本项目代码灵感来源于 [CatImmortal](https://github.com/CatImmortal) 的 [ComponentAutoBind](https://github.com/CatImmortal/ComponentAutoBindTool)。

重构了 Editor 代码逻辑，增加了一些编辑器辅助功能。

实现了 Editor 使用字符串、Runtime 使用索引获取组件引用，以消除运行时字符串占用的内存分配。

根据物体命名后缀，进行组件映射识别，支持单个物体的多组件识别。

提供了默认的组件映射规则，支持自定义组件映射规则。

支持自定义组件字段生成规则，包含字段前缀和字段命名类型。

## 基本用法

本项目包含设置文件 ComponentCollectionSettings.asset，可放置于 Asset 目录下任意位置。

设置文件仅需要一个，丢失后可在 Project 窗口中通过 "Create/Game/Component Collection Settings" 创建。

1 根据 ComponentCollectionSettings.asset 编辑的组件映射规则，编辑物体名；

2 为物体挂载 ComponentCollection 脚本；

3 设置字段生成规则 Field Name Rule，包含字段前缀，字段命名类型：组件类型名称 TypeName、组件映射的键值 TypeKey；

4.1 点击 Collect Components To Update，识别并绑定此物体（及子物体）中所有符合映射规则的组件引用。

4.2 点击 Collect Components To Add，在不修改已保存的组件列表的顺序的情况下，识别并绑定增加的组件。（首次绑定不需要）

4.3 点击 Remove Null Component 移除组件列表中的空组件。（首次绑定不需要）

5 设置自动生成的绑定代码的命名空间 NameSpace、类名 Class Name 与保存路径 Code Save Path。

6.1 点击 Generate Components Code，自动生成代码文件 ClassName.Components.cs ，包含所有绑定的组件字段。

6.2 点击 Generate Behaviour Code，自动生成代码文件 ClassName.cs，继承自 MonoBehaviour，可直接访问绑定的组件字段。

7 等待生成代码编译后，点击 Add Behaviour Code，自动添加 ClassName.cs 组件。

可以修改 ComponentCollectionSettings.asset 中设置的代码模板 Code Template。

## 其他用法

在不使用 Generate 自动生成代码时，可以自定义调用 ```ComponentCollection 的 GetComponent<T>(int)``` 根据索引获取组件。

自定义调用后，如果修改了子物体层级，或增删了子物体，可以使用 Collect To Add：在不修改旧的组件列表的顺序的情况下，增加组件。

当然，Remove Null 也不能在自定义调用后使用。移除空组件会使索引错乱。

## 感谢

❤ 再次感谢 [CatImmortal](https://github.com/CatImmortal) 的开源。
