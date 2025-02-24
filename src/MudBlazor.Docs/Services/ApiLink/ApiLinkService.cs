﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MudBlazor.Docs.Models;

namespace MudBlazor.Docs.Services
{
#nullable enable
    public class ApiLinkService : IApiLinkService
    {
        private readonly Dictionary<string, ApiLinkServiceEntry> _lookup = [];

        //constructor with DI
        public ApiLinkService(IMenuService menuService)
        {
            Register(menuService.Api); // this also registers components
            Register(menuService.Customization);
            Register(menuService.Features);
            Register(menuService.Utilities);
            RegisterAliases();
        }

        public void RegisterPage(string title, string? subtitle, Type? componentType, string? link = null)
        {
            link ??= ApiLink.GetComponentLinkFor(componentType!);
            var entry = new ApiLinkServiceEntry
            {
                Title = title,
                SubTitle = subtitle,
                ComponentType = componentType,
                Link = link
            };
            _lookup[title.ToLowerInvariant()] = entry;

            if (componentType is not null)
            {
                _lookup[componentType.Name.ToLowerInvariant()] = entry;
            }

            if (subtitle is not null)
            {
                _lookup[subtitle.ToLowerInvariant()] = entry;
            }
        }

        public Task<IReadOnlyCollection<ApiLinkServiceEntry>> Search(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Task.FromResult<IReadOnlyCollection<ApiLinkServiceEntry>>(Array.Empty<ApiLinkServiceEntry>());
            }

            var textLowerInvariant = text.ToLowerInvariant();

            return Task.FromResult<IReadOnlyCollection<ApiLinkServiceEntry>>(
                _lookup
                    .Where(x => IsMatch(x, textLowerInvariant))
                    .Select(x => x.Value)
                    .Distinct()
                    .OrderByDescending(e => e.Title.StartsWith(textLowerInvariant, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray()
            );
        }

        private void RegisterAliases()
        {
            // Add search texts here which users might search and direct them to the correct component or page
            RegisterPage("Backdrop", subtitle: "Go to Overlay", componentType: typeof(MudOverlay));
            RegisterPage("Box", subtitle: "Go to Paper", componentType: typeof(MudPaper));
            RegisterPage("ComboBox", subtitle: "Go to Select", componentType: typeof(MudSelect<T>));
            RegisterPage("Drag & Drop", subtitle: "Go to DropZone", componentType: typeof(MudDropZone<T>));
            RegisterPage("Dropdown", subtitle: "Go to Select", componentType: typeof(MudSelect<T>));
            RegisterPage("Harmonica", subtitle: "Go to ExpansionPanels", componentType: typeof(MudExpansionPanels));
            RegisterPage("Horizontal Line", subtitle: "Go to Divider", componentType: typeof(MudDivider));
            RegisterPage("Hiliter", subtitle: "Go to Highlighter", componentType: typeof(MudHighlighter));
            RegisterPage("Notification", subtitle: "Go to Snackbar", componentType: typeof(MudSnackbarProvider));
            RegisterPage("Popup", subtitle: "Go to Popover", componentType: typeof(MudPopover));
            RegisterPage("SidePanel", subtitle: "Go to Drawer", componentType: typeof(MudDrawer));
            RegisterPage("Toast", subtitle: "Go to Snackbar", componentType: typeof(MudSnackbarProvider));
            RegisterPage("Typeahead", subtitle: "Go to Autocomplete", componentType: typeof(MudAutocomplete<T>));
        }

        private void Register(IEnumerable<MudComponent> items)
        {
            foreach (var item in items)
            {
                // components
                RegisterPage(
                    title: item.Name,
                    subtitle: $"{item.ComponentName} usage examples",
                    componentType: item.Type,
                    link: $"components/{item.Link}"
                );

                // ~henon: I removed the API pages from the dropdown as this duplicates the search entries unnecessarily
                // 99% of all users are searching for the examples and the API is still accessible from the examples page
                //api
                //RegisterPage(
                //    title: $"{item.Name} API" ,
                //    subtitle: $"{item.ComponentName} API documentation",
                //    componentType: item.Type,
                //    link: ApiLink.GetApiLinkFor(item.Type)
                //    );
            }
        }

        private void Register(IEnumerable<DocsLink> links)
        {
            foreach (var link in links)
            {
                RegisterPage(
                    title: link.Title,
                    subtitle: "",
                    componentType: null,
                    link: link.Href
                );
            }
        }

        private static bool IsMatch(in KeyValuePair<string, ApiLinkServiceEntry> keyValuePair, string text)
        {
            if (keyValuePair.Key.Contains(text))
            {
                return true;
            }

            var entry = keyValuePair.Value;
            if (entry.Title.Contains(text, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (entry.SubTitle is not null && entry.SubTitle.Contains(text, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (entry.ComponentName is not null && entry.ComponentName.Contains(text, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (entry.Link.Contains(text, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
