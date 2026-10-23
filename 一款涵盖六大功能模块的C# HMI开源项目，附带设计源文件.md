![图片](https://mmbiz.qpic.cn/mmbiz_gif/AiaVicV0EDknQX3fxmkaDQ3MdecQFYVPhVOWSkibgU90XWeMVxqJIbFVjQIaC8G8ATq6acu4chlicdEGqyEeCyuqPw/640?wx_fmt=gif#imgIndex=0)



# 一款涵盖六大功能模块的C# HMI开源项目，附带设计源文件

#   

# 本文介绍

监控总览、参数配置、报警记录、趋势曲线、设备维护、系统设置——这些模块几乎每个工业 HMI 项目都要做一遍。数据采集、界面布局、状态刷新，光是搭架子就要花不少时间。

![图片](https://mmbiz.qpic.cn/sz_mmbiz_png/gSQUeibVLfBqvaibuIXLBPROeomORJnoQ0ll9ciaCJEicflSjzTuOiakRKhsBP7fzTYtxwxlCqmE6dQ88w8XhGxibVbU7PqNuzUsXJr7mT9Cv7hNU/640?wx_fmt=png&from=appmsg#imgIndex=1)

最近看到一个用C[#写的上位机模板](javascript:;)，叫 `WL.Mock_SWJDemo2`，把这几个模块都做了，或许能帮一些朋友省点搭架子的时间。

# 项目简介

它是一套面向工业设备的 HMI 监控演示模板，采用 MVVM 架构，界面与逻辑分离。覆盖自动化产线、智能制造、装备制造等场景，可作为 PLC 或运动控制器的上位机软件参考。

![图片](https://mmbiz.qpic.cn/mmbiz_png/gSQUeibVLfBpv3xIOCFHRicIF9R5ibp5PB2kJtEM2fSOvJBsIQLLe0nqncvoS9rYBUMIczHLvteKO8OJ9nSiaX6ZxyrxIQUCJMQ8oKjJiaDTGAMY/640?wx_fmt=png&from=appmsg#imgIndex=2)

# 技术架构

| 
项目

 | 

说明

 |
| :-- | :-- |
| 

开发语言

 | 

C# WPF

 |
| 

目标框架

 | 

.NET Framework 4.7.2 及以上

 |
| 

开发环境

 | 

Visual Studio 2019 或更高版本

 |
| 

架构模式

 | 

MVVM，模块化设计

 |
| 

配套资源

 | 

包含 PSD 设计稿，方便调整 UI 风格

 |

![图片](https://mmbiz.qpic.cn/mmbiz_png/gSQUeibVLfBq1icT5Yy2Mje8icwyNa9XzUxDN9iaK359QTibZKOmKgOW1Y3G3TGMB621omd5ZnEaMicPiciacH9CJSsj1rGysWOOn0NntfaEhicGK3kA/640?wx_fmt=png&from=appmsg#imgIndex=3)

# 主要解决的问题

**重复搭建**：监控总览、参数配置、报警记录等模块在不同项目中高度相似，却每次都要从零写起  
**界面与逻辑耦合**：传统写法里改界面容易牵一发而动全身，多人协作时容易互相干扰  
**参数适配难**：换一条产线或设备，往往需要改大量代码，缺乏参数化配置机制

![图片](https://mmbiz.qpic.cn/sz_mmbiz_png/gSQUeibVLfBq5Xtkd3tAJByK6ffdl7FclSsqJVOmuJ9AYGicbUVUNCttxdX6vtHXTTv8FatJibReiaLYmufTy0QvvLRwR6Q8eeVaHnhYFeNPUxU/640?wx_fmt=png&from=appmsg#imgIndex=4)

# 核心模块

| 
模块

 | 

主要功能

 |
| :-- | :-- |
| **监控总览** | 

实时数据展示、状态指示、报警标识、自动/手动刷新

 |
| **参数配置** | 

系统/工艺/设备参数分类管理，支持数值范围、格式、逻辑校验，修改后可直接下发

 |
| **报警记录** | 

高限/低限/故障/通讯异常等报警类型，支持时间范围查询、类型筛选、确认记录、数据导出

 |
| **趋势曲线** | 

实时趋势与历史趋势，时间轴缩放拖拽，数据导出，采集周期可配置

 |
| **设备维护** | 

手动启停控制、IO 调试、通讯检测、日志查看、权限验证

 |
| **系统设置** | 

用户与权限管理、通讯参数、PLC 寄存器映射、数据库配置、日志策略

 |

![图片](https://mmbiz.qpic.cn/sz_mmbiz_png/gSQUeibVLfBpNVGrjLRBicKDY7PGju82mgZS0vI6rZ6Pu3MUnBMwCfcya21ibugIHM1OJibm3Q40N1NbsEHTUCCXv3r3OqUBCYHRhgNomicHicMkc/640?wx_fmt=png&from=appmsg#imgIndex=5)

  

![图片](https://mmbiz.qpic.cn/sz_mmbiz_png/gSQUeibVLfBqzvpBG8BYRK8ziaPhQ1Ybde7xGApwaMlNcXjs5mtqan97hbbEXmVFqLyZp6vBoevdovAVLUhhVpe182BhoSibZW9ibj5jepqOX7A/640?wx_fmt=png&from=appmsg#imgIndex=6)

# 几个技术特点

### MVVM 分层设计

View 层负责展示，ViewModel 处理业务逻辑，Model 管理数据。数据从 PLC 经通讯协议传入 ViewModel，再绑定到 View 渲染，流程比较清晰。模块间耦合度较低，多人开发时相互影响较小。

### 参数化配置

每个参数包含名称、单位、默认值，支持数值范围、格式、逻辑三类校验。适配不同设备或工艺时，主要通过修改配置文件实现，不需要改动核心代码。

![图片](https://mmbiz.qpic.cn/sz_mmbiz_png/gSQUeibVLfBqBWzpaEBUWr1qJOkl8ttNVjjCU8tx7B0JdE70THKMeXET8JwxjsNZAyibgV7kYqFMKtf3RphsPb8hvGk9RGfxyEb2K78HfXSU0/640?wx_fmt=png&from=appmsg#imgIndex=7)

### 趋势曲线开箱可用

支持实时与历史模式，时间轴可缩放拖拽，数据可导出。对于需要持续观察设备状态的场景，可以直接复用。![图片](https://mmbiz.qpic.cn/sz_mmbiz_png/gSQUeibVLfBp9r5icsHUicbiaibZIZEjmUt5D8lBtT0gAhBZkqfg2JhbFoPO8QoYibyIpgI1IgOIc5l1PDzckDB9W1eAutl4sO1wE1IiaPbDzibnAtA/640?wx_fmt=png&from=appmsg#imgIndex=8)

### 附带 PSD 设计稿

仓库里提供了 UI 原图，有调整界面风格需求时，可以直接在 PSD 基础上修改。

# 项目地址

> https://github.com/agentthink/WL.Mock\_SWJDemo2

# 写在最后

这个模板有点像装修时的样板房，硬装软装基本齐全，可以直接在此基础上调整。六大模块、MVVM 架构、PSD 设计稿都准备好了，省去了从零搭架子的过程。适合作为自动化产线、智能制造等场景的上位机软件基础框架，能帮助开发者减少从零搭建界面和基础功能的工作量。

 ![图片](https://mmbiz.qpic.cn/mmbiz_gif/AiaVicV0EDknQX3fxmkaDQ3MdecQFYVPhVUPibZ01EGS6JOhPQFnDknCIdylSsicYtRqYbpxiaeY2IADmHibCWIEKVXA/640?wx_fmt=gif#imgIndex=9)  

  

  

  

  

  

**免责声明****：**本公众号所发布的内容来源于互联网，我们会尊重并维护原作者的权益。由于信息来源众多，若文章内容出现版权问题，或文中使用的图片、资料、下载链接等，如涉及侵权，请告知我们，我们将尽快处理。