﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static Friflo.Fliox.Engine.ECS.NodeFlags;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// <see cref="EntityNode"/>'s enable organizing entities in a tree graph structure.<br/>
/// The tree graph is stored in an <see cref="EntityStore"/> starting with a single <see cref="EntityStore.StoreRoot"/> entity. 
/// </summary>
/// <remarks>
/// It provide the properties listed below
/// <list type="bullet">
///   <item><see cref="Id"/> to identify an entity in its <see cref="EntityStore"/></item>
///   <item><see cref="Pid"/> used to store entities in a database</item>
///   <item><see cref="Entity"/> to access the <see cref="ECS.Entity"/> attached to a node</item>
///   <item><see cref="ParentId"/> and <see cref="ChildIds"/> to get the direct related nodes</item>
/// </list>
/// <b><see cref="Id"/></b><br/>
/// The entity id change when performing an <see cref="EntityStore"/> id cleanup.<br/>
/// The clean remove unused ids and ensure that entities with the same <see cref="Archetype"/> have consecutive ids.<br/>
/// <br/>
/// <b><see cref="Pid"/></b><br/>
/// When creating a new entity in the <see cref="EntityStore"/> it generates a random <see cref="Pid"/>
/// using <see cref="EntityStore.GenerateRandomPidForId"/>.<br/>
/// Using random pids avoid merge conflicts when multiples users make changes to the same <see cref="EntityStore"/> file / database.<br/>
/// The probability generating the same pid by two different users is:
/// <code>
///     p = 1 - exp(-r^2 / (2 * N))
///     r:  number of new entities added by a user to an existing <see cref="EntityStore"/> (not the number of all entities)
///     N:  number of possible values = int.MaxValue = 2147483647
/// </code>
/// See: https://en.wikipedia.org/wiki/Birthday_problem
/// </remarks>
public struct EntityNode
{
#region public properties
    /// <summary>Unique id within an <see cref="EntityNode"/> instance</summary>
                    public              int                 Id          =>  id;
    /// <summary>Permanent unique pid used for persistence of an entity in a database </summary>
                    public              long                Pid         =>  pid;
                    public              Archetype           Archetype   =>  archetype;
                    public              ReadOnlySpan<int>   ChildIds    =>  new (childIds, 0, childCount);
    [Browse(Never)] public              int                 ChildCount  =>  childCount;
                    public              int                 ParentId    =>  parentId;
                    public              NodeFlags           Flags       =>  flags;
                    
                    public   override   string              ToString()  => GetString();
    #endregion
    
#region internal fields
    [Browse(Never)] internal readonly   int         id;         // 4
    [Browse(Never)] internal            long        pid;        // 8
    [Browse(Never)] internal            int         parentId;   // 4
                    internal            int[]       childIds;   // 8    can be null.
    [Browse(Never)] internal            int         childCount; // 4
    [Browse(Never)] internal            NodeFlags   flags;      // 4 (1)
                    
                    internal readonly   bool        Is      (NodeFlags flag) => (flags & flag) != 0;
                    internal readonly   bool        IsNot   (NodeFlags flag) => (flags & flag) == 0;
                    
    /// <remarks>Is set to null only in <see cref="EntityStore.DeleteNode"/></remarks>
    [Browse(Never)] internal            Archetype   archetype;  //  8   can be null. Could use int to relieve GC tracing reference types 
    [Browse(Never)] internal            int         compIndex;  //  4
    [Browse(Never)] internal            int         scriptIndex;//  4
    #endregion
    
#region internal methods
    internal EntityNode(int id) {
        this.id     = id;
    }
    
    private readonly string GetString()
    {
        var sb = new StringBuilder();
        if (archetype != null) {
            EntityUtils.EntityToString(id, archetype, sb);
        } else {
            sb.Append("id: ");
            sb.Append(id);
        }
        if (childCount > 0) {
            if (sb.Length > 0) {
                sb.Append("  ");    
            }
            sb.Append("ChildCount: ");
            sb.Append(childCount);
        }
        if (flags != 0) {
            sb.Append("  flags: ");
            var startPos = sb.Length;
            if (Is(TreeNode)) {
                sb.Append("TreeNode");
            }
            if (Is(Created)) {
                if (startPos < sb.Length) {
                    sb.Append(" | ");
                }
                sb.Append("Created");
            }
        }
        return sb.ToString();
    }
    #endregion
}
