﻿using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static Friflo.Engine.ECS.ComponentChangedAction;

// ReSharper disable UnusedVariable
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
namespace Tests.Examples {

// See: https://friflo.gitbook.io/friflo.engine.ecs
public static class General
{

[Test]
public static void CreateStore()
{
    var store = new EntityStore();
}

[Test]
public static void CreateEntity()
{
    var store = new EntityStore();
    store.CreateEntity();
    store.CreateEntity();

    foreach (var entity in store.Entities) {
        Console.WriteLine($"entity {entity}");
    }
    // > entity id: 1  []       Info:  [] entity has no components
    // > entity id: 2  []
}

[Test]
public static void DeleteEntity()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.DeleteEntity();
    var isDeleted = entity.IsNull;
    Console.WriteLine($"deleted: {isDeleted}");     // > deleted: True
}

[Test]
public static void DisableEntity()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.Enabled = false;
    Console.WriteLine(entity);                      // > id: 1  [#Disabled]

    var query    = store.Query();
    Console.WriteLine($"default - {query}");        // > default - Query: []  Count: 0

    var disabled = store.Query().WithDisabled();
    Console.WriteLine($"disabled - {disabled}");    // > disabled - Query: []  Count: 1
}

[ComponentKey("my-component")]
public struct MyComponent : IComponent {
    public int value;
}

[Test]
public static void AddComponents()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();

    // add components
    entity.AddComponent(new EntityName("Hello World!"));// EntityName is build-in
    entity.AddComponent(new MyComponent { value = 42 });
    Console.WriteLine($"entity: {entity}");             // > entity: id: 1  "Hello World!"  [EntityName, Position]

    // get component
    Console.WriteLine($"name: {entity.Name.value}");    // > name: Hello World!
    var value = entity.GetComponent<MyComponent>().value;
    Console.WriteLine($"MyComponent: {value}");         // > MyComponent: 42

    // Serialize entity to JSON
    Console.WriteLine(entity.DebugJSON);
}

/// <summary>
/// <see cref="EntityStoreBase.GetUniqueEntity"/> is used to reduce code coupling.
/// It enables access to a unique entity without the need to pass the entity by external code.
/// </summary>
[Test]
public static void GetUniqueEntity()
{
    var store   = new EntityStore();
    store.CreateEntity(new UniqueEntity("Player"));     // UniqueEntity is build-in

    var player  = store.GetUniqueEntity("Player");
    Console.WriteLine($"entity: {player}");             // > entity: id: 1  [UniqueEntity]
}


public struct MyTag1 : ITag { }
public struct MyTag2 : ITag { }

[Test]
public static void AddTags()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();

    // add tags
    entity.AddTag<MyTag1>();
    entity.AddTag<MyTag2>();
    Console.WriteLine($"entity: {entity}");     // > entity: id: 1  [#MyTag1, #MyTag2]

    // get tag
    var tag1 = entity.Tags.Has<MyTag1>();
    Console.WriteLine($"tag1: {tag1}");         // > tag1: True
}


[Test]
public static void EntityQueries()
{
    var store   = new EntityStore();
    store.CreateEntity(new EntityName("entity-1"));
    store.CreateEntity(new EntityName("entity-2"), Tags.Get<MyTag1>());
    store.CreateEntity(new EntityName("entity-3"), Tags.Get<MyTag1, MyTag2>());

    // --- query components
    var queryNames = store.Query<EntityName>();
    queryNames.ForEachEntity((ref EntityName name, Entity entity) => {
        // ... 3 matches
    });
    // --- query components with tags
    var namesWithTags  = store.Query<EntityName>().AllTags(Tags.Get<MyTag1, MyTag2>());
    namesWithTags.ForEachEntity((ref EntityName name, Entity entity) => {
        // ... 1 match
    });
    // --- use query.Entities in case an iteration requires no component access
    foreach (var entity in queryNames.Entities) {
        // ... 3 matches
    }
}

public class MyScript : Script { public int data; }

[Test]
public static void AddScript()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();

    // add script
    entity.AddScript(new MyScript{ data = 123 });
    Console.WriteLine($"entity: {entity}");             // > entity: id: 1  [*MyScript]

    // get script
    var myScript = entity.GetScript<MyScript>();
    Console.WriteLine($"data: {myScript.data}");        // > data: 123
}

[Test]
public static void AddChildEntities()
{
    var store   = new EntityStore();
    var root    = store.CreateEntity();
    var child1  = store.CreateEntity();
    var child2  = store.CreateEntity();

    // add child entities
    root.AddChild(child1);
    root.AddChild(child2);

    Console.WriteLine($"entities: {root.ChildEntities}"); // > entities: Count: 2
}

[Test]
public static void EventHandlers()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.OnComponentChanged     += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Component: [MyComponent]
    entity.OnTagsChanged          += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Tags: [#MyTag1]
    entity.OnScriptChanged        += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Script: [*MyScript]
    entity.OnChildEntitiesChanged += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Child[0] = 2

    entity.AddComponent(new MyComponent());
    entity.AddTag<MyTag1>();
    entity.AddScript(new MyScript());
    entity.AddChild(store.CreateEntity());
}

[Test]
public static void ComponentEvents()
{
    var store  = new EntityStore();
    var entity = store.CreateEntity();
    entity.OnComponentChanged += ev =>
    {
        if (ev.Type == typeof(EntityName)) {
            string log = ev.Action switch
            {
                Add    => $"new: {ev.Component<EntityName>()}",
                Update => $"new: {ev.Component<EntityName>()}  old: {ev.OldComponent<EntityName>()}",
                Remove => $"old: {ev.OldComponent<EntityName>()}",
                _      => null
            };
            Console.WriteLine($"{ev.Action} {log}");
        }
    };
    entity.AddComponent(new EntityName("Peter"));
    entity.AddComponent(new EntityName("Paul"));
    entity.RemoveComponent<EntityName>();
}

/* Output
Add new: 'Peter'
Update new: 'Paul'  old: 'Peter'
Remove old: 'Paul'
*/


[Test]
public static void TagEvents()
{
    var store  = new EntityStore();
    var entity = store.CreateEntity();
    entity.OnTagsChanged += ev =>
    {
        string log = "";
        if (ev.AddedTags.  Has<MyTag1>()) { log += ", added:   MyTag1"; }
        if (ev.RemovedTags.Has<MyTag1>()) { log += ", removed: MyTag1"; }
        
        if (ev.AddedTags.  Has<MyTag2>()) { log += ", added:   MyTag2"; }
        if (ev.RemovedTags.Has<MyTag2>()) { log += ", removed: MyTag2"; }
        
        Console.WriteLine($"entity {entity.Id}{log}");
    };
    entity.AddTag<MyTag1>();
    entity.RemoveTag<MyTag1>();
    entity.AddTags(Tags.Get<MyTag1, MyTag2>());
}

/* Output
entity 1, added:   MyTag1
entity 1, removed: MyTag1
entity 1, added:   MyTag1, added:   MyTag2
*/
    
public readonly struct MySignal { }

[Test]
public static void AddSignalHandler()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.AddSignalHandler<MySignal>(signal => { Console.WriteLine(signal); }); // > entity: 1 - signal > MySignal
    entity.EmitSignal(new MySignal());
}

}

}