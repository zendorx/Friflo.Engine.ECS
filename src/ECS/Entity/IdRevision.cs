﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[StructLayout(LayoutKind.Explicit)]
internal readonly struct IdRevision  : IEquatable<IdRevision>
{
    [FieldOffset(0)]    public   readonly   int     Id;         //  4
    [FieldOffset(4)]    public   readonly   short   Revision;   //  2
    
    [Browse(Never)]
    [FieldOffset(0)]    internal readonly   long    value;      // (8) - 4 (Id) + 2 (Revision) + 2 (padding)
    
    public          bool    Equals      (IdRevision other)              => value == other.value;
    public static   bool    operator == (IdRevision a, IdRevision b)    => a.value == b.value;
    public static   bool    operator != (IdRevision a, IdRevision b)    => a.value != b.value;
    
    public override bool    Equals(object obj)  => throw new NotImplementedException("by intenetion to avoid boxing");
    public override int     GetHashCode()       => Id ^ Revision;
    
    internal IdRevision(int id, short revision) {
        Id          = id;
        Revision    = revision;
    }
}