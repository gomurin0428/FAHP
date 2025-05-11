// FAHP WebApp — app.js
// 基本的なファジィ AHP (Chang 拡張シンセティック法) と加重合成による評価ロジックを実装しています。

/* ===============================
 * データ構造
 * =============================== */

/** @typedef {[number, number, number]} TFN   // (l, m, u) */

const state = {
    criteriaNames: [],         // string[]
    altNames: [],              // string[]
    criteriaMatrix: [],        // TFN[][]  — n x n 上三角入力
    altMatrices: {},           // { [criteriaIdx: number]: TFN[][] }
};

/* ===============================
 * ユーティリティ関数 (TFN 演算)
 * =============================== */
const tfnAdd = (a, b) => [a[0] + b[0], a[1] + b[1], a[2] + b[2]];
const tfnMultiply = (a, b) => [a[0] * b[0], a[1] * b[1], a[2] * b[2]];
const tfnInverse = (a) => [1 / a[2], 1 / a[1], 1 / a[0]];

// Degree of possibility V(S_i >= S_j)
function degreePossibility(s1, s2) {
    if (s1[1] >= s2[1]) return 1;
    if (s2[0] >= s1[2]) return 0;
    return (s2[0] - s1[2]) / ((s1[1] - s1[2]) - (s2[1] - s2[0]));
}

/**
 * Chang 拡張シンセティック法で重みを算出
 * @param {TFN[][]} matrix n x n のペア比較行列
 * @returns {number[]} 正規化された重み配列 (長さ n)
 */
function computeWeightsByChang(matrix) {
    const n = matrix.length;
    // 行毎に加算
    const rowSums = new Array(n).fill(null).map(() => [0, 0, 0]);
    for (let i = 0; i < n; i++) {
        for (let j = 0; j < n; j++) {
            rowSums[i] = tfnAdd(rowSums[i], matrix[i][j]);
        }
    }

    // 全行合計
    let totalSum = [0, 0, 0];
    rowSums.forEach((r) => {
        totalSum = tfnAdd(totalSum, r);
    });
    const totalInv = tfnInverse(totalSum);

    // S_i
    const S = rowSums.map((r) => tfnMultiply(r, totalInv));

    // degree of possibility
    const d = new Array(n).fill(0);
    for (let i = 0; i < n; i++) {
        const possibilities = [];
        for (let j = 0; j < n; j++) {
            if (i === j) continue;
            possibilities.push(degreePossibility(S[i], S[j]));
        }
        d[i] = Math.min(...possibilities);
    }

    // 正規化
    const sumD = d.reduce((acc, val) => acc + val, 0);
    return d.map((val) => val / sumD);
}

/* ===============================
 * UI 生成/更新
 * =============================== */
const criteriaMatrixContainer = document.getElementById("criteriaMatrixContainer");
const altMatricesContainer = document.getElementById("altMatricesContainer");
const criteriaWeightsContainer = document.getElementById("criteriaWeightsContainer");
const altScoresContainer = document.getElementById("altScoresContainer");

// 追加: レベルと信頼度候補
const levelOptions = [1, 3, 5, 7, 9]; // 代表値レベル選択肢
const confidenceOptions = [0, 0.25, 0.5, 0.75];

// レベル (1,3,5,7,9) → 比率値へ変換。中心 5 → 1。
function levelToRatio(level, base = 2) {
    return Math.pow(base, (level - 5) / 2);
}

// 比率値を最も近いレベルへ
function ratioToNearestLevel(ratio) {
    let nearest = levelOptions[0];
    let minDiff = Infinity;
    for (const lvl of levelOptions) {
        const diff = Math.abs(levelToRatio(lvl) - ratio);
        if (diff < minDiff - 1e-9) {
            minDiff = diff;
            nearest = lvl;
        } else if (Math.abs(diff - minDiff) < 1e-9 && lvl > nearest) {
            // 同距離の場合はより大きいレベルを選択
            nearest = lvl;
        }
    }
    return nearest;
}

// 三角形ファジィ数を表示用にフォーマット (レベル値表示)
function formatTriangularForDisplay(tfn) {
    const [l, m, u] = tfn;
    const approxEq = (a, b) => Math.abs(a - b) < 1e-6;
    if (approxEq(l, 1) && approxEq(m, 1) && approxEq(u, 1)) {
        return "(5,5,5)"; // 同等
    }
    const lLvl = ratioToNearestLevel(l);
    const mLvl = ratioToNearestLevel(m);
    const uLvl = ratioToNearestLevel(u);
    return `(${lLvl},${mLvl},${uLvl})`;
}

function createMatrixTable(matrix, names, onCellClick) {
    const table = document.createElement("table");
    const n = names.length;

    // ヘッダー
    const thead = document.createElement("thead");
    const headerRow = document.createElement("tr");
    headerRow.appendChild(document.createElement("th"));
    names.forEach((name) => {
        const th = document.createElement("th");
        th.textContent = name;
        headerRow.appendChild(th);
    });
    thead.appendChild(headerRow);
    table.appendChild(thead);

    // ボディ
    const tbody = document.createElement("tbody");
    for (let i = 0; i < n; i++) {
        const row = document.createElement("tr");
        // 行ラベル
        const th = document.createElement("th");
        th.textContent = names[i];
        row.appendChild(th);

        for (let j = 0; j < n; j++) {
            const td = document.createElement("td");
            const val = matrix[i][j];
            td.textContent = formatTriangularForDisplay(val);

            if (i === j) {
                td.classList.add("diagonal");
            } else if (j > i) {
                td.classList.add("editable");
                td.addEventListener("click", () => onCellClick(i, j));
            }
            row.appendChild(td);
        }
        tbody.appendChild(row);
    }
    table.appendChild(tbody);
    return table;
}

function renderCriteriaMatrix() {
    criteriaMatrixContainer.innerHTML = "";
    if (!state.criteriaMatrix.length) return;
    const table = createMatrixTable(state.criteriaMatrix, state.criteriaNames, handleCriteriaCellClick);
    criteriaMatrixContainer.appendChild(table);
}

function renderAltMatrices() {
    altMatricesContainer.innerHTML = "";
    const nCriteria = state.criteriaNames.length;
    for (let cIdx = 0; cIdx < nCriteria; cIdx++) {
        const wrapper = document.createElement("div");
        const title = document.createElement("h3");
        title.textContent = `基準: ${state.criteriaNames[cIdx]}`;
        wrapper.appendChild(title);

        const matrix = state.altMatrices[cIdx];
        const table = createMatrixTable(matrix, state.altNames, (i, j) => handleAltCellClick(cIdx, i, j));
        wrapper.appendChild(table);
        altMatricesContainer.appendChild(wrapper);
    }
}

function renderResults(criteriaWeights, altScores) {
    // 基準重み
    criteriaWeightsContainer.innerHTML = "";
    const cwTable = document.createElement("table");
    const cwHeader = document.createElement("tr");
    cwHeader.innerHTML = "<th>基準</th><th>重み</th>";
    cwTable.appendChild(cwHeader);
    criteriaWeights.forEach((w, idx) => {
        const tr = document.createElement("tr");
        tr.innerHTML = `<td>${state.criteriaNames[idx]}</td><td>${w.toFixed(4)}</td>`;
        cwTable.appendChild(tr);
    });
    criteriaWeightsContainer.appendChild(cwTable);

    // 候補スコア
    altScoresContainer.innerHTML = "";
    const asTable = document.createElement("table");
    const asHeader = document.createElement("tr");
    asHeader.innerHTML = "<th>候補</th><th>スコア</th>";
    asTable.appendChild(asHeader);
    altScores.forEach((s, idx) => {
        const tr = document.createElement("tr");
        tr.innerHTML = `<td>${state.altNames[idx]}</td><td>${s.toFixed(4)}</td>`;
        asTable.appendChild(tr);
    });
    altScoresContainer.appendChild(asTable);
}

/* ===============================
 * イベントハンドラー
 * =============================== */
function initBlankMatrices() {
    const nCriteria = state.criteriaNames.length;
    const nAlt = state.altNames.length;

    // 基準間行列
    state.criteriaMatrix = Array.from({ length: nCriteria }, (_, i) =>
        Array.from({ length: nCriteria }, (_, j) => {
            if (i === j) return [1, 1, 1];
            return j > i ? [1, 1, 1] : [1, 1, 1]; // placeholder; reciprocal will be updated after edits
        })
    );

    // 候補行列 (基準ごと)
    state.altMatrices = {};
    for (let cIdx = 0; cIdx < nCriteria; cIdx++) {
        state.altMatrices[cIdx] = Array.from({ length: nAlt }, (_, i) =>
            Array.from({ length: nAlt }, (_, j) => {
                if (i === j) return [1, 1, 1];
                return j > i ? [1, 1, 1] : [1, 1, 1];
            })
        );
    }
}

// === モーダル関連 DOM ===
const editModal = document.getElementById("editModal");
const repSelect = document.getElementById("repSelect");
const confSelect = document.getElementById("confSelect");
const modalOkBtn = document.getElementById("modalOkBtn");
const modalCancelBtn = document.getElementById("modalCancelBtn");

/**
 * 現在編集中のセル情報を保持
 * type: 'criteria' | 'alt'
 * For 'criteria': { type, i, j }
 * For 'alt'     : { type, cIdx, i, j }
 */
let editingContext = null;

function openEditModal(context) {
    editingContext = context;
    // 既存値があれば初期選択に反映 (省略可 — デフォルト 5, confident)
    repSelect.value = "5";
    confSelect.value = "0";

    editModal.classList.remove("hidden");
}

function closeEditModal() {
    editModal.classList.add("hidden");
    editingContext = null;
}

modalCancelBtn.addEventListener("click", closeEditModal);

modalOkBtn.addEventListener("click", () => {
    if (!editingContext) return;
    const repVal = parseInt(repSelect.value, 10);
    const delta = parseFloat(confSelect.value);

    const m = levelToRatio(repVal);
    const l = m * (1 - delta);
    const u = m * (1 + delta);
    const tfn = [l, m, u];

    if (editingContext.type === "criteria") {
        const { i, j } = editingContext;
        state.criteriaMatrix[i][j] = tfn;
        state.criteriaMatrix[j][i] = [1 / u, 1 / m, 1 / l];
        renderCriteriaMatrix();
    } else if (editingContext.type === "alt") {
        const { cIdx, i, j } = editingContext;
        state.altMatrices[cIdx][i][j] = tfn;
        state.altMatrices[cIdx][j][i] = [1 / u, 1 / m, 1 / l];
        renderAltMatrices();
    }

    closeEditModal();
});

function handleCriteriaCellClick(i, j) {
    openEditModal({ type: "criteria", i, j });
}

function handleAltCellClick(cIdx, i, j) {
    openEditModal({ type: "alt", cIdx, i, j });
}

function handleGenerate() {
    const criteriaLines = document.getElementById("criteriaInput").value
        .split(/\n+/)
        .map((s) => s.trim())
        .filter(Boolean);
    const altLines = document.getElementById("altInput").value
        .split(/\n+/)
        .map((s) => s.trim())
        .filter(Boolean);

    if (criteriaLines.length < 1 || altLines.length < 1) {
        alert("基準と候補を 1 行以上入力してください。");
        return;
    }

    state.criteriaNames = criteriaLines;
    state.altNames = altLines;

    initBlankMatrices();
    renderCriteriaMatrix();
    renderAltMatrices();
}

function handleCalc() {
    if (!state.criteriaMatrix.length) {
        alert("まずペア比較行列を生成してください。");
        return;
    }
    // 基準重み
    const criteriaWeights = computeWeightsByChang(state.criteriaMatrix);

    // 各基準での候補重み
    const nAlt = state.altNames.length;
    const altWeightsPerCriterion = state.criteriaNames.map((_, cIdx) =>
        computeWeightsByChang(state.altMatrices[cIdx])
    );

    // 候補総合スコア (単純加重合成)
    const altScores = new Array(nAlt).fill(0);
    for (let aIdx = 0; aIdx < nAlt; aIdx++) {
        let acc = 0;
        for (let cIdx = 0; cIdx < criteriaWeights.length; cIdx++) {
            acc += criteriaWeights[cIdx] * altWeightsPerCriterion[cIdx][aIdx];
        }
        altScores[aIdx] = acc;
    }

    renderResults(criteriaWeights, altScores);
}

/* ===============================
 * 初期化
 * =============================== */

document.getElementById("generateBtn").addEventListener("click", handleGenerate);
document.getElementById("calcBtn").addEventListener("click", handleCalc); 