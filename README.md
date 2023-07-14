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

## 通过OnlineStatus判断GM

## 绘制腾讯文档中的玩家（该功能处于测试阶段，还不稳定》

## 其他

建议结合qol bar使用本插件  
qolbar预设示例：
``H4sIAAAAAAAACuVVvUrDUBh9FflwM41pGtvmbjYoFopVIyhKkfQ2aULTpCSpVEpBB8FNHMTZTZfiKAV9GvvjW/jdpIklXZztcMn5fs65554lfVgPrjo6EDh0KyXN48vtjusFltPc2OkxVHYMl1uLhsBBPQvE6do2IhFIP83GoxjNhX0HZ6eK23UCLFpABA78CpDzJaZqLhO/RqPJ4xsWQUik2Nrs0USodQDE0GxfjzQjW+1o1QYiibIk5wuivMXq7bBvnQHJ8gxU0YSACE8NyyMcsz5tRIu0oUbfEyRIDKjV5DaqYJO1OihS5CQUoOpcmO7GYL/0S9hbgEqCLyy8Y8D9KYvJ++30dZTOgtp6uLGCgYzvH76vb5YCMbVgNfOYDT+nH8N0Hr6+onGM755mzy/pOGzL/3d51Dgwk1YDiMiBFupfhtjEAObDOppBZUwuFjtOILMi8LliQZJlOc9lBF5GYYwhXnVDNX/uzsNnhsCIjftMQQof46DzwOsyFpBMdoDD5Lfh428jQhS1IoRGQeRzvMgLMPgBkRbGiJIGAAA=``  

仓库地址：``https://raw.githubusercontent.com/uiharuayako/DalamudPlugins/main/pluginmaster.json``  
添加仓库以获取更新
