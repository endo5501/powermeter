# PowerMeter

*[English version](README.md)*

Dyson Sphere Program (DSP) 用の MOD です。**現在の惑星・現在の星系・全星系**の電力状況を、ゲーム画面に常駐するウィジットとして表示します。

電力の様子を見るたびに統計ウィンドウを開き直す必要がなくなり、建設中でも電力の余力やエネルギー中枢のやり取りをそのまま把握できます。

![PowerMeter のウィジット](https://raw.githubusercontent.com/endo5501/powermeter/main/docs/screenshot.png)

上のスクリーンショットでは、降りている惑星が需要 288 MW に対して自前の発電は 20.6 MW しかなく、残りをエネルギー中枢からの放電 270 MW で賄っていることが読み取れます。一方で星系全体の使用率は 88% まで上がっており、発電設備の余力が少なくなってきていることも同時に分かります。

ゲームの HUD 配下に uGUI で描画しているため、フォントや UI スケールはゲーム本体に追従し、メインメニューでは自動的に隠れます。

## 表示項目

| 列 | 内容 | ゲーム内統計パネルの対応 |
|---|---|---|
| 発電 | 実際に発電されている電力 | （下段サークルの合計値） |
| 需要 | 消費側が要求している電力 | 必要消費電力 |
| 容量 | 最大発電能力 | 発電性能 |
| 使用率 | 発電 / 容量。発電設備の余力の目安 | （PowerMeter 独自） |
| 充足 | 供給 / 需要。電力不足のときだけ 100% を下回る | 給電率 |
| 充電 | 蓄電池・エネルギー中枢へ充電されている電力 | 充電工率 |
| 放電 | 蓄電池・エネルギー中枢から放電されている電力 | 放電工率 |
| 蓄電 | 蓄電池に貯まっているエネルギー | 蓄電量 |

既定では **発電・需要・容量・使用率・充電・放電** を表示します。充足と蓄電は設定で追加できます。

`使用率` は既定で 90% 以上、`充足` は 95% を下回ると警告色になります。

### 表示値についての注意

ゲーム内パネルと**末尾 1 桁がずれることがあります**（例: 35.9 GW / 35.8 GW）。これはデータの差ではなく丸め方の違いです。ゲーム側の `StringBuilderUtility.WriteKMGPower` は切り捨て、PowerMeter は有効数字 3 桁で四捨五入します。

## 動作環境

| 項目 | バージョン |
|---|---|
| Dyson Sphere Program | Unity 2022.3 系（アーリーアクセス 0.10.34 で確認） |
| BepInEx | 5.4.17 |
| MOD マネージャ | r2modman |

## 導入

Thunderstore へは配布していませんが、**r2modman のローカルインポートで通常の MOD と同じように管理できます**。他の MOD と並んで一覧に出て、有効・無効の切り替えもできます。

### 方法 A: r2modman で管理する

配布用の zip を作ります。

```
dotnet build -c Release -t:Package
```

`artifacts\endo5501-PowerMeter-<version>.zip` ができるので、r2modman の **Settings → Install local mod** から読み込みます。zip には Thunderstore V1 形式の `manifest.json` が入っているため、名前・作者・バージョン・BepInEx への依存は自動的に認識されます。

> **注意**: 方法 B の直接コピーを併用しないでください。`plugins\PowerMeter\` と r2modman が展開したフォルダの両方に同じプラグインが置かれると、GUID が重複して片方が読み込まれません。r2modman 管理へ切り替えるときは `plugins\PowerMeter\` を削除し、`Directory.Build.props` の `DeployToProfile` を `false` にしてください。

### 方法 B: 開発中の直接コピー

ビルドすると、既定でプロファイルの plugins フォルダへ直接コピーされます。コードを直して起動し直すだけで試せるので、開発中はこちらが手軽です。

```
dotnet build -c Release
```

配置先:

```
%AppData%\r2modmanPlus-local\DysonSphereProgram\profiles\Default\BepInEx\plugins\PowerMeter\
  PowerMeter.Plugin.dll
  PowerMeter.Core.dll
```

この自動コピーは `Directory.Build.props` の `DeployToProfile` で切り替えられます。一時的に止めるだけなら `/p:DeployToProfile=false` を付けてください。

どちらの方法でも、あとは r2modman からゲームを起動するだけです。初回起動時に設定ファイルが生成されます。

### 使い方

`Alt` + `P` でウィジットの表示を切り替えます（キーは設定変更可）。

## 設定

設定ファイルはここに生成されます。r2modman の Config editor からも編集できます。

```
%AppData%\r2modmanPlus-local\DysonSphereProgram\profiles\Default\BepInEx\config\com.endo5501.dsp.PowerMeter.cfg
```

位置・文字サイズ・列の増減はゲームを再起動しなくても反映されます。

### General

| キー | 既定値 | 説明 |
|---|---|---|
| `Enabled` | `true` | MOD の有効・無効 |
| `ToggleHotkey` | `P + LeftAlt` | 表示切り替えキー |
| `UpdateIntervalSeconds` | `0.5` | 再集計の間隔（秒）。0.1〜5.0 |
| `Language` | `Auto` | ラベルの言語。`Auto` / `Japanese` / `English` |

### Layout

| キー | 既定値 | 説明 |
|---|---|---|
| `Corner` | `TopRight` | 表示する画面の隅。`TopLeft` / `TopRight` / `BottomLeft` / `BottomRight` |
| `OffsetX` / `OffsetY` | `16` | 隅からのオフセット |
| `FontSize` | `14` | 文字サイズ。ウィジット全体の寸法も追従する |
| `BackgroundOpacity` | `0.55` | 背景パネルの不透明度。`0` で背景なし |

### Columns

| キー | 既定値 | 説明 |
|---|---|---|
| `ShowCapacity` | `true` | 容量の列 |
| `ShowUtilization` | `true` | 使用率の列 |
| `ShowSatisfaction` | `false` | 充足率の列 |
| `ChargeColumn` | `Split` | 充放電の出し方。`Split`（充電・放電を分ける） / `Net`（差し引き 1 列） / `Off` |
| `ShowAccumulated` | `false` | 蓄電量の列 |
| `UtilizationWarningPercent` | `90` | 使用率がこの値以上で警告色 |
| `SatisfactionWarningPercent` | `95` | 充足率がこの値未満で警告色 |

### Diagnostics

| キー | 既定値 | 説明 |
|---|---|---|
| `DiagnosticLogging` | `false` | 集計結果と丸める前の生の W / J 値を BepInEx のログへ出力する |
| `DiagnosticLogIntervalSeconds` | `5` | 出力間隔（秒） |

表示値がおかしいときは `DiagnosticLogging` を `true` にすると、`BepInEx\LogOutput.log` に生値が出ます。ゲーム内統計パネルと突き合わせるときに使ってください。

## 開発

### 構成

ゲームや Unity に依存しない純粋なロジックを `PowerMeter.Core` に切り出し、そこだけをユニットテストの対象にしています。ゲーム側の型に触れるのは `GamePowerSampler` だけです。

```
PowerMeter.sln
Directory.Build.props            ゲーム / BepInEx のパス、配置の切り替え
packaging/
  manifest.json                  Thunderstore V1 形式。author 付きなのでファイル名規約は不要
  icon.png                       256x256
src/
  PowerMeter.Core/               netstandard2.0。ゲーム・Unity 参照ゼロ
    PowerScope.cs                集計範囲 (Planet / Star / Global)
    NetworkSample.cs             電力網 1 つ分の生値
    PowerSnapshot.cs             集計結果
    PowerAggregator.cs           スコープ別の合算と各種比率
    PowerFormatter.cs            W / J / % の整形
  PowerMeter.Plugin/             net472。BepInEx グルー
    PowerMeterPlugin.cs          エントリポイント
    PowerMeterConfig.cs          設定バインディング
    GamePowerSampler.cs          ゲーム状態 -> NetworkSample の境界
    UI/PowerMeterWidget.cs       uGUI ウィジット
    UI/WidgetLabels.cs           ラベルの日本語 / 英語
tests/
  PowerMeter.Core.Tests/         net7.0 / xUnit
```

### 必要なもの

- .NET SDK 7 以降
- DSP 本体と、BepInEx 5.4.17 を導入済みの r2modman プロファイル

ゲーム DLL と BepInEx はローカルのインストール先から直接参照します。NuGet フィードの追加設定は不要で、依存パッケージは net472 の参照アセンブリとテスト関連だけです。

パスが標準と違う場合は環境変数で上書きできます。

```
DSP_GAME_DIR      ゲーム本体のフォルダ
DSP_BEPINEX_DIR   BepInEx のフォルダ（プロファイル配下）
```

### テスト

```
dotnet test
```

### パッケージング

```
dotnet build -c Release -t:Package
```

`artifacts\endo5501-PowerMeter-<version>.zip` に `manifest.json` / `icon.png` / 両方の README / `LICENSE` / 2 つの DLL をまとめます。Thunderstore のパッケージ形式そのままなので、Thunderstore へ出すときもこの zip がそのまま使えます。

バージョンは `Directory.Build.props` の `PowerMeterVersion` が唯一の情報源です。`packaging/manifest.json` の `version_number` がこれと食い違っているとパッケージング時にエラーになります。Debug 構成で実行した場合もエラーになります。

スクリーンショットは zip に含めていません。Thunderstore も r2modman も README 内の相対パス画像を解決しないため、README 側は GitHub の絶対 URL を参照しており、それでパッケージのページ上でも画像が表示されます。

### 電力値の取得元

`GameMain.data.factories[i].powerSystem.netPool[]` の各 `PowerNetwork` を直接合算しています。この値は tick をまたいで保持されるため、UI 側から安全に読み取れます。W への換算は `GameMain.tickPerSecI` 倍です。

以下は**使っていません**。理由も残しておきます。

- `FactoryProductionStat.powerGenRegister` などのレジスタ — 毎 tick 集計後にクリアされるため、UI から読むと 0 や途中値を掴む
- `AstroPowerStatPlan.CalculateAstroPowerBaseInfo()` — `statFactoryIndices` などの内部状態に依存し、`OnInit` のライフサイクル無しには単体で使えない
- `PowerNetwork.energyAccumulated` — 名前は近いが建物ツールチップ用の値。統計ウィンドウの「蓄電量」は `energyStored`

各項目はゲーム内統計パネルと実機で突き合わせて確認済みです。エネルギー中枢で受電・充電している惑星の挙動は回帰テストとして固定してあります。

## ライセンス

MIT。[LICENSE](LICENSE) を参照してください。
