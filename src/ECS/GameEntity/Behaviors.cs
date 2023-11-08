﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public struct EntityScripts
{
    internal readonly   int                 id;
    /// <summary>
    /// Invariant:<br/>
    /// <see cref="id"/> == 0   :   <see cref="classes"/> == null<br/>
    /// <see cref="id"/>  > 0   :   <see cref="classes"/> != null  <b>and</b> its Length > 0 
    /// </summary>
    internal            Script[]            classes;
    
    public   override   string              ToString() => GetString();

    internal EntityScripts (int id, Script[] classes)
    {
        this.id         = id;
        this.classes    = classes;
    }
    
    private string GetString()
    {
        if (classes == null) {
            return "unused";
        }
        var sb = new StringBuilder();
        sb.Append("id: ");
        sb.Append(id);
        sb.Append("  [");
        foreach (var script in classes) {
            sb.Append('*');
            sb.Append(script.GetType().Name);
            sb.Append(", ");
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
}