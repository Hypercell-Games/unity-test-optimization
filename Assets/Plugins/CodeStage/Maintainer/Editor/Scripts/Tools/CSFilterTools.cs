#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Core;
    using UnityEditor;

    internal class TypeFilter
    {
        public readonly bool compareBaseType;
        public readonly Type againstType;
        public readonly Type[] skipNonBaseTypes;

        public TypeFilter(Type againstType, Type[] skipNonBaseTypes)
        {
            this.againstType = againstType;
            this.skipNonBaseTypes = skipNonBaseTypes;
            compareBaseType = true;
        }

        public TypeFilter(Type againstType, bool compareBaseType = true)
        {
            this.againstType = againstType;
            this.compareBaseType = compareBaseType;
        }
    }

    internal static class CSFilterTools
    {
        public static List<AssetInfo> GetAssetInfosWithPaths(List<AssetInfo> assets, string[] paths)
        {
            var result = new List<AssetInfo>();

            foreach (var asset in assets)
            {
                if (Array.IndexOf(paths, asset.Path) != -1)
                {
                    result.Add(asset);
                }
            }

            return result;
        }

        public static List<AssetInfo> GetAssetInfosWithKind(List<AssetInfo> assets, AssetKind kind)
        {
            var result = new List<AssetInfo>();

            foreach (var asset in assets)
            {
                if (asset.Kind == kind)
                {
                    result.Add(asset);
                }
            }

            return result;
        }

        public static List<AssetInfo> GetAssetInfosWithKinds(List<AssetInfo> assets, List<AssetKind> kinds)
        {
            var result = new List<AssetInfo>();

            foreach (var asset in assets)
            {
                if (kinds.Contains(asset.Kind))
                {
                    result.Add(asset);
                }
            }

            return result;
        }

        public static List<AssetInfo> FilterAssetInfos(List<AssetInfo> assets, FilterItem filter)
        {
            return FilterAssetInfos(assets, new[] { filter });
        }

        public static List<AssetInfo> FilterAssetInfos(List<AssetInfo> assets, FilterItem[] filters)
        {
            var result = new List<AssetInfo>();

            foreach (var asset in assets)
            {
                var path = asset.Path;
                if (IsValueMatchesAnyFilter(path, filters))
                {
                    result.Add(asset);
                }
            }

            return result;
        }

        public static List<AssetInfo> FilterAssetInfos(List<AssetInfo> assets, List<TypeFilter> targetAssetTypes,
            FilterItem[] includes, FilterItem[] ignores)
        {
            var result = new List<AssetInfo>();
            var filteredByType = GetAssetInfosWithTypes(assets, targetAssetTypes);
            foreach (var assetInfo in filteredByType)
            {
                var path = assetInfo.Path;
                var skip = false;

                if (ignores != null && ignores.Length > 0)
                    skip = IsValueMatchesAnyFilter(path, ignores);

                if (skip)
                    continue;

                if (includes != null && includes.Length > 0)
                {
                    var include = IsValueMatchesAnyFilter(path, includes);
                    if (include) result.Add(assetInfo);
                }
                else
                {
                    result.Add(assetInfo);
                }
            }

            return result;
        }

        public static bool TryAddNewItemToFilters(ref FilterItem[] filters, FilterItem newItem)
        {
            foreach (var filterItem in filters)
            {
                if (filterItem.value == newItem.value)
                {
                    return false;
                }
            }

            ArrayUtility.Add(ref filters, newItem);
            return true;
        }

        public static bool IsValueMatchesAnyFilter(string value, IEnumerable<FilterItem> filters)
        {
            return IsValueMatchesAnyFilterOfKind(value, filters, FilterKind.NotSet);
        }

        public static bool HasEnabledFilters(IEnumerable<FilterItem> filters)
        {
            if (filters == null)
                return false;

            foreach (var filter in filters)
            {
                if (filter.enabled)
                    return true;
            }

            return false;
        }

        public static bool IsValueMatchesAnyFilterOfKind(string value, IEnumerable<FilterItem> filters, FilterKind onlyThisKind)
        {
            var match = false;
            var directory = string.Empty;
            var filename = string.Empty;
            var extension = string.Empty;

            foreach (var filter in filters)
            {
                if (onlyThisKind != FilterKind.NotSet)
                {
                    if (filter.kind != onlyThisKind)
                        continue;
                }

                switch (filter.kind)
                {
                    case FilterKind.Path:
                    case FilterKind.Type:
                        match = FilterMatchHelper(value, filter);
                        break;
                    case FilterKind.Directory:
                        if (directory == string.Empty)
                        {
                            directory = Path.GetDirectoryName(value);
                            if (directory != null) directory = CSPathTools.EnforceSlashes(directory);
                        }

                        if (directory != null)
                        {
                            match = FilterMatchHelper(directory, filter);
                        }
                        break;
                    case FilterKind.FileName:
                        if (filename == string.Empty)
                        {
                            filename = Path.GetFileName(value);
                            if (filename != null) filename = CSPathTools.EnforceSlashes(filename);
                        }
                        if (filename != null)
                        {
                            match = FilterMatchHelper(filename, filter);
                        }
                        break;
                    case FilterKind.Extension:
                        if (extension == string.Empty)
                        {
                            extension = Path.GetExtension(value);
                        }
                        match = string.Equals(extension, filter.value, StringComparison.OrdinalIgnoreCase);
                        break;
                    case FilterKind.NotSet:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (match)
                {
                    break;
                }
            }

            return match;
        }

        private static List<AssetInfo> GetAssetInfosWithTypes(List<AssetInfo> assets, List<TypeFilter> types)
        {
            var result = new List<AssetInfo>();

            foreach (var asset in assets)
            {
                foreach (var typeFilter in types)
                {
                    if (typeFilter.compareBaseType && asset.Type != null)
                    {
                        if (asset.Type.BaseType == typeFilter.againstType)
                        {
                            if (typeFilter.skipNonBaseTypes != null)
                            {
                                if (Array.IndexOf(typeFilter.skipNonBaseTypes, asset.Type) != -1)
                                {
                                    continue;
                                }
                            }
                            result.Add(asset);
                        }
                    }
                    else
                    {
                        if (asset.Type == typeFilter.againstType)
                        {
                            result.Add(asset);
                        }
                    }
                }
            }

            return result;
        }

        private static bool FilterMatchHelper(string value, FilterItem filter)
        {
            if (filter.exactMatch)
            {
                return string.Equals(value, filter.value, filter.ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }

            return value.IndexOf(filter.value, filter.ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) != -1;
        }

        public static FilterItem[] GetEnabledFilters(FilterItem[] filters)
        {
            return filters?.Where(filter => filter.enabled).ToArray();
        }
    }
}
