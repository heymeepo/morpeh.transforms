namespace Scellecs.Morpeh.Transforms
{
    public static class TransformsExtensions
    {
#pragma warning disable 0618
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
                entity.SetComponent(new Parent() { Value = parent });
                entity.SetComponent(new ParentChangedMarker());
            }
        }
#pragma warning restore 0618
    }
}
