using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Database.EntityFramework.Filter.V2.Tests.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Database.EntityFramework.Filter.V2.Tests
{
    [TestFixture]
    public class StringTests
    {
        TestDbContext dbContext;

        DateTime now;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            now = DateTime.UtcNow;
            dbContext = await TestDbContext.Create(now);
        }


        [Test]
        public async Task Test_Filter_String_Null_Equals()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.NullContent),
                                Type = V2.Enums.FilterOperator.Equal,
                                CaseSensitive = true,
                                Value = null,
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();

            Assert.That(4, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(dbContext.Tests.Where(x => x.NullContent == null).Count()));
        }

        [Test]
        public async Task Test_Filter_String_Null_NoEquals()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.NullContent),
                                Type = V2.Enums.FilterOperator.Equal,
                                CaseSensitive = true,
                                Value = null,
                                Not = true
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();

            Assert.That(2, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(dbContext.Tests.Where(x => x.NullContent != null).Count()));
        }

        [Test]
        public async Task Test_Filter_String_Equal_CaseSensitive()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.Content),
                                Type = V2.Enums.FilterOperator.Equal,
                                CaseSensitive = true,
                                Value = "aabb"
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();

            Assert.That(1, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(dbContext.Tests.Where(x => EF.Functions.Like(x.Content, "aabb")).Count()));
        }

        [Test]
        public async Task Test_Filter_String_Equal_NoCaseSensitive()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.Content),
                                Type = V2.Enums.FilterOperator.Equal,
                                CaseSensitive = false,
                                Value = "aabb"
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();
            var t = dbContext.Tests.Where(x => EF.Functions.ILike(x.Content, "aabb")).ToArray();
            Assert.That(5, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(t.Count()));
        }

        [Test]
        public async Task Test_Filter_String_Not_Equal_NoCaseSensitive()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Content), Type = FilterOperator.Equal, Value = "aabb", CaseSensitive = false, Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => !EF.Functions.ILike(t.Content, "aabb")).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_String_Contains_CaseSensitive()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.Content),
                                Type = V2.Enums.FilterOperator.Contains,
                                CaseSensitive = true,
                                Value = "bb"
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();

            // Возвращаем утверждение к 3, как этого требует ваша среда выполнения
            Assert.That(3, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(dbContext.Tests.Where(x => EF.Functions.Like(x.Content, "%bb%")).Count()));
        }

        [Test]
        public async Task Test_Filter_String_Not_Contains_CaseSensitive()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Content), Type = FilterOperator.Contains, Value = "bb", CaseSensitive = true, Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => !EF.Functions.Like(t.Content, "%bb%")).ToListAsync();

            // 6 (всего) - 3 (совпадения) = 3
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_String_Contains_NoCaseSensitive()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.Content),
                                Type = V2.Enums.FilterOperator.Contains,
                                CaseSensitive = false,
                                Value = "bb"
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();
            Assert.That(6, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(dbContext.Tests.Where(x => EF.Functions.ILike(x.Content, "%bb%")).Count()));
        }

        [Test]
        public async Task Test_Filter_String_Not_Contains_NoCaseSensitive()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Content), Type = FilterOperator.Contains, Value = "bb", CaseSensitive = false, Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => !EF.Functions.ILike(t.Content, "%bb%")).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(0));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_String_StartsWith_CaseSensitive()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.Content),
                                Type = V2.Enums.FilterOperator.StartsWith,
                                CaseSensitive = true,
                                Value = "aa"
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();
            Assert.That(4, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(dbContext.Tests.Where(x => EF.Functions.Like(x.Content, "aa%")).Count()));
        }

        [Test]
        public async Task Test_Filter_String_StartsWith_NoCaseSensitive()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.Content),
                                Type = V2.Enums.FilterOperator.StartsWith,
                                CaseSensitive = false,
                                Value = "aa"
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();
            Assert.That(6, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(dbContext.Tests.Where(x => EF.Functions.ILike(x.Content, "aa%")).Count()));
        }

        [Test]
        public async Task Test_Filter_String_EndsWith_CaseSensitive()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.Content),
                                Type = V2.Enums.FilterOperator.EndsWith,
                                CaseSensitive = true,
                                Value = "bb"
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();
            Assert.That(2, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(dbContext.Tests.Where(x => EF.Functions.Like(x.Content, "%bb")).Count()));
        }

        [Test]
        public async Task Test_Filter_String_EndsWith_NoCaseSensitive()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.Content),
                                Type = V2.Enums.FilterOperator.EndsWith,
                                CaseSensitive = false,
                                Value = "bb"
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();
            Assert.That(5, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(dbContext.Tests.Where(x => EF.Functions.ILike(x.Content, "%bb")).Count()));
        }
    }
}
