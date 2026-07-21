// ═══════════════════════════════════════════════════════════════════
// VALIDATION HARNESS — ShaderLib + test shaders for generator validation
// ═══════════════════════════════════════════════════════════════════
using NSL.ShaderVM;
using NSL.ShaderVM.Vulkan;
using NSL.ShaderVM.Vulkan.Attributes;
using System;

namespace NSL.ShaderVM.Experiments;

// ──────────────────────────────────────────────────────────────────
// ShaderLib — shared GLSL utility functions (copied from AIHost)
// ──────────────────────────────────────────────────────────────────
public static class ShaderLib
{
    [ShaderFunction("exp(x)")] public static float Exp(float x) => MathF.Exp(x);
    [ShaderFunction("pow(x,y)")] public static float Pow(float x, float y) => MathF.Pow(x, y);
    [ShaderFunction("cos(x)")] public static float Cos(float x) => MathF.Cos(x);
    [ShaderFunction("sin(x)")] public static float Sin(float x) => MathF.Sin(x);
    [ShaderFunction("sqrt(x)")] public static float Sqrt(float x) => MathF.Sqrt(x);
    [ShaderFunction("fma(a,b,c)")] public static float Fma(float a, float b, float c) => MathF.FusedMultiplyAdd(a, b, c);
    [ShaderFunction("uintBitsToFloat(x)")] public static float U32ToF32(uint x) => BitConverter.UInt32BitsToSingle(x);

    [ShaderFunction("f16tof32_k", Kind = ShaderFunctionKind.Inline)]
    public static float F16ToF32(uint h)
    {
        uint s = (h >> 15) & 1u;
        uint eu = (h >> 10) & 31u;
        uint m = h & 1023u;
        if (eu == 0u)
        {
            if (m == 0u) return 0.0f;
            int e2 = 1; while ((m & 1024u) == 0u) { m <<= 1; e2 -= 1; }
            m = (m & 1023u) << 13;
            uint exp = (uint)(e2 - 15 + 127);
            return U32ToF32((s << 31) | (exp << 23) | m);
        }
        if (eu == 31u)
        {
            if (m != 0u) return U32ToF32(0x7FC00000u);
            return s != 0u ? U32ToF32(0xFF800000u) : U32ToF32(0x7F800000u);
        }
        eu = eu - 15u + 127u;
        return U32ToF32((s << 31) | (eu << 23) | (m << 13));
    }

    [ShaderFunction("dequant_q4k_elem", Kind = ShaderFunctionKind.Inline)]
    public static float DequantQ4K(uint[] data, uint elementIdx)
    {
        uint blk = elementIdx / 256u;
        uint e = elementIdx % 256u;
        uint off = blk * 36u;
        float d = F16ToF32(data[off] & 0xFFFFu);
        float dmin = F16ToF32((data[off] >> 16) & 0xFFFFu);
        uint group = e / 64u;
        uint wg = e % 64u;
        uint isUpper = wg >= 32u ? 1u : 0u;
        uint wgLower = wg % 32u;
        uint si = group * 2u + isUpper;
        uint s0 = data[off + 1u];
        uint s1 = data[off + 2u];
        uint s2 = data[off + 3u];
        float sc, mn;
        if (si < 4u)
        {
            sc = (float)((s0 >> (int)(si * 8u)) & 63u);
            mn = (float)((s1 >> (int)(si * 8u)) & 63u);
        }
        else
        {
            uint j = si - 4u;
            sc = (float)(((s2 >> (int)(j * 8u)) & 0xFu) | (((s0 >> (int)(j * 8u + 6)) & 0x3u) << 4));
            mn = (float)(((s2 >> (int)(j * 8u + 4)) & 0xFu) | (((s1 >> (int)(j * 8u + 6)) & 0x3u) << 4));
        }
        uint qsByteOff = (si / 2u) * 32u + wgLower;
        uint qsIdx = off + 4u + qsByteOff / 4u;
        uint byteShift = (qsByteOff % 4u) * 8u;
        uint qsByte = (data[qsIdx] >> (int)byteShift) & 0xFFu;
        uint q = (qsByte >> (int)((si % 2u) * 4u)) & 0xFu;
        return d * sc * (float)q - dmin * mn;
    }

    [ShaderFunction("rms_norm", Kind = ShaderFunctionKind.Inline)]
    public static void RmsNorm(float[] x, float[] weight, float[] rmsOut, uint dModel, float eps, uint tid)
    {
        float partialSumSq = 0.0f;
        for (uint i = tid; i < dModel; i += 256u)
            partialSumSq = x[i] * x[i] + partialSumSq;
        rmsOut[tid] = partialSumSq;
    }

    [ShaderFunction("rms_norm_reduce", Kind = ShaderFunctionKind.Inline)]
    public static float RmsNormReduce(float[] rmsOut, uint dModel, float eps)
    {
        float totalSq = 0.0f;
        for (uint i = 0u; i < 256u; i++)
            totalSq += rmsOut[i];
        float rms = 1.0f / (Sqrt(totalSq / (float)dModel + eps));
        rmsOut[0] = rms;
        return rms;
    }

    [ShaderFunction("rope_pair", Kind = ShaderFunctionKind.Inline)]
    public static void RopePair(float x0, float x1, uint pair, float theta, float pos, uint headDim, out float o0, out float o1)
    {
        float freq = 1.0f / Pow(theta, (float)(pair * 2u) / (float)headDim);
        float angle = pos * freq;
        float c = Cos(angle);
        float s = Sin(angle);
        o0 = x0 * c - x1 * s;
        o1 = x0 * s + x1 * c;
    }

    [ShaderFunction("silu_f32", Kind = ShaderFunctionKind.Inline)]
    public static float Silu(float x)
    {
        return x / (1.0f + Exp(-x));
    }
}

// ──────────────────────────────────────────────────────────────────
// TEST 1: MatMulShader — basic compute, ctx.Fma, primary auto-map
// Source: AIHost/NSL.ShaderVMShaders/MatMulShader.cs
// ──────────────────────────────────────────────────────────────────
[VulkanShaderEntry(ShaderName = "matmul_f32", LocalSizeX = 256, TargetVersion = "vulkan1.0")]
partial class MatMulShader
{
    [ShaderBuffer(Binding = 0, Set = 0, ReadOnly = true)] public float[] A = default!;
    [ShaderBuffer(Binding = 1, Set = 0, ReadOnly = true)] public float[] B = default!;
    [ShaderBuffer(Binding = 2, Set = 0)] public float[] C = default!;

    [ShaderPushConstant] public uint M;
    [ShaderPushConstant] public uint K;
    [ShaderPushConstant] public uint N;

    public void Main(VulkanExecutionContext ctx)
    {
        uint idx = ctx.GlobalInvocationIdX;
        uint total = M * N;
        if (idx >= total) return;

        uint row = idx / N;
        uint col = idx % N;

        float sum = 0.0f;
        for (uint k = 0u; k < K; k++)
            sum = ctx.Fma(A[row * K + k], B[k * N + col], sum);

        C[row * N + col] = sum;
    }
}

// ──────────────────────────────────────────────────────────────────
// TEST 2: RmsNormShader — shared memory, barrier, ShaderLib.Sqrt
// Source: AIHost/NSL.ShaderVMShaders/RmsNormShader.cs
// ──────────────────────────────────────────────────────────────────
[VulkanShaderEntry(ShaderName = "rmsnorm_f32", LocalSizeX = 256, TargetVersion = "vulkan1.0")]
partial class RmsNormShader
{
    [ShaderBuffer(Binding = 0, Set = 0, ReadOnly = true)] public float[] X = default!;
    [ShaderBuffer(Binding = 1, Set = 0, ReadOnly = true)] public float[] Weight = default!;
    [ShaderBuffer(Binding = 2, Set = 0)] public float[] Out = default!;

    [ShaderPushConstant] public uint DModel = 2048;
    [ShaderPushConstant] public uint SeqLen = 1;
    [ShaderPushConstant] public float Eps = 1e-5f;

    [ShaderShared] public float[] Shared = new float[256];

    public void Main(VulkanExecutionContext ctx)
    {
        uint tid = ctx.LocalInvocationIdX;
        uint row = ctx.WorkGroupIdX;
        if (row >= SeqLen) return;

        uint rowBase = row * DModel;

        float partialSq = 0.0f;
        for (uint i = tid; i < DModel; i += 256u)
        {
            float v = X[rowBase + i];
            partialSq = ctx.Fma(v, v, partialSq);
        }

        Shared[tid] = partialSq;
        ctx.Barrier();
        for (uint s = 128u; s > 0u; s = s / 2u)
        {
            if (tid < s) Shared[tid] += Shared[tid + s];
            ctx.Barrier();
        }
        float rms = 1.0f / ShaderLib.Sqrt(Shared[0u] / (float)DModel + Eps);
        ctx.Barrier();

        for (uint i = tid; i < DModel; i += 256u)
        {
            Out[rowBase + i] = X[rowBase + i] * rms * Weight[i];
        }
    }
}

// ──────────────────────────────────────────────────────────────────
// TEST 3: DequantQ4KShader — complex Inline via ShaderLib
// Source: AIHost/NSL.ShaderVMShaders/DequantShaders.cs
// ──────────────────────────────────────────────────────────────────
[VulkanShaderEntry(ShaderName = "dequant_q4k_f32", LocalSizeX = 256, TargetVersion = "vulkan1.0")]
partial class DequantQ4KShader
{
    [ShaderBuffer(Binding = 0, Set = 0, ReadOnly = true)] public uint[] QData = default!;
    [ShaderBuffer(Binding = 1, Set = 0)] public float[] Output = default!;
    [ShaderPushConstant] public uint InElementOffset;
    [ShaderPushConstant] public uint OutElementOffset;

    public void Main(VulkanExecutionContext ctx)
    {
        uint gi = ctx.GlobalInvocationIdX;
        uint gidIn = gi + InElementOffset;
        uint gidOut = gi + OutElementOffset;
        Output[gidOut] = ShaderLib.DequantQ4K(QData, gidIn);
    }
}

// ──────────────────────────────────────────────────────────────────
// TEST 4: FusedFfnBlockShader — heavy cross-class Inline (RmsNorm, Silu)
// Source: AIHost/NSL.ShaderVMShaders/FusedFfnBlockShader.cs
// ──────────────────────────────────────────────────────────────────
[VulkanShaderEntry(ShaderName = "fused_ffn_block", LocalSizeX = 256, TargetVersion = "vulkan1.0", UniformsViaSSBO = true)]
partial class FusedFfnBlockShader
{
    [ShaderBuffer(Binding = 0, Set = 0, ReadOnly = true)] public float[] AttnOut = default!;
    [ShaderBuffer(Binding = 1, Set = 0, ReadOnly = true)] public float[] X = default!;
    [ShaderBuffer(Binding = 2, Set = 0, ReadOnly = true)] public float[] WO = default!;
    [ShaderBuffer(Binding = 3, Set = 0, ReadOnly = true)] public float[] FfnNorm = default!;
    [ShaderBuffer(Binding = 4, Set = 0, ReadOnly = true)] public float[] WGate = default!;
    [ShaderBuffer(Binding = 5, Set = 0, ReadOnly = true)] public float[] WUp = default!;
    [ShaderBuffer(Binding = 6, Set = 0, ReadOnly = true)] public float[] WDown = default!;
    [ShaderBuffer(Binding = 7, Set = 0)] public float[] Output = default!;
    [ShaderBuffer(Binding = 8, Set = 0)] public float[] GateUpTmp = default!;
    [ShaderBuffer(Binding = 9, Set = 0)] public float[] ResidualTmp = default!;

    [ShaderUniform] public uint DModel = 2048;
    [ShaderUniform] public uint QDim = 2048;
    [ShaderUniform] public uint FfnDim = 5504;
    [ShaderUniform] public float Eps = 1e-5f;

    public void Main(VulkanExecutionContext ctx)
    {
        uint tid = ctx.LocalInvocationIdX;

        // Step 1: W_O projection + residual
        for (uint i = tid; i < DModel; i += 256u)
        {
            float sum = 0.0f;
            for (uint j = 0u; j < QDim; j++)
                sum = ctx.Fma(AttnOut[j], WO[j + i * QDim], sum);
            ResidualTmp[i] = X[i] + sum;
        }
        ctx.Barrier();

        // Step 2: RMSNorm
        ShaderLib.RmsNorm(ResidualTmp, FfnNorm, GateUpTmp, DModel, Eps, tid);
        ctx.Barrier();
        float rmsVal = ShaderLib.RmsNormReduce(GateUpTmp, DModel, Eps);
        ctx.Barrier();

        // Step 3: Gate + Up → SiLU(gate) * up
        for (uint col = tid; col < FfnDim; col += 256u)
        {
            float sumGate = 0.0f, sumUp = 0.0f;
            for (uint k = 0u; k < DModel; k++)
            {
                float xn = ResidualTmp[k] * FfnNorm[k] * rmsVal;
                sumGate = ctx.Fma(xn, WGate[k + col * DModel], sumGate);
                sumUp = ctx.Fma(xn, WUp[k + col * DModel], sumUp);
            }
            GateUpTmp[col] = ShaderLib.Silu(sumGate) * sumUp;
        }
        ctx.Barrier();

        // Step 4: Down projection + residual
        for (uint i = tid; i < DModel; i += 256u)
        {
            float sum = 0.0f;
            for (uint col = 0u; col < FfnDim; col++)
                sum = ctx.Fma(GateUpTmp[col], WDown[col + i * FfnDim], sum);
            Output[i] = ResidualTmp[i] + sum;
        }
    }
}

// ──────────────────────────────────────────────────────────────────
// TEST 5: RowwiseSoftmaxShader — shared reduction, ctx.Exp
// Source: AIHost/NSL.ShaderVMShaders/RowwiseSoftmaxShader.cs
// ──────────────────────────────────────────────────────────────────
[VulkanShaderEntry(ShaderName = "rowwise_softmax_f32", LocalSizeX = 256, TargetVersion = "vulkan1.0")]
partial class RowwiseSoftmaxShader
{
    [ShaderBuffer(Binding = 0, Set = 0)] public float[] Data = default!;

    [ShaderPushConstant] public uint Rows;
    [ShaderPushConstant] public uint Cols;

    [ShaderShared] public float[] Shared = new float[256];

    public void Main(VulkanExecutionContext ctx)
    {
        uint row = ctx.WorkGroupIdX;
        uint tid = ctx.LocalInvocationIdX;
        if (row >= Rows) return;

        uint rowOff = row * Cols;

        float maxVal = -1e38f;
        for (uint i = tid; i < Cols; i += 256u)
        { float v = Data[rowOff + i]; if (v > maxVal) maxVal = v; }
        Shared[tid] = maxVal;
        ctx.Barrier();

        for (uint s = 128u; s > 0u; s = s / 2u)
        {
            if (tid < s)
            { float a = Shared[tid], b = Shared[tid + s]; Shared[tid] = a > b ? a : b; }
            ctx.Barrier();
        }
        float rowMax = Shared[0u];
        ctx.Barrier();

        float sumExp = 0.0f;
        for (uint i = tid; i < Cols; i += 256u)
        {
            float v = ctx.Exp(Data[rowOff + i] - rowMax);
            Data[rowOff + i] = v;
            sumExp += v;
        }
        Shared[tid] = sumExp;
        ctx.Barrier();

        for (uint s = 128u; s > 0u; s = s / 2u)
        {
            if (tid < s) Shared[tid] += Shared[tid + s];
            ctx.Barrier();
        }
        float rowSum = Shared[0u];

        float invSum = 1.0f / rowSum;
        for (uint i = tid; i < Cols; i += 256u)
            Data[rowOff + i] *= invSum;
    }
}

// ──────────────────────────────────────────────────────────────────
// TEST 6: SlotSimulationShader — Inline + ref, complex RNG
// Source: CasinoBackOffice/.../Shaders/SlotSimulationShader.cs
// (stripped #if HAS_NSL.ShaderVM, simplified)
// ──────────────────────────────────────────────────────────────────
[VulkanShaderEntry(ShaderName = "slot_sim", LocalSizeX = 256, TargetVersion = "vulkan1.0")]
partial class SlotSimulationShader
{
    [ShaderBuffer(Binding = 0, Set = 0, ReadOnly = true)] public uint[] Paytable = default!;
    [ShaderBuffer(Binding = 1, Set = 0, ReadOnly = true)] public uint[] ReelStrips = default!;
    [ShaderBuffer(Binding = 2, Set = 0, ReadOnly = true)] public uint[] StripLens = default!;
    [ShaderBuffer(Binding = 3, Set = 0, ReadOnly = true)] public uint[] StripOffsets = default!;
    [ShaderBuffer(Binding = 4, Set = 0, ReadOnly = true)] public uint[] ReelSetCW = default!;
    [ShaderBuffer(Binding = 5, Set = 0, ReadOnly = true)] public uint[] Paylines = default!;
    [ShaderBuffer(Binding = 6, Set = 0, ReadOnly = true)] public uint[] CoinValues = default!;
    [ShaderBuffer(Binding = 7, Set = 0, ReadOnly = true)] public uint[] CoinCW = default!;
    [ShaderBuffer(Binding = 8, Set = 0, ReadOnly = true)] public uint[] DragonMults = default!;
    [ShaderBuffer(Binding = 9, Set = 0, ReadOnly = true)] public uint[] DragonMultCW = default!;
    [ShaderBuffer(Binding = 10, Set = 0, ReadOnly = true)] public uint[] DragonBoosts = default!;
    [ShaderBuffer(Binding = 11, Set = 0, ReadOnly = true)] public uint[] DragonBoostCW = default!;
    [ShaderBuffer(Binding = 12, Set = 0)] public uint[] Results = default!;
    [ShaderBuffer(Binding = 13, Set = 0)] public uint[] Flags = default!;

    [ShaderUniform] public uint NumSim;
    [ShaderUniform] public uint Seed;
    [ShaderUniform] public uint Reels;
    [ShaderUniform] public uint Rows;
    [ShaderUniform] public uint NumPaylines;
    [ShaderUniform] public uint NumSymbols;
    [ShaderUniform] public uint MatchLevels;
    [ShaderUniform] public uint MinMatch;
    [ShaderUniform] public uint NumReelSets;
    [ShaderUniform] public uint NumCoinValues;
    [ShaderUniform] public uint NumDragonMults;
    [ShaderUniform] public uint NumDragonBoosts;
    [ShaderUniform] public uint ActiveMechanics;
    [ShaderUniform] public uint WildSym;
    [ShaderUniform] public uint WildExcludedReels;
    [ShaderUniform] public uint HnWTriggerSym;
    [ShaderUniform] public uint HnWTriggerCount;
    [ShaderUniform] public uint HnWRespins;
    [ShaderUniform] public uint HnWCoinChance;
    [ShaderUniform] public uint ComboDragonSym;
    [ShaderUniform] public uint ComboMonkeySym;
    [ShaderUniform] public uint ComboPandaSym;
    [ShaderUniform] public uint RowFillBonus;
    [ShaderUniform] public uint GrandPrize;

    [ShaderShared(Size = 7680)]
    public uint[] Screen = default!;

    [ShaderShared(Size = 7680)]
    public uint[] Coins = default!;

    // ── Inline RNG ──
    [ShaderFunction("pcg_next", Kind = ShaderFunctionKind.Inline)]
    private uint PcgNext(ref uint state)
    {
        state = state * 747796405u + 2891336453u;
        state ^= state >> 16;
        state *= 2246822519u;
        state ^= state >> 13;
        uint w = ((state >> (int)((state >> 28) + 4u)) ^ state) * 277803737u;
        return (w >> 22) ^ w;
    }

    // ── Inline weighted choices ──
    [ShaderFunction("pick_rs", Kind = ShaderFunctionKind.Inline)]
    private uint PickReelSet(ref uint rng)
    {
        uint total = ReelSetCW[NumReelSets - 1];
        uint roll = PcgNext(ref rng) % total;
        for (uint i = 0; i < NumReelSets; i++)
            if (roll < ReelSetCW[i]) return i;
        return NumReelSets - 1;
    }

    [ShaderFunction("pick_coin", Kind = ShaderFunctionKind.Inline)]
    private uint PickCoin(ref uint rng)
    {
        uint total = CoinCW[NumCoinValues - 1];
        uint roll = PcgNext(ref rng) % total;
        for (uint i = 0; i < NumCoinValues; i++)
            if (roll < CoinCW[i]) return CoinValues[i];
        return CoinValues[NumCoinValues - 1];
    }

    [ShaderFunction("pick_dm", Kind = ShaderFunctionKind.Inline)]
    private uint PickDMult(ref uint rng)
    {
        uint total = DragonMultCW[NumDragonMults - 1];
        uint roll = PcgNext(ref rng) % total;
        for (uint i = 0; i < NumDragonMults; i++)
            if (roll < DragonMultCW[i]) return DragonMults[i];
        return DragonMults[NumDragonMults - 1];
    }

    [ShaderFunction("pick_db", Kind = ShaderFunctionKind.Inline)]
    private uint PickDBoost(ref uint rng)
    {
        uint total = DragonBoostCW[NumDragonBoosts - 1];
        uint roll = PcgNext(ref rng) % total;
        for (uint i = 0; i < NumDragonBoosts; i++)
            if (roll < DragonBoostCW[i]) return DragonBoosts[i];
        return DragonBoosts[NumDragonBoosts - 1];
    }

    // ── MAIN ──
    public void Main(VulkanExecutionContext ctx)
    {
        uint tid = ctx.GlobalInvocationIdX;
        if (tid >= NumSim) return;
        uint lid = ctx.LocalInvocationIdX;
        uint tc = Reels * Rows;
        uint scOff = lid * 30;

        uint rng = tid * 196314165u + Seed * 3266489917u + 69609u;
        PcgNext(ref rng);

        uint rs = PickReelSet(ref rng);

        for (uint reel = 0; reel < Reels; reel++)
        {
            uint si = rs * Reels + reel;
            uint off = StripOffsets[si];
            uint len = StripLens[si];
            uint pos = PcgNext(ref rng) % len;
            for (uint row = 0; row < Rows; row++)
                Screen[scOff + row * Reels + reel] = ReelStrips[off + (pos + row) % len];
        }
        ctx.Barrier();

        uint totalWin = 0u;
        for (uint pl = 0; pl < NumPaylines; pl++)
        {
            uint b = pl * Reels;
            uint baseSym = Screen[scOff + Paylines[b] * Reels];
            if (baseSym >= NumSymbols) continue;

            uint ml = 1u;
            for (uint r = 1; r < Reels; r++)
            {
                uint sym = Screen[scOff + Paylines[b + r] * Reels + r];
                bool isWild = (sym == WildSym) && ((WildExcludedReels & (1u << (int)r)) == 0);
                if (sym == baseSym || isWild) ml++;
                else break;
            }
            if (ml < MinMatch) continue;

            uint si2 = baseSym;
            uint mi = ml - MinMatch;
            if (si2 < NumSymbols && mi < MatchLevels)
                totalWin += Paytable[si2 * MatchLevels + mi];
        }

        // Count specials
        uint cashN = 0u;
        bool hasD = false, hasM = false, hasP = false;
        for (uint i = 0; i < tc; i++)
        {
            uint sym = Screen[scOff + i];
            if (sym == HnWTriggerSym) cashN++;
            if (sym == ComboDragonSym) hasD = true;
            if (sym == ComboMonkeySym) hasM = true;
            if (sym == ComboPandaSym) hasP = true;
        }

        uint hwF = 0u;
        if (hasD) hwF |= 2u;
        if (hasM) hwF |= 4u;
        if (hasP) hwF |= 8u;

        // Hold & Win
        if ((ActiveMechanics & 1u) != 0u && cashN >= HnWTriggerCount)
        {
            hwF |= 1u;
            uint resp = HnWRespins;
            if ((ActiveMechanics & 8u) != 0u && hasP) resp++;

            uint boards = ((ActiveMechanics & 4u) != 0u && hasM) ? 2u : 1u;
            bool dragonActive = (ActiveMechanics & 2u) != 0u && hasD;

            for (uint board = 0; board < boards; board++)
            {
                for (uint i = 0; i < tc; i++)
                    Coins[scOff + i] = (Screen[scOff + i] == HnWTriggerSym) ? PickCoin(ref rng) : 0u;

                uint left = resp;
                uint iter = 0u;
                while (left > 0u && iter < 50u)
                {
                    left--; iter++; uint nc = 0u;
                    for (uint i = 0; i < tc; i++)
                    {
                        if (Coins[scOff + i] == 0u && PcgNext(ref rng) % 100u < HnWCoinChance)
                        {
                            Coins[scOff + i] = PickCoin(ref rng);
                            nc++;
                        }
                    }
                    if (nc > 0u) left = resp;
                }

                if (dragonActive)
                {
                    uint dm = PickDMult(ref rng);
                    uint db = PickDBoost(ref rng);
                    uint ap = 0u;
                    for (uint i = 0; i < tc; i++)
                    {
                        if (Coins[scOff + i] > 0u && ap < db)
                        {
                            if (PcgNext(ref rng) % (tc - i) < db - ap)
                            {
                                Coins[scOff + i] *= dm;
                                ap++;
                            }
                        }
                    }
                }

                uint prize = 0u;
                for (uint i = 0; i < tc; i++) prize += Coins[scOff + i];

                bool all = true;
                for (uint i = 0; i < tc; i++) if (Coins[scOff + i] == 0u) { all = false; break; }
                if (all) { totalWin += GrandPrize; continue; }

                for (uint row = 0; row < Rows; row++)
                {
                    bool rowFull = true;
                    for (uint col = 0; col < Reels; col++)
                        if (Coins[scOff + row * Reels + col] == 0u) { rowFull = false; break; }
                    if (rowFull) prize += RowFillBonus;
                }

                totalWin += prize;
            }

            if (totalWin > 0u) hwF |= 16u;
        }

        Results[tid] = totalWin;
        Flags[tid] = hwF;
    }
}
