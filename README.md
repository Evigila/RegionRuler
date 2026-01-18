# RegionRuler

一个用于规范化 C# 代码中  **#region **块命名的 Roslyn 分析器。

## Features

- 检查 #region 名称是否符合规范
- 支持.editorconfig自定义配置
- 支持正则表达式
- 回退默认值
- 大小写敏感配置
- 允许空名称

## Quick start

使用NUGET下载,  
或者,  
在项目文件 .csproj 中添加引用:

```xml
    <PackageReference Include="RegionRuler" Version="1.2.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
```

## Config

在项目根目录创建 .editorconfig 文件:

### Default config

```ini
root = true

[*.cs]

# Allowed region name
# 允许的Region名字
region_ruler.allowed_region_name = Main

# Using regex pattern
# 正则表达式
region_ruler.allowed_regex_pattern = ^(Private|Public).*

# Case sensitivity (default False)
# 区分大小写 (默认False)
region_ruler.case_sensitive = false

# Empty name (default False)
# 空名字 (默认False)
region_ruler.allow_empty = false
```

### 默认配置

如果不配置任何选项，分析器会使用以下默认的允许名称列表：

- **成员分类**：PUBLIC_MEMBERS, PRIVATE_MEMBERS, PROTECTED_MEMBERS, INTERNAL_MEMBERS
- **属性分类**：PUBLIC_PROPERTIES, PRIVATE_PROPERTIES, PROTECTED_PROPERTIES, INTERNAL_PROPERTIES
- **字段分类**：PUBLIC_FIELDS, PRIVATE_FIELDS, PROTECTED_FIELDS, INTERNAL_FIELDS
- **方法分类**：PUBLIC_METHODS, PRIVATE_METHODS, PROTECTED_METHODS, INTERNAL_METHODS, OVERRIDE_METHODS, ABSTRACT_METHODS, STATIC_METHODS
- **生命周期**：CONSTRUCTOR, DESTRUCTOR, INITIALIZATION
- **其他**：EVENT_CALLBACKS, HELPERS, UTILITIES