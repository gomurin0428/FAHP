# ファジィ計算の詳細ガイド (Detailed Guide to Fuzzy Calculations)

このドキュメントでは、FAHP（ファジィ階層分析法）の計算部分を初学者向けに詳しく説明します。

## 三角形ファジィ数 (Triangular Fuzzy Numbers)

### 基本概念

三角形ファジィ数（TFN）は、不確実性を表現するために使用される特殊な数値です。通常の「クリスプ」な数値（例：7）と異なり、三角形ファジィ数は三つの値で表されます：

- **下限値 (l)**: 考えられる最小値
- **中央値 (m)**: 最も可能性の高い値
- **上限値 (u)**: 考えられる最大値

これを (l, m, u) と表記します。例えば、「約7」という表現は (6, 7, 8) のような三角形ファジィ数で表すことができます。

### コードでの実装

```csharp
public readonly struct TriangularFuzzyNumber
{
    public readonly double L;
    public readonly double M;
    public readonly double U;

    public TriangularFuzzyNumber(double l, double m, double u)
    {
        L = l;
        M = m;
        U = u;
    }
}
```

三角形ファジィ数は `TriangularFuzzyNumber` 構造体として実装されています。プロパティ L, M, U はそれぞれ下限値、中央値、上限値を表します。

### 主要な演算

#### デファジィ化 (Defuzzification)

デファジィ化とは、ファジィ数を通常の数値（クリスプ値）に変換することです。最も一般的な方法は重心法で、簡単に言えば三つの値の平均です：

```csharp
public double Defuzzify() => (L + M + U) / 3.0;
```

#### 加算 (Addition)

二つの三角形ファジィ数の加算は、それぞれの対応する値を足し合わせます：

```csharp
public static TriangularFuzzyNumber operator +(in TriangularFuzzyNumber a, in TriangularFuzzyNumber b)
    => new TriangularFuzzyNumber(a.L + b.L, a.M + b.M, a.U + b.U);
```

#### 乗算 (Multiplication)

二つの三角形ファジィ数の乗算も同様に、対応する値を掛け合わせます：

```csharp
public static TriangularFuzzyNumber operator *(in TriangularFuzzyNumber a, in TriangularFuzzyNumber b)
    => new TriangularFuzzyNumber(a.L * b.L, a.M * b.M, a.U * b.U);
```

#### 逆数 (Reciprocal)

三角形ファジィ数の逆数は、各値の逆数をとりますが、順序が反転することに注意が必要です：

```csharp
public static TriangularFuzzyNumber Reciprocal(in TriangularFuzzyNumber t)
    => new TriangularFuzzyNumber(1.0 / t.U, 1.0 / t.M, 1.0 / t.L);
```

## ファジィAHPの計算手法

### サトティスケールのファジィ化

AHPでは通常、1から9のサトティスケールを使用します。FAHPでは、これらの値を三角形ファジィ数に変換します：

```csharp
public static TriangularFuzzyNumber ToTriangular(int scale)
{
    if (scale < 1 || scale > 9) throw new ArgumentOutOfRangeException(nameof(scale));
    return scale switch
    {
        1 => new TriangularFuzzyNumber(1, 1, 1),
        9 => new TriangularFuzzyNumber(9, 9, 9),
        _ => new TriangularFuzzyNumber(scale - 1, scale, scale + 1)
    };
}
```

このコードでは：
- スケール1は確実性を表すため (1, 1, 1) になります
- スケール9は最大値を表すため (9, 9, 9) になります
- その他のスケール値 n は (n-1, n, n+1) になります（例：スケール5は (4, 5, 6)）

### 重み計算アルゴリズム

FAHPの重み計算アルゴリズムは以下のステップで行われます：

1. **各行の幾何平均を計算**

```csharp
// 1. 各行の幾何平均を計算
var gms = new TriangularFuzzyNumber[n];
for (int i = 0; i < n; i++)
{
    double prodL = 1, prodM = 1, prodU = 1;
    for (int j = 0; j < n; j++)
    {
        var v = matrix[i, j];
        prodL *= v.L;
        prodM *= v.M;
        prodU *= v.U;
    }
    double exponent = 1.0 / n;
    gms[i] = new TriangularFuzzyNumber(Math.Pow(prodL, exponent), Math.Pow(prodM, exponent), Math.Pow(prodU, exponent));
}
```

2. **幾何平均の合計を計算**

```csharp
// 2. 行ごとの幾何平均を合計
var sumGM = new TriangularFuzzyNumber(gms.Sum(g => g.L), gms.Sum(g => g.M), gms.Sum(g => g.U));
```

3. **各基準の重みを計算**

```csharp
// 3. 重みを計算
var weights = new double[n];
for (int i = 0; i < n; i++)
{
    var wFuzzy = new TriangularFuzzyNumber(gms[i].L / sumGM.U, gms[i].M / sumGM.M, gms[i].U / sumGM.L);
    weights[i] = wFuzzy.Defuzzify();
}
```

4. **重みを正規化（合計が1になるように）**

```csharp
// 4. 正規化
double total = weights.Sum();
for (int i = 0; i < n; i++)
{
    weights[i] /= total;
}
```

### 一貫性比率 (CR) の計算

一貫性比率は、判断の論理的一貫性を測るための指標です。一般的に、CR < 0.1 であれば判断は一貫していると見なされます。

一貫性比率の計算手順：

1. 三角形ファジィ数行列をデファジィ化
```csharp
// 1. デファジィ化したクリスプ行列 A を作成
var a = new double[n, n];
for (int i = 0; i < n; i++)
{
    for (int j = 0; j < n; j++)
    {
        a[i, j] = matrix[i, j].Defuzzify();
    }
}
```

2. 固有ベクトルを近似計算
```csharp
// 2. 幾何平均法で固有ベクトル w を近似
var w = new double[n];
for (int i = 0; i < n; i++)
{
    double prod = 1.0;
    for (int j = 0; j < n; j++) prod *= a[i, j];
    w[i] = Math.Pow(prod, 1.0 / n);
}
double sumW = w.Sum();
for (int i = 0; i < n; i++) w[i] /= sumW;
```

3. 最大固有値を推定
```csharp
// 3. λ_max を推定: λ_i = (A * w)_i / w_i
double lambdaSum = 0.0;
for (int i = 0; i < n; i++)
{
    double rowDot = 0.0;
    for (int j = 0; j < n; j++) rowDot += a[i, j] * w[j];
    lambdaSum += rowDot / w[i];
}
double lambdaMax = lambdaSum / n;
```

4. 一貫性指標 (CI) を計算: CI = (λ_max - n) / (n - 1)
5. ランダム指標 (RI) で割って一貫性比率を求める: CR = CI / RI
```csharp
// 4. CI, CR
double ci = (lambdaMax - n) / (n - 1);
double ri = GetRandomIndex(n);
return ri <= 0 ? 0.0 : ci / ri;
```

## TOPSIS法による代替案評価

FAHPで基準の重みを計算した後、TOPSIS法を使用して代替案を評価します：

TOPSIS計算の手順：

1. 決定行列を正規化（各列のL2ノルムで割る）
```csharp
// 1. 列ごとの L2 ノルムを計算
var norm = new double[n];
for (int j = 0; j < n; j++)
{
    double sumSq = 0.0;
    for (int i = 0; i < m; i++)
    {
        double v = decisionMatrix[i, j];
        sumSq += v * v;
    }
    norm[j] = Math.Sqrt(sumSq);
    if (norm[j] == 0) norm[j] = 1; // ゼロ除算回避
}

// 2. 正規化 r_ij, 重み付け v_ij
var vMat = new double[m, n];
for (int i = 0; i < m; i++)
{
    for (int j = 0; j < n; j++)
    {
        double r = decisionMatrix[i, j] / norm[j];
        vMat[i, j] = r * weights[j];
    }
}
```

2. 理想解（各基準の最良値）と負の理想解（各基準の最悪値）を特定
```csharp
// 3. 理想点 / 反理想点
var vPlus = new double[n];
var vMinus = new double[n];
for (int j = 0; j < n; j++)
{
    double colMax = double.MinValue;
    double colMin = double.MaxValue;
    for (int i = 0; i < m; i++)
    {
        double v = vMat[i, j];
        if (v > colMax) colMax = v;
        if (v < colMin) colMin = v;
    }
    if (isBenefit[j])
    {
        vPlus[j] = colMax;
        vMinus[j] = colMin;
    }
    else
    {
        vPlus[j] = colMin; // Cost 基準は小さいほど良い
        vMinus[j] = colMax;
    }
}
```

3. 各代替案から理想解と負の理想解への距離を計算
```csharp
// 4. 距離計算
var dPlus = new double[m];
var dMinus = new double[m];
for (int i = 0; i < m; i++)
{
    double sumSqPlus = 0.0;
    double sumSqMinus = 0.0;
    for (int j = 0; j < n; j++)
    {
        double diffPlus = vMat[i, j] - vPlus[j];
        double diffMinus = vMat[i, j] - vMinus[j];
        sumSqPlus += diffPlus * diffPlus;
        sumSqMinus += diffMinus * diffMinus;
    }
    dPlus[i] = Math.Sqrt(sumSqPlus);
    dMinus[i] = Math.Sqrt(sumSqMinus);
}
```

4. 相対近さ係数を計算: C_i = d^- / (d^+ + d^-)
```csharp
// 5. 相対近さ係数
var c = new double[m];
for (int i = 0; i < m; i++)
{
    double denom = dPlus[i] + dMinus[i];
    c[i] = denom == 0 ? 0 : dMinus[i] / denom;
}
```

C_i は0から1の間の値をとり、1に近いほど良い評価となります。

## 実際の計算例

### 例：三つの基準で二つの代替案を評価

基準：コスト、品質、納期
代替案：サプライヤA、サプライヤB

1. 基準の重要度比較行列（ファジィ化後）：
```
コスト/コスト:   (1, 1, 1)
コスト/品質:     (2, 3, 4)
コスト/納期:     (4, 5, 6)
品質/品質:       (1, 1, 1)
品質/納期:       (1, 2, 3)
納期/納期:       (1, 1, 1)
```

2. 基準ごとの代替案比較行列（例：コストについて）
```
A/A: (1, 1, 1)
A/B: (1, 2, 3)
B/B: (1, 1, 1)
```

これらの入力から、アルゴリズムは以下を計算します：
- 基準の重み: [コスト: 0.64, 品質: 0.24, 納期: 0.12]
- 基準ごとの代替案重み（例）: 
  * コスト: [A: 0.67, B: 0.33]
  * 品質: [A: 0.45, B: 0.55]
  * 納期: [A: 0.5, B: 0.5]
- 最終スコア（TOPSIS適用後）: [A: 0.75, B: 0.25]

この例では、サプライヤAがより良い選択肢と評価されます。
