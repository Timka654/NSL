using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Pagination
{
    public abstract class PaginatorBaseComponent : ComponentBase
    {
        public delegate Task ChangePageDelegate(int page, int skip, int take);

        protected long _ItemsCount { get; set; }

        protected int _ItemsPerPage { get; set; }

        protected int _Page { get; set; } = 1;

        [Parameter] public long ItemsCount { get; set; }

        /// <summary>
        /// default: 25
        /// </summary>
        [Parameter] public int ItemsPerPage { get; set; } = 25;

        [Parameter] public int Page { get; set; } = 1;

        [Parameter] public ChangePageDelegate OnChangePage { get; set; }

        protected int PageCount { get; set; }

        protected Range[] PageRanges = [];

        protected bool HavePrev => _Page > 1;
        protected bool HaveNext => _Page < PageCount;

        protected int InputPage { get; set; }

        protected int startOffset { get; set; }
        protected int endOffset { get; set; }

        protected void CalculatePageCount()
        {
            PageCount = (int)(_ItemsCount / _ItemsPerPage) + (_ItemsCount % _ItemsPerPage > 0 ? 1 : 0);

            PageCount = PageCount == 0 ? 1 : PageCount;
        }


        protected void CalculatePageRanges()
        {
            startOffset = ItemsPerPage * (Page - 1);
            endOffset = (int)Math.Min(ItemsPerPage * Page, ItemsCount);

            if (PageCount < 10)
            {
                PageRanges = [new Range(1, PageCount)];
                return;
            }

            List<Range> ranges = new List<Range>();

            var lpage = 0;

            ranges.Add(new Range(1, lpage = 3));

            if (_Page > 1)
            {
                if (lpage < _Page - 3)
                {
                    ranges.Add(new Range());
                    lpage = _Page - 3;
                }

                if (_Page + 2 < PageCount)
                    ranges.Add(new Range(lpage + 1, lpage = _Page + 2));
                else
                    ranges.Add(new Range(lpage + 1, lpage = PageCount));
            }

            if (lpage < PageCount - 3)
            {
                ranges.Add(new Range());
                lpage = PageCount - 3;
            }

            if (lpage != PageCount)
                ranges.Add(new Range(lpage + 1, lpage = PageCount));

            PageRanges = ranges.ToArray();
        }

        protected virtual async Task ClickHandle(int page)
        {
            _Page = page;
            InputPage = page;

            CalculatePageRanges();

            await OnChangePage(page, (page - 1) * _ItemsPerPage, _ItemsPerPage);
        }

        protected override async Task OnInitializedAsync()
        {
            _ItemsCount = ItemsCount;
            _ItemsPerPage = ItemsPerPage;

            CalculatePageCount();

            _Page = Page;

            if (_Page > PageCount)
                _Page = PageCount;

            if (_Page < 1)
                _Page = 1;

            InputPage = _Page;

            CalculatePageRanges();

            if (Page != _Page)
            {
                await ClickHandle(_Page);
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_ItemsCount != ItemsCount || _ItemsPerPage != ItemsPerPage)
            {
                _ItemsCount = ItemsCount;
                _ItemsPerPage = ItemsPerPage;
                CalculatePageCount();
                CalculatePageRanges();
            }

            if (Page > PageCount)
                Page = PageCount;

            if (Page < 0)
                Page = 1;

            if (_Page != Page)
            {
                _Page = Page;

                CalculatePageRanges();

                await ClickHandle(_Page);
            }
        }
    }
}
