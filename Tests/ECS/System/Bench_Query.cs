﻿using System;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable ConvertToConstant.Local
namespace Tests.ECS.System;

// ReSharper disable InconsistentNaming
public static class Bench_Query
{
    private static readonly int entityCount     = 32;   // 32 /   100_000
    private static readonly int JitLoop         = 10;   // 10 / 5_000_000
    
    [Test]
    public static void Test_BenchRef()
    {
        var components = new MyComponent1[32];
        // --- enable JIT optimization
        for (long i = 0; i < JitLoop; i++) {
            bench_ref(components);
        }
        
        // --- run perf
        // 1000 ~ 42,1 ms - component: int,   42,1 - component: byte
        components = new MyComponent1[entityCount];
        var stopwatch = new Stopwatch(); stopwatch.Start();
        for (long i = 0; i < 1000; i++) {
            bench_ref(components);
        }
        Console.WriteLine($"Iterate - array: {TestUtils.StopwatchMillis(stopwatch)} ms");
    }
    
    private static void bench_ref(MyComponent1[] components) {
        Span<MyComponent1> comps = components;
        for (int n = 0; n < comps.Length; n++) {
            ++comps[n].a;
        }
    }
    
    [Test]
    public static void Test_Bench()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var archetype = store.GetArchetype(Signature.Get<MyComponent1>());
        for (int n = 0; n < 32; n++) {
            store.CreateEntity(archetype);
        }
        
        // --- enable JIT optimization
        var  query = store.Query<MyComponent1>();
        for (int i = 0; i < JitLoop; i++) {
            bench_simd(query);
            bench(query);
        }
        
        for (int n = 0; n < entityCount - 32; n++) {
            store.CreateEntity(archetype);
        }
        // --- run perf
        var stopwatch = new Stopwatch(); stopwatch.Start();
        for (int i = 0; i < 1000; i++) {
            // 1000 ~ 42,1 ms - component: int,   42,1 ms - component: byte
            bench(query);
        }
        Console.WriteLine($"Iterate - Span<MyComponent1>: {TestUtils.StopwatchMillis(stopwatch)} ms");
        

        stopwatch = new Stopwatch(); stopwatch.Start();
        for (int i = 0; i < 1000; i++) {
            // 1000 ~ 14,7 ms - component: int,   2,6 ms - component: byte
            bench_simd(query);
        }
        Console.WriteLine($"Iterate - SIMD: {TestUtils.StopwatchMillis(stopwatch)} ms");
    }
    
    private static void bench(ArchetypeQuery<MyComponent1> query)
    {
        foreach (var (component, _) in query.Chunks)
        {
            var components = component.Span;
            for (int n = 0; n < components.Length; n++) {
                ++components[n].a;
            }
        }
    }
    
    private static void bench_simd(ArchetypeQuery<MyComponent1> query)
    {
        var add = Vector256.Create<int>(1);            // create byte[32] vector - all values = 1
        foreach (var (component, _) in query.Chunks)
        {
            var bytes   = component.AsSpan256<int>();  // bytes.Length - multiple of 32
            var step    = component.StepSpan256;        // step = 32
            for (int n = 0; n < bytes.Length; n += step) {
                var slice   = bytes.Slice(n, step);
                var value   = Vector256.Create<int>(slice);
                var result  = Vector256.Add(value, add); // execute 32 add instructions at once
                result.CopyTo(slice);
            }
        }
    }
    
    /// <summary> Alternative to create <see cref="Vector256{T}"/> with custom values </summary>
    // ReSharper disable once UnusedMember.Local
    private static Vector256<byte> CreateVector256_Alternative()
    {
        Span<byte> oneBytes = stackalloc byte[32] {1,1,1,1,1,1,1,1,  2,2,2,2,2,2,2,2,  3,3,3,3,3,3,3,3,  4,4,4,4,4,4,4,4};
        return Vector256.Create<byte>(oneBytes);
    }
}