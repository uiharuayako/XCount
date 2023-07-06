# XCount

## 基本功能：人数统计

统计周围玩家总数量，战职/非战职/跨服玩家数量，搜索特定id的玩家  
```
/xc 打开主界面
/xcset 打开设置界面
/xclist 打开周边玩家列表

...还有一些命令详见插件安装界面
```

## UI功能：状态栏显示人数

目前能显示以下种类的人数：
- \<all>：全部玩家数量，实时更新
- \<war>：战职玩家数量，实际上是<all>-<nowar>算出来的
- \<nowar>：非战职玩家数量
- \<foreign>：放浪神数量
- \<inv>：不可见玩家数量，大部分情况下是切换区域时临时不可见
- \<union>：合并统计，累计计算所有你遇到的玩家，即使他们现在不在你身边，使用命令``/xcclear``清空合并统计
- \<excel>：你身边的玩家中，存在于在线列表中的玩家人数

![状态栏显示](人数.png)

## 查找“冒险者”（技术预览版，有bug请反馈喵，谢谢喵）

增加冒险者警报，检测到周围存在“冒险者”职业的玩家时，可以在其身下画一个圈圈，也可以把警报发到聊天栏，可以在设置里的“冒险者警报”一栏进行相关设置。  

正常的玩家不会拥有这个职业，可能可以用来判断GM。已经经过验证的是，开启了隐身功能的“玩家”，仍会出现在卫月的ObjectTable中，原理上，检测隐身的“玩家”是有可能的。

感谢群友胡萝卜布丁的帮助，找到了一种查找隐身GM的可能方式。不过客观来说，这一项的结果永远都是推测，因为不可能真的进行完整且确定的测试。

*这个功能还在测试中，很可能存在bug，也许在一些情况下，正在进行跨服传送（或进行其他特定行为）的玩家也会被识别成“冒险者”职业，如果频繁误报请关闭警报的开关，保留一个绘制就行，或者在处于副本内/挂机搓东西等稳定环境时再手动开启*

## 其他

建议结合qol bar使用本插件  
qolbar预设示例：
``H4sIAAAAAAAACuVVvUrDUBh9FflwM41pGtvmbjYoFopVIyhKkfQ2aULTpCSpVEpBB8FNHMTZTZfiKAV9GvvjW/jdpIklXZztcMn5fs65554lfVgPrjo6EDh0KyXN48vtjusFltPc2OkxVHYMl1uLhsBBPQvE6do2IhFIP83GoxjNhX0HZ6eK23UCLFpABA78CpDzJaZqLhO/RqPJ4xsWQUik2Nrs0USodQDE0GxfjzQjW+1o1QYiibIk5wuivMXq7bBvnQHJ8gxU0YSACE8NyyMcsz5tRIu0oUbfEyRIDKjV5DaqYJO1OihS5CQUoOpcmO7GYL/0S9hbgEqCLyy8Y8D9KYvJ++30dZTOgtp6uLGCgYzvH76vb5YCMbVgNfOYDT+nH8N0Hr6+onGM755mzy/pOGzL/3d51Dgwk1YDiMiBFupfhtjEAObDOppBZUwuFjtOILMi8LliQZJlOc9lBF5GYYwhXnVDNX/uzsNnhsCIjftMQQof46DzwOsyFpBMdoDD5Lfh428jQhS1IoRGQeRzvMgLMPgBkRbGiJIGAAA=``  

仓库地址：``https://raw.githubusercontent.com/uiharuayako/DalamudPlugins/main/pluginmaster.json``  
添加仓库以获取更新
