using System;
using System.Collections.Generic;
using System.Linq;

namespace EventGroups.App.Services
{
    public class MenuItem
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Expanded { get; set; }
        public IEnumerable<MenuItem> Children { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }

    public class NavMenuService
    {
        private MenuItem[] _allMenuItems = new[]
        {
            new MenuItem()
            {
                Name = "Solutions",
                Title = "How to get started with the Radzen Blazor components",
                Path = "/solutionslist",
                Icon = "&#xe871"
            },
            new MenuItem
            {
                Name = "Event groups",
                Title = "How to get started with the Radzen Blazor components",
                Path = "/eventgroupslist",
                Icon = "&#xe037"
            }
        };

        public IEnumerable<MenuItem> MenuItems
        {
            get
            {
                return _allMenuItems;
            }
        }

        public IEnumerable<MenuItem> Filter(string term)
        {
            bool contains(string value) => value.Contains(term, StringComparison.OrdinalIgnoreCase);

            bool filter(MenuItem example) => contains(example.Name) || (example.Tags != null && example.Tags.Any(contains));

            bool deepFilter(MenuItem example) => filter(example) || example.Children?.Any(filter) == true;

            return MenuItems.Where(category => category.Children?.Any(deepFilter) == true)
                           .Select(category => new MenuItem
                           {
                               Name = category.Name,
                               Expanded = true,
                               Children = category.Children.Where(deepFilter).Select(example => new MenuItem
                               {
                                   Name = example.Name,
                                   Path = example.Path,
                                   Icon = example.Icon,
                                   Expanded = true,
                                   Children = example.Children
                               }
                               ).ToArray()
                           }).ToList();
        }

        public MenuItem FindCurrent(Uri uri)
        {
            return MenuItems.SelectMany(example => example.Children ?? new[] { example })
                           .FirstOrDefault(example => example.Path == uri.AbsolutePath || $"/{example.Path}" == uri.AbsolutePath);
        }

        public string TitleFor(MenuItem menuItem)
        {
            if (menuItem != null)
            {
                return menuItem.Title ?? $"";
            }

            return "";
        }
    }
}