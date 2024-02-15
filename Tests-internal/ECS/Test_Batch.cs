using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_Batch
{
    [Test]
    public static void Test_Batch_Entity()
    {
        var store = new EntityStore();
        store.OnComponentAdded      += _ => { };
        store.OnComponentRemoved    += _ => { };
        store.OnTagsChanged         += _ => { }; 
        var entity = store.CreateEntity();
        
        var batch = entity.Batch;
        Assert.AreEqual("empty", batch.ToString());
        
        batch.AddComponent  (new Position(1, 1, 1))
            .AddComponent   (new EntityName("test"))
            .RemoveComponent<Rotation>()
            .AddTag         <TestTag>()
            .RemoveTag      <TestTag2>();
        Assert.AreEqual("add: [EntityName, Position, #TestTag]  remove: [Rotation, #TestTag2]", batch.ToString());
        Assert.AreEqual(5, batch.CommandCount);
        batch.Apply();
        
        Assert.AreEqual("id: 1  \"test\"  [EntityName, Position, #TestTag]", entity.ToString());
        Assert.AreEqual(new Position(1, 1, 1), entity.Position);
        
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        batch = entity.Batch;
        batch.AddComponent  (new Position(2, 2, 2))
            .RemoveComponent<EntityName>()
            .AddTags        (addTags)
            .RemoveTags     (removeTags);
        Assert.AreEqual("add: [Position, #TestTag2]  remove: [EntityName, #TestTag]", batch.ToString());
        Assert.AreEqual(4, batch.CommandCount);
        batch.Apply();
        
        Assert.AreEqual("id: 1  [Position, #TestTag2]", entity.ToString());
        Assert.AreEqual(new Position(2, 2, 2), entity.Position);
    }
    
    [Test]
    public static void Test_Batch_ApplyTo()
    {
        var store   = new EntityStore();
        var batch   = new EntityBatch();
        batch.AddComponent(new Position());
        batch.AddTag<TestTag>();
        
        var entity = store.CreateEntity();
        batch.ApplyTo(entity);
        
        var e = Assert.Throws<InvalidOperationException>(() => {
            batch.Apply();
        });
        Assert.AreEqual("Apply() can only be used on Entity.Batch. Use ApplyTo()", e!.Message);
    }
    
    [Test]
    public static void Test_Batch_QueryEntities_ApplyBatch()
    {
        var store       = new EntityStore();
        for (int n = 0; n < 10; n++) {
            store.CreateEntity();
        }
        var batch = new EntityBatch();
        batch.AddComponent(new Position(2, 3, 4));
        store.Entities.ApplyBatch(batch);
        
        var arch = store.GetArchetype(ComponentTypes.Get<Position>());
        Assert.AreEqual(10, arch.EntityCount);
        
        batch.Clear();
        batch.AddTag<TestTag>();
        
        store.Query<Position>().Entities.ApplyBatch(batch);
        
        arch = store.GetArchetype(ComponentTypes.Get<Position>(), Tags.Get<TestTag>());
        Assert.AreEqual(10, arch.EntityCount);
    }
    
    [Test]
    public static void Test_Batch_Entity_Perf()
    {
        int count       = 10; // 10_000_000 ~ #PC: 1691 ms
        var store       = new EntityStore();
        var entity      = store.CreateEntity();
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        var sw = new Stopwatch();
        sw.Start();

        for (int n = 0; n < count; n++)
        {
            entity.Batch
                .AddComponent   (new Position(1, 1, 1))
                .AddComponent   (new EntityName("test"))
                .RemoveComponent<Rotation>()
                .AddTag         <TestTag>()
                .RemoveTag      <TestTag2>()
                .Apply();
        
            entity.Batch
                .AddComponent   (new Position(2, 2, 2))
                .RemoveComponent<EntityName>()
                .AddTags        (addTags)
                .RemoveTags     (removeTags)
                .Apply();
        }
        
        Console.WriteLine($"Entity.Batch - duration: {sw.ElapsedMilliseconds} ms");
        Assert.AreEqual("id: 1  [Position, #TestTag2]", entity.ToString());
    }
    
    [Test]
    public static void Test_QueryEntities_ApplyBatch_Perf()
    {
        int count       = 10; // 100_000 ~ #PC: 1943 ms
        int entityCount = 100;
        var store   = new EntityStore();
        for (int n = 0; n < entityCount; n++) {
            store.CreateEntity();
        }
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        var batch1 = new EntityBatch();
        var batch2 = new EntityBatch();
        
        batch1
            .AddComponent   (new Position(1, 1, 1))
            .AddComponent   (new EntityName("test"))
            .RemoveComponent<Rotation>()
            .AddTag         <TestTag>()
            .RemoveTag      <TestTag2>();
        batch2
            .AddComponent   (new Position(2, 2, 2))
            .RemoveComponent<EntityName>()
            .AddTags        (addTags)
            .RemoveTags     (removeTags);
        
        var sw = new Stopwatch();
        sw.Start();

        for (int n = 0; n < count; n++)
        {
            store.Entities.ApplyBatch(batch1);
            store.Entities.ApplyBatch(batch2);
        }
        Console.WriteLine($"ApplyBatch() - duration: {sw.ElapsedMilliseconds} ms");
        
        var arch = store.GetArchetype(ComponentTypes.Get<Position>(), Tags.Get<TestTag2>());
        Assert.AreEqual(entityCount, arch.EntityCount);
    }
}

