using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using System.Numerics;

public struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
{
    public void Initialize(Simulation simulation) { }

    public void Dispose() { }

    // Basic pair filtering (broad usage)
    public bool AllowContactGeneration(
        int workerIndex,
        CollidableReference a,
        CollidableReference b)
        => true;

    // Compound child filtering
    public bool AllowContactGeneration(
        int workerIndex,
        CollidablePair pair,
        int childIndexA,
        int childIndexB)
        => true;

    // NEWER overload used by current BEPU versions
    public bool AllowContactGeneration(
        int workerIndex,
        CollidableReference a,
        CollidableReference b,
        ref float speculativeMargin)
    {
        speculativeMargin = 0.1f; // small safe default
        return true;
    }

    // Generic manifold configuration (MOST IMPORTANT ONE)
    public bool ConfigureContactManifold<TManifold>(
        int workerIndex,
        CollidablePair pair,
        ref TManifold manifold,
        out PairMaterialProperties material)
        where TManifold : unmanaged, IContactManifold<TManifold>
    {
        material = new PairMaterialProperties
        {
            FrictionCoefficient = 1f,
            MaximumRecoveryVelocity = 2f,
            SpringSettings = new SpringSettings(30f, 1f)
        };

        return true;
    }

    // Convex fallback overload (required but rarely used)
    public bool ConfigureContactManifold(
        int workerIndex,
        CollidablePair pair,
        int childIndexA,
        int childIndexB,
        ref ConvexContactManifold manifold)
    {
        return true;
    }
}

public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
{
    public Vector3 Gravity;

    public PoseIntegratorCallbacks(Vector3 gravity)
    {
        Gravity = gravity;
    }

    public AngularIntegrationMode AngularIntegrationMode
        => AngularIntegrationMode.Nonconserving;

    public bool AllowSubstepsForUnconstrainedBodies => false;
    public bool IntegrateVelocityForKinematics => false;

    public void Initialize(Simulation simulation) { }

    public void PrepareForIntegration(float dt) { }

    public void IntegrateVelocity(
    Vector<int> bodyIndices,
    Vector3Wide position,
    QuaternionWide orientation,
    BodyInertiaWide localInertia,
    Vector<int> integrationMask,
    int workerIndex,
    Vector<float> dt,
    ref BodyVelocityWide velocity)
    {
        var gravityWide = new Vector3Wide
        {
            X = Gravity.X * dt,
            Y = Gravity.Y * dt,
            Z = Gravity.Z * dt
        };

        Vector3Wide.Add(velocity.Linear, gravityWide, out velocity.Linear);
    }
}