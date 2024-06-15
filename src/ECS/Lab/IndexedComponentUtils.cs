﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class IndexedComponentUtils
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "TODO")] // TODO
    internal static GetIndexedValue<TComponent,TValue> CreateGetValue<TComponent,TValue>() where TComponent : struct, IComponent
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        var method          = typeof(IndexedComponentUtils).GetMethod(nameof(GetIndexedComponentValue), flags);
        var genericMethod   = method!.MakeGenericMethod(typeof(TComponent), typeof(TValue));
        
        var genericDelegate = Delegate.CreateDelegate(typeof(GetIndexedValue<TComponent,TValue>), genericMethod);
        return (GetIndexedValue<TComponent,TValue>)genericDelegate;
    }
    
    internal static TValue GetIndexedComponentValue<TComponent,TValue>(in TComponent component) where TComponent : struct, IIndexedComponent<TValue> {
        return component.GetIndexedValue();
    }
}

internal static class IndexedComponentUtils<TComponent, TValue>  where TComponent : struct, IComponent
{
    internal static readonly GetIndexedValue<TComponent,TValue> GetIndexedValue;
        
    static IndexedComponentUtils() {
        GetIndexedValue = IndexedComponentUtils.CreateGetValue<TComponent,TValue>();
    }
}
    
internal delegate TValue GetIndexedValue<TComponent, out TValue>(in TComponent component) where TComponent : struct, IComponent;
