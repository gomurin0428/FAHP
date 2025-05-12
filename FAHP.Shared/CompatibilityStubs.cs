#if NETSTANDARD2_0
using System;
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 定義済みの型が netstandard2.0 の参照アセンブリに存在しないため、移植コード用にダミー定義を提供します。
    /// C# 9 の init アクセサや C# 11 の required キーワードで必要になります。
    /// </summary>
    internal static class IsExternalInit { }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal sealed class RequiredMemberAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;
        public string FeatureName { get; }
        public bool IsOptional { get; init; }
    }
}
#endif 