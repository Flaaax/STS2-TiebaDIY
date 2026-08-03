## 项目说明

- TiebaDIY 是一个依赖 RitsuLib 的《杀戮尖塔 2》内容模组。
- 当前直接支持的游戏版本是 `0.107.1` 与 `0.110.0`。
- 项目使用 JML Dispatch 的入口 DLL + 多 Runtime DLL 分发机制；维护版本、构建或发布配置前必须阅读 `DIST.md`。
- RitsuLib 的包版本与依赖规则应和 FBE 保持一致，除非用户明确要求升级。
- RitsuLib 的本地源码位于 [`../Thirdparty Mods/STS2-RitsuLib-main/`](../Thirdparty%20Mods/STS2-RitsuLib-main/)；使用其 API 前优先查阅该目录下的 `src/` 与 `docs/`。常用入口包括 [`ModRelicTemplate`](../Thirdparty%20Mods/STS2-RitsuLib-main/src/Scaffolding/Content/ModRelicTemplate.cs)、[`ModCardTemplate`](../Thirdparty%20Mods/STS2-RitsuLib-main/src/Scaffolding/Content/ModCardTemplate.cs) 与 [`ModPowerTemplate`](../Thirdparty%20Mods/STS2-RitsuLib-main/src/Scaffolding/Content/ModPowerTemplate.cs)。
- 遵循上层工作区规则：禁止由代理执行构建。

## 开发指南

- 在继承原版或 RitsuLib 类型时，不要无意间声明与基类成员同名的字段、常量、属性或方法。优先换成不会冲突的名称；
如果确实需要隐藏基类成员，必须显式添加 `new`，避免产生 CS0108 警告。
例如：`private new const string PortraitPath = "res://TiebaDIY/images/cards/MovingAround.png";`。
- gitignore的默认规则是：不跟踪非文本美术素材

## 内容制作指南

### 通用规则

- `Entry.Init()` 已调用 `ModTypeDiscoveryHub.RegisterModAssembly`，用于发现 RitsuLib 内容注册注解；新增内容仍必须在具体模型类上添加对应的注册注解。
- RitsuLib 默认把公开 Entry 规范化为 `MODID_CATEGORY_TYPENAME`。例如 TiebaDIY 的 `ExampleCard` 默认为 `TIEBA_DIY_CARD_EXAMPLE_CARD`，`ExampleRelic` 默认为 `TIEBA_DIY_RELIC_EXAMPLE_RELIC`；本地化键必须以实际公开 Entry 为词干。
- 已经发布的内容不要仅因重命名 C# 类型而改变 Entry。需要稳定命名时，在注册注解上使用 `StableEntryStem`；除非兼容既有完整 ID，不要使用 `FullPublicEntry`，两者不能同时设置。
- 游戏内容文本使用原生本地化表。TiebaDIY 当前的目录约定为 `TiebaDIY/localization/zhs/<table>.json` 与 `TiebaDIY/localization/eng/<table>.json`，新增内容应同时提供中英文键。
- 新增内容的英文名称应保持简短，优先使用能准确表达概念的短名称，不要把完整中文名逐词扩写成冗长英文。
- 自定义资源放在 `TiebaDIY/images/` 等 PCK 资源目录中，代码使用 `res://TiebaDIY/...` 路径。只填写实际需要覆盖的资源，未覆盖部分保留原版行为。
- 如果不确定某项 RitsuLib API 是否存在、在两个目标版本间是否兼容，先检查项目实际引用的包版本、本地 RitsuLib 源码及 `STS2 source/` 的 `0.107.1`、`0.110.0` 分支；无法确认时退回原版模型能力，并用 `STS2_0_107_1` / `STS2_0_110_0` 隔离 ABI 差异。
- TiebaCardModel的可重写属性：IsImba 默认为false。如果用户提到此内容是“不平衡”，“IMBA”之类，应该重写此属性为true。
- 所有内容必须支持联机。在联机同步问题上需谨慎。

### 遗物

- TiebaDIY 的自定义遗物默认直接继承 RitsuLib 的 `ModRelicTemplate`，不要复制或创建 `FBERelicModel`。
- `ModRelicTemplate` 的完整类型是 `STS2RitsuLib.Scaffolding.Content.ModRelicTemplate`；它本身继承原版 `MegaCrit.Sts2.Core.Models.RelicModel`。
- 推荐的最小结构如下；遗物池类型应替换为 TiebaDIY 实际使用的池：

```csharp
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

[RegisterRelic(typeof(TiebaDIYRelicPool))]
public sealed class ExampleRelic : ModRelicTemplate
{
}
```

- 继承 `ModRelicTemplate` 不等于自动进入遗物池。使用注解注册时，需要在具体遗物类上添加 `[RegisterRelic(typeof(...))]`；`Entry.Init()` 已调用 `ModTypeDiscoveryHub.RegisterModAssembly` 来发现这些类型。
- 可以使用已经从 RitsuLib 源码确认的模板能力：`AssetProfile`、`CustomIconPath`、`CustomIconOutlinePath`、`CustomBigIconPath`、`RegisteredKeywordIds`、`AdditionalHoverTips` 和 `IncludeEnergyHoverTip`。
- 不要为了使用模板而强行依赖上述便利功能。遗物的费用、稀有度、触发时机、战斗逻辑、存档状态等核心行为，优先按照对应游戏版本的原版 `RelicModel` API 实现。
- 只有多个 TiebaDIY 遗物确实出现稳定、项目专属的重复逻辑时，才考虑新增 TiebaDIY 自己的遗物基类；不要预先增加一层空封装。

### 卡牌

- TiebaDIY 的自定义卡牌默认直接继承 RitsuLib 的 `ModCardTemplate`，不要复制或创建 `FBECardModel`。
- `ModCardTemplate` 的完整类型是 `STS2RitsuLib.Scaffolding.Content.ModCardTemplate`；它继承原版 `MegaCrit.Sts2.Core.Models.CardModel`。本地源码见 `../Thirdparty Mods/STS2-RitsuLib-main/src/Scaffolding/Content/ModCardTemplate.cs`。
- 两个目标游戏版本中，其基础构造参数均为：基础费用、`CardType`、`CardRarity`、`TargetType`、是否显示在卡牌图鉴。推荐的最小结构如下：

```csharp
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

[RegisterCard(typeof(ColorlessCardPool))]
public sealed class ExampleCard()
    : ModCardTemplate(1, CardType.Skill, CardRarity.Common, TargetType.None)
{
}
```

- 继承 `ModCardTemplate` 不等于自动进入卡池。必须用 `[RegisterCard(typeof(...))]` 指定实际卡池；角色牌使用对应角色的 `CardPoolModel`，所有角色可用的无色牌使用 `ColorlessCardPool`。不要为了方便把角色牌错误注册进无色卡池。
- 默认公开 Entry 示例：`ExampleCard` 对应 `TIEBA_DIY_CARD_EXAMPLE_CARD`。本地化写入 `TiebaDIY/localization/<语言>/cards.json`，至少提供 `.title` 与 `.description`。
- 普通卡图只需通过 `CardAssetProfile` 覆盖 `PortraitPath`，例如 `res://TiebaDIY/images/cards/ExampleCard.png`；只有确实存在独立 beta 图、边框、费用图标、材质或 overlay 时，才填写对应字段。使用命名参数，避免因长构造参数列表产生位置错误：

```csharp
public override CardAssetProfile AssetProfile { get; } = new(
    PortraitPath: "res://TiebaDIY/images/cards/ExampleCard.png");
```

- 卡牌核心行为优先使用原版 `CardModel` API：
  - 用构造参数声明基础费用、类型、稀有度与目标类型。
  - 用 `CanonicalVars` 声明 `DamageVar`、`BlockVar`、`IntVar`、`EnergyVar` 等动态变量，并在本地化描述中引用同名占位符。
  - 只要 Canonical Var 是 `PowerVar<TPower>`，本地化占位符键就与 Power 的 C# 类名完全相同（即 `typeof(TPower).Name`），必须保留 `Power` 后缀。例如 `new PowerVar<DoomPower>(...)` 的文本占位符是 `{DoomPower}` / `{DoomPower:diff()}`，不是 `{Doom}`；即使代码中通过 `DynamicVars.Doom` 访问，也不能把代码访问名当成本地化键。
  - 用 `OnPlay(PlayerChoiceContext, CardPlay)` 实现打出效果，优先调用 `CardCmd`、`CreatureCmd`、`PowerCmd`、`PlayerCmd` 等原版命令，不直接绕过命令系统修改战斗状态。
  - 用 `OnUpgrade()` 实现升级；数值使用 `DynamicVars.<变量>.UpgradeValueBy(...)`，费用使用 `EnergyCost.UpgradeBy(...)`。
  - 原版关键词与标签分别重写 `CanonicalKeywords`、`CanonicalTags`。不要使用 RitsuLib 已标记过时的 `RegisteredKeywordIds`、`RegisteredCardTagIds`；自定义关键词或标签应先通过 RitsuLib 注册，再转换成 `CardKeyword` / `CardTag`。
- `AdditionalHoverTips`、`AssetProfile` 及手牌高亮/轮廓注册是可选便利能力。没有明确需求时不要引入材质、overlay、全局卡牌类型文本修改器或额外 UI 注册。
- 只有多个 TiebaDIY 卡牌确实出现稳定、项目专属的重复逻辑时，才考虑新增 TiebaDIY 自己的卡牌基类；不要预先增加一层空封装。

- 如果卡牌带有效果“不能被打出”，通常不需要在本地化中写出来，因为游戏会自动渲染它“无法被打出”

### Power

- 能力牌通常需要一个配套的 `PowerModel`。TiebaDIY 的自定义 Power 默认直接继承 RitsuLib 的 `ModPowerTemplate`；
其完整类型是 `STS2RitsuLib.Scaffolding.Content.ModPowerTemplate`，
本身继承原版 `MegaCrit.Sts2.Core.Models.PowerModel`。
- 如果一个能力基本上是一张能力牌的“转发”，那张能力牌的描述应该直接写出此能力的效果，而不是说“获得xx能力”。类似原版“DevilForm”，
尽管它实际上给你“DevilFormPower”，但它的描述还是写“每回合获得3力量”。
- Power 是独立模型，不属于池。必须在具体类型上添加 `[RegisterPower]`，
然后由能力牌通过 `PowerCmd.Apply<TPower>(choiceContext, target, amount, applier, cardSource)` 施加。
能力牌通常用 `PowerVar<TPower>` 保存施加量，并通过 `AdditionalHoverTips` 添加 `HoverTipFactory.FromPower<TPower>()`。
- 推荐的最小结构：

```csharp
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

[RegisterPower]
public sealed class ExamplePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
```

- `PowerType` 决定 Buff/Debuff 分类；`PowerStackType.Counter` 显示并叠加层数，`PowerStackType.Single` 表示不显示层数的单实例效果。只有确实需要按施加者分别存在或允许多个实例时，才重写 `InstanceType`。
- 默认公开 Entry 示例：`ExamplePower` 对应 `TIEBA_DIY_POWER_EXAMPLE_POWER`。本地化写入 `TiebaDIY/localization/<语言>/powers.json`，至少提供 `.title` 与 `.description`；原版描述管线自动提供 `{Amount}` 等 Power 参数。
- Power 图标通常使用透明背景 PNG，放在 `TiebaDIY/images/powers/`。通过 `PowerAssetProfile` 配置 `IconPath` 与 `BigIconPath`；只有一张图时两个字段可以指向同一资源。
- Power 的核心逻辑优先使用原版 `PowerModel` Hook。修改型 Hook 应只返回修改量，不在计算阶段播放动画或改变战斗状态；需要反馈或后续行为时，配套实现对应的 `AfterModifying...` Hook。
- `ModifyPowerAmountGivenAdditive` 的返回值是要加到当前施加量上的数值，而不是最终值。例如返回传入的 `amount` 会使该次 Power 附加量翻倍。只有该 Hook 实际返回非零修改时，该模型才会收到 `AfterModifyingPowerAmountGiven`，适合在其中调用 `Flash()`。
- 是否影响玩家、敌人或特定施加者，应通过 Hook 参数中的 `giver`、`target`、`power`、`cardSource` 明确过滤；需要全场生效时不要按 Power 自身的 `Owner` 过滤。
- `ModPowerTemplate` 的 `AssetProfile`、`AdditionalHoverTips`、`RegisteredKeywordIds` 与 `IncludeEnergyHoverTip` 只是可选便利能力。类型、层数、生命周期、战斗 Hook 和多人行为仍以原版 `PowerModel` 为准。
- 只有多个 TiebaDIY Power 确实出现稳定、项目专属的重复逻辑时，才考虑新增自己的 Power 基类；不要预先增加一层空封装。
