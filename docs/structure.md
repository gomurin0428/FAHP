# リポジトリ構造 (Repository Structure)

```
FAHP/
├── FAHP.Shared/              # 共有ライブラリ
│   ├── Models/               # コアモデル
│   │   ├── TriangularFuzzyNumber.cs    # 三角形ファジィ数の定義
│   │   ├── FuzzyAHPProcessor.cs        # FAHP計算アルゴリズム
│   │   ├── CrispTOPSISProcessor.cs     # TOPSIS計算アルゴリズム
│   │   ├── ComparisonEntry.cs          # 比較エントリモデル
│   │   └── AlternativeComparisonEntry.cs
│   ├── ViewModels/           # MVVMパターン用ビューモデル
│   │   ├── MainViewModel.cs            # メイン処理ロジック
│   │   ├── CriterionTabViewModel.cs    # 基準タブUI
│   │   ├── WeightResultViewModel.cs    # 重み結果表示
│   │   └── AlternativeScoreViewModel.cs
│   └── CompatibilityStubs.cs  # 互換性スタブ
├── FAHPWebApp3/              # OpenSilverウェブアプリ
│   ├── FAHPWebApp3/          # コアアプリロジック
│   ├── FAHPWebApp3.Browser/  # ブラウザ統合
│   └── FAHPWebApp3.Simulator/ # テスト環境
├── FAHPWebApp3.Tests/        # ユニットテスト
│   ├── TriangularFuzzyNumberTests.cs
│   ├── FuzzyAHPProcessorTests.cs
│   └── CrispTOPSISProcessorTests.cs
└── docs/                     # ドキュメント
    ├── fuzzy_calculations.md  # ファジィ計算の詳細説明
    └── structure.md           # リポジトリ構造図
```
