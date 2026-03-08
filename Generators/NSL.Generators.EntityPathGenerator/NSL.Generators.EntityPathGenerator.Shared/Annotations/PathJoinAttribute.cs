using System;

namespace NSL.Entity.PathGenerator.Shared.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PathJoinAttribute : Attribute
    {
        public string TargetModel { get; }
        public string[] SourceModels { get; }

        public PathJoinAttribute(string targetModel, params string[] sourceModels)
        {
            TargetModel = targetModel;
            SourceModels = sourceModels;
        }
    }
}