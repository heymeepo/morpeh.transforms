using Scellecs.Morpeh.Systems;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Scellecs.Morpeh.Transforms
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Systems/ParentSystem")]
    public sealed class ParentSystemSO : UpdateSystem
    {
        private ParentSystem system;

        public override void OnAwake()
        {
            system = new ParentSystem() { World = World };
            system.OnAwake();
        }

        public override void OnUpdate(float deltaTime) => system.OnUpdate(deltaTime);

        public override void Dispose()
        {
            system.Dispose();
            system = null;
        }
    }
}
