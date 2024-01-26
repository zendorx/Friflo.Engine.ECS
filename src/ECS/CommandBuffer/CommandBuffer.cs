﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[Obsolete("Experimental")]
public sealed class EntityCommandBuffer
{
    private readonly    ComponentCommands[] componentCommands;
    private             ComponentTypes      changedComponents;
    private readonly    EntityChanges       entityChanges;

    
    private static readonly int             MaxStructIndex = EntityStoreBase.Static.EntitySchema.maxStructIndex;
    private static readonly ComponentType[] ComponentTypes = EntityStoreBase.Static.EntitySchema.components;
    
    public EntityCommandBuffer(EntityStore store)
    {
        entityChanges       = new EntityChanges(store);
        componentCommands   = new ComponentCommands[MaxStructIndex];
        for (int n = 1; n < MaxStructIndex; n++) {
            componentCommands[n] = ComponentTypes[n].CreateComponentCommands();
        }
    }
    
    public void Playback()
    {
        foreach (var componentType in changedComponents)
        {
            var commands = componentCommands[componentType.StructIndex];
            commands.UpdateComponentTypes(entityChanges);
        }
        MoveEntitiesToNewArchetypes();
        
        foreach (var componentType in changedComponents)
        {
            var commands = componentCommands[componentType.StructIndex];
            commands.ExecuteCommands(entityChanges);
        }
        Reset();
    }
    
    private void MoveEntitiesToNewArchetypes()
    {
        var store               = entityChanges.store;
        var nodes               = store.nodes;
        var defaultArchetype    = store.defaultArchetype;
        foreach (var (entityId, componentTypes) in entityChanges.entities)
        {
            ref var node        = ref nodes[entityId];
            var curArchetype    = node.Archetype;
            if (curArchetype.componentTypes.bitSet.value == componentTypes.bitSet.value) {
                continue;
            }
            var newArchetype = store.GetArchetype(componentTypes);
            if (curArchetype == defaultArchetype) {
                node.compIndex  = Archetype.AddEntity(newArchetype, entityId);
            } else {
                if (newArchetype == defaultArchetype) {
                    Archetype.MoveLastComponentsTo(curArchetype, node.compIndex);
                } else {
                    node.compIndex  = Archetype.MoveEntityTo(curArchetype, entityId, node.compIndex, newArchetype);
                }
            }
            node.archetype  = newArchetype;
        }
    }
    
    internal void Reset()
    {
        var commands = componentCommands;
        foreach (var componentType in changedComponents)
        {
            commands[componentType.StructIndex].commandCount = 0;
        }
        entityChanges.entities.Clear();
        changedComponents = default;
    }
        
#region component
    public void AddComponent<T>(int entityId, in T component)
        where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        changedComponents.bitSet.SetBit(structIndex);
        var commands    = (ComponentCommands<T>)componentCommands[structIndex];
        var count       = commands.commandCount; 
        if (count == commands.componentCommands.Length) {
            ArrayUtils.Resize(ref commands.componentCommands, 2 * count);
        }
        commands.commandCount   = count + 1;
        ref var command         = ref commands.componentCommands[count];
        command.change          = ComponentChangedAction.Add;
        command.entityId        = entityId;
        command.component       = component;
    }
    
    public void RemoveComponent<T>(int entityId)
        where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        changedComponents.bitSet.SetBit(structIndex);
        var commands    = (ComponentCommands<T>)componentCommands[structIndex];
        var count       = commands.commandCount; 
        if (count == commands.componentCommands.Length) {
            ArrayUtils.Resize(ref commands.componentCommands, 2 * count);
        }
        commands.commandCount   = count + 1;
        ref var command         = ref commands.componentCommands[count];
        command.change          = ComponentChangedAction.Remove;
        command.entityId        = entityId;
    }
    
    public void SetComponent<T>(int entityId, in T component)
        where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        changedComponents.bitSet.SetBit(structIndex);
        var commands    = (ComponentCommands<T>)componentCommands[structIndex];
        var count       = commands.commandCount; 
        if (count == commands.componentCommands.Length) {
            ArrayUtils.Resize(ref commands.componentCommands, 2 * count);
        }
        commands.commandCount   = count + 1;
        ref var command         = ref commands.componentCommands[count];
        command.change          = ComponentChangedAction.Update;
        command.entityId        = entityId;
        command.component       = component;
    }
    #endregion
    
#region tag
    
    public void AddTag<T>(Entity entity)
        where T : struct, ITag
    {
        
    }
    
    public void RemoveTag<T>(Entity entity)
        where T : struct, ITag
    {
        
    }
#endregion
}

