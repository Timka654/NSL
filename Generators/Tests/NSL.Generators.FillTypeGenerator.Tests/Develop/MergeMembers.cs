using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop
{
    internal partial class MergeMembers1
    {
        public MergeMembersInner1 inner { get; set; }
    }

    [FillTypeGenerate(typeof(MergeMembers1))]
    [FillTypeFromGenerate(typeof(MergeMembers1))]
    internal partial class MergeMembers2
    {
        public MergeMembersInner2 inner { get; set; }
        /// </summary>
        internal void FillFrom2(MergeMembers1 fillFrom)
        {
            inner = fillFrom.inner == null ? null : new NSL.Generators.FillTypeGenerator.Tests.Develop.MergeMembersInner2
            {
                inner = fillFrom.inner.inner == null ? null : new NSL.Generators.FillTypeGenerator.Tests.Develop.MergeMembersIInner2
                {
                    a = fillFrom.inner.inner.a
                },
                a = fillFrom.inner.a
            };
        }
    }


    internal class MergeMembersInner1
    {
        public MergeMembersIInner1 inner { get; set; }
        public int a { get; set; }
    }

    internal class MergeMembersInner2
    {
        public MergeMembersIInner2 inner { get; set; }
        public int a { get; set; }
    }


    internal class MergeMembersIInner1
    {
        public MergeMembersIIInner1 inner { get; set; }
        public int a { get; set; }
    }

    internal class MergeMembersIInner2
    {
        public int a { get; set; }
        public MergeMembersIIInner2 inner { get; set; }
    }


    internal class MergeMembersIIInner1
    {
        public int a { get; set; }
    }

    internal class MergeMembersIIInner2
    {
        public int a { get; set; }
    }
}
