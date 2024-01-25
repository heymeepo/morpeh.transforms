# Morpeh Transforms

Transform components with hierarchy for [Morpeh ECS](https://github.com/scellecs/morpeh)

## Installation

Install via git URL

```bash
https://github.com/heymeepo/morpeh.transforms.git
```

## Usage

The following components are used in the transform system:

- LocalToWorld
- LocalTransform
- PostTransformMatrix
- Parent
- Child

There are two systems that update the transform system:

- ParentSystem
- LocalToWorldSystem

To start, add the systems in the following order.
```
ParentSystem
LocalToWorldSystem
```

The minimum set of components required for the proper functioning of the system is ```LocalToWorld``` and ```LocalTransform```. 

Add these two components to your entity and set values for ```LocalTransform```. The ```LocalToWorldSystem``` will automatically update the ```LocalToWorld``` component.

```csharp
var ent = World.CreateEntity();

ent.SetComponent(new LocalToWorld());
ent.SetComponent(new LocalTransform()
{
    position = float3.zero,
    rotation = quaternion.identity,
    scale = 1f
});
```
## Hierarchy

You can transform entities based on their relationship to each other. An entity can have multiple children, but only one parent. Children can have their own child entities. These multiple levels of parent-child relationships form a transform hierarchy. The entity at the top of a hierarchy, without a parent, is the **root**.

To declare a Transform hierarchy, you must do this from the bottom up. 

To add a new entity to the hierarchy use ```SetParent(Entity parent)``` method.

```csharp
var rootEntity = World.CreateEntity();

rootEntity.SetComponent(new LocalToWorld());
rootEntity.SetComponent(new LocalTransform()
{
    position = float3.zero,
    rotation = quaternion.identity,
    scale = 1f
});

var childEntity = World.CreateEntity();

childEntity.SetComponent(new LocalToWorld());
childEntity.SetComponent(new LocalTransform()
{
    position = new float3(1f, 1f, 0f),
    rotation = quaternion.identity,
    scale = 1f
});

childEntity.SetParent(rootEntity);             //Set new parent
childEntity.SetParent(anyOtherRootEntity);     //Change parent
childEntity.SetParent(null);                   //Unparent
```
**Attention**: If the entity has a ```Parent```, Position, Rotation, and Scale are relative to that parent. If the entity doesn't have a ```Parent```, the transform is relative to the origin of the world.

**The Child component**

The Child component holds all children of a parent. ```ParentSystem``` manages this and its contents. The system will take care of maintaining the corresponding ```Child``` component. Do not modify the values inside the ```Child``` component independently.

## Destroying the Entities

If you are using this package, do not use the ```World.RemoveEntity()``` or ```Entity.Dispose()``` methods for entity destruction.

You have two options:

- Entity.Destroy()
- Entity.DestroyHierarchy()

The method ```Entity.DestroyHierarchy()``` recursively destroys the entire hierarchy of entities, starting from the one on which this method was called.

## Cleanup Components

A new type of components is introduced. To create a cleanup component, define a struct that inherits from ```ICleanupComponent```.

When you try to destroy an entity with an attached cleanup component using ```Entity.Destroy()``` or ```Entity.DestroyHierarchy()```, all non-cleanup components will be removed instead. The entity still exists until you remove all cleanup components from it manually.

The ```ParentSystem``` uses cleanup components to fix the hierarchy after entity destruction. This means that entities with ```Parent``` or ```Child``` components after ```Entity.Destroy()``` or ```Entity.DestroyHierarchy()``` will exist until the ```ParentSystem``` is performed.

## Interaction with hierarchy and dangerous operations.

Interaction with the hierarchies within ECS is a sufficiently complex and intricate task. Below are some potentially dangerous operations on entities and their solutions.

Let's assume you have declared a filter that includes the Health component. And you want to destroy all entities along with their hierarchies whose health values are less than or equal to 0.

```csharp
    private Filter healthFilter;

    public void OnAwake()
    {
        healthFilter = World.Filter.With<HealthComponent>().Build();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var entity in healthFilter)
        {
            var health = entity.GetComponent<HealthComponent>();

            if (health.value <= 0f)
            {
                entity.DestroyHierarchy();
            }
        }
    }
```

It's not okay.

The problem lies in the fact that child entities could have also included in this filter, and calling ```DestroyHierarchy``` will destroy the Health components on child entities as well. Attempting to call ```entity.GetComponent<HealthComponent>()``` on such a destroyed entity in subsequent filter iterations will result in a runtime error.

The simplest solution to this situation is to create a buffer for entities that need to be destroyed and then destroy them after iterating through the filter.

```csharp
    private Filter healthFilter;
    private Queue<Entity> entitiesToDestroy;

    public void OnAwake()
    {
        healthFilter = World.Filter.With<HealthComponent>().Build();
        entitiesToDestroy = new Queue<Entity>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var entity in healthFilter)
        {
            var health = entity.GetComponent<HealthComponent>();

            if (health.value <= 0f)
            {
                entitiesToDestroy.Enqueue(entity);
            }
        }

        while (entitiesToDestroy.Count > 0)
        {
            var entity = entitiesToDestroy.Dequeue();
            entity.DestroyHierarchy();
        }
    }
```
If you are using the JobSystem, you are likely familiar with this approach, as currently there is no capability to destroy entities during the execution of parallel jobs.

Calling the ```DestroyHierarchy``` is safe under the condition that you are not attempting to access components from the list of entities you are destroying.

It's also worth noting that if you have the ability to exclude the ```Parent``` component from the filter and operate only on **root** entities, the following operations on entities and their child entities are completely safe, even during iteration through the filter:

- Has
- AddComponent
- SetComponent
- RemoveComponent
- DestroyHierarchy
- Destroy