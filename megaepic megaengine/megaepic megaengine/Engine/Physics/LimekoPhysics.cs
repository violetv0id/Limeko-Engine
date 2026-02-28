using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities.Memory;
using Limeko.Entities;
using System.Diagnostics;
using System.Numerics;
using Vector3 = System.Numerics.Vector3;

public static class Physics
{
    public static Simulation Simulation;
    static BufferPool _bufferPool;
    public static List<Entity> RegisteredBodies = new();

    public static bool isRunning { get; private set; }

    static float accumulator;
    const float FixedTimestep = 1f / 60f;


    public static void Initialize()
    {
        isRunning = false;
        _bufferPool = new BufferPool();

        Console.WriteLine($"[BEPU]: Fixed Timestep is {FixedTimestep}");

        var solveDescription = new SolveDescription(
            velocityIterationCount: 8,
            substepCount: 1);
        Console.WriteLine($"[BEPU]: Solver IC: {solveDescription.VelocityIterationCount}");
        Console.WriteLine($"[BEPU]: Solver Substep: {solveDescription.SubstepCount}");

        Simulation = Simulation.Create(
            _bufferPool,
            new NarrowPhaseCallbacks(),
            new PoseIntegratorCallbacks(new System.Numerics.Vector3(0, -9.81f, 0)),
            solveDescription);

        isRunning = true;
    }

    public static void Dispose()
    {
        isRunning = false;
        Simulation.Dispose();
        RegisteredBodies.Clear();
    }

    public static void RegisterBody(Entity entity)
    {
        if (!isRunning)
        {
            Console.WriteLine("LimekoPhysics Error: Simulation not running.");
            return;
        }

        RegisteredBodies.Add(entity);

        var pose = new RigidPose
        {
            Position = (System.Numerics.Vector3)entity.Transform.Position,
            Orientation = (System.Numerics.Quaternion)entity.Transform.Rotation
        };

        if (entity.PhysicsShape is Box box)
        {
            entity.ShapeIndex = Simulation.Shapes.Add(box);

            if (entity.Rigidbody.isStatic)
            {
                entity.StaticHandle =
                    Simulation.Statics.Add(new StaticDescription(pose, entity.ShapeIndex));
            }
            else
            {
                var inertia = box.ComputeInertia(entity.Rigidbody.mass);

                entity.Rigidbody.Description = BodyDescription.CreateDynamic(pose, inertia, new CollidableDescription(entity.ShapeIndex, 0.1f), new BodyActivityDescription(0.01f));
                entity.DynamicHandle =
                    Simulation.Bodies.Add(entity.Rigidbody.Description);
            }
        }
        else if (entity.PhysicsShape is Sphere sphere)
        {
            entity.ShapeIndex = Simulation.Shapes.Add(sphere);

            if (entity.Rigidbody.isStatic)
            {
                entity.StaticHandle =
                    Simulation.Statics.Add(new StaticDescription(pose, entity.ShapeIndex));
            }
            else
            {
                var inertia = sphere.ComputeInertia(entity.Rigidbody.mass);

                entity.DynamicHandle =
                    Simulation.Bodies.Add(
                        BodyDescription.CreateDynamic(
                            pose,
                            inertia,
                            new CollidableDescription(entity.ShapeIndex, 0.1f),
                            new BodyActivityDescription(0.01f)));
            }
        }
        else
        {
            throw new NotSupportedException(
                $"Unsupported shape type: {entity.PhysicsShape?.GetType().Name}");
        }
    }

    public static void Step(float dt)
    {
        dt = MathF.Min(dt, 0.1f);
        accumulator += dt;

        while (accumulator >= FixedTimestep)
        {
            Simulation.Timestep(FixedTimestep);

            foreach (var entity in RegisteredBodies)
            {
                if (entity.Rigidbody.isStatic)
                    continue;

                var body = Simulation.Bodies.GetBodyReference(entity.DynamicHandle);

                entity.Transform.Position =
                    (OpenTK.Mathematics.Vector3)body.Pose.Position;

                entity.Transform.Rotation =
                    (OpenTK.Mathematics.Quaternion)body.Pose.Orientation;
            }

            accumulator -= FixedTimestep;
        }
    }

    public static bool PickEntity(System.Numerics.Vector3 origin, System.Numerics.Vector3 direction, float maxDistance, out Entity picked)
    {
        var handler = new PickHandler();
        Physics.Simulation.RayCast(origin, direction, maxDistance, ref handler);

        picked = handler.HitEntity;
        return picked != null;
    }

    public class PickHandler : IRayHitHandler
    {
        public Entity HitEntity = null;
        public float HitT = float.MaxValue;

        public bool AllowTest(CollidableReference collidable) => true;
        public bool AllowTest(CollidableReference collidable, int childIndex) => true;

        public void OnRayHit(in RayData ray, ref float maximumT, float t, in System.Numerics.Vector3 normal, CollidableReference collidable, int childIndex)
        {
            if (t >= HitT) return; // already have closer hit

            // check dynamic bodies first
            if (collidable.Mobility == CollidableMobility.Dynamic)
            {
                var bodyHandle = collidable.BodyHandle;
                HitEntity = Physics.RegisteredBodies
                               .FirstOrDefault(e => e.DynamicHandle == bodyHandle);
            }
            else if (collidable.Mobility == CollidableMobility.Static)
            {
                var staticHandle = collidable.StaticHandle;
                HitEntity = Physics.RegisteredBodies
                               .FirstOrDefault(e => e.StaticHandle == staticHandle);
            }

            if (HitEntity != null)
            {
                HitT = t;
                maximumT = t; // shorten the ray so farther hits are ignored
            }
        }
    }
}