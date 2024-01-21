namespace Scellecs.Morpeh.Transforms
{
    public static class TransformsExtensions
    { 
        public static void SetParent(this Entity entity, Entity parent) 
        { 
            if (parent.IsNullOrDisposed()) 
            {
                if (entity.Has<Parent>())
                {
                    entity.RemoveComponent<Parent>();
                }
            }
            else
            {
                entity.SetComponent(new Parent() { Value = parent.ID });
                entity.SetComponent(new ParentChangedMarker());
            }
        }
    }
}
