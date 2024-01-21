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