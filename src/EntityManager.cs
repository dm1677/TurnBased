﻿using Godot;
using System;
using System.Collections.Generic;

public class EntityManager
{
    private int entityIDCount = 1;
    
    private readonly Dictionary<Entity, List<Component>> componentList = new Dictionary<Entity, List<Component>>();
    private readonly Dictionary<Entity, List<Component>> deletedEntities = new Dictionary<Entity, List<Component>>();

    readonly List<IObserver> observers = new List<IObserver>();

    public void Register(IObserver observer)
    {
        observers.Add(observer);
    }

    void EntityAdded(Entity entity)
    {
        foreach (var o in observers)
            o.EntityAdded(entity);
    }

    void EntityRemoved(Entity entity)
    {
        foreach (var o in observers)
            o.EntityRemoved(entity);
    }

    public Entity CreateEntity()
    {
        if (entityIDCount >= int.MaxValue)
        {
            throw new IndexOutOfRangeException("Exceeded maximum possible number of entities.");
        }

        List<Component> components = new List<Component>();
        Entity entity = new Entity(entityIDCount++, components);

        componentList.Add(entity, components);
        EntityAdded(entity);

        return entity;
    }

    public bool DeleteEntity(Entity entity)
    {
        if (componentList.ContainsKey(entity))
        {
            componentList.TryGetValue(entity, out List<Component> list);

            deletedEntities.Add(entity, list);
            componentList.Remove(entity);

            EntityRemoved(entity);
            return true;
        }
        return false;
    }

    //<summary>Completely removes an entity from the game - only intended for use in Action.Undo() for replays
    public bool DisposeEntity(Entity entity)
    {
        if (componentList.ContainsKey(entity))
        {
            componentList.TryGetValue(entity, out List<Component> list);
            componentList.Remove(entity);
            list.Clear();

            EntityRemoved(entity);
            return true;
        }
        return false;
    }

    /// <summary>Returns an entity given its ID</summary>
    public Entity GetEntity(int id)
    {
        var entityList = GetEntityList();
        foreach(Entity entity in entityList.Keys)
        {
            if (entity.ID == id) return entity;
        }

        return null;
    }

    public Entity GetInactiveEntity(int id)
    {
        foreach (Entity entity in deletedEntities.Keys)
        {
            if (entity.ID == id) return entity;
        }

        return null;
    }
    
    public Dictionary<Entity, List<Component>> GetInactiveEntityList()
    {
        return deletedEntities;
    }

    public Entity RestoreEntity(int id)
    {
        var entity = GetInactiveEntity(id);
        var list = GetInactiveComponentList(entity);

        componentList.Add(entity, list);
        deletedEntities.Remove(entity);

        entity.QueuedForDeletion = false;

        EntityAdded(entity);

        return entity;
    }

    public List<Component> GetInactiveComponentList(Entity entity)
    {
        if (deletedEntities.ContainsKey(entity))
        {
            List<Component> list;
            deletedEntities.TryGetValue(entity, out list);

            return list;
        }
        else return new List<Component>();
    }

    public void AddComponent(Entity entity, Component component)
    {
        if (component != null && entity != null)
        {
            GetComponentList(entity).Add(component);
            component.Parent = entity;
        }
    }

    public TComponent AddComponent<TComponent>(Entity entity) where TComponent : Component, new()
    {
        TComponent component = new TComponent();
        AddComponent(entity, component);
        return component;
    }

    public void RemoveComponent(Entity entity, Component component)
    {
        if (component != null)
        {
            var list = GetComponentList(entity);
            if (list.Contains(component))
                list.Remove(component);
            else
                throw new ArgumentNullException("RemoveComponent() : Null component");
        }
    }

    public Dictionary<Entity, List<Component>> GetEntityList()
    {
        return componentList;
    }

    public List<Component> GetComponentList(Entity entity)
    {
        if (componentList.ContainsKey(entity))
        {
            List<Component> list;
            componentList.TryGetValue(entity, out list);

            return list;
        }
        else if (deletedEntities.ContainsKey(entity))
            throw new Exception("GetComponentList: Operation failed - attempting to retrieve an inactive entity.");
        else throw new Exception("GetComponentList: Operation failed - entity not contained in active or inactive entity list.");
    }

    public void ClearEntityList()
    {
        componentList.Clear();
    }

    //Helper functions for returning a list of every component of a specific type

    public List<Position> GetPositions()
    {
        List<Position> positions = new List<Position>();
        foreach (Entity entity in GetEntityList().Keys)
        {
            var position = entity.GetComponent<Position>();

            if (position!=null) positions.Add(position);
        }
        return positions;
    }


    //Helper functions for returning entities of a specific type (containing a specific set of components)

    /// <summary>Returns true if an entity has both a Position and a Movement component</summary>
    public bool IsUnit(Entity entity)
    {
        var position = entity.GetComponent<Position>();
        var movement = entity.GetComponent<Movement>();

        if (position != null && movement != null)
            return true;

        return false;
    }

    public List<Entity> GetEnemyUnits(int playerID)
    {
        List<Entity> enemyUnits = new List<Entity>();

        var entityList = GetEntityList().Keys;

        foreach (Entity entity in entityList)
        {
            //Enemy units
            var position = entity.GetComponent<Position>();
            var health = entity.GetComponent<Health>();
            var owner = entity.GetComponent<Owner>();

            if ((position != null && health != null && owner != null)
              &&(owner.ownedBy != (User)playerID && owner.ownedBy != User.Neutral))
            {
                enemyUnits.Add(entity);
            }
        }

        return enemyUnits;
    }

    public TComponent GetComponent<TComponent>(int entityID) where TComponent : Component
    {
        return GetEntity(entityID).GetComponent<TComponent>();
    }

    //Can return inactive components -- be careful!!!
    public TComponent TryGetComponent<TComponent>(int entityID) where TComponent : Component
    {
        Entity entity = GetEntity(entityID);
        if (entity == null) entity = GetInactiveEntity(entityID);
        if (entity == null) return null;

        return entity.GetComponent<TComponent>();
    }

    public bool QueueForDeletion(Entity entity)
    {
        if (entity != null)
        {
            entity.QueuedForDeletion = true;
            return true;
        }
        else return false;
    }

    public bool QueueForDeletion(int entityID)
    {
        var entity = GetEntity(entityID);
        return QueueForDeletion(entity);
    }

}