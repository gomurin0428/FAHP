# FAHP - ファジィ階層分析法 (Fuzzy Analytic Hierarchy Process)

## 概要 (Overview)

このリポジトリは、ファジィ階層分析法 (FAHP) と理想解への類似性に基づく手法 (TOPSIS) のOpenSilver実装です。これらは多基準意思決定技法であり、ファジィ論理を用いて不確実性を扱いながら、複数の基準に基づいて代替案を評価するために使用されます。

This repository contains an OpenSilver implementation of the Fuzzy Analytic Hierarchy Process (FAHP) and Technique for Order of Preference by Similarity to Ideal Solution (TOPSIS) methodologies. These are multi-criteria decision-making techniques used to evaluate alternatives based on multiple criteria while handling uncertainty through fuzzy logic.

### 主な機能 (Key Features)

- 評価基準と代替案の入力
- ファジィ数を使用したペアワイズ比較行列の作成
- 基準の重みと一貫性比率の計算
- FAHPとTOPSISを組み合わせた代替案のランキング
- 比較の一貫性の検証

### アプリケーション (Applications)

- **サプライヤ選定**: コスト、品質、納期、リスクなどの要素を考慮
- **プロジェクト優先順位付け**: リソース配分の意思決定
- **人材採用**: 技術スキル、ソフトスキル、経験などを評価
- **投資ポートフォリオ最適化**: リスク、利回り、流動性などの要素を考慮

## リポジトリ構造 (Repository Structure)

- **FAHP.Shared**: 共有ライブラリ。主要なモデルとビューモデルを含む
  - `Models/`: FAHPとTOPSISのコア実装
  - `ViewModels/`: UIとの連携用MVVMパターン実装
- **FAHPWebApp3**: OpenSilverウェブアプリケーション
  - ブラウザでの実行用WebAssemblyアプリケーション
- **FAHPWebApp3.Tests**: ユニットテストプロジェクト

## ファジィ計算の基本概念 (Fuzzy Calculation Basics)

### 三角形ファジィ数 (Triangular Fuzzy Number)

ファジィ階層分析法では、不確実性を扱うために**三角形ファジィ数**を使用します。これは3つの値 (l, m, u) で表現される数で:

In FAHP, **Triangular Fuzzy Numbers** (TFNs) are used to handle uncertainty. A TFN is represented by three values (l, m, u) where:

- **l (下限値/lower bound)**: 最小の可能性のある値
- **m (中央値/middle value)**: 最も可能性の高い値
- **u (上限値/upper bound)**: 最大の可能性のある値

数学的には、三角形ファジィ数は次のようなメンバーシップ関数で定義されます:

Mathematically, a TFN is defined by the following membership function:

```
μ(x) = 0,               x < l
       (x - l)/(m - l), l ≤ x ≤ m
       (u - x)/(u - m), m ≤ x ≤ u
       0,               x > u
```

### 三角形ファジィ数の演算 (Operations on TFNs)

コード内では、以下の演算が実装されています:

The following operations are implemented in the code:

1. **加算 (Addition)**:
   (l₁, m₁, u₁) + (l₂, m₂, u₂) = (l₁ + l₂, m₁ + m₂, u₁ + u₂)

2. **乗算 (Multiplication)**:
   (l₁, m₁, u₁) × (l₂, m₂, u₂) = (l₁ × l₂, m₁ × m₂, u₁ × u₂)

3. **逆数 (Reciprocal)**:
   1/(l, m, u) = (1/u, 1/m, 1/l)

4. **スカラー除算 (Scalar Division)**:
   (l, m, u) / k = (l/k, m/k, u/k)

5. **デファジィ化 (Defuzzification)**:
   defuzzify(l, m, u) = (l + m + u) / 3

### ファジィAHP手法 (Fuzzy AHP Method)

ファジィAHPは以下のステップで実行されます:

The Fuzzy AHP method follows these steps:

1. **問題の階層化**: 目標、基準、代替案の階層を構築
2. **ペアワイズ比較**: 各レベルの要素間の重要度を比較
3. **幾何平均法による重み計算**: 各行の幾何平均を計算し、ファジィ重みを導出
4. **一貫性比率の検証**: 判断の一貫性を確認 (CR < 0.1 が望ましい)
5. **TOPSIS統合**: 複数基準での代替案評価にTOPSISアルゴリズムを使用

#### Changの拡張分析法 (Chang's Extent Analysis Method)

このプロジェクトでは、重み計算に拡張分析法を実装しています:

1. 各行の幾何平均を計算
2. 行の幾何平均の合計を求める
3. ファジィ重みを計算: RowGM / TotalGM
4. 重みをデファジィ化して正規化

### TOPSIS法 (TOPSIS Method)

TOPSIS (Technique for Order of Preference by Similarity to Ideal Solution) は代替案のランキングに使用されます:

TOPSIS is used to rank alternatives:

1. 決定行列の正規化
2. 重み付け正規化決定行列の計算
3. 理想解と負の理想解の特定
4. 各代替案から理想解と負の理想解への距離計算
5. 相対近さ係数の計算 (値が1に近いほど良い)

## 使用例 (Usage Example)

1. 基準と代替案の入力
2. 比較行列の作成
3. 計算ボタンのクリック
4. 結果の確認（重み、一貫性比率、代替案スコア）

## 開発者向けガイド (Developer Guide)

### コアコンポーネント (Core Components)

- **`TriangularFuzzyNumber`**: 三角形ファジィ数を表現するクラス
- **`FuzzyAHPProcessor`**: 重み計算、一貫性比率計算などの機能を提供
- **`CrispTOPSISProcessor`**: 代替案のランキングにTOPSIS法を実装
- **`MainViewModel`**: UIとのデータバインディングを処理

### テスト (Testing)

ユニットテストは `FAHPWebApp3.Tests` プロジェクトにあります。

### 技術スタック (Technology Stack)

- **OpenSilver**: WebAssemblyを使用したSilverlight/WPFアプリケーションのモダン実装
- **.NET**: アプリケーションフレームワーク
- **MVVM**: UI設計パターン
